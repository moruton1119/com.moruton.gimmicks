using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Moruton.Gimmicks.Editor
{
    public static class MorutonAvatarPackageEditorHelper
    {
        private static string latestVersion = "";
        private static bool isChecking = false;
        private static bool isUpdating = false;
        private static string updateStatus = "";
        private const string RemotePackageJsonUrl = "https://moruton1119.github.io/com.moruton.gimmicks/index.json";
        private const string PackageName = "com.moruton.gimmicks";
        private const string GitHubReleaseBaseUrl = "https://github.com/moruton1119/com.moruton.gimmicks/releases/download";

        public static void DrawHeader()
        {
            Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/" + PackageName + "/Runtime/Common/Morulabw.png");
            GUILayout.Space(10);

            if (image != null)
            {
                var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(150));
                GUI.DrawTexture(rect, image, ScaleMode.ScaleToFit);
            }

            CheckVersion();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Booth", new GUIStyle(GUI.skin.button) { fontSize = 12, fontStyle = FontStyle.Bold }, GUILayout.Height(25)))
            {
                Application.OpenURL("https://moruton.booth.pm/");
            }

            if (GUILayout.Button("Discord", GUILayout.Height(25)))
            {
                Application.OpenURL("https://discord.gg/GHJwmyTcfX");
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        private static void CheckVersion()
        {
            string currentVersion = GetCurrentVersion();

            if (!string.IsNullOrEmpty(updateStatus))
            {
                EditorGUILayout.HelpBox(updateStatus, MessageType.Info);
            }

            if (isUpdating)
            {
                GUI.enabled = false;
            }

            if (string.IsNullOrEmpty(latestVersion) && !isChecking)
            {
                isChecking = true;
                FetchRemoteVersion();
            }

            if (!string.IsNullOrEmpty(latestVersion) && IsNewerVersion(latestVersion, currentVersion))
            {
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField($"🆕 アップデートが利用可能です! (v{currentVersion} -> v{latestVersion})", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("自動更新", GUILayout.Height(25)))
                    {
                        StartAutoUpdate();
                    }
                    if (GUILayout.Button("VCCで更新", GUILayout.Height(25)))
                    {
                        string projectPath = Directory.GetCurrentDirectory().Replace("\\", "/");
                        projectPath = Uri.EscapeDataString(projectPath);
                        Application.OpenURL($"vcc://vpm/open?path={projectPath}");
                    }
                    if (GUILayout.Button("手動ダウンロード", GUILayout.Height(25)))
                    {
                        Application.OpenURL($"https://github.com/moruton1119/com.moruton.gimmicks/releases/latest");
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
                GUILayout.Space(5);
            }
            else
            {
                EditorGUILayout.LabelField($"Version: {currentVersion}", EditorStyles.miniLabel);
            }

            GUI.enabled = true;
        }

        private static bool IsNewerVersion(string latest, string current)
        {
            if (latest == current) return false;
            try
            {
                Version vLat = new Version(latest);
                Version vCur = new Version(current);
                return vLat > vCur;
            }
            catch
            {
                return latest != current;
            }
        }

        private static string GetCurrentVersion()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Packages", PackageName, "package.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var pkg = JsonUtility.FromJson<PackageInfo>(json);
                return pkg.version;
            }
            return "0.0.0";
        }

        private static async void FetchRemoteVersion()
        {
            using (var request = UnityEngine.Networking.UnityWebRequest.Get(RemotePackageJsonUrl))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone) await Task.Yield();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    // JsonUtility doesn't support Dictionary, so we parse manually for the VPM repo structure
                    // Look for "com.moruton.gimmicks": { "versions": { "X.Y.Z": ...
                    string searchPattern = $"\"{PackageName}\"";
                    int pkgIndex = json.IndexOf(searchPattern);
                    if (pkgIndex != -1)
                    {
                        int versionsIndex = json.IndexOf("\"versions\"", pkgIndex);
                        if (versionsIndex != -1)
                        {
                            // Find the start of the first version string after "versions": { "
                            int firstQuote = json.IndexOf("\"", versionsIndex + 10);
                            // Find the end of that version string
                            int nextQuote = json.IndexOf("\"", firstQuote + 1);
                            if (firstQuote != -1 && nextQuote != -1)
                            {
                                latestVersion = json.Substring(firstQuote + 1, nextQuote - firstQuote - 1);
                            }
                        }
                    }
                }
                isChecking = false;
            }
        }

        private static async void StartAutoUpdate()
        {
            if (isUpdating || string.IsNullOrEmpty(latestVersion)) return;

            isUpdating = true;
            updateStatus = "アップデートを開始しています...";

            try
            {
                await PerformUpdate();
            }
            catch (System.Exception e)
            {
                updateStatus = $"アップデートに失敗しました: {e.Message}";
                Debug.LogError($"[MorutonGimmicks] Update failed: {e}");
            }
            finally
            {
                isUpdating = false;
            }
        }

        private static async Task PerformUpdate()
        {
            string targetVersion = latestVersion;
            // リポジトリ整理後の命名規則に合わせる
            string zipUrl = $"{GitHubReleaseBaseUrl}/v{targetVersion}/{PackageName}-{targetVersion}.zip";
            string tempPath = Path.Combine(Path.GetTempPath(), "MorutonGimmicks_Update");
            string zipPath = Path.Combine(tempPath, "update.zip");
            string extractPath = Path.Combine(tempPath, "extracted");

            updateStatus = $"v{targetVersion} をダウンロード中...";

            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(extractPath);

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                var response = await httpClient.GetAsync(zipUrl);
                if (!response.IsSuccessStatusCode)
                {
                    // vなしも試行する
                    string fallbackUrl = $"{GitHubReleaseBaseUrl}/{targetVersion}/{PackageName}-{targetVersion}.zip";
                    response = await httpClient.GetAsync(fallbackUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Download failed: {response.StatusCode} at {zipUrl}");
                    }
                }
                var bytes = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(zipPath, bytes);
            }

            updateStatus = "ダウンロード完了。展開中...";

            await Task.Run(() =>
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            });

            updateStatus = "パッケージを更新中...";

            string packagePath = $"Packages/{PackageName}";
            string absolutePackagePath = Path.GetFullPath(packagePath);

            string sourceContentPath = extractPath;
            // ZIPの中にパッケージ名ディレクトリがある場合とない場合の両方に対応
            if (Directory.Exists(Path.Combine(extractPath, PackageName)))
            {
                sourceContentPath = Path.Combine(extractPath, PackageName);
            }

            if (Directory.Exists(absolutePackagePath))
            {
                foreach (string file in Directory.GetFiles(absolutePackagePath, "*", SearchOption.AllDirectories))
                {
                    string relativePath = file.Substring(absolutePackagePath.Length).TrimStart(Path.DirectorySeparatorChar);
                    string destFile = Path.Combine(sourceContentPath, relativePath);

                    if (!File.Exists(destFile))
                    {
                        string metaFile = file + ".meta";
                        if (File.Exists(metaFile))
                        {
                            File.Delete(metaFile);
                        }
                        File.Delete(file);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(absolutePackagePath);
            }

            foreach (string file in Directory.GetFiles(sourceContentPath, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Substring(sourceContentPath.Length).TrimStart(Path.DirectorySeparatorChar);
                string destFile = Path.Combine(absolutePackagePath, relativePath);
                string destDir = Path.GetDirectoryName(destFile);

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(file, destFile, true);
            }

            updateStatus = "vpm-manifest.json を更新中...";

            UpdateVpmManifest(targetVersion);

            try
            {
                Directory.Delete(tempPath, true);
            }
            catch { }

            AssetDatabase.Refresh();

            updateStatus = $"✅ v{targetVersion} に更新完了しました！";
            Debug.Log($"[MorutonGimmicks] Successfully updated to v{targetVersion}");

            latestVersion = targetVersion;
        }

        private static void UpdateVpmManifest(string newVersion)
        {
            string projectPath = Directory.GetCurrentDirectory();
            string vpmManifestPath = Path.Combine(projectPath, "Packages", "vpm-manifest.json");

            if (!File.Exists(vpmManifestPath))
            {
                Debug.LogWarning("[MorutonGimmicks] vpm-manifest.json not found");
                return;
            }

            string json = File.ReadAllText(vpmManifestPath);

            json = UpdateJsonVersion(json, "dependencies", PackageName, newVersion);
            json = UpdateJsonVersion(json, "locked", PackageName, newVersion);

            File.WriteAllText(vpmManifestPath, json);
        }

        private static string UpdateJsonVersion(string json, string section, string packageName, string newVersion)
        {
            string searchPattern = $"\"{packageName}\"";
            int packageIndex = json.IndexOf(searchPattern);

            while (packageIndex != -1)
            {
                int sectionStart = json.LastIndexOf($"\"{section}\"", packageIndex);
                if (sectionStart == -1)
                {
                    packageIndex = json.IndexOf(searchPattern, packageIndex + 1);
                    continue;
                }

                int braceDepth = 0;
                int sectionBraceStart = -1;
                for (int i = sectionStart; i < json.Length; i++)
                {
                    if (json[i] == '{')
                    {
                        braceDepth++;
                        if (sectionBraceStart == -1) sectionBraceStart = i;
                    }
                    else if (json[i] == '}')
                    {
                        braceDepth--;
                        if (braceDepth == 0)
                        {
                            break;
                        }
                    }
                }

                if (packageIndex < sectionStart)
                {
                    packageIndex = json.IndexOf(searchPattern, packageIndex + 1);
                    continue;
                }

                int versionStart = json.IndexOf("\"version\"", packageIndex);
                if (versionStart == -1)
                {
                    packageIndex = json.IndexOf(searchPattern, packageIndex + 1);
                    continue;
                }

                int valueStart = json.IndexOf("\"", versionStart + 10) + 1;
                int valueEnd = json.IndexOf("\"", valueStart);

                if (valueStart > 0 && valueEnd > valueStart)
                {
                    json = json.Substring(0, valueStart) + newVersion + json.Substring(valueEnd);
                    break;
                }

                packageIndex = json.IndexOf(searchPattern, packageIndex + 1);
            }

            return json;
        }

        [System.Serializable]
        private class PackageInfo
        {
            public string version;
        }
    }
}
