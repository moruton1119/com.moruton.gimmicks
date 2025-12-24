using UnityEngine;
using UdonSharp;

namespace MorutonLaboratry.Script
{
    // ランタイム用の基底クラス (MorutonGimmicksの代わりとして機能)
    // エディター機能は別の Editor拡張クラス（SkyCycleBaseEditorなど）から
    // MorutonGimmickPackageEditorHelper を呼び出して実装する形にします。

    public abstract class MorutonGimmickPackage : UdonSharpBehaviour
    {
        //もるらぼのギミックに共通のEditor処理を書き込むための継承用Script
        //ランタイム側には最小限の定義のみ残します
        [SerializeField] private Texture2D dummyImage;
    }
}

#if UNITY_EDITOR
namespace MorutonLaboratry.Script
{
    using UnityEditor;

    /// <summary>
    /// もるらぼギミック共通のエディター表示機能を提供するヘルパークラス
    /// エディター拡張スクリプトから呼び出して使用します。
    /// </summary>
    public static class MorutonGimmickPackageEditorHelper
    {
        public static void DrawHeader()
        {
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
