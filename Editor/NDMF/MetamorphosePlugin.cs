using nadena.dev.ndmf;

namespace Moruton.Gimmicks.Editor.NDMF
{
    /// <summary>
    /// NDMF プラグイン: ビルド時にアイテム配置・アニメーション生成を自動実行する。
    /// MA の Transforming フェーズの後に実行される。
    /// </summary>
    public sealed class MetamorphosePlugin : Plugin<MetamorphosePlugin>
    {
        public override string QualifiedName => "com.moruton.gimmicks.metamorphose";
        public override string DisplayName => "Metamorphose (Moruton Gimmicks)";

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.modular_avatar.core")
                .Run(new MetamorphoseApplyPass());
        }
    }
}
