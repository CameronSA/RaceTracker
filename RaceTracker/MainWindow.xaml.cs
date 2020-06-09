using MahApps.Metro.Controls;
using RaceTracker.LogicHelpers;
using RaceTracker.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaceTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            this.LoadData();
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.WindowState = WindowState.Maximized;
            this.Height = SystemParameters.PrimaryScreenHeight*0.9;
            this.Width = SystemParameters.PrimaryScreenWidth*0.9;
        }

        private void LoadData()
        {
            string consoleCommand = "/C " + AppSettings.DataMiningApp + " -copydata " + AppSettings.DataFileName + " " + AppSettings.DataFilePath;
            CommandLine.ExecuteCommand(consoleCommand);
            Data.ParseFile(AppSettings.DataFilePath);
        }
    }
}
