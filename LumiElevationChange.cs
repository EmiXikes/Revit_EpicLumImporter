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

            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            List<Element> rvtLevels = levelCollector.OfClass(typeof(Level)).ToElements().ToList();

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
                ElevationAtLevel = Math.Round(defaultElev, 2),
                UseAutoLevel = true,
                ElevLevels = new System.Collections.ObjectModel.ObservableCollection<string>()
            };
            uiData.OnRequestClose += (s, e) => uiWin.Close();

            foreach (string item in (from L in rvtLevels select L.Name).ToList())
            {
                uiData.ElevLevels.Add(item);
            }

            //uiWin.Activated += winActivated;

            uiWin.Content = new ChangeElevationUI();
            uiWin.DataContext = uiData;
            uiWin.ShowDialog();

            double newElevationAtLevel = uiData.ElevationAtLevel;
            bool UseAutoLevel = uiData.UseAutoLevel;
            string UserSelectedLevelName = "";//uiData.UserSelectedLevel;

            foreach (ElementId selectedElementId in selection)
            {
                Element selectedElement = doc.GetElement(selectedElementId);
                XYZ initialPoint = (selectedElement.Location as LocationPoint).Point;
                XYZ newPoint = initialPoint;
                Level UserSelectedLevel = null;

                if (UseAutoLevel)
                {
                    Level SelectedLevel = GetCorrespondingLevel(doc, selectedElement);

                    newPoint = new XYZ(
                        initialPoint.X,
                        initialPoint.Y,
                        SelectedLevel.Elevation + (newElevationAtLevel / mmInFt)
                        );
                }
                else
                {
                    Level SelectedLevel = GetCorrespondingLevel(doc, selectedElement);
                    UserSelectedLevel = (Level)rvtLevels.FirstOrDefault(L => L.Name == UserSelectedLevelName);
                    SelectedLevel = UserSelectedLevel;

                    newPoint = new XYZ(
                        initialPoint.X,
                        initialPoint.Y,
                        SelectedLevel.Elevation + (newElevationAtLevel / mmInFt)
                        );
                }

                RecreateAtNewWorkplaneElevation(doc, selectedElementId, newPoint, UserSelectedLevel);
            }



            trans.Commit();
            return Result.Succeeded;
        }



    }
}
