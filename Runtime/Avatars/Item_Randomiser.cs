using UnityEngine;
using System.Collections.Generic;

namespace Moruton.Gimmicks
{
    [AddComponentMenu("Morulab/Avatars/Item Randomiser")]
    public class Item_Randomiser : MorutonAvatarPackage
    {
        [Header("Setup Targets")]
        [Tooltip("セットアップ対象のリスト")]
        public List<SetupTarget> targets = new List<SetupTarget>();

        [Header("Items")]
        [SerializeField] private List<ItemData> items = new List<ItemData>();

        [ContextMenu("Copy All To Target")]
        public void CopyAllToTarget()
        {
            ItemCopyUtility.CopyAllToTarget(items);
        }
    }
}
