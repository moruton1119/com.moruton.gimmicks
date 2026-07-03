#if MODULAR_AVATAR
using nadena.dev.ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(Moruton.Gimmicks.Editor.MetamorphosePlugin))]

namespace Moruton.Gimmicks.Editor
{
    public sealed class MetamorphosePlugin : Plugin<MetamorphosePlugin>
    {
        public override string QualifiedName => "com.moruton.gimmicks.metamorphose";
        public override string DisplayName => "Metamorphose (Moruton Gimmicks)";

        static MetamorphosePlugin()
        {
            Debug.Log("[MetamorphosePlugin] Assembly loaded");
        }

        protected override void Configure()
        {
            Debug.Log("[MetamorphosePlugin] Configure() - registering pass");

            // Generating PhaseでMAの前に実行
            // 公式ドキュメント推奨: https://modular-avatar.nena.dev/ja/docs/extending
            InPhase(BuildPhase.Generating)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run(new MetamorphoseApplyPass());
        }
    }
}
#endif
