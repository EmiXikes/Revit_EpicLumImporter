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
    internal class LumiFlip : HelperOps, IExternalCommand
    {
        double mmInFt = 304.8;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Transaction trans = new Transaction(doc);
            trans.Start("LumiFlip");


            var selection = uidoc.Selection.GetElementIds();
            foreach (ElementId selectedElementId in selection)
            {
                Element selectedElement1 = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)selectedElement1;
                bool flipState = selectedFamInstance.IsWorkPlaneFlipped;
                selectedFamInstance.IsWorkPlaneFlipped = !flipState;

            }

            trans.Commit();
            return Result.Succeeded;

        }
    }
}
