using UnityEngine;
using UnityEditor;
using Moruton.Gimmicks;

namespace Moruton.Gimmicks.Editor
{
    [CustomEditor(typeof(Item_Randomiser))]
    public class Item_RandomiserEditor : UnityEditor.Editor
    {
        private SerializedProperty targetsProp;
        private SerializedProperty itemsProp;
        private bool showDevMode = false;
        private int selectedTab = 0;
        private readonly string[] tabNames = { "アイテムの位置調整", "アイテム自体の入れ替え設定" };

        private void OnEnable()
        {
            targetsProp = serializedObject.FindProperty("targets");
            itemsProp = serializedObject.FindProperty("items");
        }

        public override void OnInspectorGUI()
        {
            MorutonAvatarPackageEditorHelper.DrawHeader();

            serializedObject.Update();

            EditorGUILayout.Space(10);

            DrawTabs();

            EditorGUILayout.Space(10);

            switch (selectedTab)
            {
                case 0:
                    DrawGimmickSetupContent();
                    break;
                case 1:
                    DrawItemSetupContent();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();
            {
                for (int i = 0; i < tabNames.Length; i++)
                {
                    bool isSelected = (selectedTab == i);

                    GUIStyle tabStyle = new GUIStyle(EditorStyles.toolbarButton);
                    tabStyle.fixedHeight = 25;

                    if (isSelected)
                    {
                        Color selectedColor = new Color(0.5f, 0.7f, 1f, 0.3f);
                        tabStyle.normal.background = MakeTex(2, 2, selectedColor);
                    }

                    if (GUILayout.Button(tabNames[i], tabStyle))
                    {
                        selectedTab = i;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void DrawGimmickSetupContent()
        {
            EditorGUILayout.LabelField("各アイテムの位置などを調整できます", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("設定されたターゲットを選択して調整できます。", MessageType.Info);
            EditorGUILayout.Space();

            if (targetsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("ターゲットが設定されていません。下の 'Developer Mode' から追加してください。", MessageType.Warning);
            }
            else
            {
                DrawTargetsList();
            }

            EditorGUILayout.Space(10);
            GimmickSetupHelperEditor.DrawDeveloperMode(targetsProp, ref showDevMode);
        }

        private void DrawTargetsList()
        {
            for (int i = 0; i < targetsProp.arraySize; i++)
            {
                DrawTargetItem(i);
                EditorGUILayout.Space(4);
            }
        }

        private void DrawTargetItem(int index)
        {
            SerializedProperty item = targetsProp.GetArrayElementAtIndex(index);
            string descText = item.FindPropertyRelative("description").stringValue;
            Transform targetTrans = item.FindPropertyRelative("targetObject").objectReferenceValue as Transform;

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

        private void DrawItemSetupContent()
        {
            EditorGUILayout.LabelField("好きなアイテムを登録できます", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("設定したいアイテムを登録してください", MessageType.Info);
            EditorGUILayout.Space();

            ItemSetupScriptEditor.DrawItemsList(itemsProp);

            EditorGUILayout.Space();

            if (GUILayout.Button("Copy All To Target", GUILayout.Height(30)))
            {
                ((Item_Randomiser)target).CopyAllToTarget();
            }
        }
    }
}
