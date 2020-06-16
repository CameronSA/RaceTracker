using RaceTracker.LogicHelpers;
using RaceTracker.ViewModels;
using RaceTracker.Views;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shapes;

namespace RaceTracker.Commands
{
    public class PlottingCommand : ICommand
    {
        private PlottingViewModel ViewModel { get; }

        public PlottingCommand(PlottingViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            bool canExecute = false;
            switch(parameter.ToString())
            {
                case "btPlotGeneric":
                    canExecute = true;
                    break;
            }

            return canExecute;
        }

        public void Execute(object parameter)
        {
            switch (parameter.ToString())
            {
                case "btPlotGeneric": this.PlotGeneric();
                    break;
            }
        }

        private void PlotGeneric()
        {
            this.ViewModel.View.GenericPlot.Reset();
            string xAxis = this.ViewModel.Model.XAxisSelection;
            string yAxis = this.ViewModel.Model.YAxisSelection;
            var xData = Data.ProcessedRaceData[xAxis];
            var yData = Data.ProcessedRaceData[yAxis];
            var x = xData.Data;
            var y = yData.Data;

            bool xIsNumeric = xData.Type == typeof(int) || xData.Type == typeof(double);
            bool yIsNumeric = yData.Type == typeof(int) || yData.Type == typeof(double);

            if (xIsNumeric && yIsNumeric)
            {
                var xNumeric = Data.SubArray(Data.ConvertToDoubles(x), 0, 2000);
                var yNumeric = Data.SubArray(Data.ConvertToDoubles(y), 0, 2000);
                this.ViewModel.View.GenericPlot.plt.PlotScatter(xNumeric, yNumeric);
            }
            else if (yIsNumeric)
            {
                var xString = new List<string>();
                foreach (var item in x)
                {
                    xString.Add(item.ToString());
                }

                var yNumeric = Data.ConvertToDoubles(y);
                var barCount = DataGen.Consecutive(x.Length);
                this.ViewModel.View.GenericPlot.plt.PlotBar(barCount, yNumeric);
                this.ViewModel.View.GenericPlot.plt.XTicks(barCount, xString.ToArray());
            }
            else
            {
                MessageBox.Show("Cannot plot '" + xAxis + "' against '" + yAxis + "'", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.ViewModel.View.GenericPlot.Render();
        }
    }
}
