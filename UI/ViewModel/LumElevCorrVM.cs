using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EpicLumi.UI.ViewModel
{
    public class LumElevCorrVM : INPC
    {
        private string workPlaneNamePreview;
        private int selectedLinkIndex;
        private int selectedLevelIndex2;

        public List<string> LevelData { get; set; }
        public List<string> CeilCorrectionLinks { get; set; }

        public string WorkPlaneNamePreview
        {
            get => workPlaneNamePreview; set
            {
                if (workPlaneNamePreview != value)
                {
                    workPlaneNamePreview = value;
                    MyPropertyChanged(nameof(WorkPlaneNamePreview));
                }
            }
        }

        public ICommand btnOK { get; set; }

        public Result RevitTransactionResult { get; set; }

        public event EventHandler OnRequestClose;

        public int SelectedLinkIndex
        {
            get => selectedLinkIndex; set
            {
                if (selectedLinkIndex != value)
                {
                    selectedLinkIndex = value;
                    MyPropertyChanged(nameof(SelectedLinkIndex));
                    Properties.Settings.Default.ElevCorrSelectedLinkIndex = selectedLinkIndex;
                }
            }
        }

        public int SelectedLevelIndex
        {
            get => selectedLevelIndex2; set
            {
                if (selectedLevelIndex2 != value) 
                { 
                    selectedLevelIndex2 = value; 
                    MyPropertyChanged(nameof(SelectedLevelIndex));
                    Properties.Settings.Default.ElevCorrSelectedLinkIndex = selectedLevelIndex2;
                }
            }
        }


        //public int SelectedLevelIndex
        //{
        //    get => selectedLevelIndex;

        //    set
        //    {
        //        if (selectedLevelIndex != value)
        //        {
        //            SelectedLevelIndex = value;
        //            MyPropertyChanged(nameof(SelectedLevelIndex));
        //            Properties.Settings.Default.ElevCorrSelectedLevelIndex = selectedLevelIndex;
        //        }
        //    }
        //}

        public LumElevCorrVM()
        {

            RevitTransactionResult = Result.Cancelled;
            btnOK = new RCommand(btnOKAction);

            SelectedLinkIndex = 0;
            SelectedLevelIndex = 0;

            //if (Properties.Settings.Default.ElevCorrSelectedLinkIndex <= CeilCorrectionLinks.Count)
            //{
            //    SelectedLinkIndex = Properties.Settings.Default.ElevCorrSelectedLinkIndex;
            //}

            //if (Properties.Settings.Default.ElevCorrSelectedLevelIndex <= LevelData.Count)
            //{
            //    SelectedLevelIndex = Properties.Settings.Default.ElevCorrSelectedLevelIndex;
            //}

            SelectedLinkIndex = Properties.Settings.Default.ElevCorrSelectedLinkIndex;
            SelectedLevelIndex = Properties.Settings.Default.ElevCorrSelectedLevelIndex;
        }

        private void btnOKAction(object obj)
        {
            RevitTransactionResult = Result.Succeeded;
            OnRequestClose(this, new EventArgs());
        }
    }
}
