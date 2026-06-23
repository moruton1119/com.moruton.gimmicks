using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;
using nadena.dev.modular_avatar.core;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 変身ギミックのセットアップ実行サービス。
    /// MetamorphoseEditor (UI) から分離されたロジック層。
    /// 元の PrettyCureMirrorEditor のロジックをそのまま移植。
    /// </summary>
    public static class MetamorphoseSetupService
    {
        #region Public API

        /// <summary>
        /// 全工程を実行: ワンピース差し替え → アイテム装着 → コラボ → フェード → アニメーション生成
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

            // ワンピース差し替え
            if (script.ColaboFBX != null && script.OnePiece != null)
            {
                ReplaceOnePieceChild(script.OnePiece.transform, script.ColaboFBX);
            }

            // アイテム装着
            ProcessItemAttachment(script.headTarget, script.headItems);
            ProcessItemAttachment(script.bodyTarget, script.bodyItems);
            ProcessItemAttachment(script.handTarget, script.handItems);
            ProcessItemAttachment(script.legTarget, script.legItems);

            // 追加アイテム
            if (script.colaboItemTarget != null && script.colaboItem != null)
            {
                ReplaceOnePieceChild(script.colaboItemTarget, script.colaboItem);
            }

            // フェード演出
            if (script.fadeHead != null)
                ProcessFadeAttachment(script.fadeHead, script.fadeHeadItems, script.fadeHeadMaterial);
            if (script.fadeBody != null)
                ProcessFadeAttachment(script.fadeBody, script.fadeBodyItems, script.fadeBodyMaterial);
            if (script.fadeArm != null)
                ProcessFadeAttachment(script.fadeArm, script.fadeArmItems, script.fadeArmMaterial);
            if (script.fadeLeg != null)
                ProcessFadeAttachment(script.fadeLeg, script.fadeLegItems, script.fadeLegMaterial);

            // アニメーション生成
            CreateAnimations(script);

            Debug.Log("[Metamorphose] Setup complete!");
        }

        /// <summary>
        /// ギミック色を全コラーオブジェクトに適用。
        /// </summary>
        public static void ApplyGimmickColor(PrettyCureMirror script)
        {

        #region ProcessItemAttachment (元のまま)

        private static void ProcessItemAttachment(Transform target, GameObject[] items)
        {
            if (target == null) return;
            if (items == null) items = new GameObject[0];

            var itemSet = new HashSet<GameObject>(items);

            // 不要な子を削除
            var toDelete = new List<Transform>();
            foreach (Transform child in target)
            {
                var source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                if (source != null && itemSet.Contains(source)) continue;
                if (itemSet.Contains(child.gameObject)) continue;
                toDelete.Add(child);
            }
            foreach (var child in toDelete)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }

            // 新しいアイテムを追加
            foreach (var item in items)
            {
                if (item == null) continue;

                bool exists = false;
                foreach (Transform child in target)
                {
                    var source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                    if (source == item || child.gameObject == item)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    GameObject instance;
                    if (PrefabUtility.IsPartOfPrefabAsset(item))
                    {
                        instance = (GameObject)PrefabUtility.InstantiatePrefab(item, target);
                        Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
                    }
                    else
                    {
                        instance = item;
                        Undo.SetTransformParent(instance.transform, target, "Move Item");
                    }

                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                    instance.name = item.name;
                }
            }
        }

        #endregion

        #region ProcessFadeAttachment (元のまま)

        private static void ProcessFadeAttachment(Transform target, GameObject[] items, Material material)
        {
            if (target == null) return;

            // 既存の子を全削除
            var toDelete = new List<Transform>();
            foreach (Transform child in target)
            {
                toDelete.Add(child);
            }
            foreach (var child in toDelete)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }

            if (items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;

                GameObject instance;
                if (PrefabUtility.IsPartOfPrefabAsset(item))
                {
                    instance = (GameObject)PrefabUtility.InstantiatePrefab(item, target);
                }
                else
                {
                    instance = Object.Instantiate(item, target);
                }
                Undo.RegisterCreatedObjectUndo(instance, "Create Fade Item");

                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.name = target.name;

                // マテリアル差し替え
                if (material != null)
                {
                    var renderers = instance.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        var mats = new Material[r.sharedMaterials.Length];
                        for (int i = 0; i < mats.Length; i++)
                        {
                            mats[i] = material;
                        }
                        r.sharedMaterials = mats;
                    }
                }
            }
        }

        #endregion

        #region CreateAnimations (元のまま)

        private static void CreateAnimations(PrettyCureMirror script)
        {
            string scriptAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(script);
            string basePath;

            if (!string.IsNullOrEmpty(scriptAssetPath))
            {
                basePath = Path.GetDirectoryName(scriptAssetPath);
            }
            else
            {
                basePath = "Assets/Morulab/PrettyCureMirror";
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }
            }

            string animFolder = Path.Combine(basePath, "Animation");
            if (!Directory.Exists(animFolder))
            {
                Directory.CreateDirectory(animFolder);
            }

            string enablePath = Path.Combine(animFolder, "Enable.anim");
            string disablePath = Path.Combine(animFolder, "Disable.anim");

            // Enable Animation
            AnimationClip enableClip = new AnimationClip();
            foreach (var obj in script.OffTargets)
            {
                if (obj == null) continue;
                string path = GetPath(script.Avatar, obj);
                enableClip.SetCurve(path, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 0));
            }

            string modelPath = GetPath(script.Avatar, script.Model);
            enableClip.SetCurve(modelPath, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 1));

            AssetDatabase.CreateAsset(enableClip, enablePath);

            // Disable Animation
            AnimationClip disableClip = new AnimationClip();
            foreach (var obj in script.OffTargets)
            {
                if (obj == null) continue;
                string path = GetPath(script.Avatar, obj);
                disableClip.SetCurve(path, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 1));
            }

            disableClip.SetCurve(modelPath, typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 0, 0));

            AssetDatabase.CreateAsset(disableClip, disablePath);

            AssetDatabase.SaveAssets();

            // AnimatorControllerに適用
            var controller = script.Animator.runtimeAnimatorController as AnimatorController;
            if (controller != null)
            {
                SetAnimationToState(controller, "Enable", enableClip);
                SetAnimationToState(controller, "Disable", disableClip);
                Debug.Log("[Metamorphose] Animations applied successfully.");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    LocalizationManager.GetCommon("error_dialog_title"),
                    "AnimatorController not set.",
                    LocalizationManager.GetCommon("ok"));
            }
        }

        private static string GetPath(GameObject root, GameObject child)
        {
            var path = new List<string>();
            var current = child.transform;
            while (current != null && current != root.transform)
            {
                path.Add(current.name);
                current = current.parent;
            }
            path.Reverse();
            return string.Join("/", path);
        }

        private static void SetAnimationToState(AnimatorController controller, string stateName, AnimationClip clip)
        {
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

        #endregion
    }
}
