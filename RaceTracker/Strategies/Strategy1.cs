using MahApps.Metro.Controls;
using RaceTracker.LogicHelpers;
using RaceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaceTracker.Strategies
{
    public class Strategy1
    {        
        public Strategy1(StrategiesViewModel viewModel)
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

            var raceOutcomesDictionary = new Dictionary<DateTime, Tuple<string, double, bool>>(); //Date, race type, odds, winner
            foreach(var row in dataRows)
            {
                if (row.Item4 == 1 )
                {
                    var date = new DateTime(row.Item1.Year, row.Item1.Month, row.Item1.Day, row.Item2.Hour, row.Item2.Minute, row.Item2.Second);
                    if (raceOutcomesDictionary.ContainsKey(date))
                    {
                        //MessageBox.Show("WARNING: Duplicate Outcome found on date '" + date + "'", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Warning);
                        if (row.Item5.ToLower().Trim() == "f")
                        {
                            raceOutcomesDictionary[date] = new Tuple<string, double, bool>(row.Item3, row.Item6, true);
                        }
                    }
                    else
                    {
                        raceOutcomesDictionary.Add(date, new Tuple<string, double, bool>(row.Item3, row.Item6, row.Item5.ToLower().Trim() == "f"));
                    }
                }
            }

            var unorderedRaceOutcomes = new List<Tuple<DateTime, string, double, bool>>();
            foreach (var item in raceOutcomesDictionary)
            {
                unorderedRaceOutcomes.Add(new Tuple<DateTime, string, double, bool>(item.Key, item.Value.Item1, item.Value.Item2, item.Value.Item3));
            }

            this.RaceOutcomes = unorderedRaceOutcomes.OrderBy(x => x.Item1).ToArray();
            this.DailyReports = new Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>>();
        }

        public Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>> DailyReports { get; private set; } // Key: Day. Value: item1: race time, item2: bet, item3: odds, item4: result item5: winnings

        private StrategiesViewModel ViewModel { get; }

        private Tuple<DateTime, string, double, bool>[] RaceOutcomes; // Item1: Date & Time of race (in chronological order). Item2: race type. Item3: odds Item4: Winner

        //Returns a tuple containing the winnings (item1) and the max bet (item2) for each day
        public Dictionary<DateTime, Tuple<double, double>> CalculateDailyProfitForYear()
        {
            double totalWin = double.Parse(this.ViewModel.Model.TotalWinValue.Replace("£", string.Empty));
            double minOdds = double.Parse(this.ViewModel.Model.MinOdds);
            double maxOdds = double.Parse(this.ViewModel.Model.MaxOdds);
            int year = int.Parse(this.ViewModel.Model.Year);
            int raceCutoff = int.Parse(this.ViewModel.Model.RaceCutoff);
            double lossCutoff = int.Parse(this.ViewModel.Model.PriceCutoffValue.Replace("£", string.Empty));

            var dailyProfiles = new Dictionary<DateTime, List<Tuple<DateTime, string, double, bool>>>(); //Key: Day. Value: List of race outcomes for that day in chronological order (time + race type + odds of favourite + winner)
            foreach (var outcome in RaceOutcomes)
            {
                if (outcome.Item1.Year == year)
                {
                    var day = new DateTime(outcome.Item1.Year, outcome.Item1.Month, outcome.Item1.Day);
                    var time = outcome.Item1;

                    if (dailyProfiles.ContainsKey(day))
                    {
                        dailyProfiles[day].Add(new Tuple<DateTime, string, double, bool>(time, outcome.Item2, outcome.Item3, outcome.Item4));
                    }
                    else
                    {
                        dailyProfiles.Add(day, new List<Tuple<DateTime, string, double, bool>> { new Tuple<DateTime, string, double, bool>(time, outcome.Item2, outcome.Item3, outcome.Item4) });
                    }
                }
            }

            var dailyProfits = new Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>>(); // Key: Day. Value: item1: race time, item2: bet, item3: odds, item4: result item5: winnings
            var dailySummary = new Dictionary<DateTime, Tuple<double, double>>(); // Key: Day. Value: item1: winnings, item2: max bet

            foreach (var day in dailyProfiles)
            {
                int raceCount = 0;
                double previousBets = 0;
                bool lost = false;
                foreach (var race in day.Value)
                {
                    var betOutcome = new Tuple<DateTime, double, double, bool, double>(race.Item1, 0, race.Item3, race.Item4, 0);  // race time, bet, odds, result, winnings
                    if ((race.Item2.Trim() == this.ViewModel.Model.RaceType.Trim() || this.ViewModel.Model.RaceType.Trim() == "All") && race.Item3 >= minOdds && race.Item3 <= maxOdds) // If race satisfies user input filters
                    {
                        if (raceCount <= raceCutoff || previousBets >= lossCutoff)
                        {
                            double requiredWinnings = totalWin + previousBets; // Required winnings is whatever we want to win for the day + sum of losing bets up to this point

                            double bet = requiredWinnings / race.Item3; // Required bet is the required winnings divided by the odds of that race

                            if (previousBets + bet > lossCutoff)
                            {
                                lost = true;
                                break;
                            }

                            previousBets += bet;
                            double winnings = race.Item4 ? requiredWinnings : 0;
                            betOutcome = new Tuple<DateTime, double, double, bool, double>(race.Item1, bet, race.Item3, race.Item4, winnings);
                            if (race.Item4) // If we win, we get back our initial bet + winnings and stop betting for that day
                            {
                                break;
                            }

                            raceCount++;
                        }
                        else
                        {
                            lost = true;
                            break;
                        }
                    }

                    if (dailyProfits.ContainsKey(day.Key))
                    {
                        //MessageBox.Show("ERROR: Duplicate daily profits found", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                        dailyProfits[day.Key].Add(betOutcome);
                    }
                    else
                    {
                        dailyProfits.Add(day.Key, new List<Tuple<DateTime, double, double, bool, double>> { betOutcome });
                    }
                }

                double totalWinnings;
                if (lost)
                {
                    totalWinnings = -previousBets;
                }
                else
                {
                    totalWinnings = totalWin;
                }

                if (dailySummary.ContainsKey(day.Key))
                {
                    MessageBox.Show("ERROR: Duplicate daily summaries found", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    dailySummary.Add(day.Key, new Tuple<double, double>(totalWinnings, previousBets));
                }
            }

            this.DailyReports = dailyProfits;

            return dailySummary;
        }

        private int GetMonth(string monthName)
        {
            switch (monthName.ToLower().Substring(0, 3))
            {
                case "jan": return 1;
                case "feb": return 2;
                case "mar": return 3;
                case "apr": return 4;
                case "may": return 5;
                case "jun": return 6;
                case "jul": return 7;
                case "aug": return 8;
                case "sep": return 9;
                case "oct": return 10;
                case "nov": return 11;
                case "dec": return 12;
                default: MessageBox.Show("ERROR: Could not parse date with month '" + monthName + "'", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new Exception();
            }
        }
    }
}
