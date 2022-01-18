using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
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


        public static class LumiAnnoProxySchema
        {
            readonly static Guid schemaGuid = new Guid(
              "F0A1D091-5064-45CE-8105-0F6774AC32E3");  // change this

            public static Schema GetSchema()
            {
                Schema schema = Schema.Lookup(schemaGuid);

                if (schema != null) return schema;

                SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);

                schemaBuilder.SetSchemaName("LumiAnnoProxyData");

                //FieldBuilder myField;

                schemaBuilder.AddArrayField("LumiIds", typeof(ElementId));

                return schemaBuilder.Finish();
            }
        }

        public static string GetConnectionAnnoProxyData(Document doc, FamilyInstance instance)
        {
            Entity retrievedEntity = instance.GetEntity(LumiAnnoProxySchema.GetSchema());
            IList<ElementId> assignedLumIds = retrievedEntity.Get<IList<ElementId>>("LumiIds");


            List<string> selectionELConnections = new List<string>();
            List<string> selectionELPositions = new List<string>();

            foreach (ElementId id in assignedLumIds)
            {
                Element selectedElement = doc.GetElement(id);

                if (selectedElement == null) continue;

                FamilyInstance selectedFamInstance = (FamilyInstance)selectedElement;
                ElementType selectedType = doc.GetElement(selectedElement.GetTypeId()) as ElementType;

                string paramElConnection = "";
                string paramElPos = "";
                var p = selectedFamInstance.get_Parameter(new System.Guid("41a9849c-f9a0-48fd-8b79-9a51cb222a8e"));
                if (p != null)
                {
                    paramElConnection = p.AsString();
                }
                p = selectedType.get_Parameter(new System.Guid("4ad68b64-b7cf-4e80-a76f-660a4aadc4c1"));
                if (p != null)
                {
                    paramElPos = p.AsString();
                }

                selectionELConnections.Add(paramElConnection);
                selectionELPositions.Add(paramElPos);
            }

            Dictionary<string, List<string>> groupedConnections = selectionELConnections.GroupBy(x => x).ToDictionary(g => g.Key, g => g.ToList());
            Dictionary<string, List<string>> groupedPositions = selectionELPositions.GroupBy(x => x).ToDictionary(g => g.Key, g => g.ToList());

            string resultTxt = "";

            foreach (string connectionTxt in groupedConnections.Keys)
            {
                resultTxt = resultTxt + connectionTxt + "\n";
            }

            foreach (var positionValPair in groupedPositions)
            {
                if (positionValPair.Value.Count == 1)
                {
                    resultTxt = resultTxt + positionValPair.Key + "\n";
                }
                else
                {
                    resultTxt = resultTxt + positionValPair.Value.Count.ToString() + "x" + positionValPair.Key + "\n";
                }
            }

            return resultTxt;
        }

        public XYZ PickPoint(UIDocument uidoc)
        {
            ObjectSnapTypes snapTypes = ObjectSnapTypes.None;
            XYZ point = uidoc.Selection.PickPoint(snapTypes, "Select an end point or intersection");

            return point;
            //string strCoords = "Selected point is " + point.ToString();

            //TaskDialog.Show("Revit", strCoords);
        }

        public static bool IsElementVisibleInView(View view ,Element el)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            } 

            if (el == null)
            {
                throw new ArgumentNullException(nameof(el));
            }

            // Obtain the element's document
            Document doc = el.Document;

            ElementId elId = el.Id;

            // Create a FilterRule that searches for an element matching the given Id
            FilterRule idRule = ParameterFilterRuleFactory.CreateEqualsRule(new ElementId(BuiltInParameter.ID_PARAM), elId);
            var idFilter = new ElementParameterFilter(idRule);

            // Use an ElementCategoryFilter to speed up the search, as ElementParameterFilter is a slow filter
            Category cat = el.Category;
            var catFilter = new ElementCategoryFilter(cat.Id);

            // Use the constructor of FilteredElementCollector that accepts a view id as a parameter to only search that view
            // Also use the WhereElementIsNotElementType filter to eliminate element types
            FilteredElementCollector collector =
                new FilteredElementCollector(doc, view.Id).WhereElementIsNotElementType().WherePasses(catFilter).WherePasses(idFilter);

            // If the collector contains any items, then we know that the element is visible in the given view
            return collector.Any();
        }




        public class LumInfoRect
        {
            public System.Drawing.Rectangle Rect;
            public string attr_ELGRUPA;
            public string attr_INFO1;
            public string attr_INFO2;
            public string attr_INFO3;

        }

    }
}