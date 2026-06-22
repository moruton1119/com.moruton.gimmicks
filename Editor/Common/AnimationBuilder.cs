using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// AnimationClip生成の汎用ユーティリティ。
    /// 変身ギミック等で Enable/Disable アニメーションを生成する際に共用。
    /// </summary>
    public static class AnimationBuilder
    {
        /// <summary>
        /// 指定したオブジェクトの有効/無効を切り替える Enable/Disable アニメーションを生成する。
        /// </summary>
        /// <param name="root">パス計算のルートオブジェクト</param>
        /// <param name="offTargets">Enable時にOFFにするオブジェクト群</param>
        /// <param name="toggleTarget">Enable時にON、Disable時にOFFにするオブジェクト</param>
        /// <param name="outputFolder">出力フォルダパス</param>
        /// <param name="enableClipName">Enableクリップ名 (default: "Enable")</param>
        /// <param name="disableClipName">Disableクリップ名 (default: "Disable")</param>
        /// <returns>成功時は (enableClip, disableClip)、失敗時は null</returns>
        public static (AnimationClip enableClip, AnimationClip disableClip) CreateToggleAnimations(
            GameObject root,
            GameObject[] offTargets,
            GameObject toggleTarget,
            string outputFolder,
            string enableClipName = "Enable",
            string disableClipName = "Disable")
        {
            if (root == null || toggleTarget == null) return (null, null);

            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            string enablePath = Path.Combine(outputFolder, $"{enableClipName}.anim");
            string disablePath = Path.Combine(outputFolder, $"{disableClipName}.anim");

            // Enable Animation: offTargetsをOFF、toggleTargetをON
            AnimationClip enableClip = new AnimationClip();
            foreach (var obj in offTargets)
            {
                if (obj == null) continue;
                string path = GetRelativePath(root, obj);
                if (!string.IsNullOrEmpty(path))
                    enableClip.SetCurve(path, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 0));
            }
            string togglePath = GetRelativePath(root, toggleTarget);
            if (!string.IsNullOrEmpty(togglePath))
                enableClip.SetCurve(togglePath, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 1));
            AssetDatabase.CreateAsset(enableClip, enablePath);

            // Disable Animation: offTargetsをON、toggleTargetをOFF
            AnimationClip disableClip = new AnimationClip();
            foreach (var obj in offTargets)
            {
                if (obj == null) continue;
                string path = GetRelativePath(root, obj);
                if (!string.IsNullOrEmpty(path))
                    disableClip.SetCurve(path, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 1));
            }
            if (!string.IsNullOrEmpty(togglePath))
                disableClip.SetCurve(togglePath, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 0));
            AssetDatabase.CreateAsset(disableClip, disablePath);

            AssetDatabase.SaveAssets();

            return (enableClip, disableClip);
        }

        /// <summary>
        /// root から target までの相対パスを取得する。
        /// </summary>
        public static string GetRelativePath(GameObject root, GameObject child)
        {
            if (root == null || child == null) return "";

            var path = new List<string>();
            var current = child.transform;
            while (current != null && current != root.transform)
            {
                path.Add(current.name);
                current = current.parent;
            }

            if (current == null) return "";

            path.Reverse();
            return string.Join("/", path);
        }

        /// <summary>
        /// AnimationClip を AnimatorController の指定ステートに適用する。
        /// ステートが存在しない場合は新規作成。
        /// </summary>
        public static void ApplyClipToState(AnimatorController controller, string stateName, AnimationClip clip)
        {
            if (controller == null || clip == null) return;

            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    if (state.state.name == stateName)
                    {
                        state.state.motion = clip;
                        return;
                    }
                }
            }

            var newState = controller.layers[0].stateMachine.AddState(stateName);
            newState.motion = clip;
        }
    }
}
