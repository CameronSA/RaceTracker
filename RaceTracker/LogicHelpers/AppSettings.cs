using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaceTracker.LogicHelpers
{
    public static class AppSettings
    {
        public static int MaxNumberRaceCourses
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MaxNumberRaceCourses"];
                if (int.TryParse(value, out int number) && number > 0)
                {
                    return number;
                }

                MessageBox.Show("ERROR: Invalid maximum number of racecourses: " + value + ". Check app.config", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception();
            }
        }
        public static DateTime MinDate
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MinDate"];
                try
                {
                    var date = DateTime.ParseExact(value, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                    return date;
                }
                catch(Exception e)
                {
                    MessageBox.Show("ERROR: Invalid minimum date: " + value + ". Check app.config", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    throw e;
                }
            }
        }
        public static DateTime MaxDate
        {
            get
            {
                string value = ConfigurationManager.AppSettings["MaxDate"];
                try
                {
                    var date = DateTime.ParseExact(value, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                    return date;
                }
                catch (Exception e)
                {
                    MessageBox.Show("ERROR: Invalid maximum date: " + value + ". Check app.config", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    throw e;
                }
            }
        }
        public static bool UseFullDataSet
        {
            get
            {
                string value = ConfigurationManager.AppSettings["UseFullDataSet"];
                if (value.Trim().ToLower() == "true")
                {
                    return true;
                }

                return false;
            }
        }
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
