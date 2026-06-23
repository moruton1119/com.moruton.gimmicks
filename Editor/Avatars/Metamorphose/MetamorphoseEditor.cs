using UnityEditor;
using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    [CustomEditor(typeof(PrettyCureMirror))]
    public class MetamorphoseEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            if (target != null)
            {
                var script = (PrettyCureMirror)target;
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
                MetamorphoseWindow.Show((PrettyCureMirror)target);
            }

            EditorGUILayout.Space(4);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
