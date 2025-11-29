using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BPCalculator.SeleniumTests
{
    [TestClass]
    public class BloodPressureSeleniumTests
    {
        // Base URL can be provided via RunSettings (TestRunParameters: BaseUrl) or environment variable TEST_BASEURL.
        // If it points to localhost we will spin up the app; otherwise we assume remote environment already running.
        private string _baseUrl;
        private IWebDriver _driver;
        private Process _appProcess;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            var paramBaseUrl = TestContext?.Properties?["BaseUrl"] as string;
            _baseUrl = (paramBaseUrl ?? Environment.GetEnvironmentVariable("TEST_BASEURL") ?? "http://localhost:5000").TrimEnd('/');

            if (_baseUrl.Contains("localhost"))
            {
                // Start the web application via 'dotnet run' only for local testing
                var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
                var appProjectPath = Path.Combine(solutionRoot, "BPCalculator", "BPCalculator.csproj");

                _appProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"run --project \"{appProjectPath}\" --urls={_baseUrl}",
                        WorkingDirectory = solutionRoot,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                _appProcess.Start();
            }

            // Wait until server responds (remote or local)
            WaitForServerReady(_baseUrl, TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless=new");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--window-size=1920,1080");

            _driver = new ChromeDriver(chromeOptions);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [TestCleanup]
        public void Teardown()
        {
            try { _driver?.Quit(); } catch { /* ignore */ }
            if (_appProcess != null && !_appProcess.HasExited)
            {
                try { _appProcess.Kill(true); } catch { /* ignore */ }
                _appProcess.Dispose();
            }
        }

        private static async Task WaitForServerReady(string url, TimeSpan timeout)
        {
            using var http = new HttpClient();
            var deadline = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    var resp = await http.GetAsync(url);
                    if (resp.IsSuccessStatusCode) return; // ready
                }
                catch { /* swallow until timeout */ }
                await Task.Delay(500);
            }
            throw new TimeoutException($"Server at {url} not responding within {timeout.TotalSeconds} seconds");
        }

        [TestMethod]
        public void Submit_PreHigh_Classification_ShowsExpectedValues()
        {
            _driver.Navigate().GoToUrl(_baseUrl);

            // Locate inputs by id generated from TagHelper (period replaced with underscore)
            var systolicInput = _driver.FindElement(By.Id("BP_Systolic"));
            var diastolicInput = _driver.FindElement(By.Id("BP_Diastolic"));

            systolicInput.Clear();
            systolicInput.SendKeys("135");
            diastolicInput.Clear();
            diastolicInput.SendKeys("85");

            // Submit form
            var submitButton = _driver.FindElement(By.CssSelector("form#form1 input[type='submit']"));
            submitButton.Click();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            // Wait for the readonly Category input with Pre-High value (custom condition)
            wait.Until(d => d.FindElement(By.XPath("//input[@readonly and @value='Pre-High Blood Pressure']")));

            // MAP is (135 + 2*85)/3 = (135 + 170)/3 = 305/3 = 101.6667 -> 101.7 rounded
            var mapInput = _driver.FindElement(By.XPath("//label[text()='Mean Arterial Pressure (MAP)']/following-sibling::input"));
            StringAssert.Contains(mapInput.GetAttribute("value"), "101.7");
        }

        [TestMethod]
        public void HypertensiveCrisis_ShowsAlert()
        {
            _driver.Navigate().GoToUrl(_baseUrl);

            var systolicInput = _driver.FindElement(By.Id("BP_Systolic"));
            var diastolicInput = _driver.FindElement(By.Id("BP_Diastolic"));

            systolicInput.Clear();
            systolicInput.SendKeys("185"); // crisis threshold >= 180
            diastolicInput.Clear();
            diastolicInput.SendKeys("95");

            _driver.FindElement(By.CssSelector("form#form1 input[type='submit']")).Click();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            var alert = wait.Until(d => d.FindElement(By.CssSelector("div.alert.alert-danger")));
            StringAssert.Contains(alert.Text, "Hypertensive crisis");
        }
    }
}
