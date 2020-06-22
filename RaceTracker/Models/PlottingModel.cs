using RaceTracker.LogicHelpers;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(object property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property.ToString()));
        }
    }
}
