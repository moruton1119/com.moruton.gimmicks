using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace MorulabTools.BoothLibraryManager
{
    public static class LocalAssetService
    {
        private static string ConfigPath => "Assets/MorulabTools/BoothLibraryManager/local_assets.json";

        [Serializable]
        private class LocalData
        {
            public List<BoothProduct> products = new List<BoothProduct>();
        }

        public static List<BoothProduct> LoadLocalAssets()
        {
            if (!File.Exists(ConfigPath)) return new List<BoothProduct>();

            try
            {
                string json = File.ReadAllText(ConfigPath);
                var data = JsonUtility.FromJson<LocalData>(json);
                return data.products ?? new List<BoothProduct>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BLM] Failed to load local assets: {ex.Message}");
                return new List<BoothProduct>();
            }
        }

        public static void SaveLocalAssets(List<BoothProduct> products)
        {
            try
            {
                var data = new LocalData { products = products };
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(ConfigPath, json);
                AssetDatabase.ImportAsset(ConfigPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BLM] Failed to save local assets: {ex.Message}");
            }
        }

        public static void RegisterFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;

            var product = new BoothProduct
            {
                id = "local_" + Guid.NewGuid().ToString().Substring(0, 8),
                name = Path.GetFileName(folderPath),
                shopName = "Local",
                rootFolderPath = folderPath
            };

            // Scan for unitypackages
            var files = Directory.GetFiles(folderPath, "*.unitypackage", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                product.packages.Add(new BoothPackage
                {
                    fileName = Path.GetFileName(file),
                    fullPath = file
                });
            }

            var current = LoadLocalAssets();
            current.Add(product);
            SaveLocalAssets(current);
        }
    }
}
