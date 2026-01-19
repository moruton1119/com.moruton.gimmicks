using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MorulabTools
{
    /// <summary>
    /// Tags imported products with a label containing their Product ID.
    /// Used to detect installed products even if folders are moved or renamed.
    /// </summary>
    public class BLMProductTagger : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Only act if we are currently running a queue import and have a valid product ID
            if (!AssetImportQueue.IsImporting) return;
            string pid = AssetImportQueue.CurrentProductId;
            if (string.IsNullOrEmpty(pid)) return;

            // Identify root folders for this import
            // Heuristic: Take the top-most folder under Assets/ that is part of this import
            var roots = new HashSet<string>();

            foreach (var path in importedAssets)
            {
                if (!path.StartsWith("Assets/")) continue;
                
                // e.g. Assets/MyTool/Script.cs -> Assets/MyTool
                // e.g. Assets/MyTool -> Assets/MyTool
                
                string root = GetTopLevelFolder(path);
                if (!string.IsNullOrEmpty(root))
                {
                    roots.Add(root);
                }
            }

            if (roots.Count == 0) return;

            string labelId = $"BLM_PID_{pid}";
            string labelTag = "BLM_Managed";

            foreach (var rootPath in roots)
            {
                // We want to tag the folder "Assets/MyTool"
                AssetDatabase.Refresh(); // Ensure asset is up to date? Postprocess is early.
                
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(rootPath);
                if (asset != null)
                {
                    var labels = new List<string>(AssetDatabase.GetLabels(asset));
                    bool muddy = false;
                    
                    if (!labels.Contains(labelTag)) { labels.Add(labelTag); muddy = true; }
                    if (!labels.Contains(labelId)) { labels.Add(labelId); muddy = true; }

                    if (muddy)
                    {
                        AssetDatabase.SetLabels(asset, labels.ToArray());
                        // Debug.Log($"[BLM] Tagged {rootPath} with {labelId}");
                    }
                }
            }
        }

        private static string GetTopLevelFolder(string path)
        {
            // Input: Assets/Folder/Sub/File
            // Output: Assets/Folder
            var parts = path.Split('/');
            if (parts.Length < 2) return null; // "Assets" itself or weird path
            if (parts.Length == 2) return path; // "Assets/File.txt" -> return file path (valid to label files too)
            
            // Return first directory under Assets
            return $"{parts[0]}/{parts[1]}";
        }
    }
}
