#if MODULAR_AVATAR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// バイナリデータからAnimationClipを復元する。
    /// 復元されたクリップはHideFlagsでアクセスブロックされる。
    /// </summary>
    public static class ProtectedAnimClipBuilder
    {
        /// <summary>
        /// シリアライズされたバイナリからAnimationClipを生成。
        /// </summary>
        public static AnimationClip Build(byte[] data, string clipName)
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogWarning($"[ProtectedAnimClipBuilder] Empty data for '{clipName}'.");
                return null;
            }

            try
            {
                using var ms = new MemoryStream(data);
                using var reader = new BinaryReader(ms);

                // ヘッダー
                int formatVersion = reader.ReadInt32();
                string name = reader.ReadString();
                float length = reader.ReadSingle();
                bool loopTime = reader.ReadBoolean();
                bool loopBlend = reader.ReadBoolean();

                var clip = new AnimationClip { name = string.IsNullOrEmpty(clipName) ? name : clipName };

                // カーブ
                int curveCount = reader.ReadInt32();
                for (int i = 0; i < curveCount; i++)
                {
                    string path = reader.ReadString();
                    string propertyName = reader.ReadString();
                    int typeIndex = reader.ReadInt32();
                    int keyCount = reader.ReadInt32();

                    var curve = new AnimationCurve();
                    for (int k = 0; k < keyCount; k++)
                    {
                        float time = reader.ReadSingle();
                        float value = reader.ReadSingle();
                        float inSlope = reader.ReadSingle();
                        float outSlope = reader.ReadSingle();
                        curve.AddKey(new Keyframe(time, value, inSlope, outSlope));
                    }

                    System.Type bindingType = typeIndex switch
                    {
                        0 => typeof(Transform),
                        1 => typeof(Animator),
                        2 => typeof(SkinnedMeshRenderer),
                        _ => typeof(Transform)
                    };
                    clip.SetCurve(path, bindingType, propertyName, curve);
                }

                // Animation Events
                int eventCount = reader.ReadInt32();
                for (int i = 0; i < eventCount; i++)
                {
                    var evt = new AnimationEvent
                    {
                        time = reader.ReadSingle(),
                        functionName = reader.ReadString(),
                        floatParameter = reader.ReadSingle(),
                        intParameter = reader.ReadInt32(),
                        stringParameter = reader.ReadString(),
                    };
                    clip.AddEvent(evt);
                }

                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = loopTime;
                AnimationUtility.SetAnimationClipSettings(clip, settings);

                // アクセスブロックは AddObjectToAsset 側で行う
                // (HideAndDontSave をつけると AssetDatabase.AddObjectToAsset が失敗する)

                Debug.Log($"[ProtectedAnimClipBuilder] Built clip '{clip.name}' ({curveCount} curves, {eventCount} events)");
                return clip;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ProtectedAnimClipBuilder] Failed to build '{clipName}': {e.Message}");
                return null;
            }
        }
    }
}
#endif
