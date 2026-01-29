using UnityEngine;
// using nadena.dev.modular_avatar.core; // 必要に応じてコメントアウト解除

namespace Moruton.Gimmicks
{
    /// <summary>
    /// Base class for Moruton Laboratory gimmicks (Avatar Only).
    /// </summary>
    [RequireComponent(typeof(RectTransform))] // アバターギミックは大抵RectTransformを使うため
    public abstract class MorutonGimmickPackage : MonoBehaviour
    {
        // 共通の処理があればここに記述
    }
}
