using System;

namespace SVSModel.Models;

public static class Defaults
{
    public static readonly string PriorCropColloquial = "Oat";
    public static readonly string PriorCropEndUse = "Fodder";
    public static readonly string PriorCropType = "General";
    public static readonly string PriorCropNameFull = $"{PriorCropColloquial} {PriorCropEndUse} {PriorCropType}";

    public static readonly string CurrentCropColloquial = "Oat";
    public static readonly string CurrentCropEndUse = "Fodder";
    public static readonly string CurrentCropType = "General";
    public static readonly string CurrentCropNameFull = $"{CurrentCropColloquial} {CurrentCropEndUse} {CurrentCropType}";

    public static readonly string NextCropColloquial = "Oat";
    public static readonly string NextCropEndUse = "Fodder";
    public static readonly string NextCropType = "General";
    public static readonly string NextCropNameFull = $"{NextCropColloquial} {NextCropEndUse} {NextCropType}";

    public static readonly string EstablishStage = "Seed";
    public static readonly string HarvestStage = "EarlyReproductive";
    public static readonly double SaleableYield = 10;
    public static readonly string Units = "t/ha";

    public static readonly double FieldLoss = 0;
    public static readonly double MoistureContent = 0;

    public static readonly int GrowingDays = 125;
    public static readonly DateTime EstablishDate = DateTime.Today;
    public static readonly DateTime HarvestDate = DateTime.Today.AddDays(GrowingDays);

    public static readonly string ResidueRemoval = "None removed";
    public static readonly string ResidueIncorporation = "Full (Plough)";

    public static readonly string SoilCategory = "Sedimentary";
    public static readonly string SoilTexture = "silt loam";
    public static readonly string SampleDepth = "0-30cm";
    public static readonly double PMN = 55;
    public static readonly int Splits = 3;
    public static readonly string RainPrior = "Typical";
    public static readonly string RainDuring = "Typical";
    public static readonly string IrrigationApplied = "None";
}
