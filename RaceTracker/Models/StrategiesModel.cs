using RaceTracker.LogicHelpers;
using RaceTracker.ViewModels;
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
        private List<string> monthlyDays;
        private string monthlyDay;
        private string strategy1DailyBreakdown;
        private string strategy2DailyBreakdown;
        private string strategy1DailyBreakdownTitle;
        private string strategy2DailyBreakdownTitle;
        private StrategiesViewModel viewModel;
        private DateTime date;
        private string percentageOfExpectedWins;

        public StrategiesModel(StrategiesViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public string PercentageOfExpectedWins
        {
            get
            {
                return this.percentageOfExpectedWins;
            }
            set
            {
                this.percentageOfExpectedWins = value;
                this.OnPropertyChanged("PercentageOfExpectedWins");
            }
        }

        public DateTime Date
        {
            get
            {
                return this.date;
            }
            set
            {
                this.date = value;
                this.OnPropertyChanged("Date");
                this.viewModel.DisplayStrategy2DailyBreakdown();
            }
        }

        public string Strategy1DailyBreakdownTitle
        {
            get
            {
                return this.strategy1DailyBreakdownTitle;
            }
            set
            {
                this.strategy1DailyBreakdownTitle = value;
                this.OnPropertyChanged("Strategy1DailyBreakdownTitle");
            }
        }

        public string Strategy1DailyBreakdown
        {
            get
            {
                return this.strategy1DailyBreakdown;
            }
            set
            {
                this.strategy1DailyBreakdown = value;
                this.OnPropertyChanged("Strategy1DailyBreakdown");
            }
        }
        public string Strategy2DailyBreakdownTitle
        {
            get
            {
                return this.strategy2DailyBreakdownTitle;
            }
            set
            {
                this.strategy2DailyBreakdownTitle = value;
                this.OnPropertyChanged("Strategy2DailyBreakdownTitle");
            }
        }

        public string Strategy2DailyBreakdown
        {
            get
            {
                return this.strategy2DailyBreakdown;
            }
            set
            {
                this.strategy2DailyBreakdown = value;
                this.OnPropertyChanged("Strategy2DailyBreakdown");
            }
        }

        public List<string> MonthlyDays
        {
            get
            {
                return this.monthlyDays;
            }
            set
            {
                this.monthlyDays = value;
                this.OnPropertyChanged("MonthlyDays");
                this.viewModel.DisplayStrategy1DailyBreakdown();
            }
        }

        public string MonthlyDay
        {
            get
            {
                return this.monthlyDay;
            }
            set
            {
                this.monthlyDay = value;
                this.OnPropertyChanged("MonthlyDay");
                this.viewModel.DisplayStrategy1DailyBreakdown();
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
                this.DailyPotPercentage = this.DailyPotPercentage;
                this.TotalWinPercentage = this.TotalWinPercentage;
                this.PriceCutoffPercentage = this.PriceCutoffPercentage;
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

                if (int.TryParse(this.Year, out int year) && year >= DateTime.MinValue.Year && year <= DateTime.MaxValue.Year)
                {
                    int daysInMonth = DateTime.DaysInMonth(year, UnitConversions.MonthNumber(value));
                    var days = new List<string>();
                    for (int i = 1; i <= daysInMonth; i++)
                    {
                        days.Add(i.ToString());
                    }

                    this.MonthlyDays = days;
                    this.viewModel.DisplayStrategy1DailyBreakdown();
                }
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
                if (!string.IsNullOrEmpty(this.Month))
                {
                    this.Month = this.Month;
                }
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
                this.TotalWinPercentage = this.TotalWinPercentage;
                this.PriceCutoffPercentage = this.PriceCutoffPercentage;
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
