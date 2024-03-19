using System.Collections.Generic;

namespace SVSModel.Configuration
{
    public static class Constants
    {
        /// <summary>Dictionary containing values for the proportion of maximum DM that occurs at each predefined crop stage</summary>
        public static readonly Dictionary<string, double> PropnMaxDM = new()
        {
            { "Seed", 0.0066 },
            { "Seedling", 0.015 },
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
            { "Seed", 0 },
            { "Seedling", 0.16 },
            { "Vegetative", 0.5 },
            { "EarlyReproductive", 0.61 },
            { "MidReproductive", 0.69 },
            { "LateReproductive", 0.8 },
            { "Maturity", 1.0 },
            { "Late", 1.27 }
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
            { "0-15cm", 0.75 },
            { "0-30cm", 1 }
        };

        /// <summary>Available water capacity %</summary>
        public static readonly Dictionary<string, double> AWCpct = new()
        {
            { "coarse sand", 5 },
            { "fine sand", 15 },
            { "loamy sand", 18 },
            { "sandy loam", 23 },
            { "sandy clay loam", 16 },
            { "loam", 22 },
            { "silt loam", 22 },
            { "silty clay loam", 20 },
            { "clay loam", 18 },
            { "silty clay", 20 },
            { "clay", 18 },
            { "peat", 20 }
        };

        /// <summary>The porocity (mm3 pores/mm3 soil volume) of different soil texture classes</summary>
        public static readonly Dictionary<string, double> Porosity = new()
        {
            { "coarse sand", 0.20 },
            { "fine sand", 0.25 },
            { "loamy sand", 0.30 },
            { "sandy loam", 0.30 },
            { "sandy clay loam", 0.16 },
            { "loam", 0.40 },
            { "silt loam", 0.40 },
            { "silty clay loam", 0.43 },
            { "clay loam", 0.46 },
            { "silty clay", 0.45 },
            { "clay", 0.50 },
            { "peat", 0.50 }
        };

        /// <summary>particle bulk density (g/mm3)</summary>
        public static readonly Dictionary<string, double> ParticleDensity = new()
        {
            { "Sedimentary", 2.65 },
            { "Volcanic", 1.9 }
        };
    }
}