using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using nadena.dev.modular_avatar.core;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 変身ギミックのセットアップ実行サービス。
    /// MetamorphoseEditor (UI) から分離されたロジック層。
    /// 新しい変身ギミックを作る際、このクラスを参考にサービスを追加する。
    /// </summary>
    public static class MetamorphoseSetupService
    {
        /// <summary>
        /// 全工程を実行: アイテム装着 → ワンピース → コラボ → フェード → アニメーション → MA
        /// </summary>
        public static void ExecuteFullSetup(PrettyCureMirror script)
        {
            ProcessItemAttachment(script);
            ProcessOnePiece(script);
            ProcessColaboItem(script);
            ProcessFadeAttachment(script);
            GenerateAnimations(script);
            GenerateMergeAnimator(script);

            Debug.Log("[Metamorphose] Setup complete!");
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

        /// <summary>
        /// PrefabのUnpackを実行。
        /// </summary>
        public static void UnpackPrefab(GameObject prefab)
        {
            GimmickPrefabUtility.UnpackPrefab(prefab);
        }

        #region Internal: Item Attachment

        public static void ProcessItemAttachment(PrettyCureMirror script)
        {
            ProcessPart(script.headTarget, script.headItems);
            ProcessPart(script.bodyTarget, script.bodyItems);
            ProcessPart(script.handTarget, script.handItems);
            ProcessPart(script.legTarget, script.legItems);
        }

        private static void ProcessPart(Transform target, GameObject[] items)
        {
            if (target == null || items == null) return;

            while (target.childCount > 0)
                Object.DestroyImmediate(target.GetChild(0).gameObject);

            foreach (var item in items)
            {
                if (item == null) continue;
                var instance = Object.Instantiate(item, target);
                instance.name = item.name;
            }
        }

        #endregion

        #region Internal: OnePiece & Colabo

        private static void ProcessOnePiece(PrettyCureMirror script)
        {
            if (script.OnePiece == null || script.ColaboFBX == null) return;
            GimmickPrefabUtility.ReplaceChild(script.OnePiece.transform, script.ColaboFBX);
        }

        private static void ProcessColaboItem(PrettyCureMirror script)
        {
            if (script.colaboItemTarget == null || script.colaboItem == null) return;
            var instance = Object.Instantiate(script.colaboItem, script.colaboItemTarget);
            instance.name = script.colaboItem.name;
        }

        #endregion

        #region Internal: Fade Effects

        public static void ProcessFadeAttachment(PrettyCureMirror script)
        {
            ProcessFadePart(script.fadeHead, script.fadeHeadItems, script.fadeHeadMaterial);
            ProcessFadePart(script.fadeBody, script.fadeBodyItems, script.fadeBodyMaterial);
            ProcessFadePart(script.fadeArm, script.fadeArmItems, script.fadeArmMaterial);
            ProcessFadePart(script.fadeLeg, script.fadeLegItems, script.fadeLegMaterial);
        }

        private static void ProcessFadePart(Transform target, GameObject[] items, Material fadeMaterial)
        {
            if (target == null || items == null) return;

            while (target.childCount > 0)
                Object.DestroyImmediate(target.GetChild(0).gameObject);

            foreach (var item in items)
            {
                if (item == null) continue;
                var instance = Object.Instantiate(item, target);
                instance.name = item.name;

                if (fadeMaterial != null)
                {
                    var renderers = instance.GetComponentsInChildren<Renderer>(true);
                    foreach (var renderer in renderers)
                    {
                        var materials = renderer.sharedMaterials;
                        for (int i = 0; i < materials.Length; i++)
                            materials[i] = fadeMaterial;
                        renderer.sharedMaterials = materials;
                    }
                }
            }
        }

        #endregion

        #region Internal: Animation & MA

        public static void GenerateAnimations(PrettyCureMirror script)
        {
            if (script.Avatar == null || script.Animator == null) return;

            string basePath = GetBasePath(script);
            string animFolder = Path.Combine(basePath, "Animation");

            var (enableClip, disableClip) = AnimationBuilder.CreateToggleAnimations(
                script.Avatar, script.OffTargets, script.Model, animFolder, "Enable", "Disable");

            if (enableClip == null || disableClip == null) return;

            var controller = script.Animator.runtimeAnimatorController as AnimatorController;
            if (controller != null)
            {
                AnimationBuilder.ApplyClipToState(controller, "Enable", enableClip);
                AnimationBuilder.ApplyClipToState(controller, "Disable", disableClip);
                Debug.Log("[Metamorphose] Animations applied successfully.");
            }
        }

        public static void GenerateMergeAnimator(PrettyCureMirror script)
        {
            if (script.Avatar == null) return;

            var mergeAnimator = script.Avatar.AddComponent<MergeAnimator>();
            mergeAnimator.animator = script.Animator;

            if (script.OnePiece != null && script.Model != null)
            {
                var pathMap = new MergeBlendShapePathMap();
                pathMap.Object = script.Model;
                pathMap.BlendShapeName = "nul";

                mergeAnimator.PathMap.AddItem(pathMap);
            }

            EditorUtility.SetDirty(script.Avatar);
        }

        public static string GetBasePath(MonoBehaviour script)
        {
            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(script);
            if (!string.IsNullOrEmpty(assetPath))
                return Path.GetDirectoryName(assetPath);

            string fallback = "Assets/Morulab/PrettyCureMirror";
            if (!Directory.Exists(fallback))
                Directory.CreateDirectory(fallback);
            return fallback;
        }

        #endregion
    }
}
