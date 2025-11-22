using System;
using TechTalk.SpecFlow;
using Xunit;
using BPCalculator;

namespace SpecFlowTests.Steps
{
    [Binding]
    public class BloodPressureSteps
    {
        private BloodPressure? _bp;

        [Given(@"a systolic reading of (.*) and diastolic reading of (.*)")]
        public void GivenASystolicReadingOfAndDiastolicReadingOf(int systolic, int diastolic)
        {
            _bp = new BloodPressure { Systolic = systolic, Diastolic = diastolic };
        }

        [When("the blood pressure is evaluated")]
        public void WhenTheBloodPressureIsEvaluated()
        {
            // evaluation happens via properties on BloodPressure; nothing to do
        }

        [Then("the category should be (.*)")]
        public void ThenTheCategoryShouldBe(string expectedCategory)
        {
            Assert.NotNull(_bp);
            Assert.Equal(expectedCategory, _bp!.Category.ToString());
        }
    }
}
