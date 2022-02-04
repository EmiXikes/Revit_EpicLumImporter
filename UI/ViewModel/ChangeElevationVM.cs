using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EpicLumi.UI.ViewModel
{
    public class ChangeElevationVM : INPC
    {
        private double elevationAtLevel;

        public event EventHandler OnRequestClose;
        public ICommand btn_OK { get; set; }

        public double ElevationAtLevel
        {
            get => elevationAtLevel; set
            {
                if (elevationAtLevel != value)
                {
                    elevationAtLevel = value;
                    MyPropertyChanged(nameof(ElevationAtLevel));
                }

            }
        }

        public ChangeElevationVM()
        {
            btn_OK = new RCommand(btnOK);
        }

        private void btnOK(object obj)
        {
            OnRequestClose(this, new EventArgs());
        }

    }
}
