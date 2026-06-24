using UnityEditor;
using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    [CustomEditor(typeof(Metamorphose))]
    public class MetamorphoseEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            if (target != null)
            {
                var script = (Metamorphose)target;
                script.AutoAssignAvatarAndAnimatorIfEmpty();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MorutonAvatarPackageEditorHelper.DrawHeader();

            EditorGUILayout.Space(6);

            if (GUILayout.Button("Open Metamorphose Setup", GUILayout.Height(38)))
            {
                MetamorphoseWindow.Show((Metamorphose)target);
            }

            EditorGUILayout.Space(4);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
