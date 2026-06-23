using UnityEngine;
using System.Collections.Generic;

namespace Moruton.Gimmicks
{
    /// <summary>
    /// セットアップ用のターゲット情報を保持するコンポーネント。
    /// アバターのセットアップを補助する。
    /// </summary>
    [AddComponentMenu("Morulab/Avatars/Gimmick Setup Helper")]
    public class GimmickSetupHelper : MorutonAvatarPackage
    {
        [Tooltip("Inspector表示用のイメージ（ダミー）")]
        public Sprite dummyImage;

        [Header("Setup Targets")]
        [Tooltip("セットアップ対象のリスト")]
        public List<SetupTarget> targets = new List<SetupTarget>();
    }
}
