using System;
using UnityEditor;
using UnityEngine;
using Moruton.Gimmicks.Editor;

namespace Morution.DevTools
{
    [CustomEditor(typeof(DevPosePlacement))]
    public sealed class DevPosePlacementEditor : Editor
    {
        const float DefaultSceneViewFieldOfView = 60f;
        const float MaxSceneViewDistance = 3.2e34f;
        const float MaxSceneViewSize = 3.2e34f;
        string error;
        bool showDev;

        internal static bool IsPositiveFinite(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        // SceneView.sizeの意味はUnity 2022.3の実装 (UnityEditor.CoreModule.dll逆コンパイル) に従う:
        // Perspective: cameraDistance = size / sin(perspectiveFov/2)   (GetPerspectiveCameraDistance)
        // Orthographic: cameraDistance = size * 2
        // perspectiveFovはSceneView.cameraSettings.fieldOfViewであり、縦長aspectで補正される
        // scene.camera.fieldOfView (GetVerticalFOV) とは別物。size→距離の逆変換をここで行う。
        internal static float SceneViewSizeForDistance(float distance, float fieldOfView, bool orthographic)
        {
            if (orthographic) return distance * 0.5f;
            return distance * Mathf.Sin(fieldOfView * 0.5f * Mathf.Deg2Rad);
        }

        static float SceneViewFieldOfView(SceneView sceneView)
        {
            float fieldOfView = sceneView.cameraSettings != null
                ? sceneView.cameraSettings.fieldOfView
                : DefaultSceneViewFieldOfView;
            return IsPositiveFinite(fieldOfView) && fieldOfView < 180f
                ? fieldOfView
                : DefaultSceneViewFieldOfView;
        }

        static string FocusValidationError(DevPosePlacement settings, SceneView sceneView)
        {
            if (!settings.focusTarget) return "近接フォーカス対象を指定してください。";
            if (EditorUtility.IsPersistent(settings.focusTarget) || !settings.focusTarget.gameObject.scene.IsValid())
                return "近接フォーカス対象にはシーン内のオブジェクトを指定してください。Prefabアセットは対象にできません。";
            if (!IsPositiveFinite(settings.focusDistance)) return "表示距離には0より大きい有限値を指定してください。";
            if (settings.focusDistance > MaxSceneViewDistance)
                return "表示距離がSceneビューで扱える範囲を超えています。";
            if (sceneView == null) return "利用できるSceneビューがありません。Sceneビューを開いてから実行してください。";
            float size = SceneViewSizeForDistance(settings.focusDistance,
                SceneViewFieldOfView(sceneView), sceneView.orthographic);
            if (!IsPositiveFinite(size) || size > MaxSceneViewSize)
                return "表示距離がSceneビューで扱える範囲を超えています。";
            return null;
        }

        static void Focus(SceneView sceneView, Transform focusTarget, float distance)
        {
            if (!IsPositiveFinite(distance) || distance > MaxSceneViewDistance)
                throw new InvalidOperationException("表示距離がSceneビューで扱える範囲を超えています。");
            bool orthographic = sceneView.orthographic;
            float size = SceneViewSizeForDistance(distance,
                SceneViewFieldOfView(sceneView), orthographic);
            if (!IsPositiveFinite(size) || size > MaxSceneViewSize)
                throw new InvalidOperationException("表示距離を有効なSceneビューサイズへ変換できませんでした。");
            sceneView.LookAt(focusTarget.position, sceneView.rotation, size, orthographic);
            sceneView.Repaint();
        }

        public override void OnInspectorGUI()
        {
            MorutonAvatarPackageEditorHelper.DrawHeader();

            var settings = (DevPosePlacement)target;
            var session = DevPoseSession.Active;

            // Dev開始/停止は常に表示（Dev折りたたみの外）
            if (session != null && session.Owner == settings)
            {
                EditorGUILayout.LabelField("ポーズ固定中");
                EditorGUI.BeginChangeCheck();
                float time = EditorGUILayout.Slider("Time (秒)", session.Time, 0, session.Duration);
                if (EditorGUI.EndChangeCheck())
                {
                    try { session.Sample(time); error = null; }
                    catch (Exception ex) { error = ex.Message; }
                }
                if (GUILayout.Button("Dev停止 — 人体の元ポーズに戻す")) session.Dispose();
            }
            else
            {
                string invalid = DevPoseSession.Validate(settings);
                if (invalid != null) EditorGUILayout.HelpBox(invalid, MessageType.Warning);
                using (new EditorGUI.DisabledScope(invalid != null))
                    if (GUILayout.Button("Dev開始 — ポーズ固定"))
                    {
                        try { new DevPoseSession(settings); error = null; }
                        catch (Exception ex) { error = ex.Message; }
                    }
            }

            showDev = EditorGUILayout.Foldout(showDev, "Dev", true);
            if (showDev)
            {
                using (new EditorGUI.DisabledScope(session != null || EditorApplication.isPlayingOrWillChangePlaymode))
                {
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("clip"));
                    serializedObject.ApplyModifiedProperties();
                }
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.ObjectField("親の Animator", DevPoseSession.FindAnimator(settings), typeof(Animator), true);
                EditorGUILayout.HelpBox("実アバターを現在のworld位置・回転・スケールとHips位置のままポーズ固定します。対象オブジェクトを選択し、通常のUnityツールで配置を編集できます。停止するには開始したコンポーネントを再選択してください。停止してもオブジェクトの編集は残ります。", MessageType.Info);

                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("focusTarget"), new GUIContent("近接フォーカス対象"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("focusDistance"), new GUIContent("表示距離 (m)"));
                serializedObject.ApplyModifiedProperties();
                var sceneView = SceneView.lastActiveSceneView;
                string focusInvalid = FocusValidationError(settings, sceneView);
                if (focusInvalid != null) EditorGUILayout.HelpBox(focusInvalid, MessageType.Warning);
                using (new EditorGUI.DisabledScope(focusInvalid != null))
                    if (GUILayout.Button("対象へ近接フォーカス"))
                    {
                        try
                        {
                            Focus(sceneView, settings.focusTarget, settings.focusDistance);
                            Selection.activeGameObject = settings.focusTarget.gameObject;
                            EditorGUIUtility.PingObject(settings.focusTarget.gameObject);
                            Tools.current = Tool.Move;
                            error = null;
                        }
                        catch (Exception ex) { error = ex.Message; }
                    }

                if (session != null && session.Owner != settings)
                    EditorGUILayout.HelpBox("別のコンポーネントでポーズ固定中です。時間変更・停止は開始したコンポーネントを再選択してください。", MessageType.Info);
                if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Error);
            }
        }
    }
}
