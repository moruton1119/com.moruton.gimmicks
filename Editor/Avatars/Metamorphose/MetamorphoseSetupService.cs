using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 変身ギミックのエディター用ユーティリティ。
    /// ビルド時の処理は NDMF (MetamorphoseApplyPass) が担当。
    /// </summary>
    public static class MetamorphoseSetupService
    {
        /// <summary>
        /// ギミック色を全コラーオブジェクトに適用（エディタープレビュー用）。
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
    }
}
