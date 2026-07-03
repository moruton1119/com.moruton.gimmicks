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

            // MAの処理前に実行して、アニメーションをControllerに紐づける
            // その後MAが普通にMergeAnimatorで統合する
            InPhase(BuildPhase.Transforming)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run(new MetamorphoseApplyPass());
        }
    }
}
#endif
