using System;
using Reqnroll;
using Xunit;
using BPCalculator;

namespace BPCalculator.BDD.StepDefinitions
{
    [Binding]
    public class BloodPressureSteps
    {
        private BloodPressure _bloodPressure = new BloodPressure();
        private BPCategory _resultCategory;

        [Given(@"the systolic pressure is (.*)")]
        public void GivenTheSystolicPressureIs(int systolic)
        {
            _bloodPressure.Systolic = systolic;
        }

        [Given(@"the diastolic pressure is (.*)")]
        public void GivenTheDiastolicPressureIs(int diastolic)
        {
            _bloodPressure.Diastolic = diastolic;
        }

        [When(@"the blood pressure category is calculated")]
        public void WhenTheBloodPressureCategoryIsCalculated()
        {
            _resultCategory = _bloodPressure.Category;
        }

        [Then(@"the category should be (.*)")]
        public void ThenTheCategoryShouldBe(string expectedCategory)
        {
            Assert.Equal(expectedCategory, _resultCategory.ToString());
        }
    }
}
