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
    public class LumiTagUpdate : HelperOps, IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;
            View activeView = doc.ActiveView;






            Transaction trans = new Transaction(doc);
            trans.Start("Update EpicLumi Annotations");

            RefreshLumiTags(doc);

            trans.Commit();
            return Result.Succeeded;
        }


    }
}
