using System.Reflection;
using Microsoft.Data.Analysis;
using SVSModel;
using SVSModel.Models;
using System.Diagnostics;
using System.CodeDom.Compiler;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Data;

namespace TestModel
{
    public class Test
    {
        public static void RunAllTests()
        {
            string path = Directory.GetCurrentDirectory().Split("\\SVSModelBuildDeploy\\")[0] + "\\SVSModelBuildDeploy\\TestComponents\\TestSets\\";
            List<string> sets = new List<string> { "WS2", "Residues", "Location", "Moisture" };

            //Delete graphs from previous test run
            string graphFolder = Directory.GetCurrentDirectory().Split("\\SVSModelBuildDeploy\\")[0] + "\\SVSModelBuildDeploy\\TestGraphs\\Outputs";
            string[] graphPaths = Directory.GetFiles(graphFolder);
            foreach (string graphPath in graphPaths)
                File.Delete(graphPath);

            foreach (string s in sets)
            {
                //Make config file in format that .NET DataTable is able to import
                runPythonScript(path, @"TestGraphs\MakeConfigs\"+s+".py");
                //Run each test
                runTestSet(path, s);
                //Make graphs associated with each test
                runPythonScript(path, @"TestGraphs\MakeGraphs\"+ s+".py");
            }
        }

        public static void runTestSet(string path, string set)
        {
            string[] filePaths = Directory.GetFiles(path+"\\"+set+"\\Outputs");
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

            foreach (string test in Tests)
            {
                int testRow = getTestRow(test, allTests);

                SVSModel.Configuration.Config _config = SetConfigFromDataFrame(test, allTests);

                Dictionary<System.DateTime, double> testResults = new Dictionary<System.DateTime, double>();
                Dictionary<System.DateTime, double> nApplied = fertDict(test, allFert);

                string weatherStation = allTests["WeatherStation"][testRow].ToString();

                MetDataDictionaries metData = ModelInterface.BuildMetDataDictionaries(_config.Prior.EstablishDate, _config.Following.HarvestDate.AddDays(1), weatherStation);

                object[,] output = Simulation.SimulateField(metData.MeanT, metData.Rain, metData.MeanPET, testResults, nApplied, _config);

                DataFrameColumn[] columns = new DataFrameColumn[13];
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

                DataFrame.SaveCsv(newDataframe, path + "\\" + set + "\\Outputs\\" + test + ".csv");
            }
        }

        private static void runPythonScript(string path, string pyProg)
        {
            string newPath = Path.GetFullPath(Path.Combine(path, @"..\..\"));
            string progToRun = newPath + pyProg;

            Process proc = new Process();
            proc.StartInfo.FileName = "C:\\Program Files (x86)\\Microsoft Visual Studio\\Shared\\Python39_64\\python.exe";
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.Arguments = progToRun;
            proc.Start();
        }

        public static SVSModel.Configuration.Config SetConfigFromDataFrame(string test, DataFrame allTests)
        {
            int testRow = getTestRow(test, allTests);

            List<string> coeffs = new List<string> { "InitialN",
                                                    "SoilOrder",
                                                    "SampleDepth",
                                                    "BulkDensity",
                                                    "PMNtype",
                                                    "PMN",
                                                    "Trigger",
                                                    "Efficiency",
                                                    "Splits",
                                                    "AWC",
                                                    "PrePlantRain",
                                                    "InCropRain",
                                                    "Irrigation",
                                                    "PriorCropNameFull",
                                                    "PriorSaleableYield",
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
                                                    "CurrentSaleableYield",
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
                                                    "FollowingSaleableYield",
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

            List<string> datesNames = new List<string>(){ "PriorEstablishDate", "PriorHarvestDate", "CurrentEstablishDate", "CurrentHarvestDate", "FollowingEstablishDate", "FollowingHarvestDate" };

            SVSModel.Configuration.Config ret = new SVSModel.Configuration.Config(testConfigDict);

            return ret;
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

        private static Dictionary<System.DateTime, double> fertDict(string test, DataFrame allFert)
        {
            Dictionary<System.DateTime, double> fert = new Dictionary<System.DateTime, double>();
            string site = Regex.Replace(test, "[^0-9]", "");

            foreach (DataFrameRow row in allFert.Rows)
            {
                if (row[0].ToString() == site) //if this date row holds data for current site
                {
                    DateTime date = DateTime.Parse(row[1].ToString());
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
        }

    }
}

