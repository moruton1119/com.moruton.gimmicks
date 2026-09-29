using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Morution.DevTools
{
    // Evaluate only Humanoid muscle curves. No AnimationMode, Animator controller,
    // graph, object curves or global Undo bookkeeping can capture prop edits.
    public sealed class DevPoseSession : IDisposable
    {
        public static DevPoseSession Active { get; private set; }
        public DevPosePlacement Owner { get; private set; }
        readonly Animator animator;
        readonly Avatar avatar;
        readonly Transform hips;
        readonly RootPose rootPose;
        readonly Vector3 hipsWorldPosition;
        readonly Dictionary<Transform, LocalPose> original = new Dictionary<Transform, LocalPose>();
        readonly Dictionary<Transform, LocalPose> posed = new Dictionary<Transform, LocalPose>();
        readonly Dictionary<int, AnimationCurve> curves = new Dictionary<int, AnimationCurve>();
        HumanPoseHandler handler;
        HumanPose initial;
        bool disposed;
        public float Time { get; private set; }
        public float Duration { get; private set; }
        public bool IsAlive => !disposed && Owner && animator && animator.avatar == avatar;

        struct LocalPose
        {
            readonly Vector3 position, scale;
            readonly Quaternion rotation;
            public LocalPose(Transform t) { position = t.localPosition; rotation = t.localRotation; scale = t.localScale; }
            public void Restore(Transform t) { t.localPosition = position; t.localRotation = rotation; t.localScale = scale; }
        }

        // HumanPoseHandler may apply root translation while evaluating a pose. Keep the
        // avatar root at the world pose where the session started, including parented roots.
        struct RootPose
        {
            readonly Vector3 position, localScale;
            readonly Quaternion rotation;
            public RootPose(Transform t) { position = t.position; rotation = t.rotation; localScale = t.localScale; }
            public void Restore(Transform t)
            {
                t.localScale = localScale;
                t.SetPositionAndRotation(position, rotation);
            }
        }

        public static Animator FindAnimator(DevPosePlacement settings)
        {
            if (!settings) return null;
            var descriptor = settings.GetComponentInParent<VRC.SDKBase.VRC_AvatarDescriptor>(true);
            return descriptor ? descriptor.GetComponent<Animator>() : null;
        }

        // AnimationClip finger bindings differ from HumanTrait's display names.
        static string MuscleName(string property)
        {
            return property.Replace("LeftHand.", "Left ").Replace("RightHand.", "Right ").Replace('.', ' ');
        }

        static Dictionary<int, AnimationCurve> ReadCurves(AnimationClip clip)
        {
            if (AnimationUtility.GetObjectReferenceCurveBindings(clip).Length != 0)
                throw new InvalidOperationException("Object-reference curves are not supported. Use a muscle-only pose clip.");
            var result = new Dictionary<int, AnimationCurve>();
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                int muscle = Array.IndexOf(HumanTrait.MuscleName, MuscleName(binding.propertyName));
                if (binding.type != typeof(Animator) || binding.path != "" || muscle < 0)
                    throw new InvalidOperationException("Unsupported clip binding: " + binding.path + " / " + binding.propertyName
                        + ". Only Humanoid muscles are supported (no props, root motion or blendshapes).");
                result[muscle] = AnimationUtility.GetEditorCurve(clip, binding);
            }
            if (result.Count == 0) throw new InvalidOperationException("No Humanoid muscle curves in Clip.");
            return result;
        }

        public static string Validate(DevPosePlacement settings)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return "Edit mode only.";
            if (AnimationMode.InAnimationMode()) return "Close the other animation preview first.";
            if (Active != null) return "A Dev pose session is already active.";
            if (!settings || EditorUtility.IsPersistent(settings) || !settings.gameObject.scene.IsValid()) return "Use a scene avatar instance.";
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return "Use a scene instance, not Prefab Stage.";
            var a = FindAnimator(settings);
            if (!a || !a.avatar || !a.avatar.isValid || !a.avatar.isHuman) return "Parent VRCAvatarDescriptor needs a valid root Humanoid Animator.";
            if (!a.GetBoneTransform(HumanBodyBones.Hips)) return "Humanoid Animator needs a valid Hips bone.";
            if (!settings.clip || settings.clip.legacy) return "Assign a non-Legacy Humanoid pose Clip.";
            try { ReadCurves(settings.clip); }
            catch (Exception ex) { return ex.Message; }
            return null;
        }

        public DevPoseSession(DevPosePlacement settings)
        {
            string error = Validate(settings);
            if (error != null) throw new InvalidOperationException(error);
            Owner = settings;
            animator = FindAnimator(settings);
            avatar = animator.avatar;
            hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            rootPose = new RootPose(animator.transform);
            hipsWorldPosition = hips.position;
            Duration = settings.clip.length;
            curves = ReadCurves(settings.clip);
            try
            {
                // Only the humanoid skeleton and its intermediate parent joints, NEVER
                // every descendant. Props below hands are deliberately absent.
                for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                    for (var t = animator.GetBoneTransform((HumanBodyBones)i); t && t != animator.transform; t = t.parent)
                        if (!original.ContainsKey(t)) original.Add(t, new LocalPose(t));
                handler = new HumanPoseHandler(avatar, animator.transform);
                handler.GetHumanPose(ref initial);
                Active = this;
                AssemblyReloadEvents.beforeAssemblyReload += Dispose;
                EditorApplication.playModeStateChanged += OnPlayMode;
                EditorApplication.quitting += Dispose;
                EditorApplication.update += Tick;
                EditorSceneManager.sceneSaving += OnSceneSaving;
                Sample(0);
            }
            catch { Dispose(); throw; }
        }

        public void Sample(float time)
        {
            if (!IsAlive) { Dispose(); return; }
            if (AnimationMode.InAnimationMode()) { Dispose(); return; }
            try
            {
                Time = Mathf.Clamp(time, 0, Duration);
                var pose = new HumanPose { bodyPosition = initial.bodyPosition,
                    bodyRotation = initial.bodyRotation, muscles = (float[])initial.muscles.Clone() };
                foreach (var pair in curves) pose.muscles[pair.Key] = pair.Value.Evaluate(Time);
                handler.SetHumanPose(ref pose);
                // Root motion/translation is not part of this muscle-only preview. Restore the
                // captured root first, then remove only Hips world translation. Hips rotation
                // remains the evaluated pose, so muscle-driven limb/body rotation is preserved.
                rootPose.Restore(animator.transform);
                hips.position = hipsWorldPosition;
                posed.Clear();
                foreach (var pair in original) if (pair.Key) posed.Add(pair.Key, new LocalPose(pair.Key));
                SceneView.RepaintAll();
            }
            catch { Dispose(); throw; }
        }

        void Tick()
        {
            if (!IsAlive || EditorApplication.isPlayingOrWillChangePlaymode || AnimationMode.InAnimationMode())
            { Dispose(); return; }
            // Hold only the skeleton at the session-start world placement. Local prop edits
            // remain normal Unity edits because props are absent from posed.
            rootPose.Restore(animator.transform);
            foreach (var pair in posed)
            {
                if (!pair.Key) { Dispose(); return; }
                pair.Value.Restore(pair.Key);
            }
        }
        void OnPlayMode(PlayModeStateChange state) { Dispose(); }
        void OnSceneSaving(Scene scene, string path)
        {
            // Never serialize the temporary pose into the user's scene.
            if (animator && animator.gameObject.scene == scene) Dispose();
        }
        void RestoreOriginal()
        {
            if (animator) rootPose.Restore(animator.transform);
            foreach (var pair in original) if (pair.Key) pair.Value.Restore(pair.Key);
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            AssemblyReloadEvents.beforeAssemblyReload -= Dispose;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorApplication.quitting -= Dispose;
            EditorApplication.update -= Tick;
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            try { RestoreOriginal(); }
            finally
            {
                try { handler?.Dispose(); }
                finally
                {
                    handler = null;
                    if (Active == this) Active = null;
                    SceneView.RepaintAll();
                }
            }
        }
    }
}
