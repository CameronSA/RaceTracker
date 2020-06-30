using RaceTracker.LogicHelpers;
using RaceTracker.Models;
using RaceTracker.ViewModels;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RaceTracker.Analysis
{
    public class FavouriteByPosition
    {
        public FavouriteByPosition(PlottingViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.PositionProbabilityData = new Dictionary<List<DateTime>, List<double>>();
            this.NumberRaceCoursesData = new Dictionary<List<DateTime>, List<double>>();
            this.Plotting = new Plotting();
        }

        private Dictionary<List<DateTime>, List<double>> PositionProbabilityData { get; set; }
        private Dictionary<List<DateTime>, List<double>> NumberRaceCoursesData { get; set; }
        private PlottingViewModel ViewModel { get; }
        private Plotting Plotting { get; }

        public void PlotFavouriteByPosition()
        {
            if (int.TryParse(this.ViewModel.Model.Position, out int position) && position > 0)
            {
                this.PositionProbabilityData = this.GetFavouriteByPostionData(position, this.ViewModel.Model.TimeResolutionField, this.ViewModel.Model.MinDate, this.ViewModel.Model.MaxDate);
                foreach (var set in this.PositionProbabilityData)
                {
                    this.Plotting.PlotTimeSeries(this.ViewModel.View.FavouritePlot,set.Key, set.Value, true, string.Empty, string.Empty, "Favourites Finishing in Position " + this.ViewModel.Model.Position + " (%)");
                    break;
                }

                this.NumberRaceCoursesByDate();
                this.FavouriteVsNumberRaces();
            }
            else
            {
                MessageBox.Show("ERROR: Position must be a positive integer!", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FavouriteVsNumberRaces()
        {
            var positionProbabilities = new List<double>();
            var numberRaceTracks = new List<double>();

            var positionDateIndices = new Dictionary<DateTime, int>();
            var numberRacesDateIndices = new Dictionary<DateTime, int>();
            foreach (var dataSet in this.PositionProbabilityData)
            {
                for (int i = 0; i < dataSet.Key.Count; i++)
                {
                    if (!positionDateIndices.ContainsKey(dataSet.Key[i]))
                    {
                        positionDateIndices.Add(dataSet.Key[i], i);
                    }
                    else
                    {
                        MessageBox.Show("WARNING: Duplicate dates", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                foreach (var dataIndex in positionDateIndices)
                {
                    positionProbabilities.Add(dataSet.Value[dataIndex.Value]);
                }

                break;
            }

            foreach (var dataSet in this.NumberRaceCoursesData)
            {
                for (int i = 0; i < dataSet.Key.Count; i++)
                {
                    if (!numberRacesDateIndices.ContainsKey(dataSet.Key[i]))
                    {
                        numberRacesDateIndices.Add(dataSet.Key[i], i);
                    }
                    else
                    {
                        MessageBox.Show("WARNING: Duplicate dates", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                foreach (var dataIndex in numberRacesDateIndices)
                {
                    numberRaceTracks.Add(dataSet.Value[dataIndex.Value]);
                }

                break;
            }

            this.Plotting.PlotScatter(this.ViewModel.View.FavouriteVsNumberRaceCoursesPlot, positionProbabilities, numberRaceTracks, true, "Favourites Finishing in Position " + this.ViewModel.Model.Position + " (%)", "Number of Race Courses Running", new List<string>(), string.Empty);
            
        }

        private void NumberRaceCoursesByDate()
        {
            this.NumberRaceCoursesData = CommonAnalyses.RetrieveNumberRaceCoursesData(this.ViewModel.Model.TimeResolutionField, this.ViewModel.Model.MinDate, this.ViewModel.Model.MaxDate);
            foreach (var set in NumberRaceCoursesData)
            {
                this.Plotting.PlotTimeSeries(this.ViewModel.View.FavouritePlot, set.Key, set.Value, false, string.Empty, string.Empty, "Number of Race Courses Running");
            }
        }

        private Dictionary<List<DateTime>, List<double>> GetFavouriteByPostionData(int position, string resolution, DateTime minDate, DateTime maxDate)
        {
            var relevantColumns = new List<Tuple<DateTime, int, string>>();
            var dates = new List<DateTime>();
            var positions = new List<int>();
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
                        positions.Add((int)item);
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
                relevantColumns.Add(new Tuple<DateTime, int, string>(dates[i], positions[i], expectations[i]));
            }

            return this.GetProbabilityOfFavouriteWin(relevantColumns, minDate, maxDate, position, resolution);
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

                    // Check that favourite finished in the given position
                    if (row.Item2 == position)
                    {
                        if (row.Item3.ToLower().Trim() == "f")
                        {
                            if (raceWinsPerDelimiter.ContainsKey(delimiter))
                            {
                                raceWinsPerDelimiter[delimiter] = raceWinsPerDelimiter[delimiter] += 1;
                            }
                            else
                            {
                                raceWinsPerDelimiter.Add(delimiter, 1);
                            }
                        }

                        if (racesPerDelimiter.ContainsKey(delimiter))
                        {
                            racesPerDelimiter[delimiter] = racesPerDelimiter[delimiter] += 1;
                        }
                        else
                        {
                            racesPerDelimiter.Add(delimiter, 1);
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
                probabilities.Add(100 * (numerator / denominator));
            }

            var result = new Dictionary<List<DateTime>, List<double>>
            {
                { dates, probabilities }
            };

            return result;
        }
    }
}
