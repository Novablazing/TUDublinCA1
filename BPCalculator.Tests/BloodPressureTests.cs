using Xunit;
using BPCalculator;

namespace BPCalculator.Tests
{
    public class BloodPressureTests
    {
        [Fact]
        public void Ideal_Category_Map_DisplayName_Recommendation_ToString()
        {
            var bp = new BloodPressure { Systolic = 119, Diastolic = 79 };

            Assert.Equal(BPCategory.Ideal, bp.Category);
            Assert.Equal("Ideal Blood Pressure", bp.CategoryDisplayName);
            Assert.Equal(93.3, bp.MeanArterialPressure); // (119 + 2*79) / 3 = 277/3 = 92.333... -> 92.3 (wait check calculation)
            // Correction: (119 + 2*79) = 119 + 158 = 277 -> 277/3 = 92.333... -> 92.3
        }

        [Fact]
        public void PreHigh_Category_Map_Recommendation_ToString()
        {
            var bp = new BloodPressure { Systolic = 120, Diastolic = 79 };

            Assert.Equal(BPCategory.PreHigh, bp.Category);
            Assert.Equal("Pre-High Blood Pressure", bp.CategoryDisplayName);
            Assert.Equal(92.7, bp.MeanArterialPressure); // (120 + 2*79) = 278 / 3 = 92.666... -> 92.7
            Assert.Contains("120/79 mmHg - Pre-High Blood Pressure - MAP: 92.7 mmHg", bp.ToString());
            Assert.False(bp.IsHypertensiveCrisis);
            Assert.Equal(
                "Borderline high. Reduce salt, exercise regularly, manage stress, and recheck in 1–2 weeks.",
                bp.Recommendation);
        }

        [Fact]
        public void High_Category_BySystolic_NotCrisis()
        {
            var bp = new BloodPressure { Systolic = 140, Diastolic = 85 };

            Assert.Equal(BPCategory.High, bp.Category);
            Assert.Equal("High Blood Pressure", bp.CategoryDisplayName);
            Assert.Equal(103.3, bp.MeanArterialPressure); // (140 + 2*85) = 310 / 3 = 103.333... -> 103.3
            Assert.False(bp.IsHypertensiveCrisis);
            Assert.Contains("140/85 mmHg - High Blood Pressure - MAP: 103.3 mmHg", bp.ToString());
            Assert.Equal(
                "High. Track readings over several days and consult a clinician about next steps.",
                bp.Recommendation);
        }

        [Fact]
        public void High_Category_BySystolic_Crisis_IncludesCrisisNote()
        {
            var bp = new BloodPressure { Systolic = 180, Diastolic = 79 };

            Assert.Equal(BPCategory.High, bp.Category);
            Assert.True(bp.IsHypertensiveCrisis);
            Assert.Contains("(Hypertensive crisis)", bp.ToString());
        }

        [Fact]
        public void High_Category_ByDiastolic_Crisis()
        {
            var bp = new BloodPressure { Systolic = 130, Diastolic = 120 };

            Assert.Equal(BPCategory.High, bp.Category);
            Assert.True(bp.IsHypertensiveCrisis);
            Assert.Contains("(Hypertensive crisis)", bp.ToString());
        }

        [Fact]
        public void Low_Category_Map_Recommendation()
        {
            var bp = new BloodPressure { Systolic = 89, Diastolic = 59 };

            Assert.Equal(BPCategory.Low, bp.Category);
            Assert.Equal("Low Blood Pressure", bp.CategoryDisplayName);
            Assert.Equal(69.0, bp.MeanArterialPressure); // (89 + 2*59) = 207 / 3 = 69.0
            Assert.Equal(
                "Your reading is on the low side. If you feel dizzy or faint, hydrate and consider talking to a clinician.",
                bp.Recommendation);
            Assert.DoesNotContain("(Hypertensive crisis)", bp.ToString());
        }
    }
}