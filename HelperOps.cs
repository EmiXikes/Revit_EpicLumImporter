using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EpicLumImporter
{
    [Transaction(TransactionMode.Manual)]
    public class HelperOps
    {

        public double GetRotationAngleOfInstance(Document uiDoc, FamilyInstance fi)
        {
            //UIDocument uiDoc = this.ActiveUIDocument;

            //Selection selection = uiDoc.Selection;

            //foreach (ElementId id in selection.GetElementIds())
            //{
                //FamilyInstance fi = uiDoc.Document.GetElement(
                //  id) as FamilyInstance;

                Transform trf = fi.GetTransform();

                XYZ viewDirection = uiDoc.ActiveView.ViewDirection;
                XYZ rightDirection = uiDoc.ActiveView.RightDirection;
                XYZ upDirection = uiDoc.ActiveView.UpDirection;

            //TaskDialog.Show("Trf", String.Format(
            //  "X{0} Y{1} Z{2}\nX{3} Y{4} Z{5}",
            //  trf.BasisX, trf.BasisY, trf.BasisZ,
            //  rightDirection, upDirection, viewDirection));

            //TaskDialog.Show("Rotation Angle", 
            //    (trf.BasisX.AngleOnPlaneTo(rightDirection, viewDirection) * 180 / Math.PI).ToString());

            return trf.BasisX.AngleOnPlaneTo(rightDirection, viewDirection);// * 180 / Math.PI;
            //}
        }

        public void RotateInstance(Document uiDoc, FamilyInstance fi)
        {
            //UIDocument uiDoc = this.ActiveUIDocument;

            //Selection selection = uiDoc.Selection;

            //foreach (ElementId id in selection.GetElementIds())
            //{
            //    FamilyInstance fi = uiDoc.Document.GetElement(
            //      id) as FamilyInstance;

                //using (Transaction t = new Transaction(
                //  uiDoc, "Rotate me"))
                //{
                //    t.Start();

                    LocationPoint lp = fi.Location as LocationPoint;
                    XYZ location = lp.Point;
                    XYZ direction = uiDoc.ActiveView.ViewDirection;
                    Line line = Line.CreateBound(location,
                      location - direction);
                    lp.Rotate(line, Math.PI / 2.0);
                //    t.Commit();
                //}
            //}
        }


        public static ReferencePlane CreateNewRefPlane(Document doc, double wpElevation, string newRefPlaneName)
        {
            ReferencePlane refPlane;
            // Build now data before creation
            XYZ bubbleEnd = new XYZ(1, 0, wpElevation);   // bubble end applied to reference plane
            XYZ freeEnd = new XYZ(0, 1, wpElevation);    // free end applied to reference plane.
            XYZ thirdPnt = new XYZ(1, 1, wpElevation);   // 3rd point to define reference plane.  Third point should not be on the bubbleEnd-freeEnd line 

            // Create the reference plane in X-Y, applying the active view

            FilteredElementCollector planescollector = new FilteredElementCollector(doc);
            List<Element> WorkPlanes = planescollector.OfClass(typeof(ReferencePlane)).ToElements().ToList();

            var wp = WorkPlanes.FirstOrDefault(W => W.Name == newRefPlaneName);
            if (wp != null)
            {
                refPlane = (ReferencePlane)wp;
            }
            else
            {
                refPlane = doc.Create.NewReferencePlane2(bubbleEnd, freeEnd, thirdPnt, doc.ActiveView);
                refPlane.Name = newRefPlaneName;
            }

            return refPlane;
        }


        public static XYZ GetCeilingPoint(View3D view3D, XYZ initialPosition, double revDistance)
        {

            XYZ initialDeltaPosition = initialPosition - new XYZ(0, 0, revDistance);
            XYZ rayDirection = new XYZ(0, 0, 1);

            List<BuiltInCategory> builtInCats = new List<BuiltInCategory>();

            builtInCats.Add(BuiltInCategory.OST_Roofs);
            builtInCats.Add(BuiltInCategory.OST_Ceilings);
            builtInCats.Add(BuiltInCategory.OST_Floors);
            builtInCats.Add(BuiltInCategory.OST_Walls);
            builtInCats.Add(BuiltInCategory.OST_Stairs);

            ElementMulticategoryFilter intersectFilter
              = new ElementMulticategoryFilter(builtInCats);


            ReferenceIntersector refIntersector;
            ReferenceWithContext referenceWithContext;

            refIntersector = new ReferenceIntersector(intersectFilter, FindReferenceTarget.Element, view3D);
            refIntersector.FindReferencesInRevitLinks = true;
            //refIntersector = new ReferenceIntersector(view3D) { FindReferencesInRevitLinks = true };
            referenceWithContext = refIntersector.FindNearest(initialDeltaPosition, rayDirection);

            var fref = refIntersector.Find(initialDeltaPosition, rayDirection);

            if (referenceWithContext != null)
            {
                //var i = referenceWithContext.GetType();
                return referenceWithContext.GetReference().GlobalPoint;
            }

            return initialPosition;
        }
    }
}