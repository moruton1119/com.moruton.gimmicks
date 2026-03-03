using UnityEngine;
using UnityEditor;
using Moruton.Gimmicks;

namespace Moruton.Gimmicks.Editor
{
    [CustomEditor(typeof(GimmickSetupHelper))]
    public class GimmickSetupHelperEditor : UnityEditor.Editor
    {
        private SerializedProperty targetsProp;
        private bool showDevMode = false;

        private void OnEnable()
        {
            targetsProp = serializedObject.FindProperty("targets");
        }

        public override void OnInspectorGUI()
        {
            MorutonAvatarPackageEditorHelper.DrawHeader();
            DrawGimmickSetupHelperInspector(serializedObject, targetsProp, ref showDevMode, (GimmickSetupHelper)target);
        }

        public static void DrawGimmickSetupHelperInspector(SerializedObject serializedObject, SerializedProperty targetsProp, ref bool showDevMode, GimmickSetupHelper helper)
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gimmick Setup Helper (Avatar)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("設定されたターゲットを選択して調整できます。", MessageType.Info);
            EditorGUILayout.Space();

            if (targetsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("ターゲットが設定されていません。下の 'Developer Mode' から追加してください。", MessageType.Warning);
            }
            else
            {
                DrawTargetsList(targetsProp, helper);
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DrawDeveloperMode(targetsProp, ref showDevMode);

            serializedObject.ApplyModifiedProperties();
        }

        public static void DrawTargetsList(SerializedProperty targetsProp, GimmickSetupHelper helper)
        {
            for (int i = 0; i < targetsProp.arraySize; i++)
            {
                DrawTargetItem(targetsProp, i, helper);
                EditorGUILayout.Space(4);
            }
        }

        public static void DrawTargetItem(SerializedProperty targetsProp, int index, GimmickSetupHelper helper)
        {
            string descText = "";
            Transform targetTrans = null;

            if (helper != null && helper.targets != null && index < helper.targets.Count)
            {
                descText = helper.targets[index].description;
                targetTrans = helper.targets[index].targetObject;
            }
            else
            {
                SerializedProperty item = targetsProp.GetArrayElementAtIndex(index);
                descText = item.FindPropertyRelative("description").stringValue;
                targetTrans = item.FindPropertyRelative("targetObject").objectReferenceValue as Transform;
            }

            GUILayout.BeginVertical("box");
            {
                if (!string.IsNullOrEmpty(descText))
                {
                    GUIStyle style = new GUIStyle(EditorStyles.label);
                    style.wordWrap = true;
                    style.fontSize = 12;
                    EditorGUILayout.LabelField(descText, style);
                }

                EditorGUILayout.Space(4);
                GUI.enabled = targetTrans != null;
                string btnLabel = targetTrans != null ? $"Select: {targetTrans.name}" : "Target Not Assigned";
                if (GUILayout.Button(new GUIContent(btnLabel, "クリックしてこのオブジェクトを選択状態にします"), GUILayout.Height(30)))
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
        }

        public static void DrawDeveloperMode(SerializedProperty targetsProp, ref bool showDevMode)
        {
            showDevMode = EditorGUILayout.Foldout(showDevMode, "Developer Mode (Edit Settings)");
            if (showDevMode)
            {
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
}
