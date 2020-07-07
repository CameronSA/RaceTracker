using RaceTracker.LogicHelpers;
using RaceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RaceTracker.Analysis
{
    public class OddsProbabilityProfile
    {
        public OddsProbabilityProfile(PlottingViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.Plotting = new Plotting();
        }

        public void PlotOddsProbabilityProfile()
        {

            if (int.TryParse(this.ViewModel.Model.Position, out int position))
            {
                if (double.TryParse(this.ViewModel.Model.MinOdds, out double minOdds) && double.TryParse(this.ViewModel.Model.MaxOdds, out double maxOdds))
                {
                    this.ProbabilityOfOddsWinningVsTimeSplitByRaceType(minOdds, maxOdds, position);
                }
                else
                {
                    MessageBox.Show("ERROR: Odds values must be numeric!", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("ERROR: Position must be a positive integer!", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProbabilityOfOddsWinningVsTimeSplitByRaceType(double minOdds, double maxOdds, int position)
        {
            var expectations = new List<string>();
            var odds = new List<double>();
            var positions = new List<int>();
            var dates = new List<DateTime>();
            var times = new List<DateTime>();
            var raceTypes = new List<string>();
            var rows = new List<Tuple<DateTime, DateTime, string, string, double, int>>();
            foreach (var column in Data.ProcessedRaceData)
            {
                if (column.Key.ToLower() == "expectation")
                {
                    foreach (var item in column.Value.Data)
                    {
                        expectations.Add((string)item);
                    }
                }
                else if (column.Key.ToLower() == "isp")
                {
                    foreach (var item in column.Value.Data)
                    {
                        odds.Add(double.Parse(item.ToString()));
                    }
                }
                else if (column.Key.ToLower() == "position")
                {
                    foreach (var item in column.Value.Data)
                    {
                        positions.Add((int)item);
                    }
                }
                else if (column.Key.ToLower() == "date")
                {
                    foreach (var item in column.Value.Data)
                    {
                        dates.Add(DateTime.Parse(item.ToString()));
                    }
                }
                else if (column.Key.ToLower() == "time")
                {
                    foreach (var item in column.Value.Data)
                    {
                        times.Add(DateTime.Parse(item.ToString()));
                    }
                }
                else if (column.Key.ToLower() == "race type")
                {
                    foreach (var item in column.Value.Data)
                    {
                        raceTypes.Add(item.ToString());
                    }
                }
            }

            for (int i = 0; i < dates.Count; i++)
            {
                rows.Add(new Tuple<DateTime, DateTime, string, string, double, int>(dates[i], times[i], raceTypes[i], expectations[i], odds[i], positions[i]));
            }

            var splitByRaceTypeAndFilterByDateAndOddsAndExpectation = new Dictionary<string, List<Tuple<DateTime, int>>>();
            foreach (var row in rows)
            {
                if (row.Item1 >= this.ViewModel.Model.MinDate && row.Item1 <= this.ViewModel.Model.MaxDate)
                {
                    if (row.Item5 >= minOdds && row.Item5 <= maxOdds)
                    {
                        if (row.Item4.ToLower().Trim() == "f")
                        {
                            var date = new DateTime(row.Item1.Year, row.Item1.Month, row.Item1.Day, row.Item2.Hour, row.Item2.Minute, row.Item2.Second);
                            var dataToAdd = new Tuple<DateTime, int>(date, row.Item6);
                            if (splitByRaceTypeAndFilterByDateAndOddsAndExpectation.ContainsKey(row.Item3))
                            {
                                splitByRaceTypeAndFilterByDateAndOddsAndExpectation[row.Item3].Add(dataToAdd);
                            }
                            else
                            {
                                splitByRaceTypeAndFilterByDateAndOddsAndExpectation.Add(row.Item3, new List<Tuple<DateTime, int>> { dataToAdd });
                            }
                        }
                    }
                }
            }

            this.ProbabilityOfOddsWinningSplitByRaceType(splitByRaceTypeAndFilterByDateAndOddsAndExpectation, position, minOdds, maxOdds);
            this.ProbabilityOfOddsWinningVsTime(splitByRaceTypeAndFilterByDateAndOddsAndExpectation, position, minOdds, maxOdds);
            bool reset = this.ViewModel.Model.ResetIndividual;
            foreach (var raceType in splitByRaceTypeAndFilterByDateAndOddsAndExpectation)
            {
                List<Tuple<DateTime, int>> data = raceType.Value;
                var positionsPerDay = new Dictionary<DateTime, List<int>>();
                foreach (var row in data)
                {
                    var date = new DateTime(row.Item1.Year, row.Item1.Month, row.Item1.Day);
                    if (positionsPerDay.ContainsKey(date))
                    {
                        positionsPerDay[date].Add(row.Item2);
                    }
                    else
                    {
                        positionsPerDay.Add(date, new List<int> { row.Item2 });
                    }
                }

                var dateList = new List<DateTime>();
                var probabilities = new List<double>();
                var probabilityVsTime = new List<Tuple<DateTime, double>>();
                foreach (var day in positionsPerDay)
                {
                    int numberMatches = 0;
                    foreach (var item in day.Value)
                    {
                        if (item == position || (item <= position && this.ViewModel.Model.UpToAndIncludingPosition && item > 0))
                        {
                            numberMatches++;
                        }
                    }

                    var probability = 100 * (double)numberMatches / (double)day.Value.Count;
                    dateList.Add(day.Key);
                    probabilities.Add(probability);
                    probabilityVsTime.Add(new Tuple<DateTime, double>(day.Key, probability));
                }

                string extra = this.ViewModel.Model.UpToAndIncludingPosition ? " or Less" : string.Empty;
                if (int.TryParse(this.ViewModel.Model.NumberBins, out int numberBins) && Math.Abs(numberBins) < probabilityVsTime.Count && Math.Abs(numberBins) > 0)
                {
                    var binnedData = CommonAnalyses.BinData(probabilityVsTime, Math.Abs(numberBins));
                    new Plotting().PlotTimeSeriesError(this.ViewModel.View.ProbabilityOfOddsWinningVsTimeSplitByRaceType, binnedData.Item1, binnedData.Item2, binnedData.Item3, reset, string.Empty, "Average Probability of Favourite Finishing\nin Position " + position + extra + " (%)", new List<string>(), raceType.Key + "\nOdds: " + minOdds + "-" + maxOdds + "\n# Bins: " + Math.Abs(numberBins) + "\nBin Size: " + binnedData.Item4);
                    reset = false;
                }
                else
                {
                    new Plotting().PlotTimeSeries(this.ViewModel.View.ProbabilityOfOddsWinningVsTimeSplitByRaceType, dateList, probabilities, reset, string.Empty, "Probability of Favourite Finishing\nin Position " + position + extra + " (%)", new List<string>(), raceType.Key + "\nOdds: " + minOdds + "-" + maxOdds);
                    reset = false;
                }
            }
        }

        private void ProbabilityOfOddsWinningVsTime(Dictionary<string, List<Tuple<DateTime, int>>> data, int position, double minOdds, double maxOdds)
        {
            var positionsPerDay = new Dictionary<DateTime, List<int>>();
            foreach (var raceType in data)
            {
                foreach (var row in raceType.Value)
                {
                    var date = new DateTime(row.Item1.Year, row.Item1.Month, row.Item1.Day);
                    if (positionsPerDay.ContainsKey(date))
                    {
                        positionsPerDay[date].Add(row.Item2);
                    }
                    else
                    {
                        positionsPerDay.Add(date, new List<int> { row.Item2 });
                    }
                }
            }

            var dateList = new List<DateTime>();
            var probabilities = new List<double>();
            var probabilityVsTime = new List<Tuple<DateTime, double>>();
            foreach (var day in positionsPerDay)
            {
                int numberMatches = 0;
                foreach (var item in day.Value)
                {
                    if (item == position || (item <= position && this.ViewModel.Model.UpToAndIncludingPosition && item > 0))
                    {
                        numberMatches++;
                    }
                }

                var probability = 100 * (double)numberMatches / (double)day.Value.Count;
                dateList.Add(day.Key);
                probabilities.Add(probability);
                probabilityVsTime.Add(new Tuple<DateTime, double>(day.Key, probability));
            }

            this.ProbabilityLikelihood(probabilityVsTime, position, minOdds, maxOdds);
            string extra = this.ViewModel.Model.UpToAndIncludingPosition ? " or Less" : string.Empty;
            if (int.TryParse(this.ViewModel.Model.NumberBins, out int numberBins) && Math.Abs(numberBins) < probabilityVsTime.Count && Math.Abs(numberBins) > 0)
            {
                var binnedData = CommonAnalyses.BinData(probabilityVsTime, Math.Abs(numberBins));
                new Plotting().PlotTimeSeriesError(this.ViewModel.View.ProbabilityOfOddsWinningVsTime, binnedData.Item1, binnedData.Item2, binnedData.Item3, this.ViewModel.Model.ResetIndividual, string.Empty, "Average Total Probability of Favourite Finishing\nin Position " + position + extra + " (%)", new List<string>(), "Odds: " + minOdds + "-" + maxOdds + "\n# Bins: " + Math.Abs(numberBins) + "\nBin Size: " + binnedData.Item4);
            }
            else
            {
                new Plotting().PlotTimeSeries(this.ViewModel.View.ProbabilityOfOddsWinningVsTime, dateList, probabilities, this.ViewModel.Model.ResetIndividual, string.Empty, "Total Probability of Favourite Finishing\nin Position " + position + extra + " (%)", new List<string>(), "Odds: " + minOdds + "-" + maxOdds);
            }
        }

        private void ProbabilityLikelihood(List<Tuple<DateTime, double>> probabilityVsTime, int position, double minOdds, double maxOdds)
        {
            var probabilityCount = new Dictionary<double, int>();
            int total = 0;
            foreach(var item in probabilityVsTime)
            {
                if(probabilityCount.ContainsKey(item.Item2))
                {
                    probabilityCount[item.Item2] = probabilityCount[item.Item2] += 1;
                }
                else
                {
                    probabilityCount.Add(item.Item2, 1);
                }

                total++;
            }

            var probabilities = new List<double>();
            var count = new List<double>();
            foreach(var item in probabilityCount)
            {
                probabilities.Add(item.Key);
                count.Add(100 * (double)item.Value / (double)total);
            }

            string extra = this.ViewModel.Model.UpToAndIncludingPosition ? " or Less" : string.Empty;
            new Plotting().PlotScatter(this.ViewModel.View.ProbabilityLikelihood, probabilities, count, this.ViewModel.Model.ResetIndividual, "Probability of Position " + position + extra + " (%)", "Count (% of Total)", new List<string>(), "Odds: " + minOdds + "-" + maxOdds);
        }

        private void ProbabilityOfOddsWinningSplitByRaceType(Dictionary<string, List<Tuple<DateTime, int>>> data,int position, double minOdds, double maxOdds)
        {
            var raceTypes = new List<string>();
            var probabilities = new List<double>();
            foreach(var raceType in data)
            {
                int numberMatches = 0;
                foreach (var item in raceType.Value)
                {
                    if (item.Item2 == position || (item.Item2 <= position && this.ViewModel.Model.UpToAndIncludingPosition && item.Item2 > 0))
                    {
                        numberMatches++;
                    }
                }

                raceTypes.Add(raceType.Key);
                probabilities.Add(100 * numberMatches / raceType.Value.Count);
            }

            string extra = this.ViewModel.Model.UpToAndIncludingPosition ? " or Less" : string.Empty;
            new Plotting().PlotBar(this.ViewModel.View.ProbabilityOfOddsWinningSplitByRaceType, raceTypes, probabilities, this.ViewModel.Model.ResetIndividual, "Race Type", "Probability of Favourite Finishing in Position " + position + extra + " (%)\nWith Odds: " + minOdds + "-" + maxOdds);
        }

        private PlottingViewModel ViewModel { get; }
        private Plotting Plotting { get; }
    }
}
