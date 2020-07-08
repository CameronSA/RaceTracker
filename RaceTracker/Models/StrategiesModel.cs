using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTracker.Models
{
    public class StrategiesModel : INotifyPropertyChanged
    {
        private string bank;
        private string dailyPotPercentage;
        private string totalWinPercentage;
        private string priceCutoffPercentage;
        private string raceCutoff;
        private string minOdds;
        private string maxOdds;
        private string raceType;
        private string month;
        private string year;
        private string displayTime;
        private string displayOdds;
        private string displayBet;
        private string displayWinnings;
        private string displayResult;

        public string DisplayTime
        {
            get
            {
                return this.displayTime;
            }
            set
            {
                this.displayTime = value;
                this.OnPropertyChanged("DisplayTime");
            }
        }

        public string DisplayOdds
        {
            get
            {
                return this.displayOdds;
            }
            set
            {
                this.displayOdds = value;
                this.OnPropertyChanged("DisplayOdds");
            }
        }

        public string DisplayBet
        {
            get
            {
                return this.displayBet;
            }
            set
            {
                this.displayBet = value;
                this.OnPropertyChanged("DisplayBet");
            }
        }

        public string DisplayWinnings
        {
            get
            {
                return this.displayWinnings;
            }
            set
            {
                this.displayWinnings = value;
                this.OnPropertyChanged("DisplayWinnings");
            }
        }

        public string DisplayResult
        {
            get
            {
                return this.displayResult;
            }
            set
            {
                this.displayResult = value;
                this.OnPropertyChanged("DisplayResult");
            }
        }

        public string Bank
        {
            get
            {
                return this.bank;
            }
            set
            {
                this.bank = value;
                this.OnPropertyChanged("Bank");
                this.DailyPotValue = string.Empty;
                this.TotalWinValue = string.Empty;
                this.PriceCutoffValue = string.Empty;
            }
        }

        public string DailyPotPercentage
        {
            get
            {
                return this.dailyPotPercentage;
            }
            set
            {
                this.dailyPotPercentage = value;
                this.OnPropertyChanged("DailyPotPercentage");
                this.DailyPotValue = string.Empty;
            }
        }

        public string TotalWinPercentage
        {
            get
            {
                return this.totalWinPercentage;
            }
            set
            {
                this.totalWinPercentage = value;
                this.OnPropertyChanged("TotalWinPercentage");
                this.TotalWinValue = string.Empty;
            }
        }

        public string PriceCutoffPercentage
        {
            get
            {
                return this.priceCutoffPercentage;
            }
            set
            {
                this.priceCutoffPercentage = value;
                this.OnPropertyChanged("PriceCutoffPercentage");
                this.PriceCutoffValue = string.Empty;
            }
        }

        public string RaceCutoff
        {
            get
            {
                return this.raceCutoff;
            }
            set
            {
                this.raceCutoff = value;
                this.OnPropertyChanged("RaceCutoff");
            }
        }

        public string MinOdds
        {
            get
            {
                return this.minOdds;
            }
            set
            {
                this.minOdds = value;
                this.OnPropertyChanged("MinOdds");
            }
        }

        public string MaxOdds
        {
            get
            {
                return this.maxOdds;
            }
            set
            {
                this.maxOdds = value;
                this.OnPropertyChanged("MaxOdds");
            }
        }

        public string RaceType
        {
            get
            {
                return this.raceType;
            }
            set
            {
                this.raceType = value;
                this.OnPropertyChanged("RaceType");
            }
        }

        public string Month
        {
            get
            {
                return this.month;
            }
            set
            {
                this.month = value;
                this.OnPropertyChanged("Month");
            }
        }

        public string Year
        {
            get
            {
                return this.year;
            }
            set
            {
                this.year = value;
                this.OnPropertyChanged("Year");
            }
        }

        public string DailyPotValue
        {
            get
            {
                if (double.TryParse(this.DailyPotPercentage, out double percentage) && double.TryParse(this.Bank, out double bank))
                {
                    double number = (percentage / 100.0) * bank;
                    return "£" + number;
                }

                return string.Empty;
            }
            set
            {
                this.OnPropertyChanged("DailyPotValue");
            }
        }

        public string TotalWinValue
        {
            get
            {
                if (double.TryParse(this.DailyPotPercentage, out double percentage) && double.TryParse(this.Bank, out double bank) && double.TryParse(this.TotalWinPercentage, out double totalWinPercentage))
                {
                    double pot = (percentage / 100.0) * bank;
                    double number = (totalWinPercentage / 100.0) * pot;
                    return "£" + number;
                }

                return string.Empty;
            }
            set
            {
                this.OnPropertyChanged("TotalWinValue");
            }
        }

        public string PriceCutoffValue
        {
            get
            {
                if (double.TryParse(this.DailyPotPercentage, out double percentage) && double.TryParse(this.Bank, out double bank) && double.TryParse(this.PriceCutoffPercentage, out double priceCutoffPercentage))
                {
                    double pot = (percentage / 100.0) * bank;
                    double number = (priceCutoffPercentage / 100.0) * pot;
                    return "£" + number;
                }

                return string.Empty;
            }
            set
            {
                this.OnPropertyChanged("PriceCutoffValue");
            }
        }

        public List<string> RaceTypes { get; set; }
        public List<string> Months { get; set; }
        public event PropertyChangedEventHandler PropertyChanged; 
        private void OnPropertyChanged(object property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property.ToString()));
        }
    }
}
