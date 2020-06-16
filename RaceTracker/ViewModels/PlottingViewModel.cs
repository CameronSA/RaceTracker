using RaceTracker.Commands;
using RaceTracker.LogicHelpers;
using RaceTracker.Models;
using RaceTracker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RaceTracker.ViewModels
{
    public class PlottingViewModel
    {
        public PlottingViewModel(PlottingView view)
        {
            this.Model = new PlottingModel()
            {
                DataFields = Data.ProcessedRaceData,
                DataHeaders = this.PopulateDataHeaders()
            };

            if (this.Model.DataHeaders.Count > 0)
            {
                this.Model.XAxisSelection = this.Model.DataHeaders[0];
                this.Model.YAxisSelection = this.Model.DataHeaders[0];
            }

            this.Command = new PlottingCommand(this);
            this.View = view;
        }

        private List<string> PopulateDataHeaders()
        {
            var headers = new List<string>();
            foreach(var header in Data.ProcessedRaceData.Keys)
            {
                headers.Add(header);
            }

            return headers;
        }

        public PlottingModel Model { get; }

        public PlottingView View { get; }

        public ICommand Command { get; }
    }
}
