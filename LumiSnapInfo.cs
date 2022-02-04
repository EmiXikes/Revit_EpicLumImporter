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
    public class LumiSnapInfo : HelperOps, IExternalCommand
    {
        UIApplication uiapp;
        UIDocument uidoc;
        Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

            Transaction trans = new Transaction(doc);
            trans.Start("Lumi Info");

            System.Windows.Forms.MessageBox.Show("Epic Tools © 2022 EDGARS.M, DAINA EL", "About");

            trans.Commit();
            return Result.Succeeded;
        }
    }
}
