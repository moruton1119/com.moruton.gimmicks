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

            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gimmick Setup Helper (Avatar)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("設定されたターゲットを選択して調整できます。", MessageType.Info);
            EditorGUILayout.Space();

            var helper = (GimmickSetupHelper)target;

            if (targetsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("ターゲットが設定されていません。下の 'Developer Mode' から追加してください。", MessageType.Warning);
            }
            else
            {
                TargetListDrawer.DrawTargetsListFromSerialized(targetsProp, helper.targets);
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            TargetListDrawer.DrawDeveloperMode(targetsProp, ref showDevMode);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
