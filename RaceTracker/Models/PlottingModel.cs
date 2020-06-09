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
        private Dictionary<string, List<string>> dataFields;

        public Dictionary<string, List<string>> DataFields
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
