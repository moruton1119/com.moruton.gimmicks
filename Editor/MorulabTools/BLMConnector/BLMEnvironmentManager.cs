using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MorulabTools
{
    /// <summary>
    /// Manages the environment for BLM Connector.
    /// - Checks for SQLite dependencies.
    /// - Resolves conflicts if multiple SQLite DLLs are found.
    /// - Defines 'MORULAB_HAS_SQLITE' symbol if dependencies are valid.
    /// </summary>
    [InitializeOnLoad]
    public static class BLMEnvironmentManager
    {
        private const string DEFINE_SYMBOL = "MORULAB_HAS_SQLITE";
        private const string DLL_SQLITE_NATIVE = "sqlite3.dll";
        private const string DLL_SQLITE_MANAGED = "Mono.Data.Sqlite.dll";
        
        // Path in our package
        private static string MyPluginPath => "Assets/MorulabTools/BLMConnector/Plugins";

        static BLMEnvironmentManager()
        {
            EditorApplication.delayCall += Initialize;
        }

        private static void Initialize()
        {
            // 1. Scan for DLLs in the entire project
            var nativeResult = FindDlls(DLL_SQLITE_NATIVE);
            var managedResult = FindDlls(DLL_SQLITE_MANAGED);

            bool hasConflict = nativeResult.hasConflict || managedResult.hasConflict;
            
            if (hasConflict)
            {
                // Conflict detected!
                // We have copies in our folder, but copies exist elsewhere too.
                
                // Strategy: Prioritize User's existing plugins.
                // Ask user to confirm cleanup of our redundant DLLs.
                // Or just do it if we are sure?
                // User requested: "Click delete -> auto delete".
                
                bool userAgreed = EditorUtility.DisplayDialog(
                    "BLM Connector - Dependency Conflict",
                    "Duplicate SQLite libraries were detected in your project.\n" +
                    "This usually happens if another asset uses SQLite.\n\n" +
                    "To prevent errors, BLM Connector needs to remove its internal copies and use the existing ones.\n\n" +
                    "Proceed with cleanup?",
                    "Fix Conflict (Delete BLM Copies)", "Cancel");

                if (userAgreed)
                {
                    ResolveConflict(nativeResult);
                    ResolveConflict(managedResult);
                    AssetDatabase.Refresh();
                    // After refresh, domain reload happens, logic runs again to confirm.
                    return;
                }
                else
                {
                    // User canceled. We cannot safely enable the tool.
                    Debug.LogWarning("[BLM] Dependency conflict not resolved. Tool disabled.");
                    SetDefineSymbol(false);
                    return;
                }
            }

            // 2. No Conflict (or resolved). Check if we have valid DLLs (ours or verified externals).
            bool nativeExists = nativeResult.paths.Count > 0;
            bool managedExists = managedResult.paths.Count > 0;

            if (nativeExists && managedExists)
            {
                // All good. Enable the feature.
                SetDefineSymbol(true);
            }
            else
            {
                // Missing dependencies.
                // SetDefineSymbol(false); 
                // We could prompt for download here, but usually package has them.
                // If package import failed or user deleted them manually.
            }
        }

        private struct DllSearchResult
        {
            public System.Collections.Generic.List<string> paths;
            public bool hasConflict; // True if found in MyPath AND somewhere else
            public bool hasMyCopy;
        }

        private static DllSearchResult FindDlls(string filename)
        {
            // Use AssetDatabase to find Assets
            // filename search: "t:Object filename" doesn't work well for DLLs always.
            // Directory traversal is safer but slow?
            // AssetDatabase.FindAssets is name based.
            
            var paths = new System.Collections.Generic.List<string>();
            
            // "sqlite3" or "Mono.Data.Sqlite"
            string nameNoExt = Path.GetFileNameWithoutExtension(filename);
            string[] guids = AssetDatabase.FindAssets(nameNoExt);
            
            bool hasMyCopy = false;
            bool hasOtherCopy = false;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(filename, StringComparison.OrdinalIgnoreCase)) continue;
                
                paths.Add(path);
                
                if (path.Contains("MorulabTools/BLMConnector/Plugins")) hasMyCopy = true;
                else hasOtherCopy = true;
            }

            return new DllSearchResult 
            { 
                paths = paths, 
                hasMyCopy = hasMyCopy, 
                hasConflict = (hasMyCopy && hasOtherCopy) 
            };
        }

        private static void ResolveConflict(DllSearchResult result)
        {
            if (!result.hasConflict) return;

            foreach (var path in result.paths)
            {
                if (path.Contains("MorulabTools/BLMConnector/Plugins"))
                {
                    AssetDatabase.DeleteAsset(path);
                    Debug.Log($"[BLM] Removed redundant dependency: {path}");
                }
            }
        }

        private static void SetDefineSymbol(bool enable)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defines = new System.Collections.Generic.List<string>(definesString.Split(';'));
            
            bool changed = false;
            if (enable)
            {
                if (!defines.Contains(DEFINE_SYMBOL))
                {
                    defines.Add(DEFINE_SYMBOL);
                    changed = true;
                    Debug.Log($"[BLM] Dependencies validated. Enabled {DEFINE_SYMBOL}.");
                }
            }
            else
            {
                if (defines.Contains(DEFINE_SYMBOL))
                {
                    defines.Remove(DEFINE_SYMBOL);
                    changed = true;
                    Debug.Log($"[BLM] Dependencies invalid. Disabled {DEFINE_SYMBOL}.");
                }
            }

            if (changed)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", defines));
            }
        }
    }
}
