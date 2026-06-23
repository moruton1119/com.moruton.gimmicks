using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Moruton.Gimmicks.Editor; // ItemPlacer

namespace Moruton.Gimmicks.Editor.NDMF
{
    /// <summary>
    /// NDMF Build Pass: ビルド時にアイテム配置＋アニメーション生成を行う。
    /// 削除・Prefab解除は不要（NDMF がクローンに対して動くため）。
    /// </summary>
    public sealed class MetamorphoseApplyPass : Pass<MetamorphoseApplyPass>
    {
        protected override void Execute(BuildContext ctx)
        {
            var mirror = ctx.AvatarRootObject.GetComponentInChildren<PrettyCureMirror>();
            if (mirror == null) return;

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

            // ─── アニメーション生成 ───
            CreateAnimations(ctx, mirror);
        }

        #region Animation Creation

        /// <summary>
        /// Enable/Disable アニメーションを生成し、AnimatorController に設定する。
        /// </summary>
        private static void CreateAnimations(BuildContext ctx, PrettyCureMirror mirror)
        {
            if (mirror.Animator == null) return;

            var controller = mirror.Animator.runtimeAnimatorController as AnimatorController;
            if (controller == null) return;

            // コントローラをクローン（元のアセットを変更しないため）
            var cloned = Object.Instantiate(controller);
            cloned.name = controller.name + "_Metamorphose";
            mirror.Animator.runtimeAnimatorController = cloned;

            // Enable クリップ: 旧衣装OFF、新モデルON
            var enableClip = new AnimationClip { name = "Enable" };
            // Disable クリップ: 旧衣装ON、新モデルOFF
            var disableClip = new AnimationClip { name = "Disable" };

            if (mirror.OffTargets != null)
            {
                foreach (var obj in mirror.OffTargets)
                {
                    if (obj == null) continue;
                    string path = GetRelativePath(ctx.AvatarRootObject, obj);
                    enableClip.SetCurve(path, typeof(GameObject), "m_IsActive",
                        AnimationCurve.Constant(0f, 1f / 60f, 0f));
                    disableClip.SetCurve(path, typeof(GameObject), "m_IsActive",
                        AnimationCurve.Constant(0f, 1f / 60f, 1f));
                }
            }

            if (mirror.Model != null)
            {
                string modelPath = GetRelativePath(ctx.AvatarRootObject, mirror.Model);
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
