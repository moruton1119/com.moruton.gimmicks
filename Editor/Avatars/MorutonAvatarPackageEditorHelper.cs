using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Moruton.Gimmicks.Core;

namespace Moruton.Gimmicks.Editor
{
    public static class MorutonAvatarPackageEditorHelper
    {
        private const string PackageName = "com.moruton.gimmicks";

        private static string _latestVersion = "";
        private static bool _versionChecked = false;

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
            string currentVersion = GimmicksUpdateChecker.GetCurrentVersion();

            // ステータスメッセージ表示
            if (!string.IsNullOrEmpty(GimmicksUpdateChecker.UpdateStatus))
            {
                EditorGUILayout.HelpBox(GimmicksUpdateChecker.UpdateStatus, MessageType.Info);
            }

            // 更新中はボタンを無効化
            if (GimmicksUpdateChecker.IsUpdating)
            {
                GUI.enabled = false;
            }

            // 初回のみバックグラウンドでフェッチ開始
            if (!_versionChecked)
            {
                _versionChecked = true;
                GimmicksUpdateChecker.PrefetchLatestVersion();

                // 数秒後にフェッチ結果を反映
                EditorApplication.delayCall += async () =>
                {
                    await Task.Delay(3000);
                    string latest = await GimmicksUpdateChecker.GetLatestVersionAsync();
                    if (!string.IsNullOrEmpty(latest))
                    {
                        _latestVersion = latest;
                    }
                };
            }

            // キャッシュまたは既取得の最新バージョンを確認
            string latestFromCache = _latestVersion;
            if (string.IsNullOrEmpty(latestFromCache))
            {
                latestFromCache = GimmicksUpdateChecker.GetLatestVersionCached();
            }

            if (!string.IsNullOrEmpty(latestFromCache))
            {
                _latestVersion = latestFromCache;

                if (SemVer.TryParse(latestFromCache, out var latVer) &&
                    SemVer.TryParse(currentVersion, out var curVer))
                {
                    if (!latVer.IsPreRelease && latVer > curVer)
                    {
                        DrawUpdateBanner(currentVersion, latestFromCache);
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"Version: {currentVersion} (Latest)", EditorStyles.miniLabel);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField($"Version: {currentVersion}", EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Version: {currentVersion}", EditorStyles.miniLabel);
            }

            GUI.enabled = true;
        }

        private static void DrawUpdateBanner(string currentVersion, string latestVersion)
        {
            GUI.backgroundColor = Color.yellow;
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField($"🆕 アップデートが利用可能です! (v{currentVersion} -> v{latestVersion})", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("自動更新", GUILayout.Height(25)))
                {
                    StartAutoUpdate(latestVersion);
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

        private static async void StartAutoUpdate(string targetVersion)
        {
            if (GimmicksUpdateChecker.IsUpdating) return;

            bool success = await GimmicksUpdateChecker.DownloadAndInstallUpdateAsync(targetVersion);
            if (success)
            {
                _latestVersion = targetVersion;
            }
        }
    }
}
