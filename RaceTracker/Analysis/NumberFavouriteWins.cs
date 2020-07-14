using RaceTracker.LogicHelpers;
using RaceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaceTracker.Analysis
{
    public class NumberFavouriteWins
    {        
        public NumberFavouriteWins(PlottingViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.Plotting = new Plotting();
            var relevantColumns = new List<Tuple<DateTime, DateTime, string, int, string>>();
            var dates = new List<DateTime>();
            var times = new List<DateTime>();
            var raceTypes = new List<string>();
            var positions = new List<int>();
            var expectations = new List<string>();
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
                if (column.Key.ToLower() == "time")
                {
                    foreach (var item in column.Value.Data)
                    {
                        times.Add((DateTime)item);
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
                relevantColumns.Add(new Tuple<DateTime, DateTime, string, int, string>(dates[i], times[i], raceTypes[i], positions[i], expectations[i]));
            }

            this.RelevantColumns = relevantColumns;
        }

        public void CalculateNumberFavouriteWinsVsNumberRaces(int position)
        {
            var raceDayData = new Dictionary<DateTime, Dictionary<DateTime, bool>>(); // key: date, value: time, favourite won
            foreach (var row in this.RelevantColumns)
            {
                if (row.Item4 == position || (row.Item4 <= position && this.ViewModel.Model.UpToAndIncludingPosition && position > 0))
                {
                    bool isFavourite = row.Item5.Trim().ToLower() == "f" ? true : false;
                    if (raceDayData.ContainsKey(row.Item1))
                    {
                        if (raceDayData[row.Item1].ContainsKey(row.Item2))
                        {
                            if(raceDayData[row.Item1][row.Item2])
                            {
                                if (isFavourite)
                                {
                                    MessageBox.Show("WARNING: Duplicate favourites for race: " + row.Item1.Day + "/" + row.Item1.Month + "/" + row.Item1.Year + " " + row.Item2.Hour + ":" + row.Item1.Minute, AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            else
                            {
                                raceDayData[row.Item1][row.Item2] = isFavourite;
                            }
                        }
                        else
                        {
                            raceDayData[row.Item1].Add(row.Item2, isFavourite);
                        }
                    }
                    else
                    {
                        raceDayData.Add(row.Item1, new Dictionary<DateTime, bool> { { row.Item2, isFavourite } });
                    }
                }
            }

            var dataToPlot = new Dictionary<double, Dictionary<double, double>>(); //Key: number races Value: number favourite wins (% of number races), count
            foreach (var raceDay in raceDayData)
            {
                double numberRaces = raceDay.Value.Count;
                double numberFavouriteWins = 0;
                foreach (var race in raceDay.Value)
                {
                    if (race.Value)
                    {
                        numberFavouriteWins++;
                    }
                }

                numberFavouriteWins = 100 * numberFavouriteWins / numberRaces;
                if(dataToPlot.ContainsKey(numberRaces))
                {
                    if (dataToPlot[numberRaces].ContainsKey(numberFavouriteWins))
                    {
                        dataToPlot[numberRaces][numberFavouriteWins] += 1;
                    }
                    else
                    {
                        dataToPlot[numberRaces].Add(numberFavouriteWins, 1);
                    }
                }
                else
                {
                    dataToPlot.Add(numberRaces, new Dictionary<double, double> { { numberFavouriteWins, 1 } });
                }
            }

            bool reset = true;
            foreach(var dataSeries in dataToPlot)
            {
                string seriesName = "Number Races: " + dataSeries.Key;
                var numberWins = new List<double>();
                var counts = new List<double>();
                foreach(var item in dataSeries.Value)
                {
                    numberWins.Add(item.Key);
                    counts.Add(item.Value);
                }

                string extraLabel = this.ViewModel.Model.UpToAndIncludingPosition ? " or Less" : string.Empty;
                this.Plotting.PlotScatter(this.ViewModel.View.NumberFavouriteWinsProbabilityPlot, numberWins, counts, reset, "Number of Favourites Finishing in Position " + position + extraLabel + " % of Number Races", "Count", new List<string>(), seriesName);
                reset = false;
            }
        }

        private Plotting Plotting { get; }

        private PlottingViewModel ViewModel { get; }

        private List<Tuple<DateTime, DateTime, string, int, string>> RelevantColumns { get; } // date, time, race type, position, expectation, odds

    }
}
