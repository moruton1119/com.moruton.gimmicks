using UnityEngine;

#if UDONSHARP
using UdonSharp;
#endif

namespace Moruton.Gimmicks
{
    // ワールド用の基底クラス
    [AddComponentMenu("Morulab/Worlds/Base Package")]
#if UDONSHARP
    public abstract class MorutonGimmickPackage : UdonSharpBehaviour
#else
    public abstract class MorutonGimmickPackage : MonoBehaviour
#endif
    {

    }
}
