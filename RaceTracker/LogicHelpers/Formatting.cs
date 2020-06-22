using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTracker.LogicHelpers
{
    public static class Formatting
    {
        public static string EditStringLength(string str, int length)
        {
            if (str.Length < length)
            {
                str = "0" + str;
                return EditStringLength(str, length);
            }
            else
            {
                return str;
            }
        }
    }
}
