﻿using RaceTracker.LogicHelpers;
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
                DataFields = Data.RaceData,
                DataHeaders = this.PopulateDataHeaders()
            };

            if(this.Model.DataHeaders.Count>0)
            {
                this.Model.XAxisSelection = this.Model.DataHeaders[0];
                this.Model.YAxisSelection = this.Model.DataHeaders[0];
            }
        }

        private List<string> PopulateDataHeaders()
        {
            var headers = new List<string>();
            foreach(var header in Data.RaceData.Keys)
            {
                headers.Add(header);
            }

            return headers;
        }

        public PlottingModel Model { get; }
    }
}
