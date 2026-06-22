using UnityEditor;
using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// ItemData リストの汎用Inspector描画。
    /// ItemSetupScript / Item_Randomiser 等で共用。
    /// </summary>
    public static class ItemListDrawer
    {
        private const float PreviewSize = 64f;

        /// <summary>
        /// アイテムリストのサイズ調整 + 各アイテム描画。
        /// </summary>
        public static void DrawItemsList(SerializedProperty itemsProp)
        {
            if (itemsProp == null) return;

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

        /// <summary>
        /// 個別アイテムをプレビュー付きで描画。
        /// </summary>
        public static void DrawItem(SerializedProperty itemsProp, int index)
        {
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
                            GUILayout.Box(preview, GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
                        else
                            GUILayout.Box("No Preview", GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
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
    }
}
