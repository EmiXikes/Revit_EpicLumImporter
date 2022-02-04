using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLumi
{
    [Transaction(TransactionMode.Manual)]
    public class LumiSnap : HelperOps, IExternalCommand
    {
        double mmInFt = 304.8;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            List<BuiltInCategory> snapCats = new List<BuiltInCategory>();

            snapCats.Add(BuiltInCategory.OST_Roofs);
            snapCats.Add(BuiltInCategory.OST_Ceilings);
            snapCats.Add(BuiltInCategory.OST_Floors);
            snapCats.Add(BuiltInCategory.OST_Stairs);





            Transaction trans = new Transaction(doc);
            trans.Start("LumiSnap");

            #region Getting saved settings
            // getting saved settings
            LumiSnapSettingsStorage MySettingStorage = new LumiSnapSettingsStorage();
            LumiSnapSettingsData MySettings = MySettingStorage.ReadSettings(doc);
            if (MySettings == null)
            {
                // Default Values
                MySettings = new LumiSnapSettingsData()
                {
                    DistanceFwd = 1500,
                    DistanceRev = 500,
                    ViewName = "Epic LumiSnap ceiling check View"
                };
            }

            #endregion

            #region creating snap check View
            // Getting or creating a new view and setting correct VV settings
            FilteredElementCollector colView = new FilteredElementCollector(doc);
            View3D ceilCheckView = colView.OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault<View3D>(x => x.Name == MySettings.ViewName);

            if (ceilCheckView == null)
            {
                ceilCheckView = CreateNewView(doc, MySettings.ViewName);
            }

            SetVisibleCats(doc, snapCats, ceilCheckView);

            SetVisibleLink(doc, MySettings, ceilCheckView);


            #endregion

            var selection = uidoc.Selection.GetElementIds();


            foreach (ElementId selectedElementId in selection)
            {

                Element selectedElement1 = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)selectedElement1;

                // Adjusting position based on found ceilings
                // Creating new RefPlane for new postition
                double reverseSearchDistance = 500 / mmInFt;
                XYZ initialPoint = (selectedElement1.Location as LocationPoint).Point;
                XYZ snapPoint = GetSnapSurfacePoint(
                    ceilCheckView,
                    initialPoint,
                    MySettings.DistanceRev / mmInFt,
                    MySettings.DistanceFwd / mmInFt,
                    snapCats,
                    out Reference snapRef);

                RecreateAtNewWorkplaneElevation(doc, selectedElementId, snapPoint);

            }

            trans.Commit();
            return Result.Succeeded;

        }


    }

}
