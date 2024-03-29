﻿using RaceTracker.LogicHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTracker.Models
{
    public class PlottingModel : INotifyPropertyChanged
    {
        private Dictionary<string, RaceData> dataFields;
        private List<string> dataHeaders;
        private string xAxisSelection;
        private string yAxisSelection;
        private string position;
        private List<string> timeResolutionFields;
        private string timeResolutionField;
        private DateTime minDate;
        private DateTime maxDate;
        private string individualNumberRacecoursesMin;
        private string individualNumberRacecoursesMax;
        private bool resetIndividual;
        private bool upToAndIncludingPosition;
        private bool splitByPosition;
        private string minOdds;
        private string maxOdds;
        private string numberBins;
        private string numberRacesMin;
        private string numberRacesMax;
        private string raceType;

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

        public string NumberRacesMin
        {
            get
            {
                return this.numberRacesMin;
            }
            set
            {
                this.numberRacesMin = value;
                this.OnPropertyChanged("NumberRacesMin");                
            }
        }

        public string NumberRacesMax
        {
            get
            {
                return this.numberRacesMax;
            }
            set
            {
                this.numberRacesMax = value;
                this.OnPropertyChanged("NumberRacesMax");
            }
        }

        public string NumberBins
        {
            get
            {
                return this.numberBins;
            }
            set
            {
                this.numberBins = value;
                this.OnPropertyChanged("NumberBins");
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

        public bool SplitByPosition
        {
            get
            {
                return this.splitByPosition;
            }
            set
            {
                this.splitByPosition = value;
                this.OnPropertyChanged("SplitByPosition");
            }
        }

        public bool UpToAndIncludingPosition
        {
            get
            {
                return this.upToAndIncludingPosition;
            }
            set
            {
                this.upToAndIncludingPosition = value;
                this.OnPropertyChanged("UpToAndIncludingPosition");
            }
        }

        public bool ResetIndividual
        { 
            get
            {
                return this.resetIndividual;
            }
            set
            {
                this.resetIndividual = value;
                this.OnPropertyChanged("ResetIndividual");
            }
        }

        public string IndividualNumberRacecoursesMin
        {
            get
            {
                return this.individualNumberRacecoursesMin;
            }
            set
            {
                this.individualNumberRacecoursesMin = value;
                this.OnPropertyChanged("IndividualNumberRacecoursesMin");
            }
        }

        public string IndividualNumberRacecoursesMax
        {
            get
            {
                return this.individualNumberRacecoursesMax;
            }
            set
            {
                this.individualNumberRacecoursesMax = value;
                this.OnPropertyChanged("IndividualNumberRacecoursesMax");
            }
        }

        public DateTime MinDate
        {
            get
            {
                return this.minDate;
            }
            set
            {
                this.minDate = value;
                this.OnPropertyChanged("MinDate");
            }
        }

        public DateTime MaxDate
        {
            get
            {
                return this.maxDate;
            }
            set
            {
                this.maxDate = value;
                this.OnPropertyChanged("MaxDate");
            }
        }

        public string TimeResolutionField
        {
            get
            {
                return this.timeResolutionField;
            }
            set
            {
                this.timeResolutionField = value;
                this.OnPropertyChanged("TimeResolutionField");
            }
        }
        
        public List<string> TimeResolutionFields
        {
            get
            {
                return this.timeResolutionFields;
            }
            set
            {
                this.timeResolutionFields = value;
                this.OnPropertyChanged("TimeResolutionFields");
            }
        }

        public string Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
                this.OnPropertyChanged("Position");
            }
        }

        public string XAxisSelection
        {
            get
            {
                return this.xAxisSelection;
            }
            set
            {
                this.xAxisSelection = value;
                this.OnPropertyChanged("XAxisSelection");
            }
        }

        public string YAxisSelection
        {
            get
            {
                return this.yAxisSelection;
            }
            set
            {
                this.yAxisSelection = value;
                this.OnPropertyChanged("YAxisSelection");
            }
        }

        public List<string> DataHeaders
        {
            get
            {
                return this.dataHeaders;
            }
            set
            {
                this.dataHeaders = value;
                this.OnPropertyChanged("DataHeaders");
            }
        }

        public Dictionary<string, RaceData> DataFields
        {
            get
            {
                return this.dataFields;
            }
            set
            {
                this.dataFields = value;
                this.OnPropertyChanged("DataFields");
            }
        }

        public List<string> RaceTypes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(object property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property.ToString()));
        }
    }
}
