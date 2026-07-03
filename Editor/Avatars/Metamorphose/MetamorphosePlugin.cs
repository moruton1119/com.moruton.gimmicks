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
            // Resolving PhaseでMAの前に実行
            // MAの「Clone animators」より前にClipをControllerに置く必要がある
            // そうすればMAがClone時にClipも一緒にコピーして統合してくれる
            InPhase(BuildPhase.Resolving)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run(new MetamorphoseApplyPass());
        }
    }
}
#endif
