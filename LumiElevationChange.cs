using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EpicLumi.UI.View;
using EpicLumi.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EpicLumi
{
    [Transaction(TransactionMode.Manual)]
    public class LumiElevationChange : HelperOps, IExternalCommand
    {
        double mmInFt = 304.8;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;



            Transaction trans = new Transaction(doc);
            trans.Start("Change Lum Elevation");

            double defaultElev = 0;
            var selection = uidoc.Selection.GetElementIds();
            foreach (ElementId selectedElementId in selection)
            {
                Element selectedElement = doc.GetElement(selectedElementId);
                Level SelectedLevel = GetCorrespondingLevel(doc, selectedElement);

                XYZ initialPoint = (selectedElement.Location as LocationPoint).Point;
                defaultElev = initialPoint.Z - SelectedLevel.Elevation;
                defaultElev = defaultElev * mmInFt;
                break;
            }

            // UI
            Window uiWin = new Window();
            uiWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            uiWin.Width = 100; uiWin.Height = 145;
            uiWin.ResizeMode = ResizeMode.NoResize;
            uiWin.Title = "Change Lum Elevation";

            ChangeElevationVM uiData = new ChangeElevationVM()
            {
                ElevationAtLevel = defaultElev
            };
            uiData.OnRequestClose += (s, e) => uiWin.Close();

            //uiWin.Activated += winActivated;

            uiWin.Content = new ChangeElevationUI();
            uiWin.DataContext = uiData;
            uiWin.ShowDialog();

            double newElevationAtLevel = uiData.ElevationAtLevel;

            foreach (ElementId selectedElementId in selection)
            {
                Element selectedElement = doc.GetElement(selectedElementId);

                XYZ initialPoint = (selectedElement.Location as LocationPoint).Point;
                Level SelectedLevel = GetCorrespondingLevel(doc, selectedElement);



                XYZ newPoint = new XYZ(
                    initialPoint.X,
                    initialPoint.Y,
                    SelectedLevel.Elevation + (newElevationAtLevel / mmInFt)
                    );


                RecreateAtNewWorkplaneElevation(doc, selectedElementId, newPoint);
            }



            trans.Commit();
            return Result.Succeeded;
        }


    }
}
