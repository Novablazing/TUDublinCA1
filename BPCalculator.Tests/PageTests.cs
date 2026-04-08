using Xunit;
using BPCalculator.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;

namespace BPCalculator.Tests
{
    public class PageTests
    {
        [Fact]
        public void Index_OnGet_InitializesBP()
        {
            var page = new BloodPressureModel();
            page.OnGet();
            Assert.NotNull(page.BP);
            Assert.Equal(100, page.BP.Systolic);
            Assert.Equal(60, page.BP.Diastolic);
        }

        [Fact]
        public void Index_OnPost_ValidBP_ReturnsPage()
        {
            var page = new BloodPressureModel();
            page.BP = new BloodPressure { Systolic = 120, Diastolic = 80 };
            
            var result = page.OnPost();
            
            Assert.IsType<PageResult>(result);
            Assert.True(page.ModelState.IsValid);
        }

        [Fact]
        public void Index_OnPost_InvalidBP_AddsError()
        {
            var page = new BloodPressureModel();
            page.BP = new BloodPressure { Systolic = 80, Diastolic = 90 }; // Invalid: Systolic < Diastolic
            
            var result = page.OnPost();
            
            Assert.IsType<PageResult>(result);
            Assert.False(page.ModelState.IsValid);
            Assert.True(page.ModelState.ContainsKey(string.Empty));
            Assert.Equal("Systolic must be greater than Diastolic", page.ModelState[string.Empty]?.Errors[0]?.ErrorMessage);
        }

        [Fact]
        public void Privacy_OnGet_Runs()
        {
            var mockLogger = new Mock<ILogger<PrivacyModel>>();
            var page = new PrivacyModel(mockLogger.Object);
            page.OnGet();
            // No assertions needed, just ensuring it runs without exception
        }
    }
}
