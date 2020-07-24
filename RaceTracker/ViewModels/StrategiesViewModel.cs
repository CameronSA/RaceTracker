using RaceTracker.Commands;
using RaceTracker.LogicHelpers;
using RaceTracker.Models;
using RaceTracker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RaceTracker.ViewModels
{
    public class StrategiesViewModel
    {
        public StrategiesViewModel(StrategiesView view)
        {
            var raceTypes = this.GetRaceTypes();
            var months = this.GetMonths();

            this.Model = new StrategiesModel(this)
            {
                Bank = "5000",
                DailyPotPercentage = "10",
                TotalWinPercentage = "10",
                PriceCutoffPercentage = "100",
                RaceCutoff = "5",
                MinOdds = "0",
                MaxOdds = "3",
                RaceTypes = raceTypes,
                RaceType = raceTypes[0],
                MonthlyDays = new List<string>(),
                Year = DateTime.Today.Year.ToString(),
                PercentageOfExpectedWins = "80",
                Date = new DateTime(2019, 01, 01)
            };

            this.Model.Months = months;
            this.Model.Month = months[0];
            this.Model.MonthlyDay = this.Model.MonthlyDays[0];

            this.View = view;
            this.Command = new StrategiesCommand(this);
            this.FixedYear = -1;
        }

        public StrategiesModel Model { get; }
        public StrategiesView View { get; }
        public StrategiesCommand Command { get; }
        public int FixedYear { get; set; }

        public void DisplayStrategy1DailyBreakdown()
        {
            if (FixedYear > 0)
            {
                // Key: Day. Value: item1: race time, item2: bet, item3: odds, item4: result item5: net winnings for that race
                Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>> reports = this.Command.Strategy1DailyReports;

                foreach (var report in reports)
                {
                    if (report.Key.Year == FixedYear && report.Key.Month == UnitConversions.MonthNumber(this.Model.Month) && report.Key.Day == int.Parse(this.Model.MonthlyDay))
                    {
                        this.Model.Strategy1DailyBreakdownTitle = "Daily Breakdown for " + report.Key.Day + " " + UnitConversions.MonthName(report.Key.Month) + " " + report.Key.Year;
                        this.Model.Strategy1DailyBreakdown = "Time\tBet (£)\tOdds\tResult\tWinnings (£)\n";
                        this.Model.Strategy1DailyBreakdown += "================================\n";
                        foreach(var line in report.Value)
                        {
                            string win = line.Item4 ? "Win" : "Loss";
                            this.Model.Strategy1DailyBreakdown += Formatting.EditStringLength(line.Item1.Hour.ToString(), 2) + ":" + Formatting.EditStringLength(line.Item1.Minute.ToString(), 2) + "\t" + line.Item2 + "\t" + Math.Round(line.Item3, 2) + "\t" + win + "\t" + line.Item5 + "\n";
                        }

                        break;
                    }
                }
            }
        }

        public void DisplayStrategy2DailyBreakdown()
        {
            try
            {
                // Key: Day. Value: item1: race time, item2: bet, item3: odds, item4: result item5: gross winnings for that race, including initial stakes
                Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>> reports = this.Command.Strategy2DailyReports;

                foreach (var report in reports)
                {
                    if (report.Key.Year == this.Model.Date.Year && report.Key.Month == this.Model.Date.Month && report.Key.Day == this.Model.Date.Day)
                    {
                        double totalGrossWinnings = 0;
                        double totalBet = 0;
                        this.Model.Strategy2DailyBreakdownTitle = "Daily Breakdown for " + report.Key.Day + " " + UnitConversions.MonthName(report.Key.Month) + " " + report.Key.Year;
                        this.Model.Strategy2DailyBreakdown = "Time\tBet (£)\tOdds\tResult\tGross Winnings (£)\n";
                        this.Model.Strategy2DailyBreakdown += "================================\n";
                        foreach (var line in report.Value)
                        {
                            totalGrossWinnings += line.Item5;
                            totalBet += line.Item2;
                            string win = line.Item4 ? "Win" : "Loss";
                            this.Model.Strategy2DailyBreakdown += Formatting.EditStringLength(line.Item1.Hour.ToString(), 2) + ":" + Formatting.EditStringLength(line.Item1.Minute.ToString(), 2) + "\t" + Math.Round(line.Item2, 2) + "\t" + Math.Round(line.Item3, 2) + "\t" + win + "\t" + Math.Round(line.Item5, 2) + "\n";
                        }

                        double totalNetWinnings = totalGrossWinnings - totalBet;
                        this.Model.Strategy2DailyBreakdown += "Total Bet: " + Math.Round(totalBet, 2) + "\nGross Winnings: " + Math.Round(totalGrossWinnings, 2) + "\nNet Winnings: " + Math.Round(totalNetWinnings, 2);
                        break;
                    }
                }
            }
            catch
            { }
        }

        public bool VerifyInputs()
        {
            if (!double.TryParse(this.Model.Bank, out _))
            {
                MessageBox.Show("ERROR: Bank must have a numeric value!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (!double.TryParse(this.Model.DailyPotPercentage, out _))
            {
                MessageBox.Show("ERROR: Daily Pot must have a numeric value!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (!double.TryParse(this.Model.TotalWinPercentage, out _))
            {
                MessageBox.Show("ERROR: Total Win must have a numeric value!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (!double.TryParse(this.Model.PriceCutoffPercentage, out _))
            {
                MessageBox.Show("ERROR: Price Cutoff must have a numeric value!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (!int.TryParse(this.Model.RaceCutoff, out _))
            {
                MessageBox.Show("ERROR: Race Cutoff must have an integer value!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (!double.TryParse(this.Model.MinOdds, out _))
            {
                MessageBox.Show("ERROR: Min Odds must have a numeric value!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (!double.TryParse(this.Model.MaxOdds, out _))
            {
                MessageBox.Show("ERROR: Max Odds must have a numeric value!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if (!int.TryParse(this.Model.Year, out int year) || year <= DateTime.MinValue.Year || year >= DateTime.MaxValue.Year)
            {
                MessageBox.Show("ERROR: Year invalid!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else if(!double.TryParse(this.Model.PercentageOfExpectedWins, out _))
            {
                MessageBox.Show("ERROR: Percentage of Expected Wins invalid!", AppSettings.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private List<string> GetRaceTypes()
        {
            var raceTypes = new HashSet<string>();
            foreach(var column in Data.ProcessedRaceData)
            {
                if (column.Key.ToLower() == "race type")
                {
                    foreach(var item in column.Value.Data)
                    {
                        raceTypes.Add((string)item);
                    }
                }
            }

            var returnValue = new List<string> { "All" };
            foreach(var raceType in raceTypes)
            {
                returnValue.Add(raceType);
            }

            return returnValue;
        }

        private List<string> GetMonths()
        {
            return new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        }
    }
}
