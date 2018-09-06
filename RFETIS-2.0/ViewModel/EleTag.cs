using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFETIS_2._0.ViewModel
{
    public class EleTag : INotifyPropertyChanged
    {
        private string state;
        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public string State
        {
            get { return state; }
            set
            {
                state = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("State"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
