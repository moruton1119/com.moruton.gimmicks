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
        public static void DrawHeader()
        {
            // パッケージ内部または適切なパスから画像をロードするように修正推奨だが、
            // 互換性のため一旦既存パス維持、ただしプロジェクト依存が高い点に注意
            Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MorutonLaboratry/Editor/Morulabw.png");
            GUILayout.Space(10);

            // 画像があれば描画
            if (image != null)
            {
                GUILayout.Label("MorutonLaboratory 制作", EditorStyles.boldLabel);
                GUILayout.Box(image, GUILayout.Height(100));
            }

            if (GUILayout.Button("他にもたくさんギミックあります（booth）", new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold }, GUILayout.Height(30)))
            {
                Application.OpenURL("https://moruton-world.booth.pm/");
            }

            if (GUILayout.Button("質問等あればこちらから聞いてください(Discord)"))
            {
                Application.OpenURL("https://discord.gg/GHJwmyTcfX");
            }
        }
    }
}
#endif
