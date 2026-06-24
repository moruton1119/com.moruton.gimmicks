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

            InPhase(BuildPhase.Generating)
                .Run(new MetamorphoseApplyPass());
        }
    }
}
