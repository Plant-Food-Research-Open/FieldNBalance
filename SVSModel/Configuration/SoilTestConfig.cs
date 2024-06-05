// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System;
using System.Collections.Generic;
using static SVSModel.Configuration.Constants;
using static SVSModel.Configuration.InputCategories;

namespace SVSModel.Configuration;

/// <summary>
/// Class that stores the configuration information in the correct type for a specific Soil Test.  
/// I.E. constructor takes all config settings as objects and converts them to appropriates types
/// </summary>
public class SoilTestConfig(
    DateTime testDate,
    double testValue,
    string depthOfSample,
    string typeOfTest,
    string moistureOfSample,
    string categoryOfSoil,
    string textureOfSoil)
{
    // Inputs
    private DateTime TestDate { get; } = testDate;
    private double TestValue { get; } = testValue;
    private string DepthOfSample {get;} = depthOfSample;
    private string TypeOfTest { get; } = typeOfTest;
    private string MoistureOfSample {  get; } = moistureOfSample;
    private string CategoryOfSoil { get; } = categoryOfSoil;
    private string TextureOfSoil { get; } = textureOfSoil;

    private double BulkDensity
    {
        get { return Constants.BulkDensity(CategoryOfSoil, TextureOfSoil); }
    }

    public Dictionary<DateTime, double> Result
    {
        get
        {
            double soilMF = 1;
            if (TypeOfTest == TestType.QuickTest.ToString())
            {
                soilMF = MoistureFactor[TextureOfSoil][MoistureOfSample];
            }
            
            var soilDepthFactor = SampleDepthFactor[DepthOfSample];
            var result = TestValue / soilMF * BulkDensity * 3 * soilDepthFactor;
            
            return new Dictionary<DateTime, double> { { TestDate, result } };
        }
    }
}
