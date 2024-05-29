// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System;
using System.Collections.Generic;
using static SVSModel.Configuration.InputCategories;

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
        public SoilCategoris Category { get; init; }
        public SoilTextures Texture { get; init; }
        public double PMN { get; init; }
        public int Splits { get; init; }
        public double _rawRocks { internal get; init; }
        public SampleDepths _sampleDepth { internal get; init; }
        public string _prePlantRain { internal get; init; }
        public string _inCropRain { internal get; init; }
        public string _irrigation { internal get; init; }
        
        // Calculated fields
        public double Rocks => _rawRocks / 100;
        public double SampleDepthFactor => Constants.SampleDepthFactor[_sampleDepth];
        public double BulkDensity => Constants.BulkDensity(Category, Texture);
        public double AWC => 3 * Constants.AWCpct[Texture] * (1 - Rocks);
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
            Enum.TryParse(c["SoilCategory"].ToString(), out SoilCategoris Category);
            Enum.TryParse(c["Texture"].ToString(), out SoilTextures Texture);
            PMN = Functions.Num(c["PMN"]);
            Splits = int.Parse(c["Splits"].ToString());

            _rawRocks = Functions.Num(c["Rocks"]);
            Enum.TryParse(c["SampleDepth"].ToString(), out SampleDepths _sampleDepth);
            _prePlantRain = c["PrePlantRain"].ToString();
            _inCropRain = c["InCropRain"].ToString();
            _irrigation = c["Irrigation"].ToString();
        }
    }
}