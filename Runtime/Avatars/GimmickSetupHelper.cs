using UnityEngine;
using System.Collections.Generic;

namespace Moruton.Gimmicks
{
    /// <summary>
    /// セットアップ用のターゲット情報（説明と対象オブジェクト）を保持するコンポーネント。
    /// アバターのセットアップを補助するコンポーネント
    /// </summary>
    [AddComponentMenu("Morulab/Avatars/Gimmick Setup Helper")]
    public class GimmickSetupHelper : MorutonAvatarPackage
    {
        [Tooltip("Inspector表示用のイメージ（ダミー）")]
        public Sprite dummyImage;

        [System.Serializable]
        public class SetupTarget
        {
            [Tooltip("このターゲットに関する説明文")]
            [TextArea(2, 4)]
            public string description = "説明文を入力してください";

            [Tooltip("操作対象のオブジェクト")]
            public Transform targetObject;
        }

        [Header("Setup Targets")]
        [Tooltip("セットアップ対象のリスト")]
        public List<SetupTarget> targets = new List<SetupTarget>();
    }
}
