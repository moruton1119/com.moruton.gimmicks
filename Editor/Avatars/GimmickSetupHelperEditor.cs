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
            // アバター用ヘッダーを呼び出す
            MorutonAvatarPackageEditorHelper.DrawHeader();

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
                GimmickSetupHelper helper = (GimmickSetupHelper)target;
                for (int i = 0; i < targetsProp.arraySize; i++)
                {
                    string descText = "";
                    Transform targetTrans = null;

                    if (helper != null && helper.targets != null && i < helper.targets.Count)
                    {
                        descText = helper.targets[i].description;
                        targetTrans = helper.targets[i].targetObject;
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
                    EditorGUILayout.Space(4);
                }
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
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

            serializedObject.ApplyModifiedProperties();
        }
    }
}
