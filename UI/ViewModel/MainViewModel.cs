using System;
using System.Collections.Generic;
using System.Windows.Input;
using Autodesk.Revit.UI;

namespace EpicLumi.UI.ViewModel
{
    public class MainViewModel : INPC
    {
        private string test;
        private string selectedLevel;
        private string levelOffset;
        private string ceilCorrectionZone;
        private bool ceilCorrectionToggle;
        private string ceilCorrectionTolerance;

        public List<string> LevelData { get; set; }
        public List<UniLumItem> UniLumData { get; set; }
        public List<LumSelectionData> FbLumData { get; set; }
        public List<RevitFamilyItem> RevitFamilyData { get; set; }
        public List<string> CeilCorrectionLinks { get; set; }

        public string Test
        {
            get => test; set
            {
                if (test != value)
                {
                    test = value; MyPropertyChanged(nameof(Test));
                }
            }
        }
        public string SelectedLevel
        {
            get => selectedLevel; set
            {
                if (selectedLevel != value) { selectedLevel = value; MyPropertyChanged(nameof(SelectedLevel)); }
            }
        }
        public string LevelOffset
        {
            get => levelOffset; set
            {
                if (levelOffset != value)
                {
                    levelOffset = value;
                    MyPropertyChanged(nameof(LevelOffset));
                }
                levelOffset = value;
            }
        }

        public bool CeilCorrectionToggle
        {
            get => ceilCorrectionToggle; set
            {
                if (ceilCorrectionToggle != value) { ceilCorrectionToggle = value; MyPropertyChanged(nameof(CeilCorrectionToggle)); }
            }
        }
        public string CeilCorrectionZone
        {
            get => ceilCorrectionZone; set
            {
                if (ceilCorrectionZone != value) { ceilCorrectionZone = value; MyPropertyChanged(nameof(CeilCorrectionZone)); }
            }
        }
        public string CeilCorrectionTolerance
        {
            get => ceilCorrectionTolerance; set
            {
                if(ceilCorrectionTolerance != value){ ceilCorrectionTolerance = value; MyPropertyChanged(nameof(CeilCorrectionTolerance)); }
            }
        }

        public ICommand btnOK { get; set; }
        public ICommand btnCancel { get; set; }

        public Result RevitTransactionResult { get; set; }

        public event EventHandler OnRequestClose;

        public MainViewModel()
        {
            LevelOffset = "0";
            CeilCorrectionZone = "0";
            CeilCorrectionTolerance = "0";
            CeilCorrectionToggle = false;

            Test = "...xxx...";
            FbLumData = new List<LumSelectionData>();

            btnOK = new RCommand(btnOKAction);
            btnCancel = new RCommand(btnCancelAction);

            LevelData = new List<string>() { "Level 01", "Level 02", "Level 03", "Level 04", "Level 05" };
            UniLumData = new List<UniLumItem>()
            {
                new UniLumItem(){dwgLumName = "MegaBUBU", dwgLumManufacturer = "Staigātāju kantoris"},
                new UniLumItem(){dwgLumName = "MegaBUBU", dwgLumManufacturer = "Musta"},
                new UniLumItem(){dwgLumName = "11", dwgLumManufacturer = "TRILUX"},
                new UniLumItem(){dwgLumName = "dsgsdfg", dwgLumManufacturer = "nslgkd"},
                new UniLumItem(){dwgLumName = "223", dwgLumManufacturer = "Staigātāju kantoris"},
                new UniLumItem(){dwgLumName = "MegaBUBU", dwgLumManufacturer = "Staigātāju kantoris"},
                new UniLumItem(){dwgLumName = "sdgs", dwgLumManufacturer = "Staigātāju kantoris"},
                new UniLumItem(){dwgLumName = "sgs", dwgLumManufacturer = "Neko nevar kantoris"},
                new UniLumItem(){dwgLumName = "MegaBUBU", dwgLumManufacturer = "Staigātāju kantoris"},
                new UniLumItem(){dwgLumName = "MegaBUBU", dwgLumManufacturer = "Musta"},
                new UniLumItem(){dwgLumName = "11", dwgLumManufacturer = "TRILUX"},
                new UniLumItem(){dwgLumName = "dsgsdfg", dwgLumManufacturer = "nslgkd"},
                new UniLumItem(){dwgLumName = "223", dwgLumManufacturer = "Staigātāju kantoris"},
                new UniLumItem(){dwgLumName = "MegaBUBU", dwgLumManufacturer = "Staigātāju kantoris"},
                new UniLumItem(){dwgLumName = "sdgs", dwgLumManufacturer = "Staigātāju kantoris"},
                new UniLumItem(){dwgLumName = "sgs", dwgLumManufacturer = "Neko nevar kantoris"},
            };
            RevitFamilyData = new List<RevitFamilyItem>()
            {
                new RevitFamilyItem(){ RevitFamilyName = "Super Luminary", RevitFamilyManufacturer = "Universal Awesome"},
                new RevitFamilyItem(){ RevitFamilyName = "Super Luminary 2", RevitFamilyManufacturer = "Universal Awesome"},
                new RevitFamilyItem(){ RevitFamilyName = "Super Luminary 3", RevitFamilyManufacturer = "Universal Awesome"},
                new RevitFamilyItem(){ RevitFamilyName = "Super Luminary 4", RevitFamilyManufacturer = "Universal Awesome"},
                new RevitFamilyItem(){ RevitFamilyName = "Shitty bullshit", RevitFamilyManufacturer = "Staigātāju kantoris"},
                new RevitFamilyItem(){ RevitFamilyName = "Fapcon exclusive", RevitFamilyManufacturer = "Beepboop"},
            };
        }

        private void btnCancelAction(object obj)
        {
            RevitTransactionResult = Result.Cancelled;
            OnRequestClose(this, new EventArgs());
        }

        private void btnOKAction(object obj)
        {
            RevitTransactionResult = Result.Succeeded;
            OnRequestClose(this, new EventArgs());
        }

    }

    public class UniLumItem : INPC
    {
        private string name;
        private string manufacturer;
        private RevitFamilyItem selectedRevFamItem;


        public UniLumItem()
        {

        }
        public string dwgLumName
        {
            get => name; set
            {
                if (name != value)
                {
                    name = value;
                    MyPropertyChanged(nameof(dwgLumName));
                }

            }
        }

        public string dwgLumManufacturer
        {
            get => manufacturer; set
            {
                if (manufacturer != value)
                {
                    manufacturer = value;
                    MyPropertyChanged(nameof(dwgLumManufacturer));
                }

            }
        }

        public RevitFamilyItem rvtLumFamilyItem
        {
            get => selectedRevFamItem; set
            {
                if(selectedRevFamItem != value)
                {
                    selectedRevFamItem = value;
                    MyPropertyChanged(nameof(rvtLumFamilyItem));
                }
            }
        }

    }


    public class RevitFamilyItem : INPC
    {
        private string revitFamilyName;
        private string revitFamilyManufacturer;

        public string RevitFamilyName
        {
            get => revitFamilyName; set
            {
                if (revitFamilyName != value)
                {
                    revitFamilyName = value;
                    MyPropertyChanged(nameof(RevitFamilyName));
                }

            }
        }
        public string RevitFamilyManufacturer
        {
            get => revitFamilyManufacturer; set
            {
                if(revitFamilyManufacturer != value)
                {
                    revitFamilyManufacturer = value;
                    MyPropertyChanged(nameof(RevitFamilyManufacturer));
                }
            }
        }
    }

}
