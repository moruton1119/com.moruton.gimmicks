using System;
using System.Collections.Generic;
using UnityEngine;

#if MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace Moruton.Gimmicks
{
#if MODULAR_AVATAR
    using AvatarTagBase = nadena.dev.modular_avatar.core.AvatarTagComponent;
#else
    using AvatarTagBase = UnityEngine.MonoBehaviour;
#endif

    [AddComponentMenu("Morulab/Avatars/Metamorphose")]
    [ExecuteInEditMode]
    public class Metamorphose : AvatarTagBase
    {
        [Header("基本設定")]
        [SerializeField] private Texture2D dummyImage;
        [SerializeField] private GameObject avatar;
        [SerializeField] private GameObject model;
        [SerializeField] private GameObject[] offTargets;
        [SerializeField] private Animator animator;
        
        [Header("変身後の衣装 - 頭部")]
        [SerializeField] public Transform headTarget;
        [SerializeField] public GameObject[] headItems;
        
        [Header("変身後の衣装 - 胴体")]
        [SerializeField] public Transform bodyTarget;
        [SerializeField] public GameObject[] bodyItems;
        
        [Header("変身後の衣装 - 手部")]
        [SerializeField] public Transform handTarget;
        [SerializeField] public GameObject[] handItems;
        
        [Header("変身後の衣装 - 脚部")]
        [SerializeField] public Transform legTarget;
        [SerializeField] public GameObject[] legItems;

        [Header("特殊設定 - ワンピース差し替え")]
        [SerializeField] private GameObject onePiece;
        [SerializeField] private GameObject colaboFBX;
        
        [Header("特殊設定 - 追加アイテム")]
        [SerializeField] public Transform colaboItemTarget;
        [SerializeField] public GameObject colaboItem;
        
        [Header("コラボ情報")]
        [SerializeField] public Texture2D colaboShopTex;
        [SerializeField] public string colaboShopInfo;
        
        [Header("フェード演出 - 頭部")]
        [SerializeField] public Transform fadeHead;
        [SerializeField] public GameObject[] fadeHeadItems;
        [SerializeField] public Material fadeHeadMaterial;
        
        [Header("フェード演出 - 胴体")]
        [SerializeField] public Transform fadeBody;
        [SerializeField] public GameObject[] fadeBodyItems;
        [SerializeField] public Material fadeBodyMaterial;
        
        [Header("フェード演出 - 腕部")]
        [SerializeField] public Transform fadeArm;
        [SerializeField] public GameObject[] fadeArmItems;
        [SerializeField] public Material fadeArmMaterial;
        
        [Header("フェード演出 - 脚部")]
        [SerializeField] public Transform fadeLeg;
        [SerializeField] public GameObject[] fadeLegItems;
        [SerializeField] public Material fadeLegMaterial;
        
        [Header("ギミック色設定")]
        public Color gimmickColor = Color.white;
        [SerializeField] private GameObject[] gimmickCollar;

        [Header("バナー広告URL")]
        [SerializeField] public string[] bannerAdUrls;

        [Header("エディターUIテーマ")]
        [Tooltip("エディターのUIテーマID。Devページで変更可能。")]
        [SerializeField] public string editorTheme = "Moonlight";

        /// <summary>現在のエディターテーマ設定（文字列ID）</summary>
        public string ThemeSetting => editorTheme;

        /// <summary>エディターテーマを設定</summary>
        public void SetEditorTheme(string themeId)
        {
            editorTheme = themeId;
        }

        [SerializeField] public bool showHead = true;
        [SerializeField] public bool showBody = true;
        [SerializeField] public bool showHand = true;
        [SerializeField] public bool showLeg = true;
        [SerializeField] public bool showFadeHead = true;
        [SerializeField] public bool showFadeBody = true;
        [SerializeField] public bool showFadeArm = true;
        [SerializeField] public bool showFadeLeg = true;
        
        [Header("UI表示設定")]
        [Tooltip("コラボタブを表示するかどうか")]
        [SerializeField] public bool showColaboTab = true;

        // ドロップダウン用の選択肢
        public static readonly string[] AvailableThemes = { "Moonlight", "Daylight", "Cyber", "Wizard", "Diamond" };
        
        public GameObject Avatar => avatar;
        public GameObject Model => model;
        public Animator Animator => animator;
        public GameObject[] OffTargets => offTargets;
        public GameObject OnePiece => onePiece;
        public GameObject ColaboFBX => colaboFBX;
        public GameObject[] GimmickCollar => gimmickCollar;

        /// <summary>
        /// コンポーネント追加時・Reset時に呼ばれる。
        /// アバターとAnimatorを自動アサイン。
        /// </summary>
        private void Reset()
        {
            AutoAssignAvatarAndAnimatorIfEmpty();
        }

        /// <summary>
        /// 親階層を辿ってVRCアバターディスクリプターを探し、
        /// avatar / animator が未設定なら自動でアサインする。
        /// </summary>
        public void AutoAssignAvatarAndAnimatorIfEmpty()
        {
            // avatar が未設定なら親階層を探索
            if (avatar == null)
            {
                var desc = GetComponentInParent<VRC.SDKBase.VRC_AvatarDescriptor>();
                if (desc != null)
                    avatar = desc.gameObject;
            }

            // animator が未設定なら avatar から取得
            if (animator == null && avatar != null)
            {
                animator = avatar.GetComponent<Animator>();
            }
        }
    }
}
