using System.Linq;
using nadena.dev.ndmf;
using UnityEditor.Animations;
using UnityEngine;
using Moruton.Gimmicks.Editor;

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

            if (!Validate(mirror)) return;

            GenerateAnimations(mirror);

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

        private static void GenerateAnimations(PrettyCureMirror mirror)
        {
            var controller = mirror.Animator.runtimeAnimatorController as AnimatorController;
            if (controller == null)
            {
                Debug.LogWarning("[MetamorphoseApplyPass] Animator has no AnimatorController assigned.");
                return;
            }

            var offTargets = mirror.OffTargets.Where(t => t != null).ToArray();
            var (enableClip, disableClip) = AnimationBuilder.CreateToggleClipsInMemory(
                mirror.Avatar, offTargets, mirror.Model);

            if (enableClip != null)
                AnimationBuilder.ApplyClipToState(controller, "Enable", enableClip);
            if (disableClip != null)
                AnimationBuilder.ApplyClipToState(controller, "Disable", disableClip);
        }
    }
}
