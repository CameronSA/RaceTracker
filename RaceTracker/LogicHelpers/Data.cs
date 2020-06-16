using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaceTracker.LogicHelpers
{
    public static class Data
    {
        public static Dictionary<string, RaceData> ProcessedRaceData {get;set;}

        private static Dictionary<string, string[]> RaceData { get; set; }

        private static Dictionary<string, List<string>> FileData { get; set; }

        private static string[] Headers { get; set; }

        public static void ParseFile(string filePath)
        {
            Data.ProcessedRaceData = new Dictionary<string, RaceData>();
            Data.RaceData = new Dictionary<string, string[]>();
            Data.FileData = new Dictionary<string, List<string>>();
            Data.Headers = new string[0];
            if(File.Exists(filePath))
            {
                using (var streamReader = new StreamReader(filePath))
                {
                    bool isHeader = true;
                    do
                    {
                        Data.ProcessLine(streamReader.ReadLine(), isHeader, filePath);
                        isHeader = false;
                    } while (!streamReader.EndOfStream);
                }

                foreach(var column in Data.FileData)
                {
                    if (Data.RaceData.ContainsKey(column.Key))
                    {
                        Data.RaceData[column.Key] = column.Value.ToArray();
                    }
                    else
                    {
                        Data.RaceData.Add(column.Key, column.Value.ToArray());
                    }
                }

                Data.ProcessRaceData();
            }
            else
            {
                MessageBox.Show("WARNING: Could not find data file at path '" + filePath + "'", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        public static double[] ConvertToDoubles(IEnumerable<object> list)
        {
            var items = new List<double>();

            try
            {
                foreach (var item in list)
                {
                    items.Add(double.Parse(item.ToString()));
                }
            }
            catch
            {
                MessageBox.Show("ERROR: Data is not numeric", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return items.ToArray();
        }

        public static T[] SubArray<T>(T[] array, int startIndex, int length)
        {
            var subArray = new T[length];
            for (int i = startIndex; i < length; i++)
            {
                subArray[i] = array[i];
            }

            return subArray;
        }

        private static void ProcessRaceData()
        {
            var processedRaceData = new Dictionary<string, RaceData>();
            foreach (var column in Data.RaceData)
            {
                switch (column.Key.ToLower().Trim())
                {
                    case "age":
                        Data.AddToDictionary(processedRaceData, column.Key.Trim(), Data.ProcessAgeColumn(column));
                        break;
                    case "prize":
                        Data.AddToDictionary(processedRaceData, column.Key.Trim(), Data.ProcessPrizeColumn(column));
                        break;
                    case "distance":
                        Data.AddToDictionary(processedRaceData, column.Key.Trim(), Data.ProcessDistanceColumn(column));
                        break;
                    case "date":
                        Data.AddToDictionary(processedRaceData, column.Key.Trim(), Data.ProcessDateColumn(column));
                        break;
                    case "time":
                        Data.AddToDictionary(processedRaceData, column.Key.Trim(), Data.ProcessTimeColumn(column));
                        break;
                    case "position":
                        Data.AddToDictionary(processedRaceData, column.Key.Trim(), Data.ProcessPositionColumn(column));
                        break;
                    case "draw":
                        Data.AddToDictionary(processedRaceData, column.Key.Trim(), Data.ProcessDrawColumn(column));
                        break;
                    case "horse age":
                        Data.AddToDictionary(processedRaceData, column.Key.Trim(), Data.ProcessHorseAgeColumn(column));
                        break;
                    case "isp":
                        Data.AddToDictionary(processedRaceData, column.Key.Trim(), Data.ProcessISPColumn(column));
                        break;
                    default:
                        Data.AddToDictionary(processedRaceData, column.Key.Trim(), Data.ProcessDefaultColumn(column));
                        break;
                }
            }

            var raceDataCounts = new HashSet<int>();
            foreach (var raceData in processedRaceData)
            {
                raceDataCounts.Add(raceData.Value.Data.Length);
            }

            if (raceDataCounts.Count > 1)
            {
                MessageBox.Show("ERROR: Data invalid, columns possess different lengths!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Data.ProcessedRaceData = processedRaceData;
        }

        private static void AddToDictionary<A, B>(Dictionary<A, B> dictionary, A key, B value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        private static RaceData ProcessAgeColumn(KeyValuePair<string,string[]> data)
        {
            var raceData = new RaceData(typeof(int));
            var ageData = new List<object>();
            foreach(var item in data.Value)
            {
                int age = -1;
                foreach(var character in item)
                {
                    if (int.TryParse(character.ToString(), out age))
                    {
                        break;
                    }
                }

                ageData.Add(age);
            }

            raceData.Data = ageData.ToArray();
            return raceData;
        }

        private static RaceData ProcessPrizeColumn(KeyValuePair<string, string[]> data)
        {
            var raceData = new RaceData(typeof(Tuple<string, double>));
            var currencyData = new List<object>();
            foreach (var item in data.Value)
            {
                Tuple<string, double> currencyDatum;
                try
                {
                    string currencyType = string.Empty;
                    string currencyValue = string.Empty;
                    if (!int.TryParse(item[0].ToString(), out _))
                    {
                        currencyType += item[0];
                        currencyValue = item.Substring(1);
                    }

                    if (!int.TryParse(item[item.Length - 1].ToString(), out _))
                    {
                        currencyType += item[item.Length - 1];
                        currencyValue = item.Substring(0, item.Length - 1);
                    }

                    currencyDatum = new Tuple<string, double>(
                        string.IsNullOrEmpty(currencyType.Trim()) ? "NULL" : currencyType,
                        string.IsNullOrEmpty(currencyValue.Trim()) ? 0 : double.Parse(currencyValue));
                }
                catch
                {
                    currencyDatum = new Tuple<string, double>("NULL", 0);
                }

                currencyData.Add(currencyDatum);
            }

            raceData.Data = currencyData.ToArray();
            return raceData;
        }

        private static RaceData ProcessDistanceColumn(KeyValuePair<string, string[]> data)
        {
            var raceData = new RaceData(typeof(double));
            var distanceData = new List<object>();
            foreach (var item in data.Value)
            {
                double yardComponent = 0;
                double furlongComponent = 0;
                double mileComponent = 0;
                string number = string.Empty;

                try
                {
                    foreach (var character in item)
                    {
                        if (short.TryParse(character.ToString(), out _))
                        {
                            number += character.ToString();
                        }
                        else if (!string.IsNullOrEmpty(number))
                        {
                            switch (character.ToString().ToLower())
                            {
                                case "m": mileComponent = double.Parse(number); break;
                                case "f": furlongComponent = double.Parse(number); break;
                                case "y": yardComponent = double.Parse(number); break;
                            }

                            number = string.Empty;
                        }
                    }

                    distanceData.Add(UnitConversions.YardToKilometers(yardComponent) + UnitConversions.FurlongToKilometers(furlongComponent) + UnitConversions.MileToKilometers(mileComponent));
                }
                catch
                {
                    distanceData.Add(-1);
                }
            }

            raceData.Data = distanceData.ToArray();
            return raceData;
        }

        private static RaceData ProcessDateColumn(KeyValuePair<string,string[]> data)
        {
            var raceData = new RaceData(typeof(DateTime));
            var dateData = new List<object>();
            foreach(var item in data.Value)
            {
                try
                {
                    var date = DateTime.ParseExact(item, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    dateData.Add(date);
                }
                catch
                {
                    MessageBox.Show("ERROR: Failed to parse date: '" + item.ToString() + "'. Data will not be valid!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    return raceData;
                }
            }

            raceData.Data = dateData.ToArray();
            return raceData;
        }

        private static RaceData ProcessTimeColumn(KeyValuePair<string,string[]> data)
        {
            var raceData = new RaceData(typeof(DateTime));
            var timeData = new List<object>();
            foreach (var item in data.Value)
            {
                try
                {
                    var date = DateTime.ParseExact(item, "HHmm", CultureInfo.InvariantCulture);
                    timeData.Add(date);
                }
                catch
                {
                    MessageBox.Show("ERROR: Failed to parse time: '" + item.ToString() + "'. Data will not be valid!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    return raceData;
                }
            }

            raceData.Data = timeData.ToArray();
            return raceData;
        }

        private static RaceData ProcessPositionColumn(KeyValuePair<string,string[]> data)
        {
            var raceData = new RaceData(typeof(int));
            var positionData = new List<object>();
            foreach (var item in data.Value)
            {
                if (int.TryParse(item, out int pos))
                {
                    positionData.Add(pos);
                }
                else
                {
                    positionData.Add(-1);
                }
            }

            raceData.Data = positionData.ToArray();
            return raceData;
        }

        private static RaceData ProcessDrawColumn(KeyValuePair<string,string[]> data)
        {
            var raceData = new RaceData(typeof(int));
            var drawData = new List<object>();
            foreach (var item in data.Value)
            {
                if (int.TryParse(item, out int draw))
                {
                    drawData.Add(draw);
                }
                else
                {
                    drawData.Add(-1);
                }
            }

            raceData.Data = drawData.ToArray();
            return raceData;
        }

        private static RaceData ProcessHorseAgeColumn(KeyValuePair<string,string[]> data)
        {
            var raceData = new RaceData(typeof(int));
            var horseAgeData = new List<object>();
            foreach (var item in data.Value)
            {
                if (int.TryParse(item, out int age))
                {
                    horseAgeData.Add(age);
                }
                else
                {
                    horseAgeData.Add(-1);
                }
            }

            raceData.Data = horseAgeData.ToArray();
            return raceData;
        }
        private static RaceData ProcessISPColumn(KeyValuePair<string,string[]> data)
        {
            var raceData = new RaceData(typeof(double));
            var ispData = new List<object>();
            foreach (var item in data.Value)
            {
                if (double.TryParse(item, out double isp))
                {
                    ispData.Add(isp);
                }
                else
                {
                    ispData.Add(-1);
                }
            }

            raceData.Data = ispData.ToArray(); 
            return raceData;
        }

        private static RaceData ProcessDefaultColumn(KeyValuePair<string,string[]> data)
        {

            var raceData = new RaceData(typeof(string));
            var itemData = new List<object>();
            foreach (var item in data.Value)
            {
                itemData.Add(string.IsNullOrEmpty(item) ? "NULL" : item);
            }

            raceData.Data = itemData.ToArray();
            return raceData;
        }

        private static void ProcessLine(string line, bool isHeader, string filePath)
        {
            string[] fields = line.Split(AppSettings.Delimiter);
            if (isHeader)
            {
                if (line.Trim() != AppSettings.DataHeaders.Trim())
                {
                    MessageBox.Show("WARNING: Data headers in file '" + filePath + "' unexpected", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                Data.Headers = fields;
            }
            else if (Data.Headers.Length > 0)
            {
                for (int i = 0; i < Data.Headers.Length; i++)
                {
                    if (Data.FileData.ContainsKey(Data.Headers[i]))
                    {
                        Data.FileData[Data.Headers[i]].Add(fields[i]);
                    }
                    else
                    {
                        Data.FileData.Add(Data.Headers[i], new List<string> { fields[i] });
                    }
                }
            }
            else
            {
                MessageBox.Show("WARNING: Failed to process data file at path '" + filePath + "'. File format invalid", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
