using FireSharp;
using FireSharp.Config;
using FireSharp.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLumImporter
{
    public static class FireBase
    {
        

        public static FirebaseClient FireBaseInit()
        {
            FirebaseConfig EpicFireBaseConfig = new FirebaseConfig() { 
                AuthSecret = "ZbZYBvyIhzjr8076TgvHuLdEyUN80fVu6G2CgkVU", 
                BasePath = "https://del-epic.firebaseio.com/" };

            return new FirebaseClient(EpicFireBaseConfig);

        }

        public static async Task PushLumToFB(FirebaseClient fbClient, LumSelectionData data)
        {
            try
            {
                Debug.Print("Saving to FireBase: " + data.dwgLumName);
                PushResponse response = await fbClient.PushAsync("EpicLumi/LumSelectionData", data);
                Debug.Print("Status for " + data.dwgLumName + " : " + response.StatusCode.ToString());

                //PushResponse r2 = await fbClient.

            }
            catch (Exception ex)
            {
                Debug.Print("FB error: " + ex.Message);
            }
        }

        public static async Task SetLumFB(FirebaseClient fbClient, LumSelectionData data, string key)
        {
            try
            {
                Debug.Print("Updating to FireBase: " + data.dwgLumName);
                SetResponse response = await fbClient.SetAsync("EpicLumi/LumSelectionData/" + key, data);
                Debug.Print("Status for " + data.dwgLumName + " : " + response.StatusCode.ToString());

                //PushResponse r2 = await fbClient.

            }
            catch (Exception ex)
            {
                Debug.Print("FB error: " + ex.Message);
            }
        }


        public static async Task<Dictionary<string, LumSelectionData>> GetLumsFromFB(FirebaseClient fbClient)
        {
            FirebaseResponse FBDataJson = await fbClient.GetAsync("EpicLumi/LumSelectionData");

            var FBData = JsonConvert.DeserializeObject<IDictionary<string, LumSelectionData>>(FBDataJson.Body);

            return new Dictionary<string, LumSelectionData>(FBData);

        }
    }

    public class LumSelectionData
    {
        public string dwgLumName { get; set; }
        public string rvtLumFamName { get; set; }
    }

}
