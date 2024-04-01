// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System.Collections.Generic;

namespace SVSModel.Configuration
{
    /// <summary>
    /// Class that stores the configuration information for a rotation of 3 crops in the correct type.  
    /// I.e constructor takes all config settings as objects and converts them to appropriates types
    /// </summary>
    public class FieldConfig
    {
        // Inputs
        public string WeatherStation { get; init; }
        public string SoilCategory { get; init; }
        public string SoilTexture { get; init; }
        public double PMN { get; init; }
        public int Splits { get; init; }
        public double _rawRocks { internal get; init; }
        public string _sampleDepth { internal get; init; }
        public string _prePlantRain { internal get; init; }
        public string _inCropRain { internal get; init; }
        public string _irrigation { internal get; init; }

        // Calculated fields
        public double Rocks => _rawRocks / 100;
        public double SampleDepthFactor => Constants.SampleDepthFactor[_sampleDepth];
        public double BulkDensity => Constants.ParticleDensity[SoilCategory] * Constants.Porosity[SoilTexture];
        public double AWC => 3 * Constants.AWCpct[SoilTexture] * (1 - Rocks);
        public double PrePlantRainFactor => Constants.PPRainFactors[_prePlantRain];
        public double InCropRainFactor => Constants.ICRainFactors[_inCropRain];
        public double IrrigationTrigger => Constants.IrrigationTriggers[_irrigation];
        public double IrrigationRefill => Constants.IrrigationRefill[_irrigation];

        /// <summary>
        /// Constructor used only by external webapp
        /// </summary>
        public FieldConfig() { }

        /// <summary>
        /// Constructor used only by the Excel model
        /// </summary>
        public FieldConfig(Dictionary<string, object> c)
        {
            // Only raw input values should be set in here
            WeatherStation = c["WeatherStation"].ToString();
            SoilCategory = c["SoilCategory"].ToString();
            SoilTexture = c["Texture"].ToString();
            PMN = Functions.Num(c["PMN"]);
            Splits = int.Parse(c["Splits"].ToString());
            
            _rawRocks = Functions.Num(c["Rocks"]);
            _sampleDepth = c["SampleDepth"].ToString();
            _prePlantRain = c["PrePlantRain"].ToString();
            _inCropRain = c["InCropRain"].ToString();
            _irrigation = c["Irrigation"].ToString();
        }
    }
}