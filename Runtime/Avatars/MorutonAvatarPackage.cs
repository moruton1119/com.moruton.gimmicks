using UnityEngine;
#if MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace Moruton.Gimmicks
{
    // アバター用の基底クラス
#if MODULAR_AVATAR
    public abstract class MorutonAvatarPackage : AvatarTagComponent
#else
    public abstract class MorutonAvatarPackage : MonoBehaviour
#endif
    {

    }
}
