#if !UDONSHARP
using UnityEngine;

namespace Moruton.Gimmicks
{
    // Dummy implementation for Avatar projects where UdonSharp is not available.
    // Wrapped in #if !UDONSHARP to ensure it never conflicts in World projects.
    public abstract class MorutonGimmickPackage : MonoBehaviour
    {
        // Avatar-specific implementation or empty stubs can go here.
    }
}
#endif
