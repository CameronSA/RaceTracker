using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaceTracker.LogicHelpers
{
    public static class UnitConversions
    {
        private static readonly double furlongToMile = 0.125;
        private static readonly double yardToMile = 0.000568182;
        private static readonly double mileToKM = 1.60934;

        public static double FurlongToKilometers(double furlongs)
        {
            double miles = furlongs * furlongToMile;
            return miles * mileToKM;
        }

        public static double YardToKilometers(double yards)
        {
            double miles = yards * yardToMile;
            return miles * mileToKM;
        }

        public static double MileToKilometers(double miles)
        {
            return miles * mileToKM;
        }

        public static int MonthNumber(string monthName)
        {
            switch (monthName.ToLower().Substring(0, 3))
            {
                case "jan": return 1;
                case "feb": return 2;
                case "mar": return 3;
                case "apr": return 4;
                case "may": return 5;
                case "jun": return 6;
                case "jul": return 7;
                case "aug": return 8;
                case "sep": return 9;
                case "oct": return 10;
                case "nov": return 11;
                case "dec": return 12;
                default:
                    MessageBox.Show("ERROR: Could not parse date with month '" + monthName + "'", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new Exception();
            }
        }

        public static string MonthName(int value)
        {
            switch(value)
            {
                case 1: return "January";
                case 2: return "February";
                case 3: return "March";
                case 4: return "April";
                case 5: return "May";
                case 6: return "June";
                case 7: return "July";
                case 8: return "August";
                case 9: return "September";
                case 10: return "October";
                case 11: return "November";
                case 12: return "December";
                default:
                    MessageBox.Show("ERROR: Could not parse date with month number '" + value + "'", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new Exception();
            }
        }
    }
}
