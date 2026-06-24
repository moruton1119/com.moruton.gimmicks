using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// NDMF Build Pass: ビルド時にアイテム配置＋アニメーション生成を行う。
    /// </summary>
    public sealed class MetamorphoseApplyPass : Pass<MetamorphoseApplyPass>
    {
        protected override void Execute(BuildContext ctx)
        {
            Debug.Log("[MetamorphoseApplyPass] === Execute started ===");

            var mirror = ctx.AvatarRootObject.GetComponentInChildren<PrettyCureMirror>();
            if (mirror == null)
            {
                Debug.Log("[MetamorphoseApplyPass] No PrettyCureMirror found. Skipping.");
                return;
            }

            Debug.Log($"[MetamorphoseApplyPass] Found PrettyCureMirror on '{mirror.gameObject.name}'");

            if (!Validate(mirror))
            {
                Debug.LogWarning("[MetamorphoseApplyPass] Validation failed. Skipping.");
                return;
            }

            Debug.Log("[MetamorphoseApplyPass] Validation passed. Processing...");

            UnpackAllPrefabs(ctx.AvatarRootObject);

            GenerateAnimations(ctx, mirror);

            // ─── Step 2: 変身後の衣装配置 ───
            ItemPlacer.PlaceItems(mirror.headTarget, mirror.headItems);
            ItemPlacer.PlaceItems(mirror.bodyTarget, mirror.bodyItems);
            ItemPlacer.PlaceItems(mirror.handTarget, mirror.handItems);
            ItemPlacer.PlaceItems(mirror.legTarget, mirror.legItems);

            // ─── コラボアイテム ───
            if (mirror.colaboItemTarget != null && mirror.colaboItem != null)
            {
                ItemPlacer.PlaceItems(mirror.colaboItemTarget, new[] { mirror.colaboItem });
            }

            // ─── Step 4: フェード演出アイテム配置 ───
            ItemPlacer.PlaceItems(mirror.fadeHead, mirror.fadeHeadItems, mirror.fadeHeadMaterial);
            ItemPlacer.PlaceItems(mirror.fadeBody, mirror.fadeBodyItems, mirror.fadeBodyMaterial);
            ItemPlacer.PlaceItems(mirror.fadeArm, mirror.fadeArmItems, mirror.fadeArmMaterial);
            ItemPlacer.PlaceItems(mirror.fadeLeg, mirror.fadeLegItems, mirror.fadeLegMaterial);

            // ─── ワンピース差し替え ───
            if (mirror.OnePiece != null && mirror.ColaboFBX != null)
            {
                ItemPlacer.PlaceItems(mirror.OnePiece.transform, new[] { mirror.ColaboFBX });
            }
        }

        private static void GenerateAnimations(BuildContext ctx, PrettyCureMirror mirror)
        {
            var originalController = mirror.Animator.runtimeAnimatorController as AnimatorController;
            if (originalController == null)
            {
                Debug.LogWarning("[MetamorphoseApplyPass] Animator has no AnimatorController assigned.");
                return;
            }

            // Clone the AnimatorController to prevent modifying the original asset
            var clonedController = Object.Instantiate(originalController);
            clonedController.name = originalController.name + "_Cloned";

            // Register the cloned controller to NDMF's AssetContainer
            if (ctx.AssetContainer != null)
            {
                AssetDatabase.AddObjectToAsset(clonedController, ctx.AssetContainer);
            }

            var offTargets = mirror.OffTargets.Where(t => t != null).ToArray();

            // Create toggle clips in memory
            var (enableClip, disableClip) = AnimationBuilder.CreateToggleClipsInMemory(
                mirror.Avatar, offTargets, mirror.Model);

            if (enableClip != null)
            {
                enableClip.name = "Metamorphose_Enable";
                if (ctx.AssetContainer != null)
                {
                    AssetDatabase.AddObjectToAsset(enableClip, ctx.AssetContainer);
                }
                AnimationBuilder.ApplyClipToState(clonedController, "Enable", enableClip);
            }

            if (disableClip != null)
            {
                disableClip.name = "Metamorphose_Disable";
                if (ctx.AssetContainer != null)
                {
                    AssetDatabase.AddObjectToAsset(disableClip, ctx.AssetContainer);
                }
                AnimationBuilder.ApplyClipToState(clonedController, "Disable", disableClip);
            }

            // Assign the cloned controller back to the Animator
            mirror.Animator.runtimeAnimatorController = clonedController;

            Debug.Log("[MetamorphoseApplyPass] Animation generation and controller cloning complete.");
        }

        private static bool Validate(PrettyCureMirror mirror)
        {
            if (mirror.Model == null || mirror.Animator == null)
            {
                Debug.LogWarning("[MetamorphoseApplyPass] model or animator is not set.");
                return false;
            }

            if (mirror.OffTargets == null || mirror.OffTargets.All(t => t == null))
            {
                Debug.LogWarning("[MetamorphoseApplyPass] offTargets is not set.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// クローン内の全Prefabインスタンスを完全にアンパックする。
        /// Prefab接続が残っていると子オブジェクトの移動・変更が反映されない場合がある。
        /// </summary>
        private static void UnpackAllPrefabs(GameObject root)
        {
            var prefabRoots = new List<GameObject>();
            var allTransforms = root.GetComponentsInChildren<Transform>(true);

            foreach (var t in allTransforms)
            {
                if (t == null) continue;
                if (PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject))
                    prefabRoots.Add(t.gameObject);
            }

            foreach (var prefabRoot in prefabRoots)
            {
                if (prefabRoot == null) continue;
                PrefabUtility.UnpackPrefabInstance(
                    prefabRoot,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }
        }
    }
}
