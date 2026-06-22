using System.Collections.Generic;
using UnityEngine;

namespace Moruton.Gimmicks
{
    [AddComponentMenu("Morulab/Avatars/Item Setup Script")]
    public class ItemSetupScript : MonoBehaviour
    {
        [Header("Items")]
        [SerializeField] private List<ItemData> items = new List<ItemData>();

        [ContextMenu("Copy All To Target")]
        public void CopyAllToTarget()
        {
            ItemCopyUtility.CopyAllToTarget(items);
        }
    }
}
