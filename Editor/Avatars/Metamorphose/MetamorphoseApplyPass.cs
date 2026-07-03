#if MODULAR_AVATAR
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

            var mirror = ctx.AvatarRootObject.GetComponentInChildren<Metamorphose>();
            if (mirror == null)
            {
                Debug.Log("[MetamorphoseApplyPass] No Metamorphose found. Skipping.");
                return;
            }

            Debug.Log($"[MetamorphoseApplyPass] Found Metamorphose on '{mirror.gameObject.name}'");

            if (!Validate(mirror))
            {
                Debug.LogWarning("[MetamorphoseApplyPass] Validation failed. Skipping.");
                return;
            }

            Debug.Log("[MetamorphoseApplyPass] Validation passed. Processing...");

            UnpackAllPrefabs(ctx.AvatarRootObject);

            GenerateAnimations(ctx, mirror);

            // ─── Protected Animation注入 ───
            InjectProtectedAnimations(ctx, mirror);

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

        private static void InjectProtectedAnimations(BuildContext ctx, Metamorphose mirror)
        {
            // ProtectedAnimDllが設定されていなければスキップ
            if (mirror.ProtectedAnimDll == null)
                return;

            Debug.Log("[MetamorphoseApplyPass] Protected Animation: processing...");

            // DLL読み込み
            string dllPath = ProtectedAnimLoader.GetDllPath(mirror.ProtectedAnimDll);
            if (!ProtectedAnimLoader.LoadDll(dllPath))
            {
                Debug.LogWarning("[MetamorphoseApplyPass] Protected Animation: DLL load failed.");
                return;
            }

            // 注入先Controllerの確認
            if (mirror.ProtectedAnimTargetController == null)
            {
                Debug.LogWarning("[MetamorphoseApplyPass] Protected Animation: Target controller not set.");
                return;
            }

            // Controllerをクローン
            var originalController = mirror.ProtectedAnimTargetController as AnimatorController;
            if (originalController == null)
            {
                Debug.LogWarning("[MetamorphoseApplyPass] Protected Animation: Controller is not AnimatorController.");
                return;
            }

            var clonedController = Object.Instantiate(originalController);
            clonedController.name = originalController.name + "_Protected";

            // NDMF AssetContainerに登録
            if (ctx.AssetContainer != null)
            {
                AssetDatabase.AddObjectToAsset(clonedController, ctx.AssetContainer);
            }

            // 各マッピングを復号→クリップ生成→注入
            var mappings = mirror.ProtectedAnimMappings;
            if (mappings == null || mappings.Length == 0)
            {
                Debug.LogWarning("[MetamorphoseApplyPass] Protected Animation: No mappings set.");
                return;
            }

            foreach (var mapping in mappings)
            {
                string dllKey = mapping.dllKey?.Trim();
                string stateName = mapping.stateName?.Trim();
                if (string.IsNullOrEmpty(dllKey) || string.IsNullOrEmpty(stateName)) continue;

                byte[] data = ProtectedAnimLoader.LoadDecrypted(dllKey);
                if (data == null)
                {
                    Debug.LogWarning($"[MetamorphoseApplyPass] Protected Animation: Failed to decrypt '{dllKey}'.");
                    continue;
                }

                var clip = ProtectedAnimClipBuilder.Build(data, stateName);
                if (clip == null)
                {
                    Debug.LogError($"[MetamorphoseApplyPass] Protected Animation: ClipBuilder returned null for '{dllKey}' → '{stateName}'. Animation NOT injected.");
                    continue;
                }

                // Controllerに直接追加（AssetContainerじゃなくてControllerの子にする）
                AssetDatabase.AddObjectToAsset(clip, clonedController);

                AnimationBuilder.ApplyClipToState(clonedController, stateName, clip);

                Debug.Log($"[MetamorphoseApplyPass] Protected Animation: Injected '{dllKey}' → state '{stateName}'.");
            }

            // MA MergeAnimatorがあれば更新
            var components = ctx.AvatarRootObject.GetComponentsInChildren<Component>(true);
            foreach (var comp in components)
            {
                if (comp == null) continue;
                if (comp.GetType().FullName == "nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator")
                {
                    var type = comp.GetType();
                    var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    foreach (var f in fields)
                    {
                        if (f.FieldType == typeof(RuntimeAnimatorController))
                        {
                            var val = f.GetValue(comp) as RuntimeAnimatorController;
                            if (val == originalController)
                            {
                                f.SetValue(comp, clonedController);
                                Debug.Log($"[MetamorphoseApplyPass] Updated MergeAnimator '{comp.gameObject.name}'.");
                            }
                        }
                    }
                }
            }

            Debug.Log("[MetamorphoseApplyPass] Protected Animation: complete.");
        }

        private static void GenerateAnimations(BuildContext ctx, Metamorphose mirror)
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
                ctx.AvatarRootObject, offTargets, mirror.Model);

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

            // Find and update any ModularAvatarMergeAnimator components on the avatar to point to the cloned controller
            var components = ctx.AvatarRootObject.GetComponentsInChildren<Component>(true);
            foreach (var comp in components)
            {
                if (comp != null && comp.GetType().FullName == "nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator")
                {
                    var type = comp.GetType();
                    var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    foreach (var f in fields)
                    {
                        if (f.FieldType == typeof(RuntimeAnimatorController))
                        {
                            var val = f.GetValue(comp) as RuntimeAnimatorController;
                            if (val == originalController)
                            {
                                f.SetValue(comp, clonedController);
                                Debug.Log($"[MetamorphoseApplyPass] Updated ModularAvatarMergeAnimator '{comp.gameObject.name}' field '{f.Name}' from original controller to cloned controller.");
                            }
                        }
                    }
                }
            }

            Debug.Log("[MetamorphoseApplyPass] Animation generation and controller cloning complete.");
        }

        private static bool Validate(Metamorphose mirror)
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
#endif
