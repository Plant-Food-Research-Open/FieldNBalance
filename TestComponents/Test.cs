// FieldNBalance is a program that estimates the N balance and provides N fertilizer recommendations for cultivated crops.
// Author: Hamish Brown.
// Copyright (c) 2024 The New Zealand Institute for Plant and Food Research Limited

using System.Net;
using System.IO;
using System.Reflection;
using Microsoft.Data.Analysis;
using SVSModel;
using SVSModel.Models;
using SVSModel.Simulation;
using System.Diagnostics;
using System.CodeDom.Compiler;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Data;
using System.Globalization;
using SVSModel.Configuration;

namespace TestModel
{
    public class Test
    {
        public static void RunAllTests()
        {
            string path = "";
            string root = "";
            if (Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") == null)
            {
                string fullroot = AppDomain.CurrentDomain.BaseDirectory;
                List<string> rootFrags = fullroot.Split('\\').ToList();
                
                foreach (string d in rootFrags)
                {
                    if (d == "FieldNBalance")
                        break;
                    else
                        root += d + "\\";
                }
                root = Path.Join(root, "FieldNBalance");
                path = Path.Join(root, "TestComponents", "TestSets");
            }
            else
            {
                root = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
                path = Path.Join(root, "TestComponents", "TestSets");
            }


            List<string> sets = new List<string>  { "WS1", "WS2", "Residues", "Location", "Moisture", "Losses" };

            //Delete graphs from previous test run
            string graphFolder = Path.Join(Directory.GetCurrentDirectory(), "TestGraphs", "Outputs");
            if (!Directory.Exists(graphFolder))
            {
                System.IO.Directory.CreateDirectory(graphFolder);
            }

            string[] graphPaths = Directory.GetFiles(graphFolder);
            foreach (string graphPath in graphPaths)
                File.Delete(graphPath);

            foreach (string s in sets)
            {
                //Make config file in format that .NET DataTable is able to import
                runPythonScript(root, Path.Join("TestGraphs", "MakeConfigs", $"{s}.py"));
                //Run each test
                runTestSet(path, s);
                //Make graphs associated with each test
                runPythonScript(root, Path.Join("TestGraphs", "MakeGraphs", $"{s}.py"));
            }
        }

        public static void runTestSet(string path, string set)
        {
            string graphFolder = Path.Join(path, set, "Outputs");
            if (!Directory.Exists(graphFolder))
            {
                System.IO.Directory.CreateDirectory(graphFolder);
            }

            string[] filePaths = Directory.GetFiles(graphFolder);
            foreach (string filePath in filePaths)
                File.Delete(filePath);

            var assembly = Assembly.GetExecutingAssembly();
            string testConfig = "TestComponents.TestSets." + set + ".FieldConfigs.csv";
            Stream configcsv = assembly.GetManifestResourceStream(testConfig);
            DataFrame allTests = DataFrame.LoadCsv(configcsv);

            string fertData = "TestComponents.TestSets." + set + ".FertiliserData.csv";
            Stream fertcsv = assembly.GetManifestResourceStream(fertData);
            DataFrame allFert = new DataFrame();
            if (fertcsv != null)
            {
                allFert = DataFrame.LoadCsv(fertcsv);
            }

            List<string> Tests = new List<string>();

            foreach (DataFrameRow row in allTests.Rows)
            {
                Tests.Add(row[0].ToString());
            }
            //Tests.Add("LincolnRot2_N4_Irr2_PakChoi");

            foreach (string test in Tests)
            {
                if (test[0].ToString() != ">")
                {
                    int testRow = getTestRow(test, allTests);

                    (SVSModel.Configuration.Config, double) _testData = SetConfigFromDataFrame(test, allTests);
                    SVSModel.Configuration.Config _config = _testData.Item1;
                    double initialN = _testData.Item2;

                    Dictionary<System.DateTime, double> testResults = new Dictionary<System.DateTime, double>();
                    Dictionary<System.DateTime, double> nApplied = new Dictionary<System.DateTime, double>();//fertDict(test, allFert);

                    string weatherStation = allTests["WeatherStation"][testRow].ToString();

                    bool actualWeather = weatherStation.Contains("Actual");

                    MetDataDictionaries metData = ModelInterface.BuildMetDataDictionaries(_config.Prior.EstablishDate, _config.Following.HarvestDate.AddDays(1), weatherStation, actualWeather);

                    object[,] output = Simulation.SimulateField(metData.MeanT, metData.Rain, metData.MeanPET, testResults, nApplied, _config, initialN, false);

                    DataFrameColumn[] columns = new DataFrameColumn[14];
                    List<string> OutPutHeaders = new List<string>();
                    for (int i = 0; i < output.GetLength(1); i += 1)
                    {
                        OutPutHeaders.Add(output[0, i].ToString());
                        if (i == 0)
                        {
                            columns[i] = new PrimitiveDataFrameColumn<System.DateTime>(output[0, i].ToString());
                        }
                        else
                        {
                            columns[i] = new PrimitiveDataFrameColumn<double>(output[0, i].ToString());
                        }
                    }

                    var newDataframe = new DataFrame(columns);

                    for (int r = 1; r < output.GetLength(0); r += 1)
                    {
                        List<KeyValuePair<string, object>> nextRow = new List<KeyValuePair<string, object>>();
                        for (int c = 0; c < output.GetLength(1); c += 1)
                        {
                            nextRow.Add(new KeyValuePair<string, object>(OutPutHeaders[c], output[r, c]));
                        }
                        newDataframe.Append(nextRow, true);
                    }

                    string folderName = "OutputFiles";

                    if (!Directory.Exists(folderName))
                    {
                        System.IO.Directory.CreateDirectory("OutputFiles");
                    }

                    DataFrame.SaveCsv(
                        newDataframe, Path.Join(path, set, "Outputs", $"{test}.csv")
                    );
                }
            }
        }

        private static void runPythonScript(string path, string pyProg)
        {
            string progToRun = Path.Join(path,pyProg);

            Process proc = new Process();
            proc.StartInfo.FileName = "python";
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.Arguments = progToRun;
            proc.Start();
            proc.WaitForExit();
        }

        public static (SVSModel.Configuration.Config, double) SetConfigFromDataFrame(string test, DataFrame allTests)
        {
            int testRow = getTestRow(test, allTests);

            List<string> coeffs = new List<string> {"WeatherStation",
                                                    "SoilCategory",
                                                    "Texture",
                                                    "Rocks",
                                                    "SampleDepth",
                                                    "PMN",
                                                    "Splits",
                                                    "PrePlantRain",
                                                    "InCropRain",
                                                    "Irrigation",
                                                    "PriorCropNameFull",
                                                    "PriorFieldYield",
                                                    "PriorFieldLoss",
                                                    "PriorDressingLoss",
                                                    "PriorMoistureContent",
                                                    "PriorEstablishDate",
                                                    "PriorEstablishStage",
                                                    "PriorHarvestDate",
                                                    "PriorHarvestStage",
                                                    "PriorResidueRemoval",
                                                    "PriorResidueIncorporation",
                                                    "CurrentCropNameFull",
                                                    "CurrentFieldYield",
                                                    "CurrentFieldLoss",
                                                    "CurrentDressingLoss",
                                                    "CurrentMoistureContent",
                                                    "CurrentEstablishDate",
                                                    "CurrentEstablishStage",
                                                    "CurrentHarvestDate",
                                                    "CurrentHarvestStage",
                                                    "CurrentResidueRemoval",
                                                    "CurrentResidueIncorporation",
                                                    "FollowingCropNameFull",
                                                    "FollowingFieldYield",
                                                    "FollowingFieldLoss",
                                                    "FollowingDressingLoss",
                                                    "FollowingMoistureContent",
                                                    "FollowingEstablishDate",
                                                    "FollowingEstablishStage",
                                                    "FollowingHarvestDate",
                                                    "FollowingHarvestStage",
                                                    "FollowingResidueRemoval",
                                                    "FollowingResidueIncorporation"
            };

            Dictionary<string, object> testConfigDict = new Dictionary<string, object>();
            foreach (string c in coeffs)
            {
                 testConfigDict.Add(c, allTests[c][testRow]);
            }

            testConfigDict.Add("PriorYieldUnits", "t/ha");
            testConfigDict.Add("CurrentYieldUnits", "t/ha");
            testConfigDict.Add("FollowingYieldUnits", "t/ha");
            testConfigDict.Add("PriorPopulation", "");
            testConfigDict.Add("CurrentPopulation", "");
            testConfigDict.Add("FollowingPopulation", "");


            //List<string> datesNames = new List<string>() { "PriorEstablishDate", "PriorHarvestDate", "CurrentEstablishDate", "CurrentHarvestDate", "FollowingEstablishDate", "FollowingHarvestDate" };

            SVSModel.Configuration.Config ret = new SVSModel.Configuration.Config(testConfigDict);
            
            double initialN = Constants.InitialN;
            try
            {
                initialN = Functions.Num(allTests["InitialN"][testRow]);
            }
            catch
            {                
            }

            return (ret, initialN);
        }

        private static int getTestRow(string test, DataFrame allTests)
        {
            int testRow = 0;
            bool testNotFound = true;
            while (testNotFound)
            {
                if (allTests[testRow, 0].ToString() == test)
                    testNotFound = false;
                else
                    testRow += 1;
            }
            return testRow;
        }

       /* private static Dictionary<System.DateTime, double> fertDict(string test, DataFrame allFert)
        {
            Dictionary<System.DateTime, double> fert = new Dictionary<System.DateTime, double>();
            foreach (DataFrameRow row in allFert.Rows)
            {
                if (row[0].ToString() == test) //if this date row holds data for current site
                {

                    //DateTime date = DateTime.ParseExact(row[1].ToString(), "d/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    DateTime date = (DateTime)row[1];
                    
                    DateTime last = new DateTime();
                    if (fert.Keys.Count > 0)
                    {
                        last = fert.Keys.Last();
                    }
                    if (date == last) //If alread fertiliser added for that date add it to existing total
                    {
                        fert[last] += Double.Parse(row[2].ToString());
                    }
                    else //add it to a new date
                    {
                        fert.Add(date, Double.Parse(row[2].ToString()));
                    }
                }
            }
            return fert;
        }*/

    }
}

