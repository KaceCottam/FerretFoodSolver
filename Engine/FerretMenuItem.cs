using System;

namespace FerretFoodSolver.Engine
{
    public class FerretMenuItem
    {
        public string? Description { get; set; }
        public double MusclePercent { get; set; }
        public double OrganPercent { get; set; }
        public double HeartPercent { get; set; }
        public double BonePercent { get; set; }
        public double WeightConversion { get; set; }
        public double? Solution { get; internal set; }

        public FerretMenuItem()
        {
            MusclePercent = 0;
            OrganPercent = 0;
            HeartPercent = 0;
            BonePercent = 0;
            WeightConversion = 1;
        }

        public void Verify()
        {
            if (WeightConversion <= 0)
                throw new ArgumentOutOfRangeException(nameof(WeightConversion), WeightConversion, "All parameter weight conversions must be > 0!");
            if (MusclePercent + OrganPercent + HeartPercent + BonePercent is < 0.99 or > 1.01)
                throw new ArgumentException("All parameter %s must sum to be between 99% and 101%!");
            if (MusclePercent is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(MusclePercent), MusclePercent, "Parameter Target % must be between 0.0 and 1.0!");
            if (OrganPercent is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(OrganPercent), OrganPercent, "Parameter Target % must be between 0.0 and 1.0!");
            if (HeartPercent is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(HeartPercent), HeartPercent, "Parameter Target % must be between 0.0 and 1.0!");
            if (BonePercent is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(BonePercent), BonePercent, "Parameter Target % must be between 0.0 and 1.0!");
        }
    }
}
