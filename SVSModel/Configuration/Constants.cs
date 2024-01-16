using System.Collections.Generic;

namespace SVSModel.Configuration
{
    public class Constants
    {
        /// <summary>Dictionary containing values for the proportion of maximum DM that occurs at each predefined crop stage</summary>
        public static readonly Dictionary<string, double> PropnMaxDM = new Dictionary<string, double> {
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
        public static readonly Dictionary<string, double> PropnTt = new Dictionary<string, double> {
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
        public static readonly Dictionary<string, double> UnitConversions = new Dictionary<string, double>
        {
            { "t/ha", 1000 },
            { "kg/ha", 1.0 },
            { "kg/head", 1.0 }
        };

        /// <summary>Dictionary containing conversion from specified residue treatments to proportoins returned </summary>
        public static readonly Dictionary<string, double> ResidueFactRetained = new Dictionary<string, double>
        {
            { "None removed", 1.0 },
            { "Baled", 0.2 },
            { "Burnt", 0.05 },
            { "Grazed", 0.4 },
            { "All removed", 0.0 }
        };

        /// <summary>Dictionary containing conversion from specified residue treatments to proportoins returned </summary>
        public static readonly Dictionary<string, double> ResidueIncorporation = new Dictionary<string, double>
        {
            { "None (Surface)", 0.0 },
            { "Part (Cultivate)", 0.5 },
            { "Full (Plough)", 0.95 }
        };

        /// <summary>Dictionary containing conversion from specified rainfall conditions to a factor </summary>
        public static readonly Dictionary<string, double> ICRainFactors = new Dictionary<string, double>
        {
            { "Very Wet", 1.7 },
            { "Wet", 1.35},
            { "Typical", 1.0 },
            { "Dry", 0.65 },
            { "Very Dry", 0.3 }
        };

        /// <summary>Dictionary containing conversion from specified rainfall conditions to a factor </summary>
        public static readonly Dictionary<string, double> PPRainFactors = new Dictionary<string, double>
        {
            { "Very Wet", 1.0 },
            { "Wet", 0.95},
            { "Typical", 0.9 },
            { "Dry", 0.6 },
            { "Very Dry", 0.3 }
        };

        /// <summary>Dictionary containing conversion from specified irrigation method to trigger point factors  </summary>
        public static readonly Dictionary<string, double> IrrigationTriggers = new Dictionary<string, double>
        {
            { "None", 0.0 },
            { "Some", 0.4 },
            { "Full", 0.7 }
        };

        /// <summary>Dictionary containing conversion from specified irrigation method to refill target factors </summary>
        public static readonly Dictionary<string, double> IrrigationRefill = new Dictionary<string, double>
        {
            { "None", 0.0 },
            { "Some", 0.8 },
            { "Full", 0.9 }
        };

        /// <summary>Sample depth factor to adjust measurments to equivelent of 30cm measure</summary>
        public static readonly Dictionary<string, double> SampleDepthFactor = new Dictionary<string, double>
        {
            { "0-15cm", 0.75 },
            { "0-30cm", 1 }
        };
    }
}
