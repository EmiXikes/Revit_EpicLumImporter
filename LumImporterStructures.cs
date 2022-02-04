using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static DEL_acadltlib_EM.FileIO;

namespace EpicLumi
{
    public class LumImporterStructures
    {

    }

    public class ExternalData
    {
        static string pathFamilyNamesList = System.IO.Path.Combine("C:\\Epic\\Revit", "ELI_Fams.txt");
        static string pathLumDwgExportData = System.IO.Path.Combine("C:\\Epic\\Revit", "ELI_DwgLumData.txt");
        static string pathLumDwgSummaryData = System.IO.Path.Combine("C:\\Epic\\Revit", "ELI_DwgLumSummary.txt");
        static string pathLumInfoBlockData = System.IO.Path.Combine("C:\\Epic\\Revit", "ELI_DwgLumInfoBlcks.txt");
        static string pathLumOrigins = System.IO.Path.Combine("C:\\Epic\\Revit", "ELI_DwgLumOrigins.txt");

        public List<LumDataItem> dwgLumData;
        public List<UniqueLumDataItem> dwgLumDataUnique;
        public List<LumInfoBlock> dwgLumInfoBlocks;
        public Vector2 dwgLumInfoOrigins = new Vector2();

        public ExternalData()
        {
            dwgLumData = new List<LumDataItem>();
            dwgLumDataUnique = new List<UniqueLumDataItem>();
            dwgLumInfoBlocks = new List<LumInfoBlock>();
            dwgLumInfoOrigins = new Vector2();

            dwgLumData = LoadObjFromFile<List<LumDataItem>>(pathLumDwgExportData);
            dwgLumDataUnique = LoadObjFromFile<List<UniqueLumDataItem>>(pathLumDwgSummaryData);
            dwgLumInfoBlocks = LoadObjFromFile<List<LumInfoBlock>>(pathLumInfoBlockData);
            dwgLumInfoOrigins = LoadObjFromFile<Vector2>(pathLumOrigins);
        }

    }

    public class GenLumItem // For Revit
    {
        public string name;
        public Level Level;
        public FamilySymbol familySymbol;
        public XYZ location;
        public ReferencePlane workPlane;
        public double Rotation;
        public LumInfoBlock ImportedInfo;
        //public double AdditionalAngle;
        //public bool DontFlipZ;

    }

    public class LumInfoBlock // From AutoCAD
    {
        public Vector2 PointA;
        public Vector2 PointB;
        public string attr_ELGRUPA;
        public string attr_INFO1;
        public string attr_INFO2;
        public string attr_INFO3;
    }

    public class LumDataItem // From AutoCAD
    {
        public string LumModelName;
        public string LumManufacturer;
        public Vector3 Location;
        public double Rotation;
        public string attr_ELGRUPA;
    }

    public class UniqueLumDataItem // From AutoCAD
    {
        public string lumIndex;
        public string manufacturer;
        public string luminaireModelName;
        public string itemNumber;
        public string quantity;
    }



}
