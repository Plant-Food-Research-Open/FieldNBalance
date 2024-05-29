// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System;
using System.Collections.Generic;
using SVSModel.Models;
using static SVSModel.Configuration.Constants;
using static SVSModel.Configuration.InputCategories;

namespace SVSModel.Configuration;

/// <summary>
/// Class that stores the configuration information in the correct type for a specific Soil Test .  
/// I.e constructor takes all config settings as objects and converts them to appropriates types
/// </summary>
public class SoilTestConfig
{
    // Inputs
    public DateTime TestDate { get; init; }
    public double TestValue { get; init; }
    public SampleDepth DepthOfSample {get; init;}
    public TestType TypeOfTest { get; init; }
    public TestMoisture MoistureOfSample {  get; init; } 
    public SoilCategory CategoryOfSoil { get; init; }
    public SoilTexture TextureOfSoil { get; init; }

    public double BulkDensity
    {
        get { return Constants.BulkDensity(CategoryOfSoil, TextureOfSoil); }
    }

    public Dictionary<DateTime, double> Result
    {
        get
        {
            double soilMF = 1;
            if (TypeOfTest.ToString()==TestType.QuickTest.ToString())
                soilMF = Constants.MoistureFactor[TextureOfSoil.ToString()][MoistureOfSample.ToString()];
            double soilDepthFactor = SampleDepthFactor[DepthOfSample];
            double result = TestValue / soilMF * BulkDensity * 3 * soilDepthFactor;
            return new Dictionary<DateTime, double>() { { TestDate, result } };
        }
    }
    
    /// <summary>
    /// Constructor used only by external webapp
    /// </summary>
    public SoilTestConfig() { }

    /// <summary>
    /// Constructor used only by the Excel model
    /// </summary>
    public SoilTestConfig(DateTime testDate, double testValue, SampleDepth depthOfSample, 
        TestType typeOfTest, TestMoisture moistureOfSample, SoilCategory categoryOfSoil, SoilTexture textureOfSoil)
    {
        TestDate = testDate;
        TestValue = testValue;
        DepthOfSample = depthOfSample;
        TypeOfTest = typeOfTest;  
        MoistureOfSample = moistureOfSample;
        CategoryOfSoil = categoryOfSoil;
        TextureOfSoil = textureOfSoil;
    }
}
