#if UNITY_EDITOR
using UnityEngine;
using VRC.SDKBase;

namespace Morution.DevTools
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Morulab/Avatars/Dev Pose Placement (Editor Only)")]
    public sealed class DevPosePlacement : MonoBehaviour, IEditorOnly
    {
        public AnimationClip clip;
        // Serialized compatibility only. The simple full-body preview ignores these.
        [HideInInspector] public AvatarMask mask;
        [HideInInspector] public Transform[] targets = new Transform[0];
    }
}
#endif
