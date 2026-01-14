using UnityEngine;

namespace Moruton.Gimmicks
{
    // アバター用の基底クラス
    public abstract class MorutonAvatarPackage : MonoBehaviour
    {
        // もるらぼのギミックに共通のEditor処理を書き込むための継承用Script
        [SerializeField] private Texture2D dummyImage;
    }
}
