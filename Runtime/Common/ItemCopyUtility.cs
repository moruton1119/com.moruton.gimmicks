using UnityEngine;

namespace Moruton.Gimmicks
{
    /// <summary>
    /// GameObject のコピー系ユーティリティ。
    /// Item_Randomiser / ItemSetupScript 等で共用。
    /// </summary>
    public static class ItemCopyUtility
    {
        /// <summary>
        /// sourceObject を targetParent の直下にインスタンス化する。
        /// 既存の子はすべて削除してからコピーする。
        /// </summary>
        public static void CopyAllToTarget(System.Collections.Generic.List<ItemData> items)
        {
            foreach (var item in items)
            {
                if (item.sourceObject == null || item.targetParent == null) continue;

                // 既存の子をすべて削除
                while (item.targetParent.childCount > 0)
                {
                    var child = item.targetParent.GetChild(0);
                    if (Application.isPlaying)
                        Object.Destroy(child.gameObject);
                    else
                        Object.DestroyImmediate(child.gameObject);
                }

                // 新規インスタンスを作成
                var instance = Object.Instantiate(item.sourceObject, item.targetParent);
                instance.name = item.sourceObject.name;
            }
        }
    }
}
