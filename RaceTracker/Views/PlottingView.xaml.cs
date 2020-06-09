using RaceTracker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using ScottPlot;

namespace RaceTracker.Views
{
    /// <summary>
    /// Interaction logic for PlottingView.xaml
    /// </summary>
    public partial class PlottingView : UserControl
    {
        public PlottingView()
        {
            InitializeComponent();
            var viewModel = new PlottingViewModel(this);
            this.DataContext = viewModel;
        }
    }
}
