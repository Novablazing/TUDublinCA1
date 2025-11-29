using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;

namespace BPCalculator
{
    // BP categories
    public enum BPCategory
    {
        [Display(Name = "Low Blood Pressure")] Low,
        [Display(Name = "Ideal Blood Pressure")] Ideal,
        [Display(Name = "Pre-High Blood Pressure")] PreHigh,
        [Display(Name = "High Blood Pressure")] High
    };

    public class BloodPressure
    {
        public const int SystolicMin = 70;
        public const int SystolicMax = 190;
        public const int DiastolicMin = 40;
        public const int DiastolicMax = 100;

        [Range(SystolicMin, SystolicMax, ErrorMessage = "Invalid Systolic Value")]
        public int Systolic { get; set; }                       // mmHG

        [Range(DiastolicMin, DiastolicMax, ErrorMessage = "Invalid Diastolic Value")]
        public int Diastolic { get; set; }                      // mmHG

        // calculate BP category
        public BPCategory Category
        {
            get
            {
                if (Systolic >= 140 || Diastolic >= 90)
                    return BPCategory.High;
                if ((Systolic >= 120 && Systolic <= 139) || (Diastolic >= 80 && Diastolic <= 89))
                    return BPCategory.PreHigh;
                if (Systolic < 90 || Diastolic < 60)
                    return BPCategory.Low;
                
                return BPCategory.Ideal;
            }
        }

        // Mean Arterial Pressure (MAP) in mmHg: (Systolic + 2*Diastolic) / 3
        public double MeanArterialPressure => Math.Round((Systolic + 2.0 * Diastolic) / 3.0, 1);

        // Hypertensive crisis detector (systolic >= 180 or diastolic >= 120)
        public bool IsHypertensiveCrisis => Systolic >= 180 || Diastolic >= 120;

        // Friendly display name from the BPCategory Display attribute (falls back to enum name)
        public string CategoryDisplayName
        {
            get
            {
                return Category switch
                {
                    BPCategory.Low => "Low Blood Pressure",
                    BPCategory.Ideal => "Ideal Blood Pressure",
                    BPCategory.PreHigh => "Pre-High Blood Pressure",
                    BPCategory.High => "High Blood Pressure",
                    _ => Category.ToString()
                };
            }
        }

        public string Recommendation
        {
            get
            {
                return Category switch
                {
                    BPCategory.Low =>
                        "Your reading is on the low side. If you feel dizzy or faint, hydrate and consider talking to a clinician.",
                    BPCategory.Ideal =>
                        "Great! Maintain a balanced diet, regular activity, and periodic checks.",
                    BPCategory.PreHigh =>
                        "Borderline high. Reduce salt, exercise regularly, manage stress, and recheck in 1–2 weeks.",
                    BPCategory.High =>
                        "High. Track readings over several days and consult a clinician about next steps.",
                    _ =>
                        "Consider rechecking your blood pressure to confirm this result."
                };
            }
        }

        public override string ToString()
        {
            var crisisNote = IsHypertensiveCrisis ? " (Hypertensive crisis)" : string.Empty;
            return $"{Systolic}/{Diastolic} mmHg - {CategoryDisplayName} - MAP: {MeanArterialPressure} mmHg{crisisNote}";
        }
    }
}
