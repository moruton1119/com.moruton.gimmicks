using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 変身ギミック（Metamorphose）のセットアップウィンドウ。
    /// 機能別に partial class で分割:
    ///   - MetamorphoseWindow.cs             (コア: フィールド・ライフサイクル・UI構築)
    ///   - MetamorphoseWindow.Theme.cs       (テーマ制御・オープニング演出)
    ///   - MetamorphoseWindow.Preview.cs     (プレビュー描画)
    ///   - MetamorphoseWindow.Navigation.cs  (ページ遷移・部位トグル・プロパティ生成)
    ///   - MetamorphoseWindow.Localization.cs(多言語対応)
    ///   - MetamorphoseWindow.Banner.cs      (バナー広告取得)
    ///   - MetamorphoseWindow.Buttons.cs     (ボタンコールバック・アニメーション生成)
    /// </summary>
    public partial class MetamorphoseWindow : EditorWindow
    {
        internal const string UxmlPath =
            "Packages/com.moruton.gimmicks/Editor/Avatars/Metamorphose/MetamorphoseWindow.uxml";

        private const string BoothUrl = "https://moruton.booth.pm/";
        private const string DiscordUrl = "https://discord.gg/GHJwmyTcfX";
        private const string XUrl = "https://x.com/MoruLabo";
        private const string NoteUrl = "https://note.com/mortonlaboratory";

        // ═══════════════════════════════════════════════════════════
        //  バナー広告URL — 変更する場合はここを編集
        //  詳細: Documentation/BannerAds.md
        // ═══════════════════════════════════════════════════════════
        private static readonly string[] DefaultBannerUrls =
        {
            "https://moruton.booth.pm/items/7575133",
            "https://moruton.booth.pm/items/7698424",
            "https://moruton.booth.pm/items/7341440",
            "https://moruton.booth.pm/items/6173517",
        };
        // ═══════════════════════════════════════════════════════════

        private readonly string[] _languageCodes = { "ja", "en", "ko", "it", "es" };

        private Metamorphose _target;
        private SerializedObject _so;
        private VisualElement _root;
        private bool _uiBuilt;
        private int _selectedLanguage;
        private int _currentPage;
        private string _currentThemeId;
        private bool _hasPlayedOpening;

        private static readonly string[][] PageFieldPaths =
        {
            // Page 0 (メインページ)
            new[] { "offTargets", "gimmickColor", "avatar", "headItems", "bodyItems", "handItems", "legItems" },
            // Page 1 (コラボページ) — フェードアイテム
            new[] { "fadeHeadItems", "fadeBodyItems", "fadeArmItems", "fadeLegItems" },
        };

        private static readonly (string path, string previewName)[] PreviewBindings =
        {
            ("headItems", "head-preview"),
            ("bodyItems", "body-preview"),
            ("handItems", "hand-preview"),
            ("legItems", "leg-preview"),
        };

        private static readonly (string path, string slotName)[] DevFieldBindings =
        {
            ("dummyImage", "page3-slot-dummyImage"),
            ("colaboShopTex", "page3-slot-colaboShopTex"),
            ("colaboShopInfo", "page3-slot-colaboShopInfo"),
            ("model", "page3-slot-model"),
            ("animator", "page3-slot-animator"),
            ("headTarget", "page3-slot-devHeadTarget"),
            ("bodyTarget", "page3-slot-devBodyTarget"),
            ("handTarget", "page3-slot-devHandTarget"),
            ("legTarget", "page3-slot-devLegTarget"),
            ("onePiece", "page3-slot-devOnePiece"),
            ("colaboItemTarget", "page3-slot-devColaboItemTarget"),
            ("colaboItem", "page3-slot-devColaboItem"),
            ("colaboFBX", "page3-slot-devColaboFBX"),
            ("fadeHead", "page3-slot-devFadeHead"),
            ("fadeBody", "page3-slot-devFadeBody"),
            ("fadeArm", "page3-slot-devFadeArm"),
            ("fadeLeg", "page3-slot-devFadeLeg"),
            ("fadeHeadMaterial", "page3-slot-devFadeHeadMaterial"),
            ("fadeBodyMaterial", "page3-slot-devFadeBodyMaterial"),
            ("fadeArmMaterial", "page3-slot-devFadeArmMaterial"),
            ("fadeLegMaterial", "page3-slot-devFadeLegMaterial"),
            ("gimmickCollar", "page3-slot-gimmickCollar"),
            ("bannerAdUrls", "page3-slot-bannerAdUrls"),
            ("editorTheme", "page3-slot-editorTheme"),
            ("protectedAnimDll", "page3-slot-protectedAnimDll"),
            ("protectedAnimTargetController", "page3-slot-protectedAnimTargetController"),
            ("protectedAnimLayerName", "page3-slot-protectedAnimLayerName"),
            ("protectedAnimKeys", "page3-slot-protectedAnimKeys"),
        };

        private string L(string key) => LocalizationManager.Get("Metamorphose", key);
        private string LC(string key) => LocalizationManager.GetCommon(key);

        #region Public API

        public static void Show(Metamorphose target)
        {
            if (target == null) return;
            var window = GetWindow<MetamorphoseWindow>("Metamorphose Setup");
            window.minSize = new Vector2(520, 560);
            window.SetTarget(target);
        }

        public void SetTarget(Metamorphose target)
        {
            _target = target;
            _so = target != null ? new SerializedObject(target) : null;

            if (target != null)
            {
                target.AutoAssignAvatarAndAnimatorIfEmpty();
                _lastGimmickColor = target.gimmickColor;
                titleContent = new GUIContent($"Setup - {target.gameObject.name}");
            }
            else
            {
                titleContent = new GUIContent("Metamorphose Setup");
            }

            _uiBuilt = false;
            _currentPage = 0;
            ClearPreviewCache();
            RebuildUI();
            Repaint();
        }

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            _selectedLanguage = EditorPrefs.GetInt("MetamorphoseEditor_Language", 0);
            _currentThemeId = EditorPrefs.GetString("MetamorphoseEditor_ThemeId", "Auto");
            LocalizationManager.Load("Metamorphose", _languageCodes[_selectedLanguage]);
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            _openingEffect?.Cleanup();
            _openingEffect = null;
            ClearPreviewCache();
        }

        public void CreateGUI()
        {
            RebuildUI();
        }

        private void RebuildUI()
        {
            if (_target == null || _so == null)
            {
                rootVisualElement.Clear();
                var label = new Label("No target assigned.");
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                rootVisualElement.Add(label);
                _uiBuilt = false;
                return;
            }

            if (!_uiBuilt)
            {
                BuildUI();
                _uiBuilt = true;
            }
        }

        #endregion

        #region UI Build

        private void BuildUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (visualTree == null)
            {
                rootVisualElement.Clear();
                rootVisualElement.Add(new Label($"Error: Could not load {UxmlPath}"));
                return;
            }

            _root = visualTree.CloneTree();

            // ★ ルートにテーマクラスを直接付ける（CSS変数はカスケードで全子孫に伝わる）
            _root.AddToClassList("theme-moonlight");

            CreatePropertyFields();
            RegisterPreviewCallbacks();
            SetupNavigation();
            SetupPartToggles();
            SetupButtonCallbacks();
            ApplyLocalization(_root);

            // Apply theme from Prefab setting (or fallback to EditorPrefs)
            ResolveTheme();
            ApplyTheme();
            PlayOpeningAnimation();

            _root.Bind(_so);
            UpdateAllPreviews();

            if (HasAnyMissingPreview())
                _previewRefreshTime = Time.realtimeSinceStartup + 0.15f;

            UpdateShopVisibility();

            LoadBannerCards();

            // コラボタブの初期表示状態
            UpdateColaboTabVisibility(_target.showColaboTab);

            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            _root.style.flexGrow = 1;
            rootVisualElement.Add(_root);
        }

        private void UpdateShopVisibility()
        {
            var shopSection = _root.Q<VisualElement>("colabo-shop-section");
            if (shopSection != null)
            {
                bool hasShop = _target.colaboShopTex != null || !string.IsNullOrEmpty(_target.colaboShopInfo);
                shopSection.style.display = hasShop ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        #endregion

        #region Editor Update

        private void OnSelectionChange()
        {
            var activeGo = Selection.activeGameObject;
            if (activeGo != null)
            {
                var mirror = activeGo.GetComponent<Metamorphose>();
                if (mirror != null && mirror != _target)
                {
                    SetTarget(mirror);
                }
            }
        }

        private void OnEditorUpdate()
        {
            if (_target == null) return;

            if (_target.gimmickColor != _lastGimmickColor)
            {
                _lastGimmickColor = _target.gimmickColor;
                MetamorphoseSetupService.ApplyGimmickColor(_target);
            }

            if (_previewRefreshTime > 0f && Time.realtimeSinceStartup >= _previewRefreshTime)
            {
                _so.Update();
                UpdateAllPreviews();
                if (!HasAnyMissingPreview())
                    _previewRefreshTime = -1f;
                else
                    _previewRefreshTime = Time.realtimeSinceStartup + 0.2f;
            }
        }

        #endregion
    }
}
