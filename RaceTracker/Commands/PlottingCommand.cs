using RaceTracker.Analysis;
using RaceTracker.ViewModels;
using System;
using System.Windows.Input;

namespace RaceTracker.Commands
{
    public class PlottingCommand : ICommand
    {
        private PlottingViewModel ViewModel { get; }

        public PlottingCommand(PlottingViewModel viewModel)
        {
            this.ViewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            bool canExecute = false;
            switch (parameter.ToString())
            {
                case "btPositionGo":
                    canExecute = true;
                    break;
                case "btDailyProfileGo":
                    canExecute = true;
                    break;
                case "btProbabilityOddsGo":
                    canExecute = true;
                    break;
                case "btRaceTypePerDayGo":
                    canExecute = true;
                    break;
                case "btOddsProbabilityGo":
                    canExecute = true;
                    break;
                case "btNumberFavouriteWinsGo":
                    canExecute = true;
                    break;
            }

            return canExecute;
        }

        public void Execute(object parameter)
        {
            switch (parameter.ToString())
            {
                case "btPositionGo":
                    var favouriteByPosition = new FavouriteByPosition(this.ViewModel);
                    favouriteByPosition.PlotFavouriteByPosition();
                    break;
                case "btDailyProfileGo":
                    var dailyRaceProfile = new DailyRaceProfile(this.ViewModel);
                    dailyRaceProfile.PlotDailyRaceProfile();
                    break;
                case "btProbabilityOddsGo":
                    var odds = new ProbabilityOfOddsProfile(this.ViewModel);
                    odds.PlotOddsProfile();
                    break;
                case "btRaceTypePerDayGo":
                    this.ViewModel.NumberRaceTypePerDay();
                    break;
                case "btOddsProbabilityGo":
                    var oddsProbability = new OddsProbabilityProfile(this.ViewModel);
                    oddsProbability.PlotOddsProbabilityProfile();
                    break;
                case "btNumberFavouriteWinsGo":
                    var numberFavourites = new NumberFavouriteWins(this.ViewModel);
                    numberFavourites.CalculateNumberFavouriteWinsVsNumberRaces(1, 20, 40);
                    break;
            }
        }    
    }
}
