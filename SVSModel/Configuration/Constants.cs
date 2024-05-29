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
        public static readonly Dictionary<string, double> SampleDepthFactor = new()
        {
            { "Top15cm", 0.75 },
            { "Top30cm", 1 },
            { "Top60cm", 1.25 },
            { "Top90cm", 1.5 }
        };

        /// <summary>Available water capacity %</summary>
        public static readonly Dictionary<string, double> AWCpct = new()
        {
            { "Sand",          8 },
            { "LoamySand",     18 },
            { "SandyLoam",     23 },
            { "SandyClay",     20 },
            { "SandyClayLoam", 16 },
            { "Loam",          22 },
            { "Silt",          22 },
            { "SiltLoam",      22 },
            { "SiltyClayLoam", 20 },
            { "ClayLoam",      18 },
            { "SiltyClay",     20 },
            { "Clay",          18 },
        };

        /// <summary>The porocity (mm3 pores/mm3 soil volume) of different soil texture classes</summary>
        public static readonly Dictionary<string, double> Porosity = new()
        {
            { "Sand",          0.5 },
            { "LoamySand",     0.51 },
            { "SandyLoam",     0.52 },
            { "SandyClay",     0.54 },
            { "SandyClayLoam", 0.56 },
            { "Loam",          0.54 },
            { "Silt",          0.54 },
            { "SiltLoam",      0.55 },
            { "SiltyClayLoam", 0.58 },
            { "ClayLoam",      0.58 },
            { "SiltyClay",     0.61 },
            { "Clay",          0.63 },
        };

        /// <summary>particle bulk density (g/mm3)</summary>
        public static readonly Dictionary<string, double> ParticleDensity = new()
        {
            { "Sedimentary", 2.65 },
            { "Volcanic", 1.9 },
        };

        public static double BulkDensity(string soilCategory, string soilTexture)
        {
            return Constants.ParticleDensity[soilCategory] * (1 - Constants.Porosity[soilTexture]);
        }

        public static readonly Dictionary<string, Dictionary<string, double>> MoistureFactor = new Dictionary<string, Dictionary<string, double>>()
        {
            {"Clay",          new Dictionary<string, double>() { { "Dry", 1.8}, { "Moist", 1.5},{ "Wet", 1.3} } },
            {"ClayLoam",      new Dictionary<string, double>() { { "Dry", 1.7}, { "Moist", 1.4},{ "Wet", 1.3} } },
            {"Loam",          new Dictionary<string, double>() { { "Dry", 2.0}, { "Moist", 1.5},{ "Wet", 1.3} } },
            {"LoamySand",     new Dictionary<string, double>() { { "Dry", 1.8}, { "Moist", 1.5},{ "Wet", 1.4} } },
            {"Sand",          new Dictionary<string, double>() { { "Dry", 1.8}, { "Moist", 1.5},{ "Wet", 1.4} } },
            {"SandyClay",     new Dictionary<string, double>() { { "Dry", 1.8}, { "Moist", 1.4},{ "Wet", 1.3} } },
            {"SandyClayLoam", new Dictionary<string, double>() { { "Dry", 1.9}, { "Moist", 1.6},{ "Wet", 1.4} } },
            {"SandyLoam",     new Dictionary<string, double>() { { "Dry", 2.1}, { "Moist", 1.8},{ "Wet", 1.5} } },
            {"Silt",          new Dictionary<string, double>() { { "Dry", 1.9}, { "Moist", 1.4},{ "Wet", 1.3} } },
            {"SiltLoam",      new Dictionary<string, double>() { { "Dry", 1.7}, { "Moist", 1.4},{ "Wet", 1.3} } },
            {"SiltyClay",     new Dictionary<string, double>() { { "Dry", 1.9}, { "Moist", 1.6},{ "Wet", 1.4} } },
            {"SiltyClayLoam", new Dictionary<string, double>() { { "Dry", 1.9}, { "Moist", 1.5},{ "Wet", 1.4} } },
        };
    }
}
   