using UnityEngine;
using UdonSharp;

namespace Moruton.Gimmicks
{
    // ランタイム用の基底クラス
    // プロジェクト側の MorutonLaboratry.Script とは独立した名前空間で管理
    public abstract class MorutonGimmickPackage : UdonSharpBehaviour
    {
        //もるらぼのギミックに共通のEditor処理を書き込むための継承用Script
        [SerializeField] private Texture2D dummyImage;
    }
}

#if UNITY_EDITOR
namespace Moruton.Gimmicks.Editor
{
    using UnityEditor;

    /// <summary>
    /// もるらぼギミック共通のエディター表示機能を提供するヘルパークラス
    /// </summary>
    public static class MorutonGimmickPackageEditorHelper
    {
        private static string latestVersion = "";
        private static bool isChecking = false;
        private const string RemotePackageJsonUrl = "https://raw.githubusercontent.com/moruton1119/com.moruton.gimmicks/main/package.json"; // 適切なURLに変更してください
        private const string PackageName = "com.moruton.gimmicks";

        public static void DrawHeader()
        {
            // パッケージ相対パスから画像をロード
            Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/" + PackageName + "/Runtime/Morulabw.png");
            GUILayout.Space(10);

            // 画像があれば描画
            if (image != null)
            {
                // GUILayout.Label("MorutonLaboratory 制作", EditorStyles.boldLabel);
                var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(100));
                GUI.DrawTexture(rect, image, ScaleMode.ScaleToFit);
            }

            // バージョンチェックの表示
            CheckVersion();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Booth", new GUIStyle(GUI.skin.button) { fontSize = 12, fontStyle = FontStyle.Bold }, GUILayout.Height(25)))
            {
                Application.OpenURL("https://moruton-world.booth.pm/");
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
            // 現在のパッケージバージョンを取得
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
                    if (GUILayout.Button("VCCを起動して更新する (またはBoothを確認)"))
                    {
                        // VCCのリポジトリ機能での更新を促すか、配布ページを開く
                        Application.OpenURL("https://moruton-world.booth.pm/"); 
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
            // Packagesフォルダ内の自身のpackage.jsonを読み込む
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
#endif
