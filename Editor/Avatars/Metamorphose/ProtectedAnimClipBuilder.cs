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

            Debug.Log($"[ProtectedAnimClipBuilder] Building '{clipName}' from {data.Length} bytes");

            try
            {
                using var ms = new MemoryStream(data);
                using var reader = new BinaryReader(ms);

                // ヘッダー
                int formatVersion = reader.ReadInt32();
                string name = reader.ReadString();
                float length = reader.ReadSingle();

                // === AnimationClipSettings の読み込み ===
                bool loopTime = reader.ReadBoolean();
                bool loopBlend = reader.ReadBoolean();

                // v2で追加されたフィールド（後方互換: v1の場合はデフォルト値を使用）
                bool loopBlendOrientation = formatVersion >= 2 && reader.ReadBoolean();
                bool loopBlendPositionY = formatVersion >= 2 && reader.ReadBoolean();
                bool loopBlendPositionXZ = formatVersion >= 2 && reader.ReadBoolean();
                bool keepOriginalOrientation = formatVersion >= 2 && reader.ReadBoolean();
                bool keepOriginalPositionY = formatVersion >= 2 && reader.ReadBoolean();
                bool keepOriginalPositionXZ = formatVersion >= 2 && reader.ReadBoolean();
                bool heightFromFeet = formatVersion >= 2 && reader.ReadBoolean();
                float cycleOffset = formatVersion >= 2 ? reader.ReadSingle() : 0f;
                int level = formatVersion >= 2 ? reader.ReadInt32() : 0;
                bool hasAdditiveReferencePose = formatVersion >= 2 && reader.ReadBoolean();
                // note: additiveReferenceFrameTime はUnityバージョンによって存在しないため除外

                Debug.Log($"[ProtectedAnimClipBuilder] Header: v{formatVersion}, name='{name}', length={length}, loop={loopTime}, pos={ms.Position}/{data.Length}");

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

                // === AnimationClipSettings の復元 ===
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = loopTime;
                settings.loopBlend = loopBlend;
                if (formatVersion >= 2)
                {
                    settings.loopBlendOrientation = loopBlendOrientation;
                    settings.loopBlendPositionY = loopBlendPositionY;
                    settings.loopBlendPositionXZ = loopBlendPositionXZ;
                    settings.keepOriginalOrientation = keepOriginalOrientation;
                    settings.keepOriginalPositionY = keepOriginalPositionY;
                    settings.keepOriginalPositionXZ = keepOriginalPositionXZ;
                    settings.heightFromFeet = heightFromFeet;
                    settings.cycleOffset = cycleOffset;
                    settings.level = level;
                    settings.hasAdditiveReferencePose = hasAdditiveReferencePose;
                }
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
