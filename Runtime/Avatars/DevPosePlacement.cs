#if UNITY_EDITOR
using UnityEngine;
using VRC.SDKBase;

namespace Morution.DevTools
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Morulab/Avatars/Dev Pose Placement (Editor Only)")]
    public sealed class DevPosePlacement : MonoBehaviour, IEditorOnly
    {
        [Tooltip("固定するHumanoidポーズのAnimationClipです。アバターの開始時world位置・回転・スケールとHips位置は維持されます。")]
        public AnimationClip clip;
        [Tooltip("Sceneビューで近接表示するシーン内オブジェクトです。")]
        public Transform focusTarget;
        [Tooltip("対象を近接表示するときのSceneビューカメラと対象の距離（m）です。")]
        public float focusDistance = 0.3f;
        // Serialized compatibility only. The simple full-body preview ignores these.
        [HideInInspector] public AvatarMask mask;
        [HideInInspector] public Transform[] targets = new Transform[0];
    }
}
#endif
