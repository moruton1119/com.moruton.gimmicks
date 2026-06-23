using UnityEditor;
using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// アイテムをTransformの子として配置する共通ユーティリティ。
    /// NDMFビルド時やエディタプレビュー時に使用。
    /// </summary>
    public static class ItemPlacer
    {
        /// <summary>
        /// アイテム配列をターゲットの子として配置する。
        /// </summary>
        /// <param name="target">配置先のTransform</param>
        /// <param name="items">配置するアイテム配列</param>
        public static void PlaceItems(Transform target, GameObject[] items)
        {
            if (target == null || items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;

                Place(target, item);
            }
        }

        /// <summary>
        /// アイテムをターゲットの子として配置し、マテリアルを適用する。
        /// </summary>
        /// <param name="target">配置先のTransform</param>
        /// <param name="items">配置するアイテム配列</param>
        /// <param name="material">適用するマテリアル（nullなら適用しない）</param>
        public static void PlaceItems(Transform target, GameObject[] items, Material material)
        {
            if (target == null || items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;

                var instance = Place(target, item);
                ApplyMaterial(instance, material);
                if (instance != null)
                    instance.name = target.name;
            }
        }

        /// <summary>
        /// アイテムをターゲットの子として移動（reparent）する。
        /// シーン内オブジェクトは移動、PrefabアセットはInstantiate。
        /// </summary>
        public static GameObject Place(Transform target, GameObject item)
        {
            if (target == null || item == null) return null;

            GameObject obj;

            if (PrefabUtility.IsPartOfPrefabAsset(item))
            {
                obj = Object.Instantiate(item, target);
                obj.name = item.name;
            }
            else
            {
                if (item.transform.parent == target)
                    return item;

                obj = item;
                obj.transform.SetParent(target, false);
            }

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            if (PrefabUtility.IsPartOfPrefabInstance(obj))
            {
                PrefabUtility.UnpackPrefabInstance(
                    obj,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);
            }

            return obj;
        }

        /// <summary>
        /// アイテムの子に含まれる全Rendererのマテリアルを置き換える。
        /// </summary>
        public static void ApplyMaterial(GameObject item, Material material)
        {
            if (item == null || material == null) return;

            var renderers = item.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                var mats = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = material;
                renderer.sharedMaterials = mats;
            }
        }
    }
}
