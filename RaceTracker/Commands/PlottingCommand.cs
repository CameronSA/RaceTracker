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
using System.Runtime.InteropServices;
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
            switch (parameter.ToString())
            {
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
                case "btPositionGo":
                    this.FavouriteByPosition();
                    break;
            }
        }

        private void PlotTimeSeries(IEnumerable<DateTime> x, IEnumerable<double> y, bool reset)
        {
            if (reset)
            {
                this.ViewModel.View.FavouritePlot.Reset();
            }

            var xNumeric = new List<double>();
            foreach (var item in x)
            {
                xNumeric.Add(item.ToOADate());
            }

            this.ViewModel.View.FavouritePlot.plt.PlotScatter(xNumeric.ToArray(), y.ToArray(), lineWidth: 0); this.ViewModel.View.FavouritePlot.plt.Ticks(dateTimeX: true);
            this.ViewModel.View.FavouritePlot.Render();
        }

        private void FavouriteByPosition()
        {
            if (int.TryParse(this.ViewModel.Model.Position, out int position) && position > 0)
            {
                var positionData = this.GetFavouriteByPostionData(position, this.ViewModel.Model.TimeResolutionField, this.ViewModel.Model.MinDate, this.ViewModel.Model.MaxDate);
                foreach (var set in positionData)
                {
                    this.PlotTimeSeries(set.Key, set.Value,true);
                    break;
                }
            }
            else
            {
                MessageBox.Show("ERROR: Position must be a positive integer!", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.NumberRaceCoursesByDate();
        }

        private void NumberRaceCoursesByDate()
        {
            var raceCoursesData = this.GetNumberRaceCoursesData(this.ViewModel.Model.TimeResolutionField, this.ViewModel.Model.MinDate, this.ViewModel.Model.MaxDate);
            foreach (var set in raceCoursesData)
            {
                this.PlotTimeSeries(set.Key, set.Value, false);
            }
        }

        private Dictionary<List<DateTime>, List<double>> GetNumberRaceCoursesData(string resolution, DateTime minDate, DateTime maxDate)
        {
            var relevantColumns = new List<Tuple<DateTime, string>>();
            var dates = new List<DateTime>();
            var raceTracks = new List<string>();

            foreach (var column in Data.ProcessedRaceData)
            {
                if (column.Key.ToLower() == "date")
                {
                    foreach (var item in column.Value.Data)
                    {
                        dates.Add((DateTime)item);
                    }
                }
                else if (column.Key.ToLower() == "racetrack")
                {
                    foreach (var item in column.Value.Data)
                    {
                        raceTracks.Add((string)item);
                    }
                }
            }

            for (int i = 0; i < dates.Count; i++)
            {
                relevantColumns.Add(new Tuple<DateTime, string>(dates[i], raceTracks[i]));
            }

            return this.GetNumberOfRaceTracksPerDate(relevantColumns, minDate, maxDate, resolution);
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

            return this.GetProbabilityOfFavouriteWin(relevantColumns, minDate, maxDate, position, resolution);            
        }

        private Dictionary<List<DateTime>, List<double>> GetNumberOfRaceTracksPerDate(List<Tuple<DateTime, string>> rows, DateTime minDate, DateTime maxDate, string resolution)
        {
            var dates = new List<DateTime>();
            var tracksPerDate = new List<double>();
            var tracksPerDelimiter = new Dictionary<string, HashSet<string>>();
            foreach (var row in rows)
            {
                // Check date is within range
                if (row.Item1 >= minDate && row.Item1 <= maxDate)
                {
                    string delimiter = string.Empty;
                    switch (resolution)
                    {
                        case TimeResolutionFields.Day: delimiter = Formatting.EditStringLength(row.Item1.Year.ToString(), 4) + Formatting.EditStringLength(row.Item1.Month.ToString(), 2) + Formatting.EditStringLength(row.Item1.Day.ToString(), 2); break;
                        case TimeResolutionFields.Month: delimiter = Formatting.EditStringLength(row.Item1.Year.ToString(), 4) + Formatting.EditStringLength(row.Item1.Month.ToString(), 2); break;
                        case TimeResolutionFields.Year: delimiter = Formatting.EditStringLength(row.Item1.Year.ToString(), 4); break;
                        default:
                            MessageBox.Show("ERROR: Invalid time resolution selected: " + resolution, AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            throw new Exception();
                    }

                    if (tracksPerDelimiter.ContainsKey(delimiter))
                    {
                        tracksPerDelimiter[delimiter].Add(row.Item2.Trim());
                    }
                    else
                    {
                        tracksPerDelimiter.Add(delimiter, new HashSet<string> { row.Item2.Trim() });
                    }
                }
            }

            // Count the number of unique racetracks per delimiter
            foreach (var delimiter in tracksPerDelimiter)
            {
                string dateFormat = string.Empty;
                switch (resolution)
                {
                    case TimeResolutionFields.Day: dateFormat = "yyyyMMdd"; break;
                    case TimeResolutionFields.Month: dateFormat = "yyyyMM"; break;
                    case TimeResolutionFields.Year: dateFormat = "yyyy"; break;
                    default:
                        MessageBox.Show("ERROR: Invalid time resolution selected: " + resolution, AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw new Exception();
                }

                dates.Add(DateTime.ParseExact(delimiter.Key, dateFormat, CultureInfo.InvariantCulture));
                tracksPerDate.Add(delimiter.Value.Count);
            }

            var result = new Dictionary<List<DateTime>, List<double>>
            {
                { dates, tracksPerDate }
            };

            return result;
        }

        private Dictionary<List<DateTime>, List<double>> GetProbabilityOfFavouriteWin(List<Tuple<DateTime, int, string>> rows, DateTime minDate, DateTime maxDate, int position, string resolution)
        {
            var racesPerDelimiter = new Dictionary<string, int>();
            var raceWinsPerDelimiter = new Dictionary<string, int>();
            var dates = new List<DateTime>();
            var probabilities = new List<double>();
            foreach (var row in rows)
            {
                // Check date is within range
                if (row.Item1 >= minDate && row.Item1 <= maxDate)
                {
                    string delimiter = string.Empty;
                    switch (resolution)
                    {
                        case TimeResolutionFields.Day: delimiter = Formatting.EditStringLength(row.Item1.Year.ToString(), 4) + Formatting.EditStringLength(row.Item1.Month.ToString(), 2) + Formatting.EditStringLength(row.Item1.Day.ToString(), 2); break;
                        case TimeResolutionFields.Month: delimiter = Formatting.EditStringLength(row.Item1.Year.ToString(), 4) + Formatting.EditStringLength(row.Item1.Month.ToString(), 2); break;
                        case TimeResolutionFields.Year: delimiter = Formatting.EditStringLength(row.Item1.Year.ToString(), 4); break;
                        default:
                            MessageBox.Show("ERROR: Invalid time resolution selected: " + resolution, AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            throw new Exception();
                    }

                    if (racesPerDelimiter.ContainsKey(delimiter))
                    {
                        racesPerDelimiter[delimiter] = racesPerDelimiter[delimiter] += 1;
                    }
                    else
                    {
                        racesPerDelimiter.Add(delimiter, 1);
                    }

                    // Check that favourite finished in the given position
                    if (row.Item2 == position && row.Item3.ToLower().Trim() == "f")
                    {
                        if (raceWinsPerDelimiter.ContainsKey(delimiter))
                        {
                            raceWinsPerDelimiter[delimiter] = racesPerDelimiter[delimiter] += 1;
                        }
                        else
                        {
                            raceWinsPerDelimiter.Add(delimiter, 1);
                        }
                    }
                }
            }

            // for each day/month/year, calculate the probability of the favourite finishing in the given position
            foreach (var delimiter in racesPerDelimiter)
            {
                double denominator = delimiter.Value;
                double numerator = 0;

                if (raceWinsPerDelimiter.ContainsKey(delimiter.Key))
                {
                    numerator = raceWinsPerDelimiter[delimiter.Key];
                }

                string dateFormat = string.Empty;
                switch (resolution)
                {
                    case TimeResolutionFields.Day: dateFormat = "yyyyMMdd"; break;
                    case TimeResolutionFields.Month: dateFormat = "yyyyMM"; break;
                    case TimeResolutionFields.Year: dateFormat = "yyyy"; break;
                    default:
                        MessageBox.Show("ERROR: Invalid time resolution selected: " + resolution, AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw new Exception();
                }

                dates.Add(DateTime.ParseExact(delimiter.Key, dateFormat, CultureInfo.InvariantCulture));
                probabilities.Add(numerator / denominator);
            }

            var result = new Dictionary<List<DateTime>, List<double>>
            {
                { dates, probabilities }
            };

            return result;
        }       
    }
}
