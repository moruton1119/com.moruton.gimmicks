using UnityEngine;
using UnityEditor;
using Moruton.Gimmicks;

namespace Moruton.Gimmicks.Editor
{
    [CustomEditor(typeof(ItemSetupScript))]
    public class ItemSetupScriptEditor : UnityEditor.Editor
    {
        private SerializedProperty itemsProp;
        
        private void OnEnable()
        {
            itemsProp = serializedObject.FindProperty("items");
        }
        
        public override void OnInspectorGUI()
        {
            MorutonAvatarPackageEditorHelper.DrawHeader();
            DrawItemSetupInspector(serializedObject, itemsProp, (ItemSetupScript)target);
        }
        
        public static void DrawItemSetupInspector(SerializedObject serializedObject, SerializedProperty itemsProp, ItemSetupScript script)
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Item Setup Script", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawItemsList(itemsProp);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Copy All To Target"))
            {
                script.CopyAllToTarget();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        public static void DrawItemsList(SerializedProperty itemsProp)
        {
            int arraySize = Mathf.Max(0, EditorGUILayout.IntField("Size", itemsProp.arraySize));
            while (itemsProp.arraySize < arraySize)
                itemsProp.InsertArrayElementAtIndex(itemsProp.arraySize);
            while (itemsProp.arraySize > arraySize)
                itemsProp.DeleteArrayElementAtIndex(itemsProp.arraySize - 1);
            
            EditorGUILayout.Space();
            
            for (int i = 0; i < itemsProp.arraySize; i++)
            {
                DrawItem(itemsProp, i);
                EditorGUILayout.Space();
            }
        }
        
        public static void DrawItem(SerializedProperty itemsProp, int index)
        {
            const float PreviewSize = 64f;
            
            var itemProp = itemsProp.GetArrayElementAtIndex(index);
            var sourceProp = itemProp.FindPropertyRelative("sourceObject");
            var targetProp = itemProp.FindPropertyRelative("targetParent");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField($"Item {index}", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                {
                    if (sourceProp.objectReferenceValue != null)
                    {
                        Texture2D preview = AssetPreview.GetAssetPreview(sourceProp.objectReferenceValue);
                        if (preview != null)
                        {
                            GUILayout.Box(preview, GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
                        }
                        else
                        {
                            GUILayout.Box("No Preview", GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
                        }
                    }
                    else
                    {
                        GUILayout.Box("None", GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
                    }
                    
                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.PropertyField(sourceProp, new GUIContent("Source Object"));
                        EditorGUILayout.PropertyField(targetProp, new GUIContent("Target Parent"));
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        
        public static void DrawCopyButton(ItemSetupScript script)
        {
            if (GUILayout.Button("Copy All To Target"))
            {
                script.CopyAllToTarget();
            }
        }
    }
}
