using System;
using UnityEditor;
using UnityEngine;
using Moruton.Gimmicks.Editor;

namespace Morution.DevTools
{
    [CustomEditor(typeof(DevPosePlacement))]
    public sealed class DevPosePlacementEditor : Editor
    {
        string error;
        public override void OnInspectorGUI()
        {
            MorutonAvatarPackageEditorHelper.DrawHeader();

            var settings = (DevPosePlacement)target;
            var session = DevPoseSession.Active;
            using (new EditorGUI.DisabledScope(session != null || EditorApplication.isPlayingOrWillChangePlaymode))
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("clip"));
                serializedObject.ApplyModifiedProperties();
            }
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("親の Animator", DevPoseSession.FindAnimator(settings), typeof(Animator), true);
            EditorGUILayout.HelpBox("実アバターのポーズを固定します。オバケを選択し、通常のUnityツールで配置を編集できます。停止するには開始したコンポーネントを再選択してください。停止してもオバケの編集は残ります。", MessageType.Info);
            if (session != null)
            {
                if (session.Owner == settings)
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
                    EditorGUILayout.HelpBox("別のコンポーネントでポーズ固定中です。時間変更・停止は開始したコンポーネントを再選択してください。", MessageType.Info);
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
            if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Error);
        }
    }
}
