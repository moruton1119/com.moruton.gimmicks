using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 変身ギミックのセットアップ実行サービス（エディタプレビュー用）。
    /// NDMFによるビルド時処理は別途 MetamorphosePlugin で実行。
    /// </summary>
    public static class MetamorphoseSetupService
    {
        #region Public API

        /// <summary>
        /// 全工程を実行: アイテム配置 → フェード配置 → アニメーション生成（エディタプレビュー用）。
        /// ビルド時は NDMF が自動処理するため、ここはメインのセットアップではない。
        /// </summary>
        public static void ExecuteFullSetup(PrettyCureMirror script)
        {
            if (script.Model == null || script.Animator == null || script.OffTargets == null || script.OffTargets.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    LocalizationManager.GetCommon("error_dialog_title"),
                    LocalizationManager.GetCommon("error_dialog_message"),
                    LocalizationManager.GetCommon("ok"));
                return;
            }

            // アイテム配置（プレビュー用、削除なし）
            PlaceItems(script.headTarget, script.headItems);
            PlaceItems(script.bodyTarget, script.bodyItems);
            PlaceItems(script.handTarget, script.handItems);
            PlaceItems(script.legTarget, script.legItems);

            // フェード配置（プレビュー用、削除なし）
            PlaceFadeItems(script.fadeHead, script.fadeHeadItems, script.fadeHeadMaterial);
            PlaceFadeItems(script.fadeBody, script.fadeBodyItems, script.fadeBodyMaterial);
            PlaceFadeItems(script.fadeArm, script.fadeArmItems, script.fadeArmMaterial);
            PlaceFadeItems(script.fadeLeg, script.fadeLegItems, script.fadeLegMaterial);

            // アニメーション生成（プレビュー用）
            CreateAnimations(script);

            Debug.Log("[Metamorphose] Editor preview complete!");
        }

        /// <summary>
        /// ギミック色を全コラーオブジェクトに適用。
        /// </summary>
        public static void ApplyGimmickColor(PrettyCureMirror script)
        {
            if (script.GimmickCollar == null) return;

            foreach (var collar in script.GimmickCollar)
            {
                if (collar == null) continue;

                var renderers = collar.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    foreach (var mat in renderer.sharedMaterials)
                    {
                        if (mat == null || !mat.HasProperty("_Color")) continue;
                        mat.color = script.gimmickColor;
                    }
                }
            }
        }

        #endregion

        #region Simple Placement (No Deletion)

        private static void PlaceItems(Transform target, GameObject[] items)
        {
            if (target == null || items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;

                var instance = Object.Instantiate(item, target);
                instance.name = item.name;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
            }
        }

        private static void PlaceFadeItems(Transform target, GameObject[] items, Material material)
        {
            if (target == null || items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;

                var instance = Object.Instantiate(item, target);
                instance.name = target.name;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;

                // マテリアル差し替え
                if (material != null)
                {
                    var renderers = instance.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        var mats = new Material[r.sharedMaterials.Length];
                        for (int i = 0; i < mats.Length; i++)
                            mats[i] = material;
                        r.sharedMaterials = mats;
                    }
                }
            }
        }

        #endregion

        #region Animation Creation (Editor Preview Only)

        private static void CreateAnimations(PrettyCureMirror script)
        {
            if (script.Animator == null) return;

            var controller = script.Animator.runtimeAnimatorController as AnimatorController;
            if (controller == null) return;

            // コントローラをクローン（元のアセットを変更しないため）
            var cloned = Object.Instantiate(controller);
            cloned.name = controller.name + "_Metamorphose";
            script.Animator.runtimeAnimatorController = cloned;

            // Enable クリップ: 旧衣装OFF、新モデルON
            var enableClip = new AnimationClip { name = "Enable" };
            // Disable クリップ: 旧衣装ON、新モデルOFF
            var disableClip = new AnimationClip { name = "Disable" };

            if (script.OffTargets != null)
            {
                foreach (var obj in script.OffTargets)
                {
                    if (obj == null) continue;
                    string path = GetRelativePath(script.Avatar, obj);
                    enableClip.SetCurve(path, typeof(GameObject), "m_IsActive",
                        AnimationCurve.Constant(0f, 1f / 60f, 0f));
                    disableClip.SetCurve(path, typeof(GameObject), "m_IsActive",
                        AnimationCurve.Constant(0f, 1f / 60f, 1f));
                }
            }

            if (script.Model != null)
            {
                string modelPath = GetRelativePath(script.Avatar, script.Model);
                enableClip.SetCurve(modelPath, typeof(GameObject), "m_IsActive",
                    AnimationCurve.Constant(0f, 1f / 60f, 1f));
                disableClip.SetCurve(modelPath, typeof(GameObject), "m_IsActive",
                    AnimationCurve.Constant(0f, 1f / 60f, 0f));
            }

            SetClipToState(cloned, "Enable", enableClip);
            SetClipToState(cloned, "Disable", disableClip);
        }

        private static string GetRelativePath(GameObject root, GameObject child)
        {
            var parts = new List<string>();
            var current = child.transform;
            while (current != null && current != root.transform)
            {
                parts.Add(current.name);
                current = current.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }

        private static void SetClipToState(AnimatorController controller, string stateName, AnimationClip clip)
        {
            // 既存のステートを探す
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

            // なければ作成
            var newState = controller.layers[0].stateMachine.AddState(stateName);
            newState.motion = clip;
        }

        #endregion
    }
}
