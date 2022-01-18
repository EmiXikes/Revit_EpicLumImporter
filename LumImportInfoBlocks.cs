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
    public class LumImportInfoBlocks : HelperOps, IExternalCommand
    {
        ExternalData extData;
        double mmInFt = 304.8;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;
            View activeView = doc.ActiveView;

            extData = new ExternalData();

            var infos = extData.dwgLumInfoBlocks;

            var lumInstancesVisibleInView = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_LightingFixtures)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(x => IsElementVisibleInView(doc.ActiveView, x)).ToList();

            XYZ DeltaOrigins = GetOriginDelta(doc);

            List<LumInfoRect> rectList = new List<LumInfoRect>();

            // get data from autocad export and create rectangles
            foreach (var item in infos)
            {
                LumInfoRect LumInfoRect = new LumInfoRect()
                {
                    Rect = new System.Drawing.Rectangle(
                    (int)(item.PointA.X + DeltaOrigins.X * mmInFt),
                    (int)(item.PointA.Y + DeltaOrigins.X * mmInFt),
                    Math.Abs((int)(item.PointA.X - (int)item.PointB.X + DeltaOrigins.X * mmInFt)),
                    Math.Abs((int)(item.PointA.Y - (int)item.PointB.Y + DeltaOrigins.X * mmInFt))
                    ),
                    attr_ELGRUPA = item.attr_ELGRUPA,
                    attr_INFO1 = item.attr_INFO1,
                    attr_INFO2 = item.attr_INFO2,
                    attr_INFO3 = item.attr_INFO3,
                };

                //LumInfoRect LumInfoRect2 = new LumInfoRect()
                //{
                //    Rect = new System.Drawing.Rectangle(
                //        (int)item.PointA.X,
                //        (int)item.PointA.Y,
                //        Math.Abs((int)item.PointA.X - (int)item.PointB.X),
                //        Math.Abs((int)item.PointA.Y - (int)item.PointB.Y)
                //        ),
                //    attr_ELGRUPA = item.attr_ELGRUPA,
                //    attr_INFO1 = item.attr_INFO1,
                //    attr_INFO2 = item.attr_INFO2,
                //    attr_INFO3 = item.attr_INFO3,
                //};

                rectList.Add(LumInfoRect);

            }
                        
            rectList.OrderBy(x => x.Rect.Width * x.Rect.Height).Reverse();

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Lumi Info Import");

            // check if luminarie corrdinates are inside of the rectangles
            foreach (var rect in rectList)
            {
                foreach (var Lum in lumInstancesVisibleInView)
                {
                    XYZ lumPoint = (Lum.Location as LocationPoint).Point;

                    System.Drawing.Point blckCoords = 
                        new System.Drawing.Point(
                            (int)((lumPoint.X + DeltaOrigins.X) * mmInFt),
                            (int)((lumPoint.Y + DeltaOrigins.Y) * mmInFt)
                            );


                    if (rect.Rect.Contains(blckCoords))
                    {
                        Lum.get_Parameter(new System.Guid("41a9849c-f9a0-48fd-8b79-9a51cb222a8e")).Set(rect.attr_ELGRUPA);

                    }
                }

            }


            // update tags
            string ProxyAnnoTagName = "EpicAnnotationProxy";

            var epicAnotationInstances = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_LightingFixtures)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>().Where(x => x.Name == ProxyAnnoTagName).ToList();

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
        private XYZ GetOriginDelta(Document doc)
        {
            var revitLumOrignsInstance = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ElectricalFixtures)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(x => x.Symbol.FamilyName.Contains("LumInfoOrigins")).ToList()[0];

            if (revitLumOrignsInstance == null) { return new XYZ(0, 0, 0); }

            var revitLumOrignsPoint = (revitLumOrignsInstance.Location as LocationPoint).Point;

            XYZ RevitLumInfoOrigins = new XYZ(revitLumOrignsPoint.X, revitLumOrignsPoint.Y, 0);
            XYZ AutoCADLumInfoOrigins = new XYZ(extData.dwgLumInfoOrigins.X / mmInFt, extData.dwgLumInfoOrigins.Y / mmInFt, 0);

            XYZ DeltaOrigins = RevitLumInfoOrigins - AutoCADLumInfoOrigins;
            return DeltaOrigins;
        }

    }
}
