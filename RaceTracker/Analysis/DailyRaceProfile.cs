using RaceTracker.LogicHelpers;
using RaceTracker.Models;
using RaceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RaceTracker.Analysis
{
    public class DailyRaceProfile
    {
        public DailyRaceProfile(PlottingViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.Plotting = new Plotting();
            this.NumberRaceCoursesData = new Dictionary<List<DateTime>, List<double>>();
            this.DailyRaceProfileData = new Dictionary<List<DateTime>, List<double>>();
            this.OverallRaceProfileDataVsNumberRaceCourses = new Dictionary<List<double>, List<double>>();
            this.individualPlotSeries = new List<string>();
        }

        private List<string> individualPlotSeries { get; set; }
        private Dictionary<List<DateTime>, List<double>> NumberRaceCoursesData { get; set; }
        private Dictionary<List<DateTime>, List<double>> DailyRaceProfileData { get; set; }
        private Dictionary<List<double>, List<double>> OverallRaceProfileDataVsNumberRaceCourses { get; set; } // Key: Number racecourses. Value: Race profile
        private PlottingViewModel ViewModel { get; }
        private Plotting Plotting { get; }

        public void PlotDailyRaceProfile()
        {
            if (int.TryParse(this.ViewModel.Model.Position, out int position) && position > 0)
            {
                this.DailyRaceProfileData = this.GetDailyRaceProfileData(position, this.ViewModel.Model.UpToAndIncludingPosition, this.ViewModel.Model.MinDate, this.ViewModel.Model.MaxDate);

                this.NumberRaceCoursesByDate();

                foreach (var set in DailyRaceProfileData)
                {
                    string ylabel;
                    if (this.ViewModel.Model.UpToAndIncludingPosition)
                    {
                        ylabel = "Number of Races Before Favourite Finishes In Position " + this.ViewModel.Model.Position + " or Less";
                    }
                    else
                    {
                        ylabel = "Number of Races Before Favourite Finishes In Position " + this.ViewModel.Model.Position;
                    }

                    this.Plotting.PlotTimeSeries(this.ViewModel.View.DailyProfilePlot, set.Key, set.Value, false, string.Empty, string.Empty, ylabel);
                    break;
                }

                this.DailyRaceProfileVsNumberRaceCourses();

                if (int.TryParse(this.ViewModel.Model.IndividualNumberRacecoursesMin, out int numberRacecoursesMin) && int.TryParse(this.ViewModel.Model.IndividualNumberRacecoursesMax, out int numberRacecoursesMax) && numberRacecoursesMin > 0 && numberRacecoursesMax > 0)
                {
                    bool reset = this.ViewModel.Model.ResetIndividual;
                    for (int i = numberRacecoursesMin; i <= numberRacecoursesMax; i++)
                    {
                        this.DailyRaceProfilePerNumberRaceCourses(i, this.ViewModel.Model.ResetIndividual);
                    }

                    this.ViewModel.Model.ResetIndividual = reset;
                }
                else if (!string.IsNullOrEmpty(this.ViewModel.Model.IndividualNumberRacecoursesMin) || !string.IsNullOrEmpty(this.ViewModel.Model.IndividualNumberRacecoursesMin))
                {
                    MessageBox.Show("ERROR: Min-Max values must consist of positive integers!", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("ERROR: Position must be a positive integer!", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DailyRaceProfilePerNumberRaceCourses(int numberRaceCourses, bool reset)
        {
            // Key: Number races before favourite win. Value: Count/Number of days with x number of race courses
            var numberRacesBeforeFavouriteWinVsCount = new Dictionary<double, double>();
            var raceProfile = new List<double>();
            var raceProfileCount = new List<double>();
            int daysWithNumberOfRaceCourses = CommonAnalyses.RetrieveNumberOfDaysWithGivenNumberOfRaceCourses(numberRaceCourses, this.ViewModel.Model.MinDate, this.ViewModel.Model.MaxDate);

            foreach (var dataSet in this.OverallRaceProfileDataVsNumberRaceCourses)
            {
                for (int i = 0; i < dataSet.Key.Count; i++)
                {
                    if ((int)dataSet.Key[i] == numberRaceCourses)
                    {
                        if (!numberRacesBeforeFavouriteWinVsCount.ContainsKey(dataSet.Value[i]))
                        {
                            numberRacesBeforeFavouriteWinVsCount.Add(dataSet.Value[i], 0);
                        }
                        else
                        {
                            numberRacesBeforeFavouriteWinVsCount[dataSet.Value[i]] = numberRacesBeforeFavouriteWinVsCount[dataSet.Value[i]] += (1.0 / (double)daysWithNumberOfRaceCourses);
                        }
                    }
                }
            }

            foreach (var item in numberRacesBeforeFavouriteWinVsCount)
            {
                raceProfile.Add(item.Key);
                raceProfileCount.Add(item.Value);
            }

            if (reset)
            {
                this.individualPlotSeries = new List<string>();
                this.ViewModel.Model.ResetIndividual = false;
            }

            string seriesName;
            string ylabel;

            if (this.ViewModel.Model.UpToAndIncludingPosition)
            {
                seriesName = "Position Up To and Including " + this.ViewModel.Model.Position + ": Number of Courses = " + numberRaceCourses;
                ylabel = "Number of Races Before Favourite Finishes in Position " + this.ViewModel.Model.Position + " or Less";
            }
            else
            {
                seriesName = "Position " + this.ViewModel.Model.Position + ": Number of Courses = " + numberRaceCourses;
                ylabel = "Number of Races Before Favourite Finishes in Position " + this.ViewModel.Model.Position;
            }

            this.Plotting.PlotScatter(this.ViewModel.View.DailyProfileVsNumberRaceCoursesIndividualPlot, raceProfile, raceProfileCount, reset, ylabel, "Count/# Days", this.individualPlotSeries, seriesName, -1, 45, -0.02, 0.8);
        }

        private void DailyRaceProfileVsNumberRaceCourses()
        {
            var numberRacesBeforeFavouriteWin = new List<double>();
            var numberRaceTracks = new List<double>();

            var positionDateIndices = new Dictionary<DateTime, int>();
            var numberRacesDateIndices = new Dictionary<DateTime, int>();
            foreach (var dataSet in this.DailyRaceProfileData)
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
                    numberRacesBeforeFavouriteWin.Add(dataSet.Value[dataIndex.Value]);
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

            this.OverallRaceProfileDataVsNumberRaceCourses = new Dictionary<List<double>, List<double>>
            {
                { numberRaceTracks, numberRacesBeforeFavouriteWin }
            };

            string xLabel;
            if (this.ViewModel.Model.UpToAndIncludingPosition)
            {
                xLabel = "Number of Races Before Favourite Finishes in Position " + this.ViewModel.Model.Position;
            }
            else
            {
                xLabel = "Number of Races Before Favourite Finishes in Position " + this.ViewModel.Model.Position + " or Less";
            }

            this.Plotting.PlotScatterLabels(this.ViewModel.View.DailyProfileVsNumberRaceCoursesOverallPlot, numberRacesBeforeFavouriteWin, numberRaceTracks, xLabel, "Number of Race Courses Running Per Day", NormalisingFactors.NumberOfRaceTracks, this.ViewModel.Model.MinDate, this.ViewModel.Model.MaxDate);
        }

        private void NumberRaceCoursesByDate()
        {
            this.NumberRaceCoursesData = CommonAnalyses.RetrieveNumberRaceCoursesData(TimeResolutionFields.Day, this.ViewModel.Model.MinDate, this.ViewModel.Model.MaxDate);
            foreach (var set in NumberRaceCoursesData)
            {
                this.Plotting.PlotTimeSeries(this.ViewModel.View.DailyProfilePlot, set.Key, set.Value, true, string.Empty, string.Empty, "Number of Race Courses Running");
            }
        }

        private Dictionary<List<DateTime>, List<double>> GetDailyRaceProfileData(int position, bool upToAndIncludingPosition, DateTime minDate, DateTime maxDate)
        {
            var relevantColumns = new List<Tuple<DateTime, DateTime, int, string>>();
            var dates = new List<DateTime>();
            var times = new List<DateTime>();
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
                else if (column.Key.ToLower() == "time")
                {
                    foreach (var item in column.Value.Data)
                    {
                        times.Add((DateTime)item);
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
                relevantColumns.Add(new Tuple<DateTime, DateTime, int, string>(dates[i], times[i], positions[i], expectations[i]));
            }

            return this.GetNumberOfRacesWithoutFavouriteWin(relevantColumns, minDate, maxDate, position, upToAndIncludingPosition);
        }

        private Dictionary<List<DateTime>, List<double>> GetNumberOfRacesWithoutFavouriteWin(List<Tuple<DateTime, DateTime, int, string>> rows, DateTime minDate, DateTime maxDate, int position, bool upToAndIncludingPosition)
        {
            // Race dates within given date range (x axis)
            var dates = new List<DateTime>();
            // Number of races without the favourite finishing in the given position (y axis)
            var numberRacesWithoutWin = new List<double>();
            // Key: day of race. Value: list of race times for that day
            var dailyRaceTimesDictionary = new Dictionary<DateTime, List<DateTime>>();
            // Key: Race day. Value: ordered race times for that day with a boolean determining whether the favourite finished in the given position
            var raceOutcomes = new Dictionary<DateTime, List<Tuple<DateTime, bool>>>();
            // Unique race days
            var raceDays = new HashSet<DateTime>();
            // Find race days within given range
            foreach (var row in rows)
            {
                if (row.Item1 >= minDate && row.Item1 <= maxDate)
                {
                    raceDays.Add(row.Item1);
                }
            }

            // For each race day, find race times and outcomes
            foreach (var day in raceDays)
            {
                // Key: Race time. Value: favourite finished in the given position
                var raceOutcome = new Dictionary<DateTime, bool>();
                var raceOutcomeList = new List<Tuple<DateTime, bool>>();

                foreach (var row in rows)
                {
                    // If the favourite came in the given position for that race, add it to a dictionary (This should take into account draws by only overwriting a value if said value is true)
                    if (row.Item1 == day && (row.Item3 == position || (row.Item3 <= position && upToAndIncludingPosition)))
                    {
                        bool favourite = false;
                        if (row.Item4.ToLower().Trim() == "f")
                        {
                            favourite = true;
                        }

                        if (!raceOutcome.ContainsKey(row.Item2))
                        {
                            raceOutcome.Add(row.Item2, favourite);
                        }
                        else if (!raceOutcome[row.Item2])
                        {
                            raceOutcome[row.Item2] = favourite;
                        }
                    }
                }

                // Convert dictionary to a a list ordered by race time
                foreach (var item in raceOutcome)
                {
                    raceOutcomeList.Add(new Tuple<DateTime, bool>(item.Key, item.Value));
                }

                raceOutcomeList = raceOutcomeList.OrderBy(x => x.Item1).ToList();

                // Add list to dictionary of race days
                if (!raceOutcomes.ContainsKey(day))
                {
                    raceOutcomes.Add(day, raceOutcomeList);
                }
                else
                {
                    MessageBox.Show("ERROR: Duplicate race days found", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // For each race outcome, count the number of races before a favourite finishes in the given position
            foreach (var outcome in raceOutcomes)
            {
                dates.Add(outcome.Key);
                int numberRaces = 0;
                foreach (var race in outcome.Value)
                {
                    if (race.Item2)
                    {
                        break;
                    }

                    numberRaces++;
                }

                numberRacesWithoutWin.Add((double)numberRaces);
            }

            return new Dictionary<List<DateTime>, List<double>>
            {
                { dates, numberRacesWithoutWin }
            };
        }
    }
}
