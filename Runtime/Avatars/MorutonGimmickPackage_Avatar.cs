using UnityEngine;

namespace Moruton.Gimmicks
{
    // Dummy implementation for Avatar projects where UdonSharp is not available.
    // This allows scripts referencing MorutonGimmickPackage to compile without errors,
    // although they obviously won't function as Udon behaviours.
    public abstract class MorutonGimmickPackage : MonoBehaviour
    {
        // Avatar-specific implementation or empty stubs can go here.
    }
}
