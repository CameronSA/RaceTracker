using RaceTracker.LogicHelpers;
using RaceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace RaceTracker.Analysis
{
    public class ProbabilityOfOddsProfile
    {
        public ProbabilityOfOddsProfile(PlottingViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.Plotting = new Plotting();
            this.ProbabilityOfFavouriteWithOddsWinningRaceTypeSeriesNames = new List<string>();
            this.ProbabilityOfFavouriteWithOddsWinningRaceTypeSeriesValues = new List<double[]>();
        }

        private List<double[]> ProbabilityOfFavouriteWithOddsWinningRaceTypeSeriesValues { get; }

        private List<string> ProbabilityOfFavouriteWithOddsWinningRaceTypeSeriesNames { get; }

        private PlottingViewModel ViewModel { get; }

        private Plotting Plotting { get; }

        public void PlotOddsProfile()
        {
            if (int.TryParse(this.ViewModel.Model.Position, out int position) && position > 0)
            {
                if (double.TryParse(this.ViewModel.Model.MinOdds, out double minOdds) && double.TryParse(this.ViewModel.Model.MaxOdds, out double maxOdds))
                {
                    this.ProbabilityOfFavouritesHavingOddRange(minOdds, maxOdds);
                    this.ProbabilityOfFavouriteWithOddsWinningRaceType(position, minOdds, maxOdds);
                    this.ProbabilityOfOddRangesWinningVsTime(position, minOdds, maxOdds);
                    if (int.TryParse(this.ViewModel.Model.NumberBins, out int numberBins) && Math.Abs(numberBins) > 0)
                    {
                        this.FavouriteOddsVsTime(this.ViewModel.Model.ResetIndividual, Math.Abs(numberBins));
                    }
                    else
                    {
                        MessageBox.Show("ERROR: Number of bins must be an integer!", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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

        private void ProbabilityOfFavouritesHavingOddRange(double minOdds, double maxOdds)
        {
            var expectations = new List<string>();
            var odds = new List<double>();
            var oddsRanges = new List<Tuple<double, double>>();
            double delimiter = (maxOdds - minOdds) / 20.0;
            for (double i = minOdds; i < maxOdds; i += delimiter)
            {
                oddsRanges.Add(new Tuple<double, double>(Math.Round(i, 2), Math.Round(i + delimiter, 2)));
            }

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
            }

            var expectationVsOdds = new List<Tuple<string, double>>();
            for (int i = 0; i < expectations.Count; i++)
            {
                expectationVsOdds.Add(new Tuple<string, double>(expectations[i], odds[i]));
            }

            var favouriteOdds = new List<double>();
            foreach(var item in expectationVsOdds)
            {
                if (item.Item1.ToLower().Trim() == "f")
                {
                    favouriteOdds.Add(item.Item2);
                }
            }

            var oddsRangeProbability = new Dictionary<string, double>();
            foreach(var oddsrange in oddsRanges)
            {
                double count = 0;
                foreach (var oddsValue in favouriteOdds)
                {
                    if (oddsValue >= oddsrange.Item1 && oddsValue < oddsrange.Item2)
                    {
                        count++;
                    }
                }

                string key = oddsrange.Item1 + " - " + oddsrange.Item2;
                if(oddsRangeProbability.ContainsKey(key))
                {
                    oddsRangeProbability[key] = oddsRangeProbability[key] + count;
                }
                else
                {
                    oddsRangeProbability.Add(key, 100 * (count / (double)favouriteOdds.Count));
                }
            }

            var oddsRangeList = new List<string>();
            var probability = new List<double>();

            foreach(var item in oddsRangeProbability)
            {
                oddsRangeList.Add(item.Key);
                probability.Add(item.Value);
            }

            this.Plotting.PlotBar(this.ViewModel.View.ProbabilityOfFavouritesHavingOddRange, oddsRangeList, probability, true, string.Empty, "Probability of Favourite Within Odds Range (%)", rotation: 0);
        }

        private void ProbabilityOfFavouriteWithOddsWinningRaceType(int position, double minOdds, double maxOdds)
        {
            var expectations = new List<string>();
            var odds = new List<double>();
            var raceTypes = new List<string>();
            var positions = new List<int>();
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
                else if (column.Key.ToLower() == "race type")
                {
                    foreach (var item in column.Value.Data)
                    {
                        raceTypes.Add((string)item);
                    }
                }
                else if(column.Key.ToLower() == "position")
                {
                    foreach(var item in column.Value.Data)
                    {
                        positions.Add((int)item);
                    }
                }
            }

            var rows = new List<Tuple<string, double, string, int>>();
            for (int i = 0; i < expectations.Count; i++)
            {
                rows.Add(new Tuple<string, double, string, int>(expectations[i], odds[i], raceTypes[i], positions[i]));
            }

            var oddsValuesPerRaceType = new Dictionary<string, List<Tuple<double, int>>>();
            foreach (var row in rows)
            {
                if (row.Item1.ToLower().Trim() == "f")
                {
                    if (oddsValuesPerRaceType.ContainsKey(row.Item3))
                    {
                        oddsValuesPerRaceType[row.Item3].Add(new Tuple<double, int>(row.Item2, row.Item4));
                    }
                    else
                    {
                        oddsValuesPerRaceType.Add(row.Item3, new List<Tuple<double, int>> { new Tuple<double, int>(row.Item2, row.Item4) });
                    }
                }
            }

            var probabilityOddsPerRaceType = new Dictionary<string, double>();
            foreach(var raceType in oddsValuesPerRaceType)
            {
                double numberOdds = 0;
                double numberOddsWithinRange = 0;
                foreach (var item in raceType.Value)
                {
                    if (item.Item2 == position || (item.Item2 <= position && this.ViewModel.Model.UpToAndIncludingPosition && item.Item2 > 0))
                    {
                        numberOdds++;
                        if (item.Item1 >= minOdds && item.Item1 <= maxOdds)
                        {
                            numberOddsWithinRange++;
                        }
                    }
                }

                double probability = 100 * (numberOddsWithinRange / numberOdds);

                if (probabilityOddsPerRaceType.ContainsKey(raceType.Key))
                {
                    MessageBox.Show("ERROR: Duplicate race types found", AppSettings.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    probabilityOddsPerRaceType.Add(raceType.Key, probability);
                }
            }

            var raceTypesList = new List<string>();
            var probabilities = new List<double>();
            foreach(var item in probabilityOddsPerRaceType)
            {
                raceTypesList.Add(item.Key);
                probabilities.Add(item.Value);
            }

            string yLabel;
            if(this.ViewModel.Model.UpToAndIncludingPosition)
            {
                yLabel = "Probability of Favourite finishing Up To Given Position\nPossessing Given Odds Range";
            }
            else
            {
                yLabel = "Probability of Favourite Finishing in Given Position (%)\nPossessing Given Odds Range";
            }

            string seriesName;
            if (this.ViewModel.Model.UpToAndIncludingPosition)
            {
                seriesName = "Odds Range: " + minOdds + " - " + maxOdds + ". Position " + position + " or Less";
            }
            else
            {
                seriesName = "Odds Range: " + minOdds + " - " + maxOdds + ". Position " + position;
            }

            this.Plotting.PlotBarGroups(this.ViewModel.View.ProbabilityOfFavouriteWithOddsWinningRaceType, raceTypesList, probabilities, this.ProbabilityOfFavouriteWithOddsWinningRaceTypeSeriesValues,this.ViewModel.Model.ResetIndividual, "Race Type", yLabel, this.ProbabilityOfFavouriteWithOddsWinningRaceTypeSeriesNames, seriesName, yMin: 0, yMax: 100);
        }

        private void ProbabilityOfOddRangesWinningVsTime(int position, double minOdds, double maxOdds)
        {
            var expectations = new List<string>();
            var dates = new List<DateTime>();
            var times = new List<DateTime>();
            var odds = new List<double>();
            var raceTypes = new List<string>();
            var positions = new List<int>();
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
                else if (column.Key.ToLower() == "expectation")
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
                else if (column.Key.ToLower() == "race type")
                {
                    foreach (var item in column.Value.Data)
                    {
                        raceTypes.Add((string)item);
                    }
                }
                else if (column.Key.ToLower() == "position")
                {
                    foreach (var item in column.Value.Data)
                    {
                        positions.Add((int)item);
                    }
                }
            }

            var rows = new List<Tuple<DateTime, DateTime, string, double, string, int>>();
            for (int i = 0; i < dates.Count; i++)
            {
                rows.Add(new Tuple<DateTime, DateTime, string, double, string, int>(dates[i], times[i], expectations[i], odds[i], raceTypes[i], positions[i]));
            }

            //Key: race time, value: expectations, odds, race type, positions
            var raceDetails = new Dictionary<string, List<Tuple<string, double, string, int>>>();
            foreach (var row in rows)
            {
                var details = new Tuple<string, double, string, int>(row.Item3, row.Item4, row.Item5, row.Item6);
                string key = row.Item1 + "]" + row.Item2;
                if (raceDetails.ContainsKey(key))
                {
                    raceDetails[key].Add(details);
                }
                else
                {
                    raceDetails.Add(key, new List<Tuple<string, double, string, int>> { details });
                }
            }

            var raceResultOddsAndType = new Dictionary<string, Tuple<double, string>>();
            foreach (var race in raceDetails)
            {
                foreach (var result in race.Value)
                {
                    if (result.Item1.ToLower().Trim() == "f")
                    {
                        if (result.Item4 == position || (result.Item4 <= position && this.ViewModel.Model.UpToAndIncludingPosition && result.Item4 > 0))
                        {
                            if(raceResultOddsAndType.ContainsKey(race.Key))
                            {
                                raceResultOddsAndType[race.Key] = new Tuple<double, string>(result.Item2, result.Item3);
                            }
                            else
                            {
                                raceResultOddsAndType.Add(race.Key, new Tuple<double, string>(result.Item2, result.Item3));
                            }
                        }
                    }
                }
            }

            var raceDayOddsCountAndRaceType = new Dictionary<DateTime, List<Tuple<bool, string>>>(); //Tuple: matched odds, race type
            foreach (var race in raceResultOddsAndType)
            {
                bool oddsMatched = false;
                if (race.Value.Item1 >= minOdds && race.Value.Item1 <= maxOdds)
                {
                    oddsMatched = true;
                }

                var dateString = race.Key.Split(new char[] { ']' })[0];
                var date = DateTime.Parse(dateString);
                if (raceDayOddsCountAndRaceType.ContainsKey(date))
                {
                    raceDayOddsCountAndRaceType[date].Add(new Tuple<bool, string>(oddsMatched, race.Value.Item2));
                }
                else
                {
                    raceDayOddsCountAndRaceType.Add(date, new List<Tuple<bool, string>> { new Tuple<bool, string>(oddsMatched, race.Value.Item2) });
                }
            }

            var dayOddsProbabilityAndRaceType = new Dictionary<DateTime, Tuple<double, string>>();
            foreach(var day in raceDayOddsCountAndRaceType)
            {
                double matchedOdds = 0;
                double totalOdds = 0;
                string raceType = string.Empty;
                foreach(var item in day.Value)
                {
                    if(item.Item1)
                    {
                        matchedOdds++;
                    }

                    totalOdds++;

                    if(string.IsNullOrEmpty(raceType))
                    {
                        raceType = item.Item2;
                    }
                }

                if(dayOddsProbabilityAndRaceType.ContainsKey(day.Key))
                {
                    dayOddsProbabilityAndRaceType[day.Key] = new Tuple<double, string>(100 * (matchedOdds / totalOdds), raceType);
                }
                else
                {
                    dayOddsProbabilityAndRaceType.Add(day.Key, new Tuple<double, string>(100 * (matchedOdds / totalOdds), raceType));
                }
            }

            var dateVsProbabilitySplitOnRaceType = new Dictionary<string, List<Tuple<DateTime, double>>>();            
            foreach (var day in dayOddsProbabilityAndRaceType) // Date, probability of odds, race type
            {
                if (dateVsProbabilitySplitOnRaceType.ContainsKey(day.Value.Item2))
                {
                    dateVsProbabilitySplitOnRaceType[day.Value.Item2].Add(new Tuple<DateTime, double>(day.Key, day.Value.Item1));
                }
                else
                {
                    dateVsProbabilitySplitOnRaceType.Add(day.Value.Item2, new List<Tuple<DateTime, double>> { new Tuple<DateTime, double>(day.Key, day.Value.Item1) });
                }
            }

            string yLabel;
            if (this.ViewModel.Model.UpToAndIncludingPosition)
            {
                yLabel   = "Probability of Favourite Finishing Up To Position\nBeing Within Given Odds Range (%)";                
            }
            else
            {
                yLabel = "Probability of Favourite Finishing in Given Position\nBeing Within Given Odds Range (%)";
            }

            var seriesList = new List<string>();
            bool reset = true;
            foreach (var raceType in dateVsProbabilitySplitOnRaceType)
            {
                var dateList = new List<DateTime>();
                var probabilities = new List<double>();
                foreach (var item in raceType.Value)
                {
                    dateList.Add(item.Item1);
                    probabilities.Add(item.Item2);
                }

                string seriesName;
                if (this.ViewModel.Model.UpToAndIncludingPosition)
                {
                    seriesName = raceType.Key + ". Up To Position: " + position + ". Odds range: " + minOdds + " - " + maxOdds;
                }
                else
                {
                    seriesName = raceType.Key + ". Position: " + position + ". Odds range: " + minOdds + " - " + maxOdds;
                }

                this.Plotting.PlotTimeSeries(this.ViewModel.View.ProbabilityOfOddRangesWinningVsTime, dateList, probabilities, reset, string.Empty, yLabel, seriesList, seriesName);
                reset = false;
            }
        }

        private void FavouriteOddsVsTime(bool reset, int numberBins)
        {
            var rows = new List<Tuple<DateTime, DateTime, double, string>>();
            var expectations = new List<string>();
            var dates = new List<DateTime>();
            var times = new List<DateTime>();
            var odds = new List<double>();
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
                else if (column.Key.ToLower() == "expectation")
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
            }

            for (int i = 0; i < dates.Count; i++)
            {
                rows.Add(new Tuple<DateTime, DateTime, double, string>(dates[i], times[i], odds[i], expectations[i]));
            }

            var oddsList = new List<double>();
            var dateList = new List<DateTime>();
            var oddsVsTime = new List<Tuple<DateTime, double>>();
            foreach(var row in rows)
            {
                if (row.Item4.Trim().ToLower() == "f")
                {
                    var date = new DateTime(row.Item1.Year, row.Item1.Month, row.Item1.Day, row.Item2.Hour, row.Item2.Minute, row.Item2.Second);                    
                    oddsList.Add(row.Item3);
                    dateList.Add(date);
                    oddsVsTime.Add(new Tuple<DateTime, double>(date, row.Item3));
                }
            }

            if (numberBins < oddsVsTime.Count)
            {
                var binnedData = CommonAnalyses.BinData(oddsVsTime, numberBins);
                this.Plotting.PlotTimeSeriesError(this.ViewModel.View.FavouriteOddsVsTime, binnedData.Item1, binnedData.Item2, binnedData.Item3, reset, string.Empty, "Average Favourite Odds", new List<string>(), "# Bins: " + numberBins + "\nBin Size: " + binnedData.Item4);
            }
            else
            {
                this.Plotting.PlotTimeSeries(this.ViewModel.View.FavouriteOddsVsTime, dateList, oddsList, reset, string.Empty, "Favourite Odds", new List<string>(), "Unbinned");
            }
        }
    }
}
