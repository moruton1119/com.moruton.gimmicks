using UnityEngine;
using System.Collections.Generic;

namespace Moruton.Gimmicks
{
    [AddComponentMenu("Morulab/Avatars/Item Randomiser")]
    public class Item_Randomiser : MorutonAvatarPackage
    {
        [System.Serializable]
        public class SetupTarget
        {
            [Tooltip("このターゲットに関する説明文")]
            [TextArea(2, 4)]
            public string description = "説明文を入力してください";

            [Tooltip("操作対象のオブジェクト")]
            public Transform targetObject;
        }

        [System.Serializable]
        public class ItemData
        {
            public GameObject sourceObject;
            public Transform targetParent;
        }

        [Header("Setup Targets")]
        [Tooltip("セットアップ対象のリスト")]
        public List<SetupTarget> targets = new List<SetupTarget>();

        [Header("Items")]
        [SerializeField] private List<ItemData> items = new List<ItemData>();

        [ContextMenu("Copy All To Target")]
        public void CopyAllToTarget()
        {
            foreach (var item in items)
            {
                if (item.sourceObject == null || item.targetParent == null) continue;

                while (item.targetParent.childCount > 0)
                {
                    var child = item.targetParent.GetChild(0);
                    if (Application.isPlaying)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }

                var instance = Instantiate(item.sourceObject, item.targetParent);
                instance.name = item.sourceObject.name;
            }
        }
    }
}
