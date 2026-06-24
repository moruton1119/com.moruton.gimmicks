using UnityEditor;
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

                var particleSystems = collar.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var ps in particleSystems)
                {
                    Undo.RecordObject(ps, "Change Gimmick Color");
                    var main = ps.main;
                    main.startColor = script.gimmickColor;
                    EditorUtility.SetDirty(ps);
                }
            }
        }
    }
}
