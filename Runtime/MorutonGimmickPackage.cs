#if MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace Moruton.Gimmicks
{
    /// <summary>
    /// Base class for Moruton Laboratory gimmicks (Avatar Only).
    /// </summary>
#if MODULAR_AVATAR
    public abstract class MorutonGimmickPackage : AvatarTagComponent
#else
    public abstract class MorutonGimmickPackage : MonoBehaviour
#endif
    {
        // 共通の処理があればここに記述
    }
}
