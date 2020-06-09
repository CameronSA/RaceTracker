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
        private Dictionary<string, string[]> dataFields;

        private List<string> dataHeaders;

        private string xAxisSelection;

        private string yAxisSelection;

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

        public Dictionary<string, string[]> DataFields
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
