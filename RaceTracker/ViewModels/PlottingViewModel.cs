﻿using RaceTracker.Analysis;
using RaceTracker.Commands;
using RaceTracker.LogicHelpers;
using RaceTracker.Models;
using RaceTracker.Views;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RaceTracker.ViewModels
{
    public class PlottingViewModel
    {
        public PlottingViewModel(PlottingView view)
        {
            this.Model = new PlottingModel()
            {
                DataFields = Data.ProcessedRaceData,
                DataHeaders = this.PopulateDataHeaders(),
                Position = "1",
                TimeResolutionFields = new List<string> { TimeResolutionFields.Day, TimeResolutionFields.Month, TimeResolutionFields.Year },
                TimeResolutionField = TimeResolutionFields.Day,
                MinDate = CommonAnalyses.MinDataDate,
                MaxDate = CommonAnalyses.MaxDataDate,
                IndividualNumberRacecoursesMin = "1",
                IndividualNumberRacecoursesMax = "10",
                MinOdds = "0",
                MaxOdds = "20",
            };

            if (this.Model.DataHeaders.Count > 0)
            {
                this.Model.XAxisSelection = this.Model.DataHeaders[0];
                this.Model.YAxisSelection = this.Model.DataHeaders[0];
            }

            this.Command = new PlottingCommand(this);
            this.View = view;

            this.NumberRaceCoursesByDate();
            this.NumberRaceCoursesPerDayCount();
            this.NumberRaceTypes();
        }

        private void NumberRaceCoursesByDate()
        {
            var data = CommonAnalyses.RetrieveNumberRaceCoursesData(TimeResolutionFields.Day, CommonAnalyses.MinDataDate, CommonAnalyses.MaxDataDate);
            foreach (var set in data)
            {
                new Plotting().PlotTimeSeries(this.View.NumberRaceTracksPerDay, set.Key, set.Value, true, string.Empty, "Number of Race Courses", new List<string>(), string.Empty);
                break;
            }
        }

        private void NumberRaceCoursesPerDayCount()
        {
            var data = CommonAnalyses.GetNumberRaceCoursesByDayCount();
            foreach(var set in data)
            {
                var seriesNames = new List<string>();
                new Plotting().PlotScatter(this.View.NumberRaceTracksPerDayCount, set.Key, set.Value, true, "Number of Race Courses", "Count", seriesNames, "Total");
                this.NumberRaceCoursesPerDayCountBySeason(seriesNames);
                break;
            }
        }

        private void NumberRaceCoursesPerDayCountBySeason(List<string> seriesNames)
        {
            var data = CommonAnalyses.GetNumberRaceCoursesByDayCountBySeason();
            foreach(var season in data)
            {
                foreach (var set in season.Item2)
                {
                    new Plotting().PlotScatter(this.View.NumberRaceTracksPerDayCount, set.Key, set.Value, false, "Number of Race Courses", "Count", seriesNames, season.Item1);
                    break;
                }
            }
        }

        private void NumberRaceTypes()
        {
            var data = CommonAnalyses.GetNumberRaceTypes(CommonAnalyses.MinDataDate, CommonAnalyses.MaxDataDate);
            foreach (var set in data)
            {
                new Plotting().PlotBar(this.View.NumberRaceTypes, set.Key, set.Value, true, "Race Type", "Count");
                break;
            }

            this.NumberRaceTypePerDay(data);
        }

        private void NumberRaceTypePerDay(Dictionary<List<string>, List<double>> raceTypeTotals)
        {
            var data = CommonAnalyses.NumberRaceTypePerDay;
            var seriesNames = new List<string>();
            bool reset = true;
            foreach (var set in data)
            {
                foreach (var raceTypeTotal in raceTypeTotals)
                {
                    string series = set.Key;
                    var raceTypeIndex = raceTypeTotal.Key.IndexOf(series);
                    var total = raceTypeTotal.Value[raceTypeIndex];
                    var percentages = new List<double>();
                    foreach (var point in set.Value.Item2)
                    {
                        percentages.Add(100 * point / total);
                    }

                    new Plotting().PlotTimeSeries(this.View.NumberRaceTypePerDay, set.Value.Item1, percentages, reset, string.Empty, "Number of Occurances as a Percentage\nof the Total for that Race Type", seriesNames, series);
                    reset = false;
                    break;
                }
            }
        }

        private List<string> PopulateDataHeaders()
        {
            var headers = new List<string>();
            foreach(var header in Data.ProcessedRaceData.Keys)
            {
                headers.Add(header);
            }

            return headers;
        }

        public PlottingModel Model { get; }

        public PlottingView View { get; }

        public ICommand Command { get; }
    }
}
