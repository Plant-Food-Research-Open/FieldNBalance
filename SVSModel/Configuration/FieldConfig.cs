using System.Collections.Generic;

namespace SVSModel.Configuration
{
    /// <summary>
    /// Class that stores the configuration information for a rotation of 3 crops in the correct type.  
    /// I.e constructor takes all config settings as objects and converts them to appropriates types
    /// </summary>
    public class FieldConfig
    {
        public double InitialN { get; set; }
        public string SoilOrder { get; set; }
        public double SampleDepthFactor { get; set; }
        public double BulkDensity { get; set; }
        public string PMNtype { get; set; }
        public double PMNconversion
        {
            get
            {
                if (PMNtype == "PMN")
                    return 1.0;
                else
                    return 0.964;
            }
        }
        public double PMN { get; set; }
        public double Trigger { get; set; }
        public double Efficiency { get; set; }
        public int Splits { get; set; }
        public double AWC { get; set; }
        public double PrePlantRainFactor { get; set; }
        public double InCropRainFactor { get; set; }
        public double IrrigationTrigger { get; set; }
        public double IrrigationRefill { get; set; }

        public FieldConfig() { }

        public FieldConfig(Dictionary<string, object> c)
        {
            InitialN = Functions.Num(c["InitialN"]);
            SoilOrder = c["SoilOrder"].ToString();
            SampleDepthFactor = Constants.SampleDepthFactor[c["SampleDepth"].ToString()];
            BulkDensity = Functions.Num(c["BulkDensity"]);
            PMNtype = c["PMNtype"].ToString();
            PMN = Functions.Num(c["PMN"]);
            Trigger = Functions.Num(c["Trigger"]);
            Efficiency = Functions.Num(c["Efficiency"]) / 100;
            Splits = int.Parse(c["Splits"].ToString());
            AWC = Functions.Num(c["AWC"]);
            PrePlantRainFactor = Constants.PPRainFactors[c["PrePlantRain"].ToString()];
            InCropRainFactor = Constants.ICRainFactors[c["InCropRain"].ToString()];
            IrrigationRefill = Constants.IrrigationRefill[c["Irrigation"].ToString()];
            IrrigationTrigger = Constants.IrrigationTriggers[c["Irrigation"].ToString()];
        }
    }
}
