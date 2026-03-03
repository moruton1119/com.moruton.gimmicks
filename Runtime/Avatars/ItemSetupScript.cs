using System.Collections.Generic;
using UnityEngine;

namespace Moruton.Gimmicks
{
    [AddComponentMenu("Morulab/Avatars/Item Setup Script")]
    public class ItemSetupScript : MonoBehaviour
    {
        [System.Serializable]
        public class ItemData
        {
            public GameObject sourceObject;
            public Transform targetParent;
        }
        
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
