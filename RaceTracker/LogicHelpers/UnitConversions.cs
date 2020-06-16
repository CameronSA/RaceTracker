using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
