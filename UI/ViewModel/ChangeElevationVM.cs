using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EpicLumi.UI.ViewModel
{
    public class ChangeElevationVM : INPC
    {
        private double elevationAtLevel;
        private bool useAutoLevel;
        private string userSelectedLevel;
        private bool levelSelectionEnabled;

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

        public bool UseAutoLevel
        {
            get => useAutoLevel; set
            {
                if (useAutoLevel != value)
                {
                    useAutoLevel = value;
                    MyPropertyChanged(nameof(UseAutoLevel));
                    if (useAutoLevel)
                    {
                        LevelSelectionEnabled = false;
                    } else
                    {
                        LevelSelectionEnabled = true;
                    }
                }

            }
        }

        public string UserSelectedLevel
        {
            get => userSelectedLevel; set
            {
                if (userSelectedLevel != value)
                {
                    userSelectedLevel = value;
                    MyPropertyChanged(nameof(UserSelectedLevel));
                }

            }
        }

        public bool LevelSelectionEnabled
        {
            get => levelSelectionEnabled; set
            {
                if(levelSelectionEnabled != value)
                {
                    levelSelectionEnabled = value;
                    MyPropertyChanged(nameof(LevelSelectionEnabled));
                }
                
            }
        }
        public ObservableCollection<string> ElevLevels { get; set; }

        public ChangeElevationVM()
        {
            UseAutoLevel = true;
            btn_OK = new RCommand(btnOK);
        }

        private void btnOK(object obj)
        {
            OnRequestClose(this, new EventArgs());
        }

    }
}
