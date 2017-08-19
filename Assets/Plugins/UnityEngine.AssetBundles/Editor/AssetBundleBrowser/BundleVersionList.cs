using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.AssetBundles.AssetBundleDataSource;
using UnityEngine.AssetBundles.AssetBundleModel;

namespace UnityEngine.AssetBundles
{
    [Serializable]
    public class BundleVersionList
    { 
        public List<URLVersionPair> BundlesList;

        public URLVersionPair SearchURL(string str)
        {
            return BundlesList.Find(x => x.URL.Equals(str, StringComparison.OrdinalIgnoreCase));
        }

        public void Sort()
        {
            if (BundlesList == null || BundlesList.Count == 0) { return; }
                BundlesList.Sort((p1, p2) => { return p1.URL.CompareTo(p2.URL); });
            return;
        }

        public int SearchIdx(string str)
        {
            return BundlesList.FindIndex(x => x.URL.Equals(str, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Serializable]
    public class URLVersionPair
    {
        public string URL;
        public int NewCRC;
        public bool PreDownload;
    }

    public static class BundleVersionListHelper
    {
        static public void CreateBundleList(ABBuildInfo buildInfo)
        {
            BundleVersionList bundleList = new BundleVersionList();

            bundleList.BundlesList = new List<URLVersionPair>();
            List<BundleInfo> list = new List<BundleInfo>();
            GetAllBundle(Model.m_RootLevelBundles.GetChildList(), list);
            foreach (var item in list)
            {
                bundleList.BundlesList.Add(new URLVersionPair()
                {
                    URL = item.m_Name.fullNativeName,
                    NewCRC = File.ReadAllBytes(buildInfo.outputDirectory + "/" + item.m_Name.fullNativeName).GetHashCode(),
                    PreDownload = item.proLoad
                });
            }

            string json_text = LitJson.JsonMapper.ToJson(bundleList);
            StreamWriter fileWriter = new StreamWriter(buildInfo.outputDirectory + "/BundleList.json");
            fileWriter.WriteLine(json_text);
            fileWriter.Close();
            fileWriter.Dispose();

            AssetDatabase.Refresh();
        }

        static List<BundleInfo> GetAllBundle()
        {
            List<BundleInfo> list = new List<BundleInfo>();
            var dic = AssetBundleModel.Model.m_RootLevelBundles.GetChildList();
            GetAllBundle(dic, list);
            return list;
        }

        static void GetAllBundle(Dictionary<string, BundleInfo>.ValueCollection dic, List<BundleInfo> list)
        {
            foreach (var item in dic)
            {
                BundleFolderInfo folderInfo = item as BundleFolderInfo;
                if (folderInfo != null)
                {
                    var dic2 = folderInfo.GetChildList();
                    GetAllBundle(dic2, list);
                }
                else
                {
                    list.Add(item);
                }
            }
        }
    }
}