using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RaceTracker.LogicHelpers
{
    public class RaceData
    {
        public RaceData(Type type)
        {
            this.Type = type;
            this.Data = new object[0];
        }

        public Type Type { get; }

        public object[] Data { get; set; }        
    }
}
