using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// SetupTarget リストの汎用Inspector描画。
    /// GimmickSetupHelper / Item_Randomiser 等で共用。
    /// </summary>
    public static class TargetListDrawer
    {
        /// <summary>
        /// ターゲットリストを表示する。
        /// </summary>
        public static void DrawTargetsList(List<SetupTarget> targets)
        {
            if (targets == null) return;

            for (int i = 0; i < targets.Count; i++)
            {
                DrawTargetItem(targets[i]);
                EditorGUILayout.Space(4);
            }
        }

        /// <summary>
        /// 個別のターゲットを表示する。
        /// </summary>
        public static void DrawTargetItem(SetupTarget target)
        {
            if (target == null) return;

            GUILayout.BeginVertical("box");
            {
                if (!string.IsNullOrEmpty(target.description))
                {
                    GUIStyle style = new GUIStyle(EditorStyles.label)
                    {
                        wordWrap = true,
                        fontSize = 12
                    };
                    EditorGUILayout.LabelField(target.description, style);
                }

                EditorGUILayout.Space(4);
                GUI.enabled = target.targetObject != null;
                string btnLabel = target.targetObject != null
                    ? $"Select: {target.targetObject.name}"
                    : "Target Not Assigned";
                if (GUILayout.Button(new GUIContent(btnLabel, "クリックしてこのオブジェクトを選択状態にします"), GUILayout.Height(30)))
                {
                    if (target.targetObject != null)
                    {
                        Selection.activeGameObject = target.targetObject.gameObject;
                        EditorGUIUtility.PingObject(target.targetObject.gameObject);
                        SceneView.FrameLastActiveSceneView();
                    }
                }
                GUI.enabled = true;
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// SerializedPropertyベースのターゲットリスト表示。
        /// </summary>
        public static void DrawTargetsListFromSerialized(SerializedProperty targetsProp, List<SetupTarget> targets)
        {
            if (targetsProp == null) return;

            for (int i = 0; i < targetsProp.arraySize; i++)
            {
                string descText = "";
                Transform targetTrans = null;

                if (targets != null && i < targets.Count)
                {
                    descText = targets[i].description;
                    targetTrans = targets[i].targetObject;
                }
                else
                {
                    SerializedProperty item = targetsProp.GetArrayElementAtIndex(i);
                    descText = item.FindPropertyRelative("description").stringValue;
                    targetTrans = item.FindPropertyRelative("targetObject").objectReferenceValue as Transform;
                }

                GUILayout.BeginVertical("box");
                {
                    if (!string.IsNullOrEmpty(descText))
                    {
                        GUIStyle style = new GUIStyle(EditorStyles.label)
                        {
                            wordWrap = true,
                            fontSize = 12
                        };
                        EditorGUILayout.LabelField(descText, style);
                    }

                    EditorGUILayout.Space(4);
                    GUI.enabled = targetTrans != null;
                    string btnLabel = targetTrans != null
                        ? $"Select: {targetTrans.name}"
                        : "Target Not Assigned";
                    if (GUILayout.Button(btnLabel, GUILayout.Height(30)))
                    {
                        if (targetTrans != null)
                        {
                            Selection.activeGameObject = targetTrans.gameObject;
                            EditorGUIUtility.PingObject(targetTrans.gameObject);
                            SceneView.FrameLastActiveSceneView();
                        }
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }
        }

        /// <summary>
        /// DeveloperMode: ターゲットの追加・削除・編集を行う。
        /// </summary>
        public static void DrawDeveloperMode(SerializedProperty targetsProp, ref bool showDevMode)
        {
            showDevMode = EditorGUILayout.Foldout(showDevMode, "Developer Mode (Edit Settings)");
            if (!showDevMode) return;

            EditorGUILayout.HelpBox("ここでリストの追加・削除や、説明文・ターゲットの編集が行えます。", MessageType.None);

            for (int i = 0; i < targetsProp.arraySize; i++)
            {
                SerializedProperty item = targetsProp.GetArrayElementAtIndex(i);
                SerializedProperty description = item.FindPropertyRelative("description");
                SerializedProperty targetObject = item.FindPropertyRelative("targetObject");

                GUILayout.BeginVertical("helpbox");
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Item {i}", EditorStyles.miniLabel);
                    if (GUILayout.Button("Remove", GUILayout.Width(60), GUILayout.Height(16)))
                    {
                        targetsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    GUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(description, new GUIContent("Description"));
                    EditorGUILayout.PropertyField(targetObject, new GUIContent("Target"));
                }
                GUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            if (GUILayout.Button("+ Add New Target"))
            {
                targetsProp.InsertArrayElementAtIndex(targetsProp.arraySize);
                var newItem = targetsProp.GetArrayElementAtIndex(targetsProp.arraySize - 1);
                newItem.FindPropertyRelative("description").stringValue = "新しいターゲットの説明";
                newItem.FindPropertyRelative("targetObject").objectReferenceValue = null;
            }
        }
    }
}
