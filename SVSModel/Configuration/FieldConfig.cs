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
        public string SoilCategory { get; set; }
        public string SoilTexture { get; set; }
        public double Rocks { get; set; }
        public double SampleDepthFactor { get; set; }
        public double BulkDensity { get; set; }
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
            InitialN = 50;
            SoilCategory = c["SoilCategory"].ToString();
            SoilTexture = c["Texture"].ToString();
            Rocks = Functions.Num(c["Rocks"])/100;
            SampleDepthFactor = Constants.SampleDepthFactor[c["SampleDepth"].ToString()];
            BulkDensity = Constants.ParticleDensity[c["SoilCategory"].ToString()] * Constants.Porosity[c["Texture"].ToString()];
            PMN = Functions.Num(c["PMN"]);
            Trigger = 30;
            Efficiency = 1.0;
            Splits = int.Parse(c["Splits"].ToString());
            AWC = 3 * Constants.AWCpct[SoilTexture] * (1-Rocks);
            PrePlantRainFactor = Constants.PPRainFactors[c["PrePlantRain"].ToString()];
            InCropRainFactor = Constants.ICRainFactors[c["InCropRain"].ToString()];
            IrrigationRefill = Constants.IrrigationRefill[c["Irrigation"].ToString()];
            IrrigationTrigger = Constants.IrrigationTriggers[c["Irrigation"].ToString()];
        }
    }
}
