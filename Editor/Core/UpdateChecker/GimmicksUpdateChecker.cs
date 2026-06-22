using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Moruton.Gimmicks.Core
{
    /// <summary>
    /// VPMリポジトリの index.json から最新バージョンを取得し、
    /// アップデートの有無を判定するチェッカー。
    /// com.moruton.gimmicks 専用。
    /// </summary>
    public static class GimmicksUpdateChecker
    {
        private const string PackageName = "com.moruton.gimmicks";
        private const string RemotePackageJsonUrl = "https://moruton1119.github.io/com.moruton.gimmicks/index.json";
        private const string GitHubReleaseBaseUrl = "https://github.com/moruton1119/com.moruton.gimmicks/releases/download";

        /// <summary>
        /// キャッシュ: (latestVersion, fetchTime)
        /// </summary>
        private static (string version, DateTime fetchedAt)? _cache;

        private const double CacheDurationMinutes = 30;

        /// <summary>
        /// 最新バージョンを取得する（非同期）。
        /// index.json の versions から全バージョンを列挙し、SemVer で最大の安定版を返す。
        /// </summary>
        /// <param name="includePrerelease">ベータ版等のプレリリースを含めるか</param>
        public static async Task<string> GetLatestVersionAsync(bool includePrerelease = false)
        {
            // キャッシュチェック
            if (_cache.HasValue)
            {
                if ((DateTime.Now - _cache.Value.fetchedAt).TotalMinutes < CacheDurationMinutes)
                {
                    return _cache.Value.version;
                }
            }

            string json = await FetchJsonAsync(RemotePackageJsonUrl);
            if (json == null) return null;

            // versions オブジェクト内の全バージョンキーを抽出
            var versions = ExtractAllVersions(json);
            if (versions.Count == 0)
            {
                Debug.LogWarning($"[GimmicksUpdateChecker] No versions found in {RemotePackageJsonUrl}");
                return null;
            }

            // SemVer で最大のものを選択
            string latest = SelectLatestVersion(versions, includePrerelease);

            _cache = (latest, DateTime.Now);
            return latest;
        }

        /// <summary>
        /// 同期版（Editorメインスレッド用）。
        /// 初回呼び出し時はキャッシュがないため null を返す可能性がある。
        /// </summary>
        public static string GetLatestVersionCached()
        {
            if (_cache.HasValue)
            {
                if ((DateTime.Now - _cache.Value.fetchedAt).TotalMinutes < CacheDurationMinutes)
                {
                    return _cache.Value.version;
                }
            }
            return null;
        }

        /// <summary>
        /// バックグラウンドでフェッチを開始する（結果はキャッシュに格納される）。
        /// </summary>
        public static void PrefetchLatestVersion()
        {
            if (_cache.HasValue &&
                (DateTime.Now - _cache.Value.fetchedAt).TotalMinutes < CacheDurationMinutes)
            {
                return; // キャッシュが有効
            }

            _ = GetLatestVersionAsync();
        }

        /// <summary>
        /// 現在インストールされているバージョンを取得する。
        /// </summary>
        public static string GetCurrentVersion()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Packages", PackageName, "package.json");
            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    var pkg = JsonUtility.FromJson<PackageInfo>(json);
                    return pkg.version;
                }
                catch { }
            }
            return "0.0.0";
        }

        /// <summary>
        /// アップデートが利用可能か判定する（安定版のみ）。
        /// </summary>
        public static bool IsUpdateAvailable()
        {
            string latest = GetLatestVersionCached();
            if (string.IsNullOrEmpty(latest)) return false;

            string current = GetCurrentVersion();

            if (!SemVer.TryParse(current, out var curVer)) return false;
            if (!SemVer.TryParse(latest, out var latVer)) return latest != current;

            // 安定版のみ通知
            if (latVer.IsPreRelease) return false;

            return latVer > curVer;
        }

        // ─── 自己更新（Auto Update）───

        private static bool _isUpdating;
        private static string _updateStatus;

        /// <summary>更新中かどうか</summary>
        public static bool IsUpdating => _isUpdating;

        /// <summary>現在の更新ステータスメッセージ</summary>
        public static string UpdateStatus => _updateStatus;

        /// <summary>
        /// パッケージを自動ダウンロード＆インストールする。
        /// </summary>
        /// <param name="targetVersion">インストール対象バージョン</param>
        public static async Task<bool> DownloadAndInstallUpdateAsync(string targetVersion)
        {
            if (_isUpdating) return false;

            _isUpdating = true;
            _updateStatus = $"v{targetVersion} をダウンロード中...";

            try
            {
                string zipUrl = $"{GitHubReleaseBaseUrl}/v{targetVersion}/{PackageName}-{targetVersion}.zip";
                string tempPath = Path.Combine(Path.GetTempPath(), "MorutonGimmicks_Update");
                string zipPath = Path.Combine(tempPath, "update.zip");
                string extractPath = Path.Combine(tempPath, "extracted");

                // tempディレクトリをクリア
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
                Directory.CreateDirectory(tempPath);
                Directory.CreateDirectory(extractPath);

                // DL
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(5);
                    var response = await httpClient.GetAsync(zipUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        // vなしも試行
                        string fallbackUrl = $"{GitHubReleaseBaseUrl}/{targetVersion}/{PackageName}-{targetVersion}.zip";
                        response = await httpClient.GetAsync(fallbackUrl);
                        if (!response.IsSuccessStatusCode)
                            throw new Exception($"Download failed: {response.StatusCode}");
                    }
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    File.WriteAllBytes(zipPath, bytes);
                }

                _updateStatus = "ダウンロード完了。展開中...";

                // 解凍
                await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, extractPath));

                _updateStatus = "パッケージを更新中...";

                // 差分更新
                string absolutePackagePath = Path.GetFullPath($"Packages/{PackageName}");
                string sourceContentPath = extractPath;

                // ZIP内にパッケージ名ディレクトリがある場合の対応
                if (Directory.Exists(Path.Combine(extractPath, PackageName)))
                    sourceContentPath = Path.Combine(extractPath, PackageName);

                // 古いファイルの削除（不要なファイルを残さない）
                if (Directory.Exists(absolutePackagePath))
                {
                    foreach (string file in Directory.GetFiles(absolutePackagePath, "*", SearchOption.AllDirectories))
                    {
                        string relativePath = file.Substring(absolutePackagePath.Length)
                            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        string destFile = Path.Combine(sourceContentPath, relativePath);

                        if (!File.Exists(destFile))
                        {
                            string metaFile = file + ".meta";
                            if (File.Exists(metaFile))
                                File.Delete(metaFile);
                            File.Delete(file);
                        }
                    }
                }

                // 新ファイルのコピー
                foreach (string file in Directory.GetFiles(sourceContentPath, "*", SearchOption.AllDirectories))
                {
                    string relativePath = file.Substring(sourceContentPath.Length)
                        .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    string destFile = Path.Combine(absolutePackagePath, relativePath);
                    string destDir = Path.GetDirectoryName(destFile);

                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    File.Copy(file, destFile, true);
                }

                // vpm-manifest.json 更新
                _updateStatus = "vpm-manifest.json を更新中...";
                UpdateVpmManifest(targetVersion);

                // 一時ファイル削除
                try { Directory.Delete(tempPath, true); } catch { }

                AssetDatabase.Refresh();

                _updateStatus = $"✅ v{targetVersion} に更新完了しました！";
                Debug.Log($"[GimmicksUpdateChecker] Successfully updated to v{targetVersion}");

                // キャッシュ更新
                _cache = (targetVersion, DateTime.Now);

                return true;
            }
            catch (Exception e)
            {
                _updateStatus = $"更新に失敗しました: {e.Message}";
                Debug.LogError($"[GimmicksUpdateChecker] Update failed: {e}");
                return false;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        #region Private Methods

        private static async Task<string> FetchJsonAsync(string url)
        {
            try
            {
                using (var request = UnityEngine.Networking.UnityWebRequest.Get(url))
                {
                    var operation = request.SendWebRequest();
                    while (!operation.isDone) await Task.Yield();

                    if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        return request.downloadHandler.text;
                    }
                    else
                    {
                        Debug.LogWarning($"[GimmicksUpdateChecker] Failed to fetch {url}: {request.error}");
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GimmicksUpdateChecker] Exception fetching {url}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// index.json からパッケージの全バージョンキーを抽出する。
        /// JsonUtilityがDictionaryを扱えないため、簡易パーサーで抽出。
        /// </summary>
        private static List<string> ExtractAllVersions(string json)
        {
            var versions = new List<string>();

            // "com.moruton.gimmicks" を探す
            string searchPattern = $"\"{PackageName}\"";
            int pkgIndex = json.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase);
            if (pkgIndex == -1) return versions;

            // "versions" を探す
            int versionsIndex = json.IndexOf("\"versions\"", pkgIndex);
            if (versionsIndex == -1) return versions;

            // versions の { から対応する } までをスキャン
            int braceStart = json.IndexOf('{', versionsIndex);
            if (braceStart == -1) return versions;

            int depth = 0;
            int braceEnd = -1;
            for (int i = braceStart; i < json.Length; i++)
            {
                if (json[i] == '{') depth++;
                else if (json[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        braceEnd = i;
                        break;
                    }
                }
            }

            if (braceEnd == -1) return versions;

            string versionsBlock = json.Substring(braceStart + 1, braceEnd - braceStart - 1);

            // "X.Y.Z" のようなキーをすべて抽出
            int pos = 0;
            while (pos < versionsBlock.Length)
            {
                int start = versionsBlock.IndexOf('"', pos);
                if (start == -1) break;
                int end = versionsBlock.IndexOf('"', start + 1);
                if (end == -1) break;

                string key = versionsBlock.Substring(start + 1, end - start - 1);

                // キーの直後に : がある場合のみバージョンキーと判定
                int colonIdx = versionsBlock.IndexOf(':', end);
                if (colonIdx != -1 && colonIdx < versionsBlock.Length)
                {
                    int nextBrace = versionsBlock.IndexOf('{', colonIdx);
                    int nextQuote = versionsBlock.IndexOf('"', colonIdx);
                    if (nextBrace != -1 && (nextQuote == -1 || nextBrace < nextQuote))
                    {
                        if (SemVer.TryParse(key, out _))
                        {
                            versions.Add(key);
                        }
                    }
                }

                pos = end + 1;
            }

            return versions;
        }

        /// <summary>
        /// SemVerで比較し、最新バージョンを選択する。
        /// </summary>
        private static string SelectLatestVersion(List<string> versions, bool includePrerelease)
        {
            SemVer best = default;
            string bestStr = null;
            bool initialized = false;

            foreach (var v in versions)
            {
                if (!SemVer.TryParse(v, out var sv)) continue;

                if (!includePrerelease && sv.IsPreRelease) continue;

                if (!initialized || sv > best)
                {
                    best = sv;
                    bestStr = v;
                    initialized = true;
                }
            }

            return bestStr ?? (versions.Count > 0 ? versions[0] : null);
        }

        /// <summary>
        /// vpm-manifest.json のバージョンを更新する
        /// </summary>
        private static void UpdateVpmManifest(string newVersion)
        {
            string vpmManifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "vpm-manifest.json");
            if (!File.Exists(vpmManifestPath))
            {
                Debug.LogWarning("[GimmicksUpdateChecker] vpm-manifest.json not found");
                return;
            }

            string json = File.ReadAllText(vpmManifestPath);
            json = UpdateJsonVersion(json, "dependencies", newVersion);
            json = UpdateJsonVersion(json, "locked", newVersion);
            File.WriteAllText(vpmManifestPath, json);
        }

        private static string UpdateJsonVersion(string json, string section, string newVersion)
        {
            string searchPattern = $"\"{PackageName}\"";
            int packageIndex = json.IndexOf(searchPattern);

            while (packageIndex != -1)
            {
                int sectionStart = json.LastIndexOf($"\"{section}\"", packageIndex);
                if (sectionStart == -1)
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

                int valueStart = json.IndexOf('"', versionStart + 10) + 1;
                int valueEnd = json.IndexOf('"', valueStart);

                if (valueStart > 0 && valueEnd > valueStart)
                {
                    json = json.Substring(0, valueStart) + newVersion + json.Substring(valueEnd);
                    break;
                }

                packageIndex = json.IndexOf(searchPattern, packageIndex + 1);
            }

            return json;
        }

        #endregion

        [System.Serializable]
        private class PackageInfo
        {
            public string version;
        }
    }
}
