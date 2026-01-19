using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MorulabTools
{
    /// <summary>
    /// Handles sequential importing of .unitypackage files to prevent Unity crashes.
    /// Persists state across domain reloads using EditorPrefs.
    /// </summary>
    [InitializeOnLoad]
    public static class AssetImportQueue
    {
        private const string PREF_QUEUE = "BLM_ImportQueue_List";
        private const string PREF_IS_IMPORTING = "BLM_ImportQueue_IsImporting";
        private const string PREF_INTERACTIVE = "BLM_ImportQueue_Interactive";

        [Serializable]
        private class QueueItem
        {
            public string path;
            public string productId;
        }

        private static Queue<QueueItem> importQueue = new Queue<QueueItem>();
        private static bool isImporting = false;
        private static QueueItem currentItem = null;

        static AssetImportQueue()
        {
            LoadState();
            if (isImporting && importQueue.Count > 0)
            {
                // We were interrupted by a domain reload (likely script compilation in the previous package)
                // Resume after a short delay to let Unity settle
                EditorApplication.delayCall += () =>
                {
                    Debug.Log("[BLM] Resuming import queue after domain reload...");
                    isImporting = false; // Reset flag to allow CheckQueue to trigger ProcessNext
                    CheckQueue();
                };
            }
            else if (isImporting)
            {
                // Importing flag was true but queue is empty? just reset.
                isImporting = false;
                SaveState();
            }
        }

        public static bool InteractiveMode
        {
            get => EditorPrefs.GetBool(PREF_INTERACTIVE, true);
            set => EditorPrefs.SetBool(PREF_INTERACTIVE, value);
        }


        public static void Enqueue(string packagePath, string productId)
        {
            if (string.IsNullOrEmpty(packagePath)) return;
            if (Contains(packagePath)) return;
            importQueue.Enqueue(new QueueItem { path = packagePath, productId = productId });
            SaveState();
        }

        public static void EnqueueMultiple(IEnumerable<string> packagePaths, string productId)
        {
            foreach (var path in packagePaths)
            {
                if (!string.IsNullOrEmpty(path) && !Contains(path))
                    importQueue.Enqueue(new QueueItem { path = path, productId = productId });
            }
            SaveState();
        }

        private static bool Contains(string path)
        {
            foreach (var item in importQueue) if (item.path == path) return true;
            return false;
        }

        public static void StartImport()
        {
            Debug.Log("[BLM] StartImport requested.");

            // If the user clicks "Process Queue" and we are stuck (isImporting=true but nothing happening), 
            // force a reset to continue.
            if (isImporting)
            {
                Debug.LogWarning("[BLM] Import system thinks it is already running. If stuck, trigger a domain reload or restart Unity.");
                return;
            }

            if (importQueue.Count == 0)
            {
                Debug.Log("[BLM] Queue is empty.");
                return;
            }
            CheckQueue();
        }

        public static void ClearQueue()
        {
            importQueue.Clear();
            isImporting = false;
            SaveState();
            Debug.Log("[BLM] Queue Cleared.");
        }

        private static void ProcessNext()
        {
            if (importQueue.Count == 0)
            {
                Debug.Log("[BLM] Queue finished.");
                isImporting = false;
                SaveState();
                return;
            }

            isImporting = true;
            SaveState(); // Persist "We are working on it"

            currentItem = importQueue.Dequeue();
            SaveState(); // Persist "Item removed from queue"

            if (!System.IO.File.Exists(currentItem.path))
            {
                Debug.LogError($"[BLM] Package file not found, skipping: {currentItem.path}");
                isImporting = false;
                CheckQueue();
                return;
            }

            CleanupEvents();

            AssetDatabase.importPackageCompleted += OnImportCompleted;
            AssetDatabase.importPackageCancelled += OnImportCancelled;
            AssetDatabase.importPackageFailed += OnImportFailed;

            Debug.Log($"[BLM] Importing Package: {System.IO.Path.GetFileName(currentItem.path)} (ID: {currentItem.productId})");

            try
            {
                AssetDatabase.ImportPackage(currentItem.path, interactive: InteractiveMode);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BLM] Exception starting import: {ex.Message}");
                OnImportFailed(currentItem.path, ex.Message);
            }
        }

        private static void CheckQueue()
        {
            if (isImporting) return;
            ProcessNext();
        }

        public static event Action OnImportFinishedAction;

        private static void OnImportCompleted(string packageName)
        {
            Debug.Log($"[BLM] Import Completed: {packageName}");

            // Success! Mark as installed logic here
            if (currentItem != null && !string.IsNullOrEmpty(currentItem.productId))
            {
                BLMHistory.MarkAsInstalled(currentItem.productId);
            }

            OnImportFinishedAction?.Invoke();

            CleanupEvents();
            isImporting = false;
            SaveState();
            // Delay slightly to allow Unity to refresh assets before next import
            EditorApplication.delayCall += CheckQueue;
        }

        private static void OnImportCancelled(string packageName)
        {
            Debug.LogWarning($"[BLM] Import Cancelled: {packageName}");
            CleanupEvents();
            isImporting = false;
            SaveState();
            // If cancelled, likely user wants to stop?
            // Or skip? For batch import, usually skip.
            // But if interactive dialog was cancelled, maybe they want to stop the whole batch?
            // Let's assume continue to next for now, or we could stop.
            // Given "Process Queue" intent, continue is safer, but maybe dangerous if user panic canceled.
            // Let's CONTINUE for now.
            EditorApplication.delayCall += CheckQueue;
        }

        private static void OnImportFailed(string packageName, string errorMessage)
        {
            Debug.LogError($"[BLM] Import Failed: {packageName} - {errorMessage}");
            CleanupEvents();
            isImporting = false;
            SaveState();
            EditorApplication.delayCall += CheckQueue;
        }

        private static void CleanupEvents()
        {
            AssetDatabase.importPackageCompleted -= OnImportCompleted;
            AssetDatabase.importPackageCancelled -= OnImportCancelled;
            AssetDatabase.importPackageFailed -= OnImportFailed;
        }

        private static void SaveState()
        {
            // Format: Path|Id\nPath|Id
            var lines = new List<string>();
            foreach (var item in importQueue) lines.Add($"{item.path}|{item.productId}");
            EditorPrefs.SetString(PREF_QUEUE, string.Join("\n", lines));
            EditorPrefs.SetBool(PREF_IS_IMPORTING, isImporting);
        }

        private static void LoadState()
        {
            string q = EditorPrefs.GetString(PREF_QUEUE, "");
            importQueue.Clear();
            if (!string.IsNullOrEmpty(q))
            {
                foreach (var line in q.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 2) importQueue.Enqueue(new QueueItem { path = parts[0], productId = parts[1] });
                    else if (parts.Length == 1) importQueue.Enqueue(new QueueItem { path = parts[0], productId = "" }); // legacy compat
                }
            }
            isImporting = EditorPrefs.GetBool(PREF_IS_IMPORTING, false);
        }

        public static int RemainingCount => importQueue.Count;
        public static bool IsImporting => isImporting;

        public static string[] GetQueueItems()
        {
            var list = new List<string>();
            foreach (var item in importQueue) list.Add(item.path);
            return list.ToArray();
        }
        public static string CurrentProductId => currentItem?.productId;
    }
}
