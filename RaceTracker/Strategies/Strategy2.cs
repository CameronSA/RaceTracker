using RaceTracker.Analysis;
using RaceTracker.LogicHelpers;
using RaceTracker.ViewModels;
using RaceTracker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaceTracker.Strategies
{
    public class Strategy2
    {
        public Strategy2(StrategiesViewModel viewModel)
        {
            this.ViewModel = viewModel;
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
                else if (column.Key.ToLower() == "time")
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

            var dataRows = new List<Tuple<DateTime, DateTime, string, int, string, double>>(); //Date, Time, Race type, Position, Expectation, Odds
            for (int i = 0; i < dates.Count; i++)
            {
                dataRows.Add(new Tuple<DateTime, DateTime, string, int, string, double>(dates[i], times[i], raceTypes[i], positions[i], expectations[i], odds[i]));
            }

            var dailyRaces = new Dictionary<DateTime, Dictionary<DateTime, List<Tuple<string, int, string, double>>>>(); //Key: Date. Value: dictionary of race type, position, expectation and odds for each race time
            foreach (var row in dataRows)
            {
                if (dailyRaces.ContainsKey(row.Item1))
                {
                    if (dailyRaces[row.Item1].ContainsKey(row.Item2))
                    {
                        dailyRaces[row.Item1][row.Item2].Add(new Tuple<string, int, string, double>(row.Item3, row.Item4, row.Item5, row.Item6));
                    }
                    else
                    {
                        dailyRaces[row.Item1].Add(row.Item2, new List<Tuple<string, int, string, double>> { new Tuple<string, int, string, double>(row.Item3, row.Item4, row.Item5, row.Item6) });
                    }
                }
                else
                {
                    dailyRaces.Add(row.Item1, new Dictionary<DateTime, List<Tuple<string, int, string, double>>> { { row.Item2, new List<Tuple<string, int, string, double>> { new Tuple<string, int, string, double>(row.Item3, row.Item4, row.Item5, row.Item6) } } });
                }
            }

            this.DailyRaceOutcomes = dailyRaces;
            this.DailyReports = new Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>>();
        }

        public Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>> DailyReports { get; private set; } // Key: Day. Value: item1: race time, item2: bet, item3: odds, item4: result item5: net winnings for that race

        private StrategiesViewModel ViewModel { get; }

        private Dictionary<DateTime, Dictionary<DateTime, List<Tuple<string, int, string, double>>>> DailyRaceOutcomes; //Key: Date. Value: dictionary of race type, position, expectation and odds for each race time

        //Returns a tuple containing the total bet (item1) and the winnings (item2) for each day
        public Dictionary<DateTime, Tuple<double, double>> CalculateDailyProfits()
        {
            double minOdds = double.Parse(this.ViewModel.Model.MinOdds);
            double maxOdds = double.Parse(this.ViewModel.Model.MaxOdds);
            var returnValue = new Dictionary<DateTime, Tuple<double, double>>();
            var expectedNumberWins = new NumberFavouriteWins(new PlottingViewModel(new PlottingView())).CalculateNumberFavouriteWinsVsNumberRacesFiltered(1, minOdds, maxOdds);

            // Filter by day
            foreach (var day in this.DailyRaceOutcomes)
            {
                // Filter by race
                var raceOutcomes = new List<Tuple<DateTime, double, bool, bool>>(); // race time, favourite odds, race bet on, race won
                var odds = new List<double>();
                var raceTypes = new List<string>();
                foreach (var race in day.Value)
                {
                    // Filter by race result to get a list of results for that race
                    var raceResultList = new List<Tuple<DateTime, string, int, string, double>>(); // race time, race type, position, expectation, odds 
                    foreach (var raceResult in race.Value)
                    {
                        raceResultList.Add(new Tuple<DateTime, string, int, string, double>(race.Key, raceResult.Item1, raceResult.Item2, raceResult.Item3, raceResult.Item4));
                    }

                    bool outcomeFound = false;
                    var raceTime = new DateTime();
                    bool raceWon = false;
                    foreach (var raceResult in raceResultList)
                    {
                        raceTime = raceResult.Item1;
                        // Find the favourite and note its odds, whether or not it won, and whether to bet on it (in principle) based on user input filters (race type + odds)                         
                        if (raceResult.Item4.ToLower().Trim() == "f")
                        {
                            odds.Add(raceResult.Item5);
                            raceTypes.Add(raceResult.Item2.ToLower().Trim());

                            // Check position
                            if (raceResult.Item3 == 1)
                            {
                                raceWon = true;
                            }

                            raceOutcomes.Add(new Tuple<DateTime, double, bool, bool>(raceResult.Item1, raceResult.Item5, true, raceWon));
                            outcomeFound = true;
                            break;
                        }
                    }

                    if (!outcomeFound)
                    {
                        // If there is no favourite (joint favourite instead for example), don't bet
                        raceOutcomes.Add(new Tuple<DateTime, double, bool, bool>(raceTime, 0, false, false)); ;
                    }
                }

                double averageOdds = odds.Average();
                // calc mode race type

                // Calculate winnings based on daily race outcomes
                var bettingOutcome = this.CalculateDailyWinnings(day.Key, raceOutcomes);
                if (returnValue.ContainsKey(day.Key))
                {
                    MessageBox.Show("ERROR: Duplicate daily profits found for date '" + day.Key + "'", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    returnValue.Add(day.Key, bettingOutcome);
                }
            }

            return returnValue;
        }
    }
}
