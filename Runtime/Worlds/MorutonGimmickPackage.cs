using UnityEngine;

#if UDONSHARP
using UdonSharp;
#endif

namespace Moruton.Gimmicks
{
    // ワールド用の基底クラス
#if UDONSHARP
    public abstract class MorutonGimmickPackage : UdonSharpBehaviour
#else
    public abstract class MorutonGimmickPackage : MonoBehaviour
#endif
    {
        // もるらぼのギミックに共通のEditor処理を書き込むための継承用Script
        [SerializeField] private Texture2D dummyImage;
    }
}
