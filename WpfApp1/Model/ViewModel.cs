using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace WpfApp1.Model
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       
        private StrokeCollection inkStrokes;
        public StrokeCollection InkStrokes
        {
            get { return inkStrokes; }
            set
            {
                inkStrokes = value;
                OnPropertyChanged("InkStrokes");
            }
        }

    }
}
