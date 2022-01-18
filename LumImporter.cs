using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EpicLumImporter.UI.ViewModel;
using FireSharp;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static DEL_acadltlib_EM.FileIO;

namespace EpicLumImporter
{
    [Transaction(TransactionMode.Manual)]
    public class LumImporter : HelperOps, IExternalCommand
    {
        Result transResult = Result.Cancelled;

        //static string pathFamilyNamesList = System.IO.Path.Combine("C:\\Epic\\Revit", "ELI_Fams.txt");
        //static string pathLumDwgExportData = System.IO.Path.Combine("C:\\Epic\\Revit", "ELI_DwgLumData.txt");
        //static string pathLumDwgSummaryData = System.IO.Path.Combine("C:\\Epic\\Revit", "ELI_DwgLumSummary.txt");
        //static string pathLumInfoBlockData = System.IO.Path.Combine("C:\\Epic\\Revit", "ELI_DwgLumInfoBlcks.txt");
        //static string pathLumOrigins = System.IO.Path.Combine("C:\\Epic\\Revit", "ELI_DwgLumOrigins.txt");

        double mmInFt = 304.8;

        //List<LumDataItem> dwgLumData;
        //List<UniqueLumDataItem> dwgLumDataUnique;
        //List<LumInfoBlock> dwgLumInfoBlocks;
        //Vector2 dwgLumInfoOrigins = new Vector2();

        //private void ReloadExternalData()
        //{
        //    dwgLumData = new List<LumDataItem>();
        //    dwgLumDataUnique = new List<UniqueLumDataItem>();
        //    dwgLumInfoBlocks = new List<LumInfoBlock>();

        //    dwgLumData = LoadObjFromFile<List<LumDataItem>>(pathLumDwgExportData);
        //    dwgLumDataUnique = LoadObjFromFile<List<UniqueLumDataItem>>(pathLumDwgSummaryData);
        //    dwgLumInfoBlocks = LoadObjFromFile<List<LumInfoBlock>>(pathLumInfoBlockData);
        //    dwgLumInfoOrigins = LoadObjFromFile<Vector2>(pathLumOrigins);
        //}

        ExternalData extData;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Luminaire Import");

            extData = new ExternalData();
            //ReloadExternalData();

            #region Collectors

            FilteredElementCollector lightcollector = new FilteredElementCollector(doc);
            List<Element> rvtLightingFixtures = lightcollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_LightingFixtures).ToList();

            FilteredElementCollector electricalFixturesCollector = new FilteredElementCollector(doc);
            List<Element> rvtElectricalFixtures = electricalFixturesCollector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_ElectricalFixtures).ToList();

            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            List<Element> rvtLevels = levelCollector.OfClass(typeof(Level)).ToElements().ToList();

            List<Document> linkedDocs = new List<Document>();
            List<string> linkedDocsStr = new List<string>();
            foreach (Document LinkedDoc in uiapp.Application.Documents)
            {
                if (LinkedDoc.IsLinked)
                {
                    linkedDocs.Add(LinkedDoc);
                    linkedDocsStr.Add(System.IO.Path.GetFileName(LinkedDoc.PathName));
                }
            }

            #endregion

            #region Debug Log Info

            Debug.Print("\nLighting Fixtures");
            foreach (Element LightingFixture in rvtLightingFixtures)
            {
                Debug.Print(string.Format("{0} [{1}]", LightingFixture.Name, LightingFixture.Id));
            }

            Debug.Print("\nLevels");
            foreach (Level lvl in rvtLevels)
            {
                Debug.Print(string.Format("{0} [{1}]", lvl.Name, lvl.Id));
            }
            Debug.Print("\nLuminaire Elevations");
            var LumsByElevation1 = extData.dwgLumData.GroupBy(L => L.Location.Z).ToList();
            foreach (var LumElevation in LumsByElevation1)
            {
                Debug.Print(string.Format("{0}  [{1}]", LumElevation.Key, LumElevation.Count()));
            }

            #endregion

            FirebaseClient fb = FireBase.FireBaseInit();
            var taskFbLumData = Task.Run(async () => await FireBase.GetLumsFromFB(fb));
            var fbLumData = taskFbLumData.WaitAndUnwrapException();

            #region UI
            // UI
            Window ImporterUI = new Window();
            ImporterUI.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ImporterUI.Width = 820; ImporterUI.Height = 550;
            ImporterUI.ResizeMode = ResizeMode.NoResize;
            ImporterUI.Title = "Epic Luminaire Importer";

            // Preparing UI values
            MainViewModel uiData = new MainViewModel();
            uiData.Test = "Testing... 123";
            uiData.LevelData = new List<string>();
            uiData.CeilCorrectionLinks = linkedDocsStr;
            foreach (var item in rvtLevels)
            {
                uiData.LevelData.Add(item.Name);
            }

            // Name list for availible rvt families
            uiData.RevitFamilyData = new List<RevitFamilyItem>();
            foreach (var item in rvtLightingFixtures)
            {
                uiData.RevitFamilyData.Add(new RevitFamilyItem
                {
                    RevitFamilyName = item.Name,
                    RevitFamilyManufacturer = item.LookupParameter("Manufacturer").AsValueString()
                });
            }

            // List for lum names from autocad export. Also checking if there are any matches in the saved FB database.
            uiData.UniLumData = new List<UniLumItem>();
            foreach (var item in extData.dwgLumDataUnique)
            {
                RevitFamilyItem selFamItem = new RevitFamilyItem();
                if (fbLumData != null)  // check if any firebase items found
                {
                    var S = fbLumData.FirstOrDefault(x => x.Value.dwgLumName == item.luminaireModelName);
                    if (S.Value != null) // check if firebase items contain the given LUM name from dwg export
                    {
                        var selFamItemFB = uiData.RevitFamilyData.FirstOrDefault(x => x.RevitFamilyName == S.Value.rvtLumFamName);
                        if (selFamItemFB != null)  // check if FB result can be matched to availible rvt family names
                        {
                            selFamItem = selFamItemFB; // return matched family
                        }
                        
                    }

                }

                // new entry for
                uiData.UniLumData.Add(new UniLumItem()
                {
                    dwgLumName = item.luminaireModelName,
                    dwgLumManufacturer = item.manufacturer,
                    rvtLumFamilyItem = selFamItem,
                });
            }

            uiData.OnRequestClose += (s, e) => ImporterUI.Close();

            // Display the UI
            ImporterUI.Content = new UI.View.MainPanel();
            ImporterUI.DataContext = uiData;
            ImporterUI.ShowDialog();

            #endregion

            transResult = uiData.RevitTransactionResult;

            if (transResult == Result.Cancelled)
            {
                trans.Dispose();
                return Result.Cancelled;
            }

            // Get values from UI

            // Save data to FireBase
            string str = "";
            foreach (var item in uiData.UniLumData)
            {
                str += item.dwgLumName + "   --->   " + item.rvtLumFamilyItem.RevitFamilyName + "\n";

                // check if matching entry already exists in FB
                var existingMatchingFireBaseEntry = fbLumData.FirstOrDefault(x => x.Value.dwgLumName == item.dwgLumName);
                if (existingMatchingFireBaseEntry.Key == "" || existingMatchingFireBaseEntry.Key == null)
                {
                    // add new if doesn't exist
                    var task = Task.Run(async () => await FireBase.PushLumToFB(
                        fb,
                        new LumSelectionData
                        {
                            dwgLumName = item.dwgLumName,
                            rvtLumFamName = item.rvtLumFamilyItem.RevitFamilyName
                        }
                        ));
                    task.WaitAndUnwrapException();
                }
                else
                {
                    // modify if exists and is different
                    if (existingMatchingFireBaseEntry.Value.rvtLumFamName != item.rvtLumFamilyItem.RevitFamilyName)
                    {
                        var task = Task.Run(async () => await FireBase.SetLumFB(
                            fb,
                            new LumSelectionData
                            {
                                dwgLumName = item.dwgLumName,
                                rvtLumFamName = item.rvtLumFamilyItem.RevitFamilyName
                            },
                            existingMatchingFireBaseEntry.Key
                            ));
                    }
                }
            }
            Debug.Print(str);

            double floorOffset = double.Parse(uiData.LevelOffset, System.Globalization.CultureInfo.CurrentCulture) / mmInFt;

            // Level
            Level SelectedLevel = (Level)rvtLevels.FirstOrDefault(L => L.Name == uiData.SelectedLevel);

            // Generating Workplanes and building to be generated Lum list

            List<GenLumItem> LumsToGenerate = new List<GenLumItem>();

            var LumsByElevation = extData.dwgLumData.GroupBy(L => L.Location.Z).ToList();
            foreach (var LumElevation in LumsByElevation)
            {
                double wpElevation = SelectedLevel.Elevation + floorOffset + LumElevation.Key / mmInFt;
                string newRefPlaneName = String.Format(
                    "EpicLumWorkPlane_{0}_EL{1}",
                    SelectedLevel.Name, (int)(LumElevation.Key + floorOffset)
                    );

                ReferencePlane refPlane = CreateNewRefPlane(doc, wpElevation, newRefPlaneName);

                foreach (var item in LumElevation)
                {
                    var revitFamName = uiData.UniLumData.FirstOrDefault(x => x.dwgLumName == item.LumModelName).rvtLumFamilyItem.RevitFamilyName;

                    LumsToGenerate.Add(new GenLumItem
                    {
                        Level = SelectedLevel,
                        workPlane = refPlane,
                        location = new XYZ(item.Location.X, item.Location.Y, item.Location.Z),
                        Rotation = item.Rotation,
                        name = item.LumModelName,
                        familySymbol = (FamilySymbol)rvtLightingFixtures.FirstOrDefault(F => F.Name == revitFamName),
                        ImportedInfo = new LumInfoBlock() { attr_ELGRUPA = item.attr_ELGRUPA }
                    }); ;
                }
            }

            // Creating new family instances

            XYZ DeltaOrigins = GetOriginDelta(doc);

            foreach (var Lum in LumsToGenerate)
            {
                FamilyInstance instance = GenerateLumInstance(doc, DeltaOrigins, Lum);

                var elConnectionParam = instance.get_Parameter(new System.Guid("41a9849c-f9a0-48fd-8b79-9a51cb222a8e")).Set(Lum.ImportedInfo.attr_ELGRUPA);
            }

            // ending





            System.Windows.Forms.MessageBox.Show(
                String.Format("Lums: {0}\nUnique Lums: {1}\nInfoBlocks: {2}",
                extData.dwgLumData.Count.ToString(), extData.dwgLumDataUnique.Count.ToString(), extData.dwgLumInfoBlocks.Count.ToString()), "Success!!");

            trans.Commit();
            return transResult;
        }

        private FamilyInstance GenerateLumInstance(Document doc, XYZ DeltaOrigins, GenLumItem Lum)
        {
            XYZ refDir = new XYZ(1, 0, 0);
            XYZ locationFeet = new XYZ(
                (Lum.location.X / mmInFt) + DeltaOrigins.X,
                (Lum.location.Y / mmInFt) + DeltaOrigins.Y,
                0
                );

            var familySymbol = Lum.familySymbol;
            familySymbol.Activate();
            FamilyInstance instance = doc.Create.NewFamilyInstance(
                Lum.workPlane.GetReference(), locationFeet, refDir, familySymbol);

            XYZ axisPoint1 = locationFeet;
            XYZ axisPoint2 = new XYZ(
                locationFeet.X,
                locationFeet.Y,
                locationFeet.Z + 1
                );

            Line axis = Line.CreateBound(axisPoint1, axisPoint2);
            double setRotation = Lum.Rotation;

            ElementTransformUtils.RotateElement(
                doc,
                instance.Id,
                axis,
                setRotation);
            return instance;
        }

        private XYZ GetOriginDelta(Document doc)
        {
            var revitLumOrignsInstance = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ElectricalFixtures)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(x => x.Symbol.FamilyName.Contains("LumInfoOrigins")).ToList()[0];

            if(revitLumOrignsInstance == null) { return new XYZ(0,0,0); }

            var revitLumOrignsPoint = (revitLumOrignsInstance.Location as LocationPoint).Point;

            XYZ RevitLumInfoOrigins = new XYZ(revitLumOrignsPoint.X, revitLumOrignsPoint.Y, 0);
            XYZ AutoCADLumInfoOrigins = new XYZ(extData.dwgLumInfoOrigins.X / mmInFt, extData.dwgLumInfoOrigins.Y / mmInFt, 0);

            XYZ DeltaOrigins = RevitLumInfoOrigins - AutoCADLumInfoOrigins;
            return DeltaOrigins;
        }
    }


}
