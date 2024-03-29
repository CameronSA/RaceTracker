﻿using RaceTracker.Analysis;
using RaceTracker.Strategies;
using RaceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Input;

namespace RaceTracker.Commands
{
    public class StrategiesCommand : ICommand
    {
        public StrategiesCommand(StrategiesViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.Strategy1DailyReports = new Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>>();
            this.Strategy2DailyReports = new Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>>();
        }

        private StrategiesViewModel ViewModel { get; }

        public Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>> Strategy1DailyReports { get; private set; } 
        public Dictionary<DateTime, List<Tuple<DateTime, double, double, bool, double>>> Strategy2DailyReports { get; private set; } 

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            bool canExecute = false;
            switch(parameter.ToString())
            {
                case "btStrategy1Go":
                    canExecute = true;
                    break;
                case "btStrategy2Go":
                    canExecute = true;
                    break;
            }

            return canExecute;
        }

        public void Execute(object parameter)
        {
            switch(parameter.ToString())
            {
                case "btStrategy1Go":
                    this.ExecuteStrategy1();       
                    break;
                case "btStrategy2Go":
                    this.ExecuteStrategy2();
                    break;
            }
        }

        private void ExecuteStrategy1()
        {
            if (this.ViewModel.VerifyInputs())
            {
                var strategy1 = new Strategy1(this.ViewModel);
                var dailyBetAndWinnings = strategy1.CalculateDailyProfits();
                this.Strategy1DailyReports = strategy1.DailyReports;
                this.ViewModel.FixedYear = int.Parse(this.ViewModel.Model.Year);
                var formattedData = this.GetYearlyStrategy1PlotData(dailyBetAndWinnings, this.ViewModel.FixedYear);

                this.CreateAnnualStrategy1Plot(formattedData);
                this.ViewModel.DisplayStrategy1DailyBreakdown();
            }
        }

        private void ExecuteStrategy2()
        {
            if (this.ViewModel.VerifyInputs())
            {
                var strategy2 = new Strategy2(this.ViewModel);
                var dailyBetAndGrossWinnings = strategy2.CalculateDailyWinnings(); // Gross winnings includes initial stakes
                this.Strategy2DailyReports = strategy2.DailyReports;
                var formattedData = this.GetStrategy2PlotData(dailyBetAndGrossWinnings);

                this.CreateStrategy2Plot(formattedData);
                this.ViewModel.DisplayStrategy2DailyBreakdown();
            }
        }

        private List<Tuple<DateTime, double, double>> GetStrategy2PlotData(Dictionary<DateTime, Tuple<double, double>> dailyBetAndGrossWinnings)
        {
            var data = new List<Tuple<DateTime, double, double>>(); //date, total bet, net winnings
            foreach(var day in dailyBetAndGrossWinnings)
            {
                double netWinnings = day.Value.Item2 - day.Value.Item1;
                data.Add(new Tuple<DateTime, double, double>(day.Key, day.Value.Item1, netWinnings));
            }

            return data;
        }

        // Output: Date, total bet, daily winnings
        private List<Tuple<DateTime, double, double>> GetYearlyStrategy1PlotData(Dictionary<DateTime, Tuple<double, double>> dailyBetAndWinnings, int year)
        {
            var yearlyData = new List<Tuple<DateTime, double, double>>(); // date, total bet, daily winnings            
            foreach(var day in dailyBetAndWinnings)
            {
                if (day.Key.Year == year)
                {
                    if (day.Value.Item2 > 0)
                    {
                        yearlyData.Add(new Tuple<DateTime, double, double>(day.Key, day.Value.Item1, day.Value.Item2));
                    }
                    else
                    {
                        yearlyData.Add(new Tuple<DateTime, double, double>(day.Key, day.Value.Item1, -day.Value.Item1));
                    }
                }
            }

            return yearlyData;
        }

        // Input: Date, total bet, daily winnings taking into account losses
        private void CreateStrategy2Plot(List<Tuple<DateTime, double, double>> data)
        {
            var orderedData = data.OrderBy(x => x.Item1);
            var dates = new List<DateTime>();
            var totalWinnings = new List<double>();
            double sumOfWinnings = 0;
            foreach (var row in orderedData)
            {
                sumOfWinnings += row.Item3;
                dates.Add(row.Item1);
                totalWinnings.Add(sumOfWinnings);
            }

            new Plotting().PlotTimeSeries(this.ViewModel.View.AnnualPlotStrategy2, dates, totalWinnings, true, string.Empty, "Total Winnings (£)", new List<string>(), string.Empty);
        }

        // Input: Date, total bet, daily winnings taking into account losses
        private void CreateAnnualStrategy1Plot(List<Tuple<DateTime, double, double>> data)
        {
            var orderedData = data.OrderBy(x => x.Item1);
            var dates = new List<DateTime>();
            var totalWinnings = new List<double>();
            double sumOfWinnings = 0;
            foreach(var row in orderedData)
            {
                sumOfWinnings += row.Item3;
                dates.Add(row.Item1);
                totalWinnings.Add(sumOfWinnings);
            }

            new Plotting().PlotTimeSeries(this.ViewModel.View.AnnualPlotStrategy1, dates, totalWinnings, true, string.Empty, "Total Winnings (£)", new List<string>(), string.Empty);
        }
    }
}
