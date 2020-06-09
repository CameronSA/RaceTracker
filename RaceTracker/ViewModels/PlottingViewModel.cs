using RaceTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTracker.ViewModels
{
    public class PlottingViewModel
    {
        public PlottingViewModel()
        {
            this.Model = new PlottingModel()
            {
                DataFields = new Dictionary<string, List<string>>()
            };
        }

        public PlottingModel Model { get; }
    }
}
