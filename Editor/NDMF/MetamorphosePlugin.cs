using nadena.dev.ndmf;
using UnityEngine;

namespace Moruton.Gimmicks.Editor.NDMF
{
    /// <summary>
    /// NDMF プラグイン: ビルド時にアイテム配置・アニメーション生成を自動実行する。
    /// </summary>
    public sealed class MetamorphosePlugin : Plugin<MetamorphosePlugin>
    {
        public override string QualifiedName => "com.moruton.gimmicks.metamorphose";
        public override string DisplayName => "Metamorphose (Moruton Gimmicks)";

        static MetamorphosePlugin()
        {
            Debug.Log("[MetamorphosePlugin] Assembly loaded — plugin type registered.");
        }

        protected override void Configure()
        {
            Debug.Log("[MetamorphosePlugin] Configure() called — registering pass.");

            InPhase(BuildPhase.Transforming)
                .Run(new MetamorphoseApplyPass());

            Debug.Log("[MetamorphosePlugin] Pass registered in Transforming phase.");
        }
    }
}
