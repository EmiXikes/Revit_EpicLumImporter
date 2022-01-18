using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLumImporter
{
    [Transaction(TransactionMode.Manual)]
    public class LumiAnnotationUpdate : HelperOps, IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;
            View activeView = doc.ActiveView;

            string ProxyAnnoTagName = "EpicAnnotationProxy";

            var epicAnotationInstances = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_LightingFixtures)    
                .OfClass(typeof(FamilyInstance))    
                .Cast<FamilyInstance>().Where(x=>x.Name == ProxyAnnoTagName).ToList();




            Transaction trans = new Transaction(doc);
            trans.Start("Update EpicLumi Annotations");

            foreach (var fi in epicAnotationInstances)
            {
                string epicAnnotationTxt = GetConnectionAnnoProxyData(doc, fi);

                var proxyAnnoParam = fi.get_Parameter(new System.Guid("4ad138e7-de2f-430e-96bc-f3387e6186ca"));
                if (proxyAnnoParam != null)
                {
                    proxyAnnoParam.Set(epicAnnotationTxt);
                }
            }

            trans.Commit();
            return Result.Succeeded;
        }
    }
}
