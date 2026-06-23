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

            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Item Setup Script", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            ItemListDrawer.DrawItemsList(itemsProp);

            EditorGUILayout.Space();

            if (GUILayout.Button("Copy All To Target"))
            {
                ((ItemSetupScript)target).CopyAllToTarget();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
