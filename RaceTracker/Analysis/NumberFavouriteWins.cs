using RaceTracker.LogicHelpers;
using RaceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            var relevantColumns = new List<Tuple<DateTime, DateTime, string, int, string, double>>();
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
                relevantColumns.Add(new Tuple<DateTime, DateTime, string, int, string, double>(dates[i], times[i], raceTypes[i], positions[i], expectations[i], odds[i]));
            }

            this.RelevantColumns = relevantColumns;
        }

        public double CalculateNumberFavouriteWinsVsNumberRacesFiltered(int position, double minOdds, double maxOdds)
        {
            var raceDayData = new Dictionary<DateTime, Dictionary<DateTime, Tuple<double, string, bool>>>(); // key: date, value: time, odds of favourite, race type, favourite won
            foreach (var row in this.RelevantColumns)
            {
                if (row.Item4 == position || (row.Item4 <= position && this.ViewModel.Model.UpToAndIncludingPosition && position > 0))
                {
                    bool isFavourite = row.Item5.Trim().ToLower() == "f";
                    if (raceDayData.ContainsKey(row.Item1))
                    {
                        if (raceDayData[row.Item1].ContainsKey(row.Item2))
                        {
                            if (raceDayData[row.Item1][row.Item2].Item3)
                            {
                                if (isFavourite)
                                {
                                    //MessageBox.Show("WARNING: Duplicate favourites for race: " + row.Item1.Day + "/" + row.Item1.Month + "/" + row.Item1.Year + " " + row.Item2.Hour + ":" + row.Item1.Minute, AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            else
                            {
                                raceDayData[row.Item1][row.Item2] = new Tuple<double, string, bool>(row.Item6, row.Item3, isFavourite);
                            }
                        }
                        else
                        {
                            raceDayData[row.Item1].Add(row.Item2, new Tuple<double, string, bool>(row.Item6, row.Item3, isFavourite));
                        }
                    }
                    else
                    {
                        raceDayData.Add(row.Item1, new Dictionary<DateTime, Tuple<double, string, bool>> { { row.Item2, new Tuple<double, string, bool>(row.Item6, row.Item3, isFavourite) } });
                    }
                }
            }

            var numberRacesVsPercentageFavouriteWins = new Dictionary<double, List<double>>(); //Key: number races Value: number favourite wins (% of number races)
            foreach (var raceDay in raceDayData)
            {
                double avOdds = 0;
                foreach(var race in raceDay.Value)
                {
                    avOdds += race.Value.Item1 / (double)raceDay.Value.Count;
                }

                bool isWithinOddsRange = avOdds >= minOdds && avOdds <= maxOdds;

                if (isWithinOddsRange)
                {
                    double numberRaces = raceDay.Value.Count;
                    double numberFavouriteWins = 0;
                    foreach (var race in raceDay.Value)
                    {
                        if (race.Value.Item3)
                        {
                            numberFavouriteWins++;
                        }
                    }

                    double percentageFavouriteWins = 100 * numberFavouriteWins / numberRaces;
                    if (numberRacesVsPercentageFavouriteWins.ContainsKey(numberRaces))
                    {
                        numberRacesVsPercentageFavouriteWins[numberRaces].Add(percentageFavouriteWins);
                    }
                    else
                    {
                        numberRacesVsPercentageFavouriteWins.Add(numberRaces, new List<double> { percentageFavouriteWins });
                    }
                }
            }

            var dataToPlot = new List<double>();
            foreach (var numberRaces in numberRacesVsPercentageFavouriteWins)
            {
                foreach (var percentage in numberRaces.Value)
                {
                    dataToPlot.Add(percentage);
                }                
            }

            string extraLabel = this.ViewModel.Model.UpToAndIncludingPosition ? " or Less" : string.Empty;
            return this.Plotting.PlotGaussian(this.ViewModel.View.NumberFavouriteWinsProbabilityFiltersPlot, dataToPlot, "Number of Favourites Finishing in Position " + position + extraLabel + " (% of Number Races)", "Count", "Average Odds: " + minOdds + " - " + maxOdds, "%", this.ViewModel.Model.ResetIndividual);
        }

        public void CalculateNumberFavouriteWinsVsNumberRaces(int position, int minNumberRaces, int maxNumberRaces)
        {
            var raceDayData = new Dictionary<DateTime, Dictionary<DateTime, bool>>(); // key: date, value: time, favourite won
            foreach (var row in this.RelevantColumns)
            {
                if (row.Item4 == position || (row.Item4 <= position && this.ViewModel.Model.UpToAndIncludingPosition && position > 0))
                {
                    bool isFavourite = row.Item5.Trim().ToLower() == "f";
                    if (raceDayData.ContainsKey(row.Item1))
                    {
                        if (raceDayData[row.Item1].ContainsKey(row.Item2))
                        {
                            if(raceDayData[row.Item1][row.Item2])
                            {
                                if (isFavourite)
                                {
                                    //MessageBox.Show("WARNING: Duplicate favourites for race: " + row.Item1.Day + "/" + row.Item1.Month + "/" + row.Item1.Year + " " + row.Item2.Hour + ":" + row.Item1.Minute, AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
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

            var numberRacesVsPercentageFavouriteWins = new Dictionary<double, List<double>>(); //Key: number races Value: number favourite wins (% of number races)
            foreach(var raceDay in raceDayData)
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

                double percentageFavouriteWins = 100 * numberFavouriteWins / numberRaces;
                if(numberRacesVsPercentageFavouriteWins.ContainsKey(numberRaces))
                {
                    numberRacesVsPercentageFavouriteWins[numberRaces].Add(percentageFavouriteWins);
                }
                else
                {
                    numberRacesVsPercentageFavouriteWins.Add(numberRaces, new List<double> { percentageFavouriteWins });
                }
            }

            var dataToPlot = new List<double>();
            foreach (var numberRaces in numberRacesVsPercentageFavouriteWins)
            {
                if (numberRaces.Key >= minNumberRaces && numberRaces.Key <= maxNumberRaces)
                {
                    foreach (var percentage in numberRaces.Value)
                    {
                        dataToPlot.Add(percentage);
                    }
                }
            }

            string extraLabel = this.ViewModel.Model.UpToAndIncludingPosition ? " or Less" : string.Empty;
            this.Plotting.PlotGaussian(this.ViewModel.View.NumberFavouriteWinsProbabilityPlot, dataToPlot, "Number of Favourites Finishing in Position " + position + extraLabel + " (% of Number Races)", "Count", "Number Races: " + minNumberRaces + " - " + maxNumberRaces, "%", this.ViewModel.Model.ResetIndividual);

            //var dataToPlot = new Dictionary<double, Dictionary<double, double>>(); //Key: number races Value: number favourite wins (% of number races), count
            //foreach (var raceDay in raceDayData)
            //{
            //    double numberRaces = raceDay.Value.Count;
            //    double numberFavouriteWins = 0;
            //    foreach (var race in raceDay.Value)
            //    {
            //        if (race.Value)
            //        {
            //            numberFavouriteWins++;
            //        }
            //    }

            //    numberFavouriteWins = 100 * numberFavouriteWins / numberRaces;
            //    if(dataToPlot.ContainsKey(numberRaces))
            //    {
            //        if (dataToPlot[numberRaces].ContainsKey(numberFavouriteWins))
            //        {
            //            dataToPlot[numberRaces][numberFavouriteWins] += 1;
            //        }
            //        else
            //        {
            //            dataToPlot[numberRaces].Add(numberFavouriteWins, 1);
            //        }
            //    }
            //    else
            //    {
            //        dataToPlot.Add(numberRaces, new Dictionary<double, double> { { numberFavouriteWins, 1 } });
            //    }
            //}

            //bool reset = true;
            //foreach(var dataSeries in dataToPlot)
            //{
            //    string seriesName = "Number Races: " + dataSeries.Key;
            //    var numberWins = new List<double>();
            //    var counts = new List<double>();
            //    foreach(var item in dataSeries.Value)
            //    {
            //        numberWins.Add(item.Key);
            //        counts.Add(item.Value);
            //    }

            //    string extraLabel = this.ViewModel.Model.UpToAndIncludingPosition ? " or Less" : string.Empty;
            //    this.Plotting.PlotScatter(this.ViewModel.View.NumberFavouriteWinsProbabilityPlot, numberWins, counts, reset, "Number of Favourites Finishing in Position " + position + extraLabel + " % of Number Races", "Count", new List<string>(), seriesName);
            //    reset = false;
            //}
        }

        private Plotting Plotting { get; }

        private PlottingViewModel ViewModel { get; }

        private List<Tuple<DateTime, DateTime, string, int, string, double>> RelevantColumns { get; } // date, time, race type, position, expectation, odds

    }
}
