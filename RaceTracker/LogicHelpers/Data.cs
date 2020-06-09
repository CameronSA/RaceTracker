using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaceTracker.LogicHelpers
{
    public static class Data
    {        
        public static void ParseFile(string filePath)
        {
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
            }
            else
            {
                MessageBox.Show("WARNING: Could not find data file at path '" + filePath + "'", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static void ProcessLine(string line, bool isHeader, string filePath)
        {
            string[] fields = line.Split(AppSettings.Delimiter);
            if (isHeader)
            {
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

        public static Dictionary<string,string[]> RaceData { get; set; }

        private static Dictionary<string, List<string>> FileData { get; set; }

        private static string[] Headers { get; set; }
    }
}
