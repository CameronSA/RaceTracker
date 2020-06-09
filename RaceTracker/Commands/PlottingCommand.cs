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
            string xAxis = this.ViewModel.Model.XAxisSelection;
            string yAxis = this.ViewModel.Model.YAxisSelection;
            //var x = Data.SubArray(Data.GetColumnValues(xAxis).ToArray(), 0, 20);
            //var y = Data.SubArray(Data.GetColumnValues(yAxis).ToArray(), 0, 20);
            var x = Data.GetColumnValues(xAxis).ToArray();
            var y = Data.GetColumnValues(yAxis).ToArray();

            bool xIsNumeric = false;
            bool yIsNumeric = false;

            foreach (var item in x)
            {
                if (!double.TryParse(item, out _))
                {
                    xIsNumeric = false;
                    break;
                }
                else
                {
                    xIsNumeric = true;
                }
            }

            foreach (var item in y)
            {
                if (!double.TryParse(item, out _))
                {
                    yIsNumeric = false;
                    break;
                }
                else
                {
                    yIsNumeric = true;
                }
            }

            if (xIsNumeric && yIsNumeric)
            {
                var xNumeric = Data.ConvertToDoubles(x);
                var yNumeric = Data.ConvertToDoubles(y);
                this.ViewModel.View.GenericPlot.plt.PlotScatter(xNumeric, yNumeric);
            }
            else if (yIsNumeric)
            {
                var yNumeric = Data.ConvertToDoubles(y);
                var barCount = DataGen.Consecutive(x.Length);
                this.ViewModel.View.GenericPlot.plt.PlotBar(barCount, yNumeric);
                this.ViewModel.View.GenericPlot.plt.XTicks(barCount, x.ToArray());
            }
            else
            {
                MessageBox.Show("Cannot plot '" + xAxis + "' against '" + yAxis + "'", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            this.ViewModel.View.GenericPlot.Render();
        }
    }
}
