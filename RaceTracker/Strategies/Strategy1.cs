using MahApps.Metro.Controls;
using RaceTracker.LogicHelpers;
using RaceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

            var dailyRaces = new Dictionary<DateTime, Dictionary<DateTime, List<Tuple<string, int, string, double>>>>(); //Key: Date. Value: dictionary of race type, position, expectation and odds for each race time
            foreach (var row in dataRows)
            {
                if (dailyRaces.ContainsKey(row.Item1))
                {
                    if(dailyRaces[row.Item1].ContainsKey(row.Item2))
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
            var returnValue = new Dictionary<DateTime, Tuple<double, double>>();
            // Filter by day
            foreach (var day in this.DailyRaceOutcomes)
            {
                // Filter by race
                var raceOutcomes = new List<Tuple<DateTime, double, bool, bool>>(); // race time, favourite odds, race bet on, race won
                foreach(var race in day.Value)
                {
                    //Filter by race result to get a list of results for that race
                    var raceResultList = new List<Tuple<DateTime, string, int, string, double>>(); // race time, race type, position, expectation, odds 
                    foreach (var raceResult in race.Value)
                    {
                        raceResultList.Add(new Tuple<DateTime, string, int, string, double>(race.Key, raceResult.Item1, raceResult.Item2, raceResult.Item3, raceResult.Item4));
                    }

                    bool outcomeFound = false;
                    var raceTime = new DateTime();
                    bool raceWon = false;
                    bool betOnRace = false;
                    foreach (var raceResult in raceResultList)
                    {
                        raceTime = raceResult.Item1;
                        // Find the favourite and note its odds, whether or not it won, and whether to bet on it (in principle) based on user input filters (race type + odds)                         
                        if (raceResult.Item4.ToLower().Trim() == "f")
                        {
                            //if (day.Key.Year == 2018 && day.Key.Month == 8 && day.Key.Day == 17)
                            //{
                            //    MessageBox.Show("outside loop" + raceResult.Item1 + " " + raceResult.Item2 + " " + raceResult.Item3 + " " + raceResult.Item4 + " " + raceResult.Item5);
                            //}

                            // Check position
                            if (raceResult.Item3 == 1)
                            {
                                //if (day.Key.Year == 2018 && day.Key.Month == 8 && day.Key.Day == 17)
                                //{
                                //    MessageBox.Show(raceResult.Item1 + " " + raceResult.Item2 + " " + raceResult.Item3 + " " + raceResult.Item4 + " " + raceResult.Item5);
                                //}

                                raceWon = true;
                            }

                            // Filter on race type
                            if (raceResult.Item2.ToLower().Trim() == this.ViewModel.Model.RaceType.ToLower().Trim() || this.ViewModel.Model.RaceType.ToLower().Trim() == "all")
                            {
                                // Filter on odds
                                if (raceResult.Item5 >= double.Parse(this.ViewModel.Model.MinOdds) && raceResult.Item5 <= double.Parse(this.ViewModel.Model.MaxOdds))
                                {
                                    // If all the filters passed, the race is worth betting on
                                    betOnRace = true;
                                }
                            }

                            raceOutcomes.Add(new Tuple<DateTime, double, bool, bool>(raceResult.Item1, raceResult.Item5, betOnRace, raceWon));
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

        // Input: date, list of tuples of race time, odds, bet on race, race won (for that day). Output: total bet and winnings
        private Tuple<double, double> CalculateDailyWinnings(DateTime day, List<Tuple<DateTime, double, bool, bool>> raceOutcomes)
        {
            var raceOutcomesArray = raceOutcomes.OrderBy(x => x.Item1).ToArray();
            double totalWinningsRequirement = double.Parse(this.ViewModel.Model.TotalWinValue.Replace("£", string.Empty));
            double dailyPot = double.Parse(this.ViewModel.Model.DailyPotValue.Replace("£", string.Empty));
            double totalBet = 0;
            var dailyBettingProfile = new List<Tuple<DateTime, double, double, bool, double>>(); // race time, bet, odds, result, net winnings for that race
            int numberRacesCutoff = int.Parse(this.ViewModel.Model.RaceCutoff);
            double lossCutoff = double.Parse(this.ViewModel.Model.PriceCutoffValue.Replace("£", string.Empty));
            int numberRacesPassed = 0;
            bool raceWon = false;
            double actualWinnings = 0;
            foreach (var raceOutcome in raceOutcomesArray)
            {
                double odds = raceOutcome.Item2;
                double betRequirement = (totalWinningsRequirement + totalBet) / odds;
                if (raceOutcome.Item3)
                {
                    // If the number of races cutoff or the loss cutoff is reached, we stop betting
                    if (numberRacesPassed >= numberRacesCutoff || totalBet + betRequirement > lossCutoff)
                    {
                        break;
                    }

                    double totalBetMinusLastBet = totalBet;
                    // if the race is a betting candidate, add the bet to the total bet to be taken into account for the next race in the event of a loss
                    totalBet += betRequirement;
                    double roundedBetRequirement = Math.Round(betRequirement, 2);

                    // if the race is won, we stop betting and return the winnings
                    if (raceOutcome.Item4)
                    {
                        dailyBettingProfile.Add(new Tuple<DateTime, double, double, bool, double>(raceOutcome.Item1, roundedBetRequirement, odds, true, Math.Round((roundedBetRequirement * odds) - totalBetMinusLastBet, 2)));
                        raceWon = true;
                        actualWinnings = Math.Round((roundedBetRequirement * odds) - totalBetMinusLastBet, 2);
                        break;
                    }
                    else
                    {
                        dailyBettingProfile.Add(new Tuple<DateTime, double, double, bool, double>(raceOutcome.Item1, roundedBetRequirement, odds, false, 0));
                    }

                    numberRacesPassed++;
                }
                else
                {
                    dailyBettingProfile.Add(new Tuple<DateTime, double, double, bool, double>(raceOutcome.Item1, 0, odds, raceOutcome.Item4, 0));
                }
            }

            if (this.DailyReports.ContainsKey(day))
            {
                MessageBox.Show("ERROR: Duplicate daily reports found for date '" + day + "'", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                this.DailyReports.Add(day, dailyBettingProfile);
            }

            double roundedTotalBet = Math.Round(totalBet, 2);
            if (raceWon)
            {
                return new Tuple<double, double>(roundedTotalBet, actualWinnings);
            }
            else
            {
                return new Tuple<double, double>(roundedTotalBet, 0);
            }
        }
    }
}
