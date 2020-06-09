using RaceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace RaceTracker.Views
{
    /// <summary>
    /// Interaction logic for PlottingView.xaml
    /// </summary>
    public partial class PlottingView : UserControl
    {
        public PlottingView()
        {
            var viewModel = new PlottingViewModel();
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
