using RaceTracker.LogicHelpers;
using RaceTracker.Models;
using RaceTracker.ViewModels;
using RaceTracker.Views;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                case "btPositionGo":
                    canExecute = true;
                    break;
            }

            return canExecute;
        }

        public void Execute(object parameter)
        {
            switch (parameter.ToString())
            {
                //case "btPlotGeneric": this.PlotGeneric();
                //    break;
                case "btPositionGo":
                    this.FavouriteByPosition();
                    break;
            }
        }

        private void PlotTimeSeries(IEnumerable<DateTime> x, IEnumerable<double> y)
        {
            this.ViewModel.View.FavouritePlot.Reset();

            var xNumeric = new List<double>();
            foreach (var item in x)
            {
                xNumeric.Add(item.ToOADate());
            }

            this.ViewModel.View.FavouritePlot.plt.PlotScatter(xNumeric.ToArray(), y.ToArray(), lineWidth: 0);                        this.ViewModel.View.FavouritePlot.plt.Ticks(dateTimeX: true);
            this.ViewModel.View.FavouritePlot.Render();
        }

        private void FavouriteByPosition()
        {
            if (int.TryParse(this.ViewModel.Model.Position, out int position) && position > 0)
            {
                var data = this.GetFavouriteByPostionData(position, this.ViewModel.Model.TimeResolutionField, this.ViewModel.Model.MinDate, this.ViewModel.Model.MaxDate);
                foreach (var set in data)
                {
                    this.PlotTimeSeries(set.Key, set.Value);
                    break;
                }
            }
            else
            {
                MessageBox.Show("ERROR: Position must be a positive integer!", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Dictionary<List<DateTime>, List<double>> GetFavouriteByPostionData(int position, string resolution, DateTime minDate, DateTime maxDate)
        {
            var relevantColumns = new List<Tuple<DateTime, int, string>>();
            var dates = new List<DateTime>();
            var postions = new List<int>();
            var expectations = new List<string>();

            foreach (var column in Data.ProcessedRaceData)
            {
                if (column.Key.ToLower() == "date")
                {
                    foreach (var item in column.Value.Data)
                    {
                        dates.Add((DateTime)item);
                    }
                }
                else if (column.Key.ToLower() == "position")
                {
                    foreach (var item in column.Value.Data)
                    {
                        postions.Add((int)item);
                    }
                }
                else if (column.Key.ToLower() == "expectation")
                {
                    foreach (var item in column.Value.Data)
                    {
                        expectations.Add((string)item);
                    }
                }
            }

            for (int i = 0; i < dates.Count; i++)
            {
                relevantColumns.Add(new Tuple<DateTime, int, string>(dates[i], postions[i], expectations[i]));
            }

            switch (resolution)
            {
                case TimeResolutionFields.Day: return this.GetProbabilityOfFavouriteWinByDay(relevantColumns, minDate, maxDate, position);
                case TimeResolutionFields.Month: return this.GetProbabilityOfFavouriteWinByMonth(relevantColumns, minDate, maxDate, position);
                case TimeResolutionFields.Year: return this.GetProbabilityOfFavouriteWinByYear(relevantColumns, minDate, maxDate, position);
                default: 
                    MessageBox.Show("ERROR: Invalid time resolution selected: " + resolution, AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    throw new Exception();
            }
        }

        private Dictionary<List<DateTime>,List<double>> GetProbabilityOfFavouriteWinByDay(List<Tuple<DateTime, int, string>> rows, DateTime minDate, DateTime maxDate, int position)
        {
            var racesPerDay = new Dictionary<string, int>();
            var raceWinsPerDay = new Dictionary<string, int>();
            var dates = new List<DateTime>();
            var probabilities = new List<double>();
            foreach (var row in rows)
            {
                // Check date is within range
                if (row.Item1 >= minDate && row.Item1 <= maxDate)
                {
                    var day = Formatting.EditStringLength(row.Item1.Year.ToString(), 4) + Formatting.EditStringLength(row.Item1.Month.ToString(), 2) + Formatting.EditStringLength(row.Item1.Day.ToString(), 2);
                    if (racesPerDay.ContainsKey(day))
                    {
                        racesPerDay[day] = racesPerDay[day] += 1;
                    }
                    else
                    {
                        racesPerDay.Add(day, 1);
                    }

                    // Check that favourite finished in the given position
                    if (row.Item2 == position && row.Item3.ToLower().Trim() == "f")
                    {
                        if (raceWinsPerDay.ContainsKey(day))
                        {
                            raceWinsPerDay[day] = racesPerDay[day] += 1;
                        }
                        else
                        {
                            raceWinsPerDay.Add(day, 1);
                        }
                    }
                }
            }

            // for each day, calculate the probability of the favourite finishing in the given position
            foreach(var day in racesPerDay)
            {
                double denominator = day.Value;
                double numerator = 0;

                if(raceWinsPerDay.ContainsKey(day.Key))
                {
                    numerator = raceWinsPerDay[day.Key];
                }

                dates.Add(DateTime.ParseExact(day.Key, "yyyyMMdd", CultureInfo.InvariantCulture));
                probabilities.Add(numerator / denominator);
            }

            var result = new Dictionary<List<DateTime>, List<double>>
            {
                { dates, probabilities }
            };

            return result;
        }

        private Dictionary<List<DateTime>, List<double>> GetProbabilityOfFavouriteWinByMonth(List<Tuple<DateTime, int, string>> rows, DateTime minDate, DateTime maxDate, int position)
        {
            var racesPerMonth = new Dictionary<string, int>();
            var raceWinsPerMonth = new Dictionary<string, int>();
            var dates = new List<DateTime>();
            var probabilities = new List<double>();
            foreach (var row in rows)
            {
                // Check date is within range
                if (row.Item1 >= minDate && row.Item1 <= maxDate)
                {
                    var month = Formatting.EditStringLength(row.Item1.Year.ToString(), 4) + Formatting.EditStringLength(row.Item1.Month.ToString(), 2);
                    if (racesPerMonth.ContainsKey(month))
                    {
                        racesPerMonth[month] = racesPerMonth[month] += 1;
                    }
                    else
                    {
                        racesPerMonth.Add(month, 1);
                    }

                    // Check that favourite finished in the given position
                    if (row.Item2 == position && row.Item3.ToLower().Trim() == "f")
                    {
                        if (raceWinsPerMonth.ContainsKey(month))
                        {
                            raceWinsPerMonth[month] = racesPerMonth[month] += 1;
                        }
                        else
                        {
                            raceWinsPerMonth.Add(month, 1);
                        }
                    }
                }
            }

            // for each month, calculate the probability of the favourite finishing in the given position
            foreach (var month in racesPerMonth)
            {
                double denominator = month.Value;
                double numerator = 0;

                if (raceWinsPerMonth.ContainsKey(month.Key))
                {
                    numerator = raceWinsPerMonth[month.Key];
                }

                dates.Add(DateTime.ParseExact(month.Key, "yyyyMM", CultureInfo.InvariantCulture));
                probabilities.Add(numerator / denominator);
            }

            var result = new Dictionary<List<DateTime>, List<double>>
            {
                { dates, probabilities }
            };

            return result;
        }

        private Dictionary<List<DateTime>, List<double>> GetProbabilityOfFavouriteWinByYear(List<Tuple<DateTime, int, string>> rows, DateTime minDate, DateTime maxDate, int position)
        {
            var racesPerYear = new Dictionary<string, int>();
            var raceWinsPerYear = new Dictionary<string, int>();
            var dates = new List<DateTime>();
            var probabilities = new List<double>();
            foreach (var row in rows)
            {
                // Check date is within range
                if (row.Item1 >= minDate && row.Item1 <= maxDate)
                {
                    var year = Formatting.EditStringLength(row.Item1.Year.ToString(), 4);
                    if (racesPerYear.ContainsKey(year))
                    {
                        racesPerYear[year] = racesPerYear[year] += 1;
                    }
                    else
                    {
                        racesPerYear.Add(year, 1);
                    }

                    // Check that favourite finished in the given position
                    if (row.Item2 == position && row.Item3.ToLower().Trim() == "f")
                    {
                        if (raceWinsPerYear.ContainsKey(year))
                        {
                            raceWinsPerYear[year] = racesPerYear[year] += 1;
                        }
                        else
                        {
                            raceWinsPerYear.Add(year, 1);
                        }
                    }
                }
            }

            // for each year, calculate the probability of the favourite finishing in the given position
            foreach (var month in racesPerYear)
            {
                double denominator = month.Value;
                double numerator = 0;

                if (raceWinsPerYear.ContainsKey(month.Key))
                {
                    numerator = raceWinsPerYear[month.Key];
                }

                dates.Add(DateTime.ParseExact(month.Key, "yyyy", CultureInfo.InvariantCulture));
                probabilities.Add(numerator / denominator);
            }

            var result = new Dictionary<List<DateTime>, List<double>>
            {
                { dates, probabilities }
            };

            return result;
        }

        //private void PlotGeneric()
        //{
        //    this.ViewModel.View.GenericPlot.Reset();
        //    string xAxis = this.ViewModel.Model.XAxisSelection;
        //    string yAxis = this.ViewModel.Model.YAxisSelection;
        //    var xData = Data.ProcessedRaceData[xAxis];
        //    var yData = Data.ProcessedRaceData[yAxis];
        //    var x = xData.Data;
        //    var y = yData.Data;

        //    bool xIsNumeric = xData.Type == typeof(int) || xData.Type == typeof(double);
        //    bool yIsNumeric = yData.Type == typeof(int) || yData.Type == typeof(double);

        //    if (xIsNumeric && yIsNumeric)
        //    {
        //        var xNumeric = Data.SubArray(Data.ConvertToDoubles(x), 0, 2000);
        //        var yNumeric = Data.SubArray(Data.ConvertToDoubles(y), 0, 2000);
        //        this.ViewModel.View.GenericPlot.plt.PlotScatter(xNumeric, yNumeric);
        //    }
        //    else if (yIsNumeric)
        //    {
        //        var xString = new List<string>();
        //        foreach (var item in x)
        //        {
        //            xString.Add(item.ToString());
        //        }

        //        var yNumeric = Data.ConvertToDoubles(y);
        //        var barCount = DataGen.Consecutive(x.Length);
        //        this.ViewModel.View.GenericPlot.plt.PlotBar(barCount, yNumeric);
        //        this.ViewModel.View.GenericPlot.plt.XTicks(barCount, xString.ToArray());
        //    }
        //    else
        //    {
        //        MessageBox.Show("Cannot plot '" + xAxis + "' against '" + yAxis + "'", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //    }

        //    this.ViewModel.View.GenericPlot.Render();
        //}
    }
}
