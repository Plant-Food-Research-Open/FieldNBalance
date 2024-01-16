using System;
using System.Collections.Generic;
using CsvHelper.Configuration;

namespace SVSModel.Models
{
    public class CropCoefficient
    {
        public string UniqueName { get; set; }
        public int Index { get; set; }
        public string EndUse { get; set; }
        public string Group { get; set; }
        public string ColloquialName { get; set; }
        public string Type { get; set; }
        public string Family { get; set; }
        public string Genus { get; set; }
        public string SpecificEpithet { get; set; }
        public string SubSpecies { get; set; }
        public string SpeciesName { get; set; }
        public string EpithetAndSubSpecies { get; set; }
        public string TypicalEstablishStage { get; set; }
        public string TypicalEstablishMonth { get; set; }
        public string TypicalHarvestStage { get; set; }
        public string TypicalHarvestMonth { get; set; }
        public double TypicalYield { get; set; }
        public string TypicalYieldUnits { get; set; }
        public string YieldType { get; set; }
        public double TypicalPopulationPerHa { get; set; }
        public string TotalOrDry { get; set; }
        public double TypicalDressingLossPercent { get; set; }
        public double TypicalFieldLossPercent { get; set; }
        public double TypicalHI { get; set; }
        public double HIRange { get; set; }
        public double MoisturePercent { get; set; }
        public double PRoot { get; set; }
        public double MaxRD { get; set; }
        public double ACover { get; set; }
        public double RCover { get; set; }
        public double RootN { get; set; }
        public double StOverN { get; set; }
        public double ProductN { get; set; }
    }

    public sealed class CropCoefficientMap : ClassMap<CropCoefficient>
    {
        public CropCoefficientMap()
        {
            Map(c => c.UniqueName).Name("UniqueName");
            Map(c => c.Index).Name("Index").Default(0);
            Map(c => c.EndUse).Name("EndUse");
            Map(c => c.Group).Name("Group");
            Map(c => c.ColloquialName).Name("Colloquial Name");
            Map(c => c.Type).Name("Type");
            Map(c => c.Family).Name("Family");
            Map(c => c.Genus).Name("Genus");
            Map(c => c.SpecificEpithet).Name("Specific epithet");
            Map(c => c.SubSpecies).Name("Sub species");
            Map(c => c.SpeciesName).Name("Species name");
            Map(c => c.EpithetAndSubSpecies).Name("Epithet and sub species");
            Map(c => c.TypicalEstablishStage).Name("Typical Establish Stage");
            Map(c => c.TypicalEstablishMonth).Name("Typical Establish month");
            Map(c => c.TypicalHarvestStage).Name("Typical Harvest Stage");
            Map(c => c.TypicalHarvestMonth).Name("Typical Harvest month");
            Map(c => c.TypicalYield).Name("Typical Yield").Default(0);
            Map(c => c.TypicalYieldUnits).Name("Typical Yield Units");
            Map(c => c.YieldType).Name("Yield type");
            Map(c => c.TypicalPopulationPerHa).Name("Typical Population (/ha)").Default(0);
            Map(c => c.TotalOrDry).Name("TotalOrDry");
            Map(c => c.TypicalDressingLossPercent).Name("Typical Dressing Loss %").Default(0);
            Map(c => c.TypicalFieldLossPercent).Name("Typical Field Loss %").Default(0);
            Map(c => c.TypicalHI).Name("Typical HI").Default(0);
            Map(c => c.HIRange).Name("HI Range").Default(0);
            Map(c => c.MoisturePercent).Name("Moisture %").Default(0);
            Map(c => c.PRoot).Name("P Root").Default(0);
            Map(c => c.MaxRD).Name("Max RD").Default(0);
            Map(c => c.ACover).Name("A cover").Default(0);
            Map(c => c.RCover).Name("rCover").Default(0);
            Map(c => c.RootN).Name("Root [N]").Default(0);
            Map(c => c.StOverN).Name("Stover [N]").Default(0);
            Map(c => c.ProductN).Name("Product [N]").Default(0);
        }
    }

    public class WeatherStationData
    {
        public int DOY { get; set; }
        public double MeanT { get; set; }
        public double Rain { get; set; }
        public double MeanPET { get; set; }
    }

    public class MetDataDictionaries
    {
        public Dictionary<DateTime, double> MeanT { get; set; }
        public Dictionary<DateTime, double> Rain { get; set; }
        public Dictionary<DateTime, double> MeanPET { get; set; }
    }

    public class DailyNBalance
    {
        public DateTime Date { get; set; }
        public double SoilMineralN { get; set; }
        public double UptakeN { get; set; }
        public double ResidueN { get; set; }
        public double SoilOMN { get; set; }
        public double FertiliserN { get; set; }
        public double CropN { get; set; }
        public double ProductN { get; set; }
        public double LostN { get; set; }
        public double RSWC { get; set; }
        public double Drainage { get; set; }
        public double Irrigation { get; set; }
        public double GreenCover { get; set; }
    }
}
