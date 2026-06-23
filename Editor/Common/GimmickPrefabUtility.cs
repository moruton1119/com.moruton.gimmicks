using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// Prefab操作の汎用ユーティリティ。
    /// 変身ギミックやアイテムセットアップ等で共用。
    /// </summary>
    public static class GimmickPrefabUtility
    {
        /// <summary>
        /// Prefabを展開（Unpack）する。
        /// </summary>
        public static void UnpackPrefab(GameObject prefab, bool completely = true)
        {
            if (prefab == null) return;

            if (PrefabUtility.IsPartOfPrefabInstance(prefab))
            {
                if (completely)
                    PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                else
                    PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            }
        }

        /// <summary>
        /// 親の子のうち、指定名と一致しないものを削除し、
        /// 一致するものが無ければ新規にインスタンス化して配置する（元の ReplaceOnePieceChild と同じロジック）。
        /// Prefabの場合は接続を維持。Undo対応。
        /// </summary>
        public static void ReplaceChild(Transform parent, GameObject newItem)
        {
            if (parent == null || newItem == null) return;

            // 既存の子を削除（名前が一致しないもの）
            var toDelete = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in parent)
            {
                if (child.name != newItem.name)
                    toDelete.Add(child);
            }
            foreach (var child in toDelete)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }

            // 新しいアイテムを追加
            bool exists = false;
            foreach (Transform child in parent)
            {
                if (child.name == newItem.name)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                GameObject instance;
                if (PrefabUtility.IsPartOfPrefabAsset(newItem))
                {
                    instance = (GameObject)PrefabUtility.InstantiatePrefab(newItem, parent);
                    Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
                }
                else
                {
                    instance = newItem;
                    Undo.SetTransformParent(instance.transform, parent, "Move Item");
                }

                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.name = newItem.name;
            }
        }
    }
}
