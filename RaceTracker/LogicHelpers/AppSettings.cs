using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTracker.LogicHelpers
{
    public static class AppSettings
    {
        public static string DataHeaders
        { 
            get
            {
                return ConfigurationManager.AppSettings["DataHeaders"];
            }
        }
        public static char Delimiter
        {
            get
            {
                return ',';
            }
        }

        public static string AppName
        { 
            get
            {
                return "Race Tracker";
            }
        }

        public static string DataFileName
        {
            get
            {
                return "OutputProcessedRaceData.txt";
            }
        }

        public static string DataFileDirectory
        {
            get
            {
                string directory = AppDomain.CurrentDomain.BaseDirectory + @"RaceData\";
                if(!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return directory;
            }
        }

        public static string DataFilePath
        {
            get
            {
                return AppSettings.DataFileDirectory + AppSettings.DataFileName;
            }
        }

        public static string DataMiningApp
        {
            get
            {
                return ConfigurationManager.AppSettings["DataMiningApp"];
            }
        }
    }
}
