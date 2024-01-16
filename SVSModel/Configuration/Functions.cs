using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Analysis;

namespace SVSModel.Configuration
{
    public class Functions
    {
        /// <summary>
        /// calculates the difference between daily values in an array
        /// </summary>
        /// <param name="integral">array of values to be differentiated to give delta</param>
        /// <returns>An array of deltas</returns>
        public static double[] calcDelta(double[] integral)
        {
            double prior = integral[0];
            double[] delta = new double[integral.Length];
            delta[0] = 0;
            for (int i = 1; i < integral.Length; i++)
            {
                delta[i] = integral[i] - prior;
                prior = integral[i];
            }
            return delta;
        }

        /// <summary>
        /// Function to convert a 2D array with a row of keys and a row of values into a dictionary
        /// </summary>
        /// <param name="arr">2D arry to be converted</param>
        /// <returns>dictionary converted from arr</returns>
        public static Dictionary<string, object> dictMaker(object[,] arr)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            int Nrows = arr.GetLength(0);
            for (int r = 0; r < Nrows; r++)
            {
                dict.Add(arr[r, 0].ToString(), arr[r, 1]);
            }
            return dict;
        }

        /// <summary>
        /// Function to extract a row from a 2D array into a date indexed dictionary assuming date is in the first column in the array is dates
        /// </summary>
        /// <param name="arr">2D arry to be converted</param>
        /// <param name="colName">The header name of the column to extract</param>
        /// <returns>dictionary converted from arr</returns>
        public static Dictionary<DateTime, double> dictMaker(object[,] arr, string colName)
        {
            Dictionary<DateTime, double> dict = new Dictionary<DateTime, double>();
            int Nrows = arr.GetLength(0);
            int Ncols = arr.GetLength(1);
            for (int c = 0; c < Ncols; c++)
            {
                if (arr[0, c].ToString() == colName)
                {
                    for (int r = 1; r < Nrows; r++)
                    {
                        if ((arr[r, 0] == null) || (arr[r, c] == null))
                        { }
                        else if ((arr[r, 0].ToString() == "") || (arr[r, c].ToString() == ""))
                        { }
                        else
                        {
                            dict.Add(Date(arr[r, 0]), Num(arr[r, c]));
                        }
                    }
                }
            }
            return dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Function to convert a 2D array with a row of date keys and a row of values into a dictionary
        /// </summary>
        /// <param name="date">An array of DateTimes</param>
        /// <param name="values">An array of doubles</param>
        /// <returns>dictionary converted from arr</returns>
        public static Dictionary<DateTime, double> dictMaker(DateTime[] dates, double[] values)
        {
            Dictionary<DateTime, double> dict = new Dictionary<DateTime, double>();
            for (int r = 0; r < dates.Length; r++)
            {
                dict.Add(dates[r], values[r]);
            }
            return dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Function to extract a row from a 2D array into a date indexed dictionary
        /// </summary>
        /// <param name="arr">2D arry to be converted</param>
        /// <param name="colName">The header name of the column to extract</param>
        /// <returns>dictionary converted from arr</returns>
        public static double GetFinal(object[,] arr, string colName)
        {
            double ret = new double();
            int Nrows = arr.GetLength(0);
            int Ncols = arr.GetLength(1);
            for (int c = 0; c < Ncols; c++)
            {
                if (arr[0, c].ToString() == colName)
                {
                    ret = Double.Parse(arr[Nrows - 1, c].ToString());
                }
            }
            return ret;
        }

        /// <summary>
        /// Function that packs an array of variables into a specified column in a 2D array
        /// </summary>
        /// <param name="colInd">index position of the column</param>
        /// <param name="column">array to be packed into column</param>
        /// <param name="df">the 2D array that the column is to be packed into</param>
        public static void packRows(int colInd, DateTime[] column, ref object[,] df)
        {
            for (int currentRow = 0; currentRow < column.Length; currentRow++)
            {
                df[currentRow + 1, colInd] = column[currentRow];
            }
        }

        /// <summary>
        /// Function that packs an array of variables into a specified column in a 2D array
        /// </summary>
        /// <param name="colInd">index position of the column</param>
        /// <param name="column">array to be packed into column</param>
        /// <param name="df">the 2D array that the column is to be packed into</param>
        public static void packRows(int colInd, Dictionary<DateTime, double> column, ref object[,] df)
        {
            List<DateTime> dates = new List<DateTime>(column.Keys);
            int currentRow = 0;
            foreach (DateTime d in dates)
            {
                df[currentRow + 1, colInd] = column[d];
                currentRow += 1;
            }
        }

        /// <summary>
        /// Function that packs an array of variables into a specified column in a 2D array
        /// </summary>
        /// <param name="colInd">index position of the column</param>
        /// <param name="column">array to be packed into column</param>
        /// <param name="df">the 2D array that the column is to be packed into</param>
        public static object[,] packDataFrame(DataFrame DF)
        {
            int rows = (int)DF.Rows.Count + 2;
            int cols = (int)DF.Columns.Count;
            object[,] df = new object[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (r == 0)
                    {
                        df[r, c] = DF.Columns[c].Name;
                    }
                    else if (r == 1)
                    {
                        df[r, c] = c;
                    }
                    else
                    {
                        df[r, c] = DF[r - 2, c];
                    }
                }
            }
            return df;
        }

        /// <summary>
        /// Takes an array of daily scaller values (0-1) and multiplies them by the final value to give Daily State Variable values 
        /// </summary>
        /// <param name="scaller">2D array of daily values for 0-1 scaller</param>
        /// <param name="final">The Daily State Variable value on the last day of the simulation</param>
        /// <param name="correction">A factor to apply Stage of harvest correction</param>
        /// <returns>An array of Daily State Variables for the model</returns>
        public static Dictionary<DateTime, double> scaledValues(Dictionary<DateTime, double> scaller, double final, double correction)
        {
            Dictionary<DateTime, double> sv = new Dictionary<DateTime, double>();
            foreach (DateTime d in scaller.Keys)
                sv.Add(d, scaller[d] * final * correction);
            return sv;
        }

        /// <summary>
        /// Creates an array of dates for the duration of the simulation
        /// </summary>
        /// <param name="start">Date to start series</param>
        /// <param name="end">Date to end series</param>
        /// <returns>a continious array of dates between the start and end specified</returns>
        public static DateTime[] DateSeries(DateTime start, DateTime end)
        {
            List<DateTime> ret = new List<DateTime>();
            for (DateTime d = start; d <= end; d = d.AddDays(1))
                ret.Add(d);
            return ret.ToArray();
        }

        /// <summary>
        /// Returns an accumulation of Thermal time over the duration of the dialy input values
        /// </summary>
        /// <param name="Tt">array of daily average temperatures</param>
        /// <returns>Array of accumulated thermal time</returns>
        public static Dictionary<DateTime, double> AccumulateTt(DateTime[] dates, Dictionary<DateTime, double> Tt)
        {
            Dictionary<DateTime, double> tt = new Dictionary<DateTime, double>();
            foreach (DateTime d in dates)
            {
                if (d == dates[0]) // if today is the first day the above will throw
                {
                    tt.Add(d, Tt[d]);
                }
                else
                {
                    tt.Add(d, tt[d.AddDays(-1)] + Tt[d]);
                }
            }
            return tt;
        }

        /// <summary>
        /// Function to convert a 2D array with a row of keys and a row of values into a dictionary
        /// </summary>
        /// <param name="arr">2D arry to be converted</param>
        /// <returns>dictionary converted from arr</returns>
        public static List<string> ValidateConfig(object[,] arr)
        {
            List<string> errorlist = new List<string>();
            int Nrows = arr.GetLength(0);
            for (int r = 0; r < Nrows; r++)
            {
                if (arr[r, 1].ToString() == "")
                    errorlist.Add(arr[r, 0].ToString());
                if (arr[r, 1] == null)
                    errorlist.Add(arr[r, 0].ToString());
            }
            return errorlist;
        }

        public static DateTime Date(object configDate)
        {
            if (configDate.GetType() == typeof(double))
            {
                return DateTime.FromOADate((double)configDate);
            }
            else
            {
                return (DateTime)configDate;
            }
        }
        public static double Num(object configDouble)
        {

            return Double.Parse(configDouble.ToString());
        }

        public static Dictionary<DateTime, double> ApplyRainfallFactor(Dictionary<DateTime, double> meanRain, Config config)
        {
            Dictionary<DateTime, double> adjustedmeanRain =  new Dictionary<DateTime, double>();

            foreach (DateTime d in meanRain.Keys) 
            { 
                double todayRain = meanRain[d];
                if ((d>= config.Current.EstablishDate) && (d<= config.Current.HarvestDate))
                    todayRain *= config.Field.InCropRainFactor;
                adjustedmeanRain.Add(d, todayRain);
            }
            return adjustedmeanRain;
        }
    }
}
