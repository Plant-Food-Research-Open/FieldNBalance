// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System;
using System.Collections.Generic;
using SVSModel.Configuration;
using ExcelDna.Integration;
using System.Linq;
using SVSModel.Models;
using static SVSModel.Configuration.Constants;
using static SVSModel.Configuration.InputCategories;

namespace SVSModel.Excel
{
    public interface IMyFunctions
    {
        object[,] GetDailyNBalance(object[,] met, object[,] config, object[,] testResults, object[,] nApplied);

        object[,] GetDailyCropData(double[] Tt, object[,] Config);

        object[,] GetCropCoefficients();

        object[,] GetSoilTestResult(object testDate, double testValue, string depthOfSample,
                                               string typeOfTest, string moistureOfTest,
                                               string soilCategory, string soilTexture);
    }

    public static class MyFunctions //: IMyFunctions
    {
        /// <summary>
        /// Takes soil test input values and returns a value in kg N/ha
        /// </summary>
        /// <param name="testDate">Date of test</param>
        /// <param name="testValue">value of test in mgN/g soil</param>
        /// <param name="depthOfSample">0-15, 30, 60 or 90cm</param>
        /// <param name="typeOfTest">Lab or Quicktest</param>
        /// <param name="moistureOfTest">Wet, Moist, dry</param>
        /// <param name="categoryOfSoil">Volcanic or sedementary</param>
        /// <param name="textureOfSoil">a valid texture class</param>
        /// <returns>Soil nitrogen in kg/ha</returns>
        public static object[,] GetSoilTestResult(object testDate, double testValue, 
                                                                     string depthOfSample,
                                                                     string typeOfTest, string moistureOfTest,
                                                                     string categoryOfSoil, string textureOfSoil)

        {
            DateTime _testDate = Functions.Date(testDate);
            Enum.TryParse(depthOfSample, out SampleDepths _depthOfSample);
            Enum.TryParse(typeOfTest, out TestType _typeOfTest);
            Enum.TryParse(moistureOfTest, out  TestMoisture _moistureOfTest);
            Enum.TryParse(categoryOfSoil, out SoilCategoris _categoryOfSoil);
            Enum.TryParse(textureOfSoil, out SoilTextures _textureOfSoil);
            SoilTestConfig test = new SoilTestConfig(_testDate, testValue, _depthOfSample,
                                                       _typeOfTest, _moistureOfTest,
                                                       _categoryOfSoil, _textureOfSoil);
            object[,] st = new object[1, 2];
            st[0, 0] = test.Result.Keys.First();
            st[0, 1] = test.Result.Values.First();
            return st;
        }

        /// <summary>
        /// Function that takes input data in 2D array format and calculates a N balance for a 3 crops rotation and returns N balance variables in 2D array format
        /// </summary>
        /// <param name="met">2D Array with dates in first column and daily meterological data over the duration of the rotation in the second column</param>
        /// <param name="config">2D array with parameter names and values for crop field configuration parameters</param>
        /// <param name="testResults">2D array with dates in teh first column and soil N test results in the second</param>
        /// <returns>Dictionary with parameter names as keys and parameter values as values</returns>
        [ExcelFunction(Description = "Returns full N balance results")]
        public static object[,] GetDailyNBalance(object[,] met, object[,] config, object[,] testResults, object[,] nApplied)
        {
            List<string> configErrors = Functions.ValidateConfig(config);

            for (var r = 0; r < met.GetLength(0); r++)
            {
                for (var c = 0; c < met.GetLength(1); c++)
                {
                    if (met[r, c].GetType() == typeof(ExcelDna.Integration.ExcelEmpty) ||
                        met[r, c].GetType() == typeof(ExcelDna.Integration.ExcelError))
                        met[r, c] = null;
                }
            }


            if (configErrors.Count == 0)
            {
                Dictionary<DateTime, double> _tt = Functions.dictMaker(met, "MeanT");
                Dictionary<DateTime, double> _rain = Functions.dictMaker(met, "Rain");
                Dictionary<DateTime, double> _pet = Functions.dictMaker(met, "MeanPET");
                Dictionary<DateTime, double> _testResults = Functions.dictMaker(testResults, "Value");
                Dictionary<DateTime, double> _nApplied = Functions.dictMaker(nApplied, "Amount");
                var _config = new Config(Functions.dictMaker(config));

                return Simulation.Simulation.SimulateField(_tt, _rain, _pet, _testResults, _nApplied, _config, Constants.InitialN);
            }
            else
            {
                object[,] listOfComplaints = new object[configErrors.Count, 3];
                int c = 0;
                foreach (string e in configErrors)
                {
                    listOfComplaints[c, 2] = e;
                    c++;
                }
                return listOfComplaints;
            }
        }

        [ExcelFunction(Description = "Gets crop coefficient table")]
        public static object[,] GetCropCoefficients()
        {
            //Trace.WriteLine("exxcelentinterface");
            return Functions.packDataFrame(Crop.LoadCropCoefficients());
        }

        
    }
}
