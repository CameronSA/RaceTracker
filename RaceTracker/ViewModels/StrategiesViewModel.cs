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

            this.Model = new StrategiesModel
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
                Months = months,
                Month = months[0],
                Year = DateTime.Today.Year.ToString(),
            };

            this.View = view;
            this.Command = new StrategiesCommand(this);
        }

        public StrategiesModel Model { get; }
        public StrategiesView View { get; }
        public ICommand Command { get; }

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
