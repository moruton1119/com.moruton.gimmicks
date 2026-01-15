using UnityEngine;
using UnityEditor;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// もるらぼギミック共通のエディター表示機能を提供するヘルパークラス (Avatar用)
    /// </summary>
    public static class MorutonAvatarPackageEditorHelper
    {
        private static string latestVersion = "";
        private static bool isChecking = false;
        private const string RemotePackageJsonUrl = "https://raw.githubusercontent.com/moruton1119/com.moruton.gimmicks/main/package.json";
        private const string PackageName = "com.moruton.gimmicks";

        public static void DrawHeader()
        {
            // パッケージ相対パスから画像をロード (Runtime/Common/に配置)
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
            if (string.IsNullOrEmpty(latestVersion) && !isChecking)
            {
                isChecking = true;
                FetchRemoteVersion();
            }

            if (!string.IsNullOrEmpty(latestVersion) && latestVersion != currentVersion)
            {
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField($"🆕 アップデートが利用可能です! (v{currentVersion} -> v{latestVersion})", EditorStyles.boldLabel);
                    if (GUILayout.Button("VCCを開いて更新"))
                    {
                        // 現在のプロジェクトのパスを取得し、バックスラッシュをスラッシュに置換してVCCで開く
                        string projectPath = System.IO.Directory.GetCurrentDirectory().Replace("\\", "/");
                        projectPath = System.Uri.EscapeDataString(projectPath);
                        Application.OpenURL($"vcc://vpm/open?path={projectPath}");
                    }
                }
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
                GUILayout.Space(5);
            }
            else
            {
                EditorGUILayout.LabelField($"Version: {currentVersion}", EditorStyles.miniLabel);
            }
        }

        private static string GetCurrentVersion()
        {
            string path = "Packages/" + PackageName + "/package.json";
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset != null)
            {
                var pkg = JsonUtility.FromJson<PackageInfo>(asset.text);
                return pkg.version;
            }
            return "0.0.0";
        }

        private static async void FetchRemoteVersion()
        {
            using (var request = UnityEngine.Networking.UnityWebRequest.Get(RemotePackageJsonUrl))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone) await System.Threading.Tasks.Task.Yield();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    var pkg = JsonUtility.FromJson<PackageInfo>(request.downloadHandler.text);
                    latestVersion = pkg.version;
                }
                isChecking = false;
            }
        }

        [System.Serializable]
        private class PackageInfo
        {
            public string version;
        }
    }
}
