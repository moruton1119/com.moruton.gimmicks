using UnityEngine;

namespace Moruton.Gimmicks
{
    /// <summary>
    /// セットアップ対象の汎用データ構造。
    /// GimmickSetupHelper / Item_Randomiser 等で共用。
    /// </summary>
    [System.Serializable]
    public class SetupTarget
    {
        [Tooltip("このターゲットに関する説明文")]
        [TextArea(2, 4)]
        public string description = "説明文を入力してください";

        [Tooltip("操作対象のオブジェクト")]
        public Transform targetObject;
    }

    /// <summary>
    /// アイテムのコピーデータ構造。
    /// Item_Randomiser / ItemSetupScript 等で共用。
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        [Tooltip("複製元のオブジェクト")]
        public GameObject sourceObject;

        [Tooltip("複製先の親オブジェクト")]
        public Transform targetParent;
    }
}
