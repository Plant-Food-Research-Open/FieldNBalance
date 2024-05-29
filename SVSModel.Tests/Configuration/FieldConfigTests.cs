// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System.Collections.Generic;
using SVSModel.Configuration;
using SVSModel.Models;
using Xunit;
using static SVSModel.Configuration.InputCategories;

namespace SVSModel.Tests.Configuration;

public class FieldConfigTests
{
    private static readonly string WeatherStation = Defaults.WeatherStation;
    private static readonly SoilCategoris SoilCategory = Defaults.SoilCategory;
    private static readonly SoilTextures Texture = Defaults.SoilTexture;
    private static readonly double PMN = Defaults.PMN;
    private static readonly int Splits = Defaults.Splits;
    private static readonly double Rocks = 10;
    private static readonly SampleDepths SampleDepth = Defaults.SampleDepth;
    private static readonly string PrePlantRain = Defaults.RainPrior;
    private static readonly string InCropRain = Defaults.RainDuring;
    private static readonly string Irrigation = Defaults.IrrigationApplied;

    private readonly Dictionary<string, object> ExcelInputDict = new()
    {
        { "WeatherStation", WeatherStation },
        { "SoilCategory", SoilCategory },
        { "Texture", Texture },
        { "PMN", PMN },
        { "Splits", Splits },
        { "Rocks", Rocks },
        { "SampleDepth", SampleDepth },
        { "PrePlantRain", PrePlantRain },
        { "InCropRain", InCropRain },
        { "Irrigation", Irrigation }
    };

    [Fact]
    public void Test_FieldConfig_Excel_Gets_Values_Correctly()
    {
        var fieldConfig = new FieldConfig(ExcelInputDict);

        Assert.Equal(fieldConfig.Rocks, Rocks / 100);
        Assert.Equal(fieldConfig.SampleDepthFactor, Constants.SampleDepthFactor[SampleDepth]);
        Assert.Equal(fieldConfig.BulkDensity, Constants.BulkDensity(SoilCategory, Texture));
        Assert.Equal(fieldConfig.AWC, 3 * Constants.AWCpct[Texture] * (1 - Rocks / 100));
        Assert.Equal(fieldConfig.PrePlantRainFactor, Constants.PPRainFactors[PrePlantRain]);
        Assert.Equal(fieldConfig.InCropRainFactor, Constants.ICRainFactors[InCropRain]);
        Assert.Equal(fieldConfig.IrrigationTrigger, Constants.IrrigationTriggers[Irrigation]);
        Assert.Equal(fieldConfig.IrrigationRefill, Constants.IrrigationRefill[Irrigation]);
    }

    [Fact]
    public void Test_FieldConfig_Both_Constructors_Match()
    {
        var fieldConfig = new FieldConfig
        {
            Category = SoilCategory,
            Texture = Texture,
            PMN = PMN,
            Splits = Splits,
            _rawRocks = Rocks,
            SampleDepth = SampleDepth,
            _prePlantRain = PrePlantRain,
            _inCropRain = InCropRain,
            _irrigation = Irrigation
        };
        
        var fieldConfigExcel = new FieldConfig(ExcelInputDict);
        
        Assert.Equal(fieldConfig.Category, fieldConfigExcel.Category);
        Assert.Equal(fieldConfig.Texture, fieldConfigExcel.Texture);
        Assert.Equal(fieldConfig.Rocks, fieldConfigExcel.Rocks);
        Assert.Equal(fieldConfig.SampleDepthFactor, fieldConfigExcel.SampleDepthFactor);
        Assert.Equal(fieldConfig.BulkDensity, fieldConfigExcel.BulkDensity);
        Assert.Equal(fieldConfig.PMN, fieldConfigExcel.PMN);
        Assert.Equal(fieldConfig.Splits, fieldConfigExcel.Splits);
        Assert.Equal(fieldConfig.AWC, fieldConfigExcel.AWC);
        Assert.Equal(fieldConfig.PrePlantRainFactor, fieldConfigExcel.PrePlantRainFactor);
        Assert.Equal(fieldConfig.InCropRainFactor, fieldConfigExcel.InCropRainFactor);
        Assert.Equal(fieldConfig.IrrigationTrigger, fieldConfigExcel.IrrigationTrigger);
        Assert.Equal(fieldConfig.IrrigationRefill, fieldConfigExcel.IrrigationRefill);
    }
}