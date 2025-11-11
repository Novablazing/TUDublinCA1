using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace BPCalculator
{
    // BP categories
    public enum BPCategory
    {
        [Display(Name="Low Blood Pressure")] Low,
        [Display(Name="Ideal Blood Pressure")]  Ideal,
        [Display(Name="Pre-High Blood Pressure")] PreHigh,
        [Display(Name ="High Blood Pressure")]  High
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
                if ((Systolic >= 120 && Systolic <= 139) ||(Diastolic >= 80 && Diastolic <= 89))
                    return BPCategory.PreHigh;
                if (Systolic < 90 || Diastolic < 60)
                    return BPCategory.Low;
                if (Systolic < 120 && Diastolic < 80)
                    return BPCategory.Ideal;
                // implement as part of project
                //throw new NotImplementedException("not implemented yet");
                return BPCategory.Ideal;
                //return new BPCategory();                       // replace this
            }
        }
    }
}
