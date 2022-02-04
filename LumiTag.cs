using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLumi
{
    [Transaction(TransactionMode.Manual)]
    public class LumiTag : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;
            View activeView = doc.ActiveView;

            string ProxyFamName = "EpicLumiProxy";
            string ProxyTagName = "EpicLumiProxyTag";

            #region Collectors

            FilteredElementCollector lightcollector = new FilteredElementCollector(doc);
            List<Element> rvtLightingFixtures = lightcollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_LightingFixtures).ToList();

            FilteredElementCollector lightTag = new FilteredElementCollector(doc);
            List<Element> rvtLightingTags = lightTag.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_LightingFixtureTags).ToList();

            var ProxyAnnoTag = rvtLightingTags.FirstOrDefault(t => t.Name == ProxyTagName);

            ElementId ProxyAnnoTagId = ProxyAnnoTag.Id;

            FilteredElementCollector lvlCollector = new FilteredElementCollector(doc);
            ICollection<Element> lvlCollection = lvlCollector.OfClass(typeof(Level)).ToElements();

            Parameter associatedlevelName = activeView.LookupParameter("Associated Level");
            Level associatedLevel = null;
            foreach (Element l in lvlCollection)
            {
                Level lvl = l as Level;
                if (lvl.Name == associatedlevelName.AsString())
                {
                    associatedLevel = lvl;
                }
            }
            #endregion

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Lumi Annotate");

            var selection = uidoc.Selection.GetElementIds();

            IList<ElementId> selectedIds = (IList<ElementId>)selection;

            XYZ pickedPoint = PickPoint(uidoc);

            FamilySymbol familySymbol = (FamilySymbol)rvtLightingFixtures.FirstOrDefault(F => F.Name == ProxyFamName);
            familySymbol.Activate();
            FamilyInstance instance = doc.Create.NewFamilyInstance(
                pickedPoint + new XYZ(0, 0, 2),
                familySymbol,
                associatedLevel,
                Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            Entity entity = new Entity(LumiAnnoProxySchema.GetSchema());
            entity.Set<IList<ElementId>>("LumiIds", selectedIds);
            instance.SetEntity(entity);


            string resultTxt = GetConnectionAnnoProxyData(doc, instance);


            var proxyAnnoParam = instance.get_Parameter(new System.Guid("4ad138e7-de2f-430e-96bc-f3387e6186ca"));
            if (proxyAnnoParam != null)
            {
                proxyAnnoParam.Set(resultTxt);
            }


            //TaskDialog.Show("Revit", resultTxt);

            XYZ testPoint = (instance.Location as LocationPoint).Point;

            IndependentTag newTag = IndependentTag.Create(
                doc,
                activeView.Id,
                new Reference(instance),
                false,
                TagMode.TM_ADDBY_CATEGORY,
                TagOrientation.Horizontal,
                pickedPoint);

            newTag.ChangeTypeId(ProxyAnnoTagId);

            trans.Commit();
            return Result.Succeeded;
        }


    }
}
