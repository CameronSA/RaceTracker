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
        }

        private StrategiesViewModel ViewModel { get; }

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
            }
        }

        private void ExecuteStrategy1()
        {
            if (this.ViewModel.VerifyInputs())
            {
                var strategy1 = new Strategy1(this.ViewModel);
                var dailyProfit = strategy1.CalculateDailyProfitForYear();
                foreach (var day in strategy1.DailyReports)
                {
                    foreach (var race in day.Value)
                    {
                        this.ViewModel.Model.DisplayTime += race.Item1+"\n";
                        this.ViewModel.Model.DisplayBet += race.Item2 + "\n";
                        this.ViewModel.Model.DisplayOdds += race.Item3 + "\n";
                        this.ViewModel.Model.DisplayResult += race.Item4 ? "Won\n" : "Lost\n";
                        this.ViewModel.Model.DisplayWinnings += race.Item5 + "\n";
                        MessageBox.Show(this.ViewModel.Model.DisplayTime + "\n" + this.ViewModel.Model.DisplayBet + "\n" + this.ViewModel.Model.DisplayOdds + "\n" + this.ViewModel.Model.DisplayResult + "\n" + this.ViewModel.Model.DisplayWinnings);
                    }

                    break;
                }
            }
        }
    }
}
