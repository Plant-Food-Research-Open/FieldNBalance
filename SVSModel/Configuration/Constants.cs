// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System.Collections.Generic;
using System.ComponentModel;
using static SVSModel.Configuration.InputCategories;

namespace SVSModel.Configuration
{
    public static class Constants
    {
        public const double Trigger = 30;
        public const double InitialN = 50;

        /// <summary>Dictionary containing values for the proportion of maximum DM that occurs at each predefined crop stage</summary>
        public static readonly Dictionary<string, double> PropnMaxDM = new()
        {
            { "Seed", 0.004 },
            { "Seedling", 0.011 },
            { "Vegetative", 0.5 },
            { "EarlyReproductive", 0.75 },
            { "MidReproductive", 0.86 },
            { "LateReproductive", 0.95 },
            { "Maturity", 0.9933 },
            { "Late", 0.9995 }
        };

        /// <summary>Dictionary containing values for the proportion of thermal time to maturity that has accumulate at each predefined crop stage</summary>
        public static readonly Dictionary<string, double> PropnTt = new()
        {
            { "Seed", -0.0517 },
            { "Seedling", 0.050 },
            { "Vegetative", 0.5 },
            { "EarlyReproductive", 0.5847 },
            { "MidReproductive", 0.6815 },
            { "LateReproductive", 0.7944 },
            { "Maturity", 0.999 },
            { "Late", 1.2957 }
        };

        /// <summary>Dictionary containing conversion from specified units to kg/ha which are the units that the model works in </summary>
        public static readonly Dictionary<string, double> UnitConversions = new()
        {
            { "t/ha", 1000 },
            { "kg/ha", 1.0 },
            { "kg/head", 1.0 }
        };

        /// <summary>Dictionary containing conversion from specified residue treatments to proportoins returned </summary>
        public static readonly Dictionary<string, double> ResidueFactRetained = new()
        {
            { "None removed", 1.0 },
            { "Baled", 0.2 },
            { "Burnt", 0.05 },
            { "Grazed", 0.4 },
            { "All removed", 0.0 }
        };

        /// <summary>Dictionary containing conversion from specified residue treatments to proportoins returned </summary>
        public static readonly Dictionary<string, double> ResidueIncorporation = new()
        {
            { "None (Surface)", 0.0 },
            { "Part (Cultivate)", 0.5 },
            { "Full (Plough)", 0.95 }
        };

        /// <summary>Dictionary containing conversion from specified rainfall conditions to a factor </summary>
        public static readonly Dictionary<string, double> ICRainFactors = new()
        {
            { "Very Wet", 1.7 },
            { "Wet", 1.35 },
            { "Typical", 1.0 },
            { "Dry", 0.65 },
            { "Very Dry", 0.3 }
        };

        /// <summary>Dictionary containing conversion from specified rainfall conditions to a factor </summary>
        public static readonly Dictionary<string, double> PPRainFactors = new()
        {
            { "Very Wet", 1.0 },
            { "Wet", 0.95 },
            { "Typical", 0.9 },
            { "Dry", 0.6 },
            { "Very Dry", 0.3 }
        };

        /// <summary>Dictionary containing conversion from specified irrigation method to trigger point factors  </summary>
        public static readonly Dictionary<string, double> IrrigationTriggers = new()
        {
            { "None", 0.0 },
            { "Some", 0.4 },
            { "Full", 0.7 }
        };

        /// <summary>Dictionary containing conversion from specified irrigation method to refill target factors </summary>
        public static readonly Dictionary<string, double> IrrigationRefill = new()
        {
            { "None", 0.0 },
            { "Some", 0.8 },
            { "Full", 0.9 }
        };

        /// <summary>Sample depth factor to adjust measurments to equivelent of 30cm measure</summary>
        public static readonly Dictionary<SampleDepth, double> SampleDepthFactor = new()
        {
            { SampleDepth.Top15cm, 0.75 },
            { SampleDepth.Top30cm, 1 },
            { SampleDepth.Top60cm, 1.25 },
            { SampleDepth.Top90cm, 1.5 }
        };

        /// <summary>Available water capacity %</summary>
        public static readonly Dictionary<SoilTexture, double> AWCpct = new()
        {
            { SoilTexture.Sand,          8 },
            { SoilTexture.LoamySand,     18 },
            { SoilTexture.SandyLoam,     23 },
            { SoilTexture.SandyClay,     20 },
            { SoilTexture.SandyClayLoam, 16 },
            { SoilTexture.Loam,          22 },
            { SoilTexture.Silt,          22 },
            { SoilTexture.SiltLoam,      22 },
            { SoilTexture.SiltyClayLoam, 20 },
            { SoilTexture.ClayLoam,      18 },
            { SoilTexture.SiltyClay,     20 },
            { SoilTexture.Clay,          18 },
        };

        /// <summary>The porocity (mm3 pores/mm3 soil volume) of different soil texture classes</summary>
        public static readonly Dictionary<SoilTexture, double> Porosity = new()
        {
            { SoilTexture.Sand,          0.5 },
            { SoilTexture.LoamySand,     0.51 },
            { SoilTexture.SandyLoam,     0.52 },
            { SoilTexture.SandyClay,     0.54 },
            { SoilTexture.SandyClayLoam, 0.56 },
            { SoilTexture.Loam,          0.54 },
            { SoilTexture.Silt,          0.54 },
            { SoilTexture.SiltLoam,      0.55 },
            { SoilTexture.SiltyClayLoam, 0.58 },
            { SoilTexture.ClayLoam,      0.58 },
            { SoilTexture.SiltyClay,     0.61 },
            { SoilTexture.Clay,          0.63 },
        };

        /// <summary>particle bulk density (g/mm3)</summary>
        public static readonly Dictionary<SoilCategory, double> ParticleDensity = new()
        {
            { SoilCategory.Sedimentary, 2.65 },
            { SoilCategory.Volcanic, 1.9 },
        };

        public static double BulkDensity(SoilCategory soilCategory, SoilTexture soilTexture)
        {
            return Constants.ParticleDensity[soilCategory] * (1 - Constants.Porosity[soilTexture]);
        }

        public static readonly Dictionary<string, Dictionary<string, double>> MoistureFactor = new Dictionary<string, Dictionary<string, double>>()
        {
            {SoilTexture.Clay.ToString(),          new Dictionary<string, double>() { { "Dry", 1.8}, { "Moist", 1.5},{ "Wet", 1.3} } },
            {SoilTexture.ClayLoam.ToString(),      new Dictionary<string, double>() { { "Dry", 1.7}, { "Moist", 1.4},{ "Wet", 1.3} } },
            {SoilTexture.Loam.ToString(),          new Dictionary<string, double>() { { "Dry", 2.0}, { "Moist", 1.5},{ "Wet", 1.3} } },
            {SoilTexture.LoamySand.ToString(),     new Dictionary<string, double>() { { "Dry", 1.8}, { "Moist", 1.5},{ "Wet", 1.4} } },
            {SoilTexture.Sand.ToString(),          new Dictionary<string, double>() { { "Dry", 1.8}, { "Moist", 1.5},{ "Wet", 1.4} } },
            {SoilTexture.SandyClay.ToString(),     new Dictionary<string, double>() { { "Dry", 1.8}, { "Moist", 1.4},{ "Wet", 1.3} } },
            {SoilTexture.SandyClayLoam.ToString(), new Dictionary<string, double>() { { "Dry", 1.9}, { "Moist", 1.6},{ "Wet", 1.4} } },
            {SoilTexture.SandyLoam.ToString(),     new Dictionary<string, double>() { { "Dry", 2.1}, { "Moist", 1.8},{ "Wet", 1.5} } },
            {SoilTexture.Silt.ToString(),          new Dictionary<string, double>() { { "Dry", 1.9}, { "Moist", 1.4},{ "Wet", 1.3} } },
            {SoilTexture.SiltLoam.ToString(),      new Dictionary<string, double>() { { "Dry", 1.7}, { "Moist", 1.4},{ "Wet", 1.3} } },
            {SoilTexture.SiltyClay.ToString(),     new Dictionary<string, double>() { { "Dry", 1.9}, { "Moist", 1.6},{ "Wet", 1.4} } },
            {SoilTexture.SiltyClayLoam.ToString(), new Dictionary<string, double>() { { "Dry", 1.9}, { "Moist", 1.5},{ "Wet", 1.4} } },
        };
    }
}
   