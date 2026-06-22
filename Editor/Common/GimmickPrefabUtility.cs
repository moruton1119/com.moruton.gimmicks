using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        /// Prefabを指定親の下にインスタンス化する。
        /// </summary>
        public static GameObject InstantiateUnder(GameObject source, Transform parent, string overrideName = null)
        {
            if (source == null || parent == null) return null;

            var instance = Instantiate(source, parent);
            instance.name = overrideName ?? source.name;
            return instance;
        }

        /// <summary>
        /// 親オブジェクトの子を別オブジェクトで置き換える。
        /// 既存の子を削除して新しいものを追加する。
        /// </summary>
        public static void ReplaceChild(Transform parent, GameObject replacement, string name = null)
        {
            if (parent == null || replacement == null) return;

            while (parent.childCount > 0)
            {
                var child = parent.GetChild(0);
                DestroyImmediate(child.gameObject);
            }

            var instance = Instantiate(replacement, parent);
            instance.name = name ?? replacement.name;
        }

        /// <summary>
        /// 複数アイテムをそれぞれのターゲット親にコピーする。
        /// </summary>
        public static void CopyItems(List<ItemData> items)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                if (item.sourceObject == null || item.targetParent == null) continue;

                while (item.targetParent.childCount > 0)
                {
                    var child = item.targetParent.GetChild(0);
                    DestroyImmediate(child.gameObject);
                }

                var instance = Instantiate(item.sourceObject, item.targetParent);
                instance.name = item.sourceObject.name;
            }
        }
    }
}
