using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    public class MetamorphoseWindow : EditorWindow
    {
        internal const string UxmlPath =
            "Packages/com.moruton.gimmicks/Editor/Avatars/Metamorphose/MetamorphoseWindow.uxml";

        private const string BoothUrl = "https://moruton.booth.pm/";
        private const string DiscordUrl = "https://discord.gg/GHJwmyTcfX";

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

        private const int PreviewTexSize = 128;
        private const int TotalPages = 4;

        private readonly string[] _languageCodes = { "ja", "en", "ko", "it", "es" };

        private Metamorphose _target;
        private SerializedObject _so;
        private VisualElement _root;
        private bool _uiBuilt;
        private int _selectedLanguage;
        private int _currentPage;
        private bool _isLightTheme;
        private bool _hasPlayedOpening;
        private Color _lastGimmickColor;
        private float _previewRefreshTime = -1f;
        private readonly Dictionary<Object, Texture2D> _previewTexCache = new();
        private readonly HashSet<Texture2D> _ownedPreviewTextures = new();

        private readonly List<BannerCardState> _bannerCardStates = new();
        private bool _bannerLoading;

        private struct BannerCardState
        {
            public string url;
            public string title;
            public Texture2D image;
            public bool loaded;
            public bool failed;
        }

        private static readonly string[][] PageFieldPaths =
        {
            new[] { "avatar", "offTargets", "gimmickColor" },
            new[] { "headItems", "bodyItems", "handItems", "legItems" },
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
        };

        private string L(string key) => LocalizationManager.Get("Metamorphose", key);
        private string LC(string key) => LocalizationManager.GetCommon(key);

        #region Public

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
            _isLightTheme = EditorPrefs.GetBool("MetamorphoseEditor_LightTheme", false);
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

        #region Theme

        /// <summary>
        /// PrefabのThemeSettingに従って実際のテーマを決定する。
        /// Autoの場合はEditorPrefs（直前の手動切替）を使用。
        /// </summary>
        private void ResolveTheme()
        {
            if (_target == null) return;

            switch (_target.ThemeSetting)
            {
                case Metamorphose.EditorThemeMode.Moonlight:
                    _isLightTheme = false;
                    break;
                case Metamorphose.EditorThemeMode.Daylight:
                    _isLightTheme = true;
                    break;
                default: // Auto
                    _isLightTheme = EditorPrefs.GetBool("MetamorphoseEditor_LightTheme", false);
                    break;
            }
        }

        private void ApplyTheme()
        {
            if (_root == null) return;

            // _root と rootVisualElement 両方にクラスを付与
            _root.EnableInClassList("light-theme", _isLightTheme);
            rootVisualElement.EnableInClassList("light-theme", _isLightTheme);

            // 背景色と文字色をインラインで強制上書き（USS変数が反映されない環境用）
            if (_isLightTheme)
            {
                _root.style.backgroundColor = new Color(1f, 0.94f, 0.96f, 1f); // #fff0f5
                _root.style.color = new Color(0.545f, 0.412f, 0.078f, 1f);    // #8b6914
            }
            else
            {
                _root.style.backgroundColor = new Color(0.102f, 0.055f, 0.180f, 1f); // #1a0e2e
                _root.style.color = new Color(0.941f, 0.902f, 1f, 1f);              // #f0e6ff
            }

            // 各セクションの背景も更新
            UpdateThemeColors();

            var toggle = _root.Q<Button>("theme-toggle");
            if (toggle != null)
            {
                toggle.text = _isLightTheme ? "☀️" : "🌙";
            }
        }

        private void UpdateThemeColors()
        {
            // Topbar
            var topbar = _root.Q<VisualElement>("topbar");
            if (topbar != null)
                topbar.style.backgroundColor = _isLightTheme
                    ? new Color(1f, 1f, 1f, 1f)
                    : new Color(0.141f, 0.086f, 0.220f, 1f);

            // Sidebar
            var sidebar = _root.Q<VisualElement>("sidebar");
            if (sidebar != null)
                sidebar.style.backgroundColor = _isLightTheme
                    ? new Color(0.988f, 0.894f, 0.925f, 1f)
                    : new Color(0.075f, 0.035f, 0.122f, 1f);

            // Content panel
            var content = _root.Q<VisualElement>("content-panel");
            if (content != null)
                content.style.backgroundColor = _isLightTheme
                    ? new Color(1f, 0.94f, 0.96f, 1f)
                    : new Color(0.102f, 0.055f, 0.180f, 1f);

            // Banner
            var banner = _root.Q<VisualElement>("banner");
            if (banner != null)
                banner.style.backgroundColor = _isLightTheme
                    ? new Color(0.973f, 0.843f, 0.910f, 1f)
                    : new Color(0.059f, 0.027f, 0.098f, 1f);
        }

        private void ToggleTheme()
        {
            _isLightTheme = !_isLightTheme;
            EditorPrefs.SetBool("MetamorphoseEditor_LightTheme", _isLightTheme);

            // Prefabの設定も更新（Auto → 明示的設定に切り替え）
            if (_target != null)
            {
                Undo.RecordObject(_target, "Change Editor Theme");
                _so.Update();
                var prop = _so.FindProperty("editorTheme");
                if (prop != null)
                {
                    prop.enumValueIndex = _isLightTheme
                        ? (int)Metamorphose.EditorThemeMode.Daylight
                        : (int)Metamorphose.EditorThemeMode.Moonlight;
                    _so.ApplyModifiedProperties();
                }
            }

            ApplyTheme();
        }

        #endregion

        #region Opening Animation

        private MagicalOpeningEffect _openingEffect;

        /// <summary>
        /// ウィンドウを開いた時のオープニング演出。
        /// MagicalOpeningEffect でグラデーション背景 + 粒子 + グローを描画。
        /// Moonlight: 紫の闇からピンクの光が広がる
        /// Daylight: 白い光から金のキラキラが降る
        /// </summary>
        private void PlayOpeningAnimation()
        {
            if (_root == null || _hasPlayedOpening) return;
            _hasPlayedOpening = true;

            // MagicalOpeningEffect を生成してルートに被せる
            _openingEffect = new MagicalOpeningEffect(_isLightTheme, onComplete: () =>
            {
                _openingEffect = null;
            });

            // タイトルラベルを追加（テキストは VisualElement の子として配置）
            _openingEffect.AddTitleLabel();

            _root.Add(_openingEffect);

            // 再生開始
            EditorApplication.delayCall += () =>
            {
                _openingEffect?.Play();
            };
        }

        #endregion

        #region Navigation

        private void SetupNavigation()
        {
            for (int i = 0; i < TotalPages; i++)
            {
                int pageIdx = i;
                var btn = _root.Q<Button>($"nav-{i}");
                if (btn != null)
                    btn.clicked += () => SwitchPage(pageIdx);
            }
        }

        private void SwitchPage(int pageIndex)
        {
            if (_currentPage == pageIndex) return;

            var oldBtn = _root.Q<Button>($"nav-{_currentPage}");
            if (oldBtn != null) oldBtn.RemoveFromClassList("active");

            var newBtn = _root.Q<Button>($"nav-{pageIndex}");
            if (newBtn != null) newBtn.AddToClassList("active");

            var oldPage = _root.Q<VisualElement>($"page-{_currentPage}");
            if (oldPage != null) oldPage.style.display = DisplayStyle.None;

            var newPage = _root.Q<VisualElement>($"page-{pageIndex}");
            if (newPage != null) newPage.style.display = DisplayStyle.Flex;

            _currentPage = pageIndex;
        }

        #endregion

        #region Part Toggles

        private void SetupPartToggles()
        {
            SetupPartToggle("toggle-head", "section-head", "Head", () => _target.showHead, "showHead");
            SetupPartToggle("toggle-body", "section-body", "Body", () => _target.showBody, "showBody");
            SetupPartToggle("toggle-hand", "section-hand", "Hand", () => _target.showHand, "showHand");
            SetupPartToggle("toggle-leg", "section-leg", "Leg", () => _target.showLeg, "showLeg");
            SetupPartToggle("toggle-fadeHead", "section-fadeHead", "F-Head", () => _target.showFadeHead, "showFadeHead");
            SetupPartToggle("toggle-fadeBody", "section-fadeBody", "F-Body", () => _target.showFadeBody, "showFadeBody");
            SetupPartToggle("toggle-fadeArm", "section-fadeArm", "F-Arm", () => _target.showFadeArm, "showFadeArm");
            SetupPartToggle("toggle-fadeLeg", "section-fadeLeg", "F-Leg", () => _target.showFadeLeg, "showFadeLeg");
        }

        private void SetupPartToggle(string toggleName, string sectionName, string displayName, System.Func<bool> getter, string propertyPath)
        {
            var toggle = _root.Q<Button>(toggleName);
            var section = _root.Q<VisualElement>(sectionName);
            if (toggle == null || section == null) return;

            bool isVisible = getter();
            ApplyPartToggleVisual(toggle, section, displayName, isVisible);

            toggle.clicked += () =>
            {
                isVisible = !isVisible;

                _so.Update();
                var prop = _so.FindProperty(propertyPath);
                if (prop != null)
                {
                    prop.boolValue = isVisible;
                    _so.ApplyModifiedProperties();
                }
                else
                {
                    Undo.RecordObject(_target, "Toggle Part Visibility");
                    switch (propertyPath)
                    {
                        case "showHead": _target.showHead = isVisible; break;
                        case "showBody": _target.showBody = isVisible; break;
                        case "showHand": _target.showHand = isVisible; break;
                        case "showLeg": _target.showLeg = isVisible; break;
                        case "showFadeHead": _target.showFadeHead = isVisible; break;
                        case "showFadeBody": _target.showFadeBody = isVisible; break;
                        case "showFadeArm": _target.showFadeArm = isVisible; break;
                        case "showFadeLeg": _target.showFadeLeg = isVisible; break;
                    }
                    EditorUtility.SetDirty(_target);
                }

                ApplyPartToggleVisual(toggle, section, displayName, isVisible);
            };
        }

        private void ApplyPartToggleVisual(Button toggle, VisualElement section, string displayName, bool isVisible)
        {
            toggle.text = $"{displayName}: {(isVisible ? "ON" : "OFF")}";
            toggle.EnableInClassList("part-toggle-off", !isVisible);
            section.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        #endregion

        #region PropertyField Generation

        private void CreatePropertyFields()
        {
            for (int pageIdx = 0; pageIdx < PageFieldPaths.Length; pageIdx++)
            {
                foreach (var path in PageFieldPaths[pageIdx])
                {
                    var slot = _root.Q<VisualElement>($"page{pageIdx}-slot-{path}");
                    if (slot == null) continue;
                    slot.Add(new PropertyField { bindingPath = path });
                }
            }

            foreach (var (path, slotName) in DevFieldBindings)
            {
                var slot = _root.Q<VisualElement>(slotName);
                if (slot == null) continue;
                slot.Add(new PropertyField { bindingPath = path });
            }
        }

        #endregion

        #region Preview

        private void RegisterPreviewCallbacks()
        {
            foreach (var (path, previewName) in PreviewBindings)
            {
                var slot = _root.Q<VisualElement>($"page1-slot-{path}");
                if (slot == null) continue;

                var pf = slot.Q<PropertyField>();
                if (pf == null) continue;

                var capturedPath = path;
                var capturedPreview = previewName;

                pf.RegisterValueChangeCallback(evt =>
                {
                    _so.Update();
                    UpdatePartPreview(capturedPreview, capturedPath);
                    _previewRefreshTime = Time.realtimeSinceStartup + 0.15f;
                });
            }

            var colorSlot = _root.Q<VisualElement>("page0-slot-gimmickColor");
            if (colorSlot != null)
            {
                var pf = colorSlot.Q<PropertyField>();
                if (pf != null)
                {
                    pf.RegisterValueChangeCallback(evt =>
                    {
                        _so.ApplyModifiedProperties();
                        MetamorphoseSetupService.ApplyGimmickColor(_target);
                    });
                }
            }
        }

        private void UpdateAllPreviews()
        {
            foreach (var (path, previewName) in PreviewBindings)
                UpdatePartPreview(previewName, path);

            var colaboContainer = _root.Q<VisualElement>("colabo-image-container");
            if (colaboContainer != null)
            {
                colaboContainer.Clear();
                if (_target.colaboShopTex != null)
                {
                    var img = new Image { image = _target.colaboShopTex };
                    img.AddToClassList("colabo-shop-image");
                    colaboContainer.Add(img);
                }
            }
        }

        private void UpdatePartPreview(string previewElementName, string propertyPath)
        {
            var container = _root.Q<VisualElement>(previewElementName);
            if (container == null) return;

            container.Clear();

            var prop = _so.FindProperty(propertyPath);
            if (prop == null || !prop.isArray) return;

            for (int i = 0; i < prop.arraySize; i++)
            {
                var go = prop.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                if (go == null) continue;

                var previewTex = GetPreviewTexture(go);

                var img = new Image();
                if (previewTex != null)
                    img.image = previewTex;
                img.AddToClassList("preview-thumb");
                container.Add(img);
            }
        }

        private Texture2D GetPreviewTexture(GameObject go)
        {
            if (go == null) return null;

            var tex = AssetPreview.GetAssetPreview(go);
            if (tex != null)
            {
                _previewTexCache[go] = tex;
                return tex;
            }

            if (_previewTexCache.TryGetValue(go, out var cached))
                return cached;

            tex = AssetPreview.GetMiniThumbnail(go);
            if (tex != null)
            {
                _previewTexCache[go] = tex;
                return tex;
            }

            tex = RenderCloseUpPreview(go);
            if (tex != null)
            {
                _previewTexCache[go] = tex;
                _ownedPreviewTextures.Add(tex);
                return tex;
            }

            return null;
        }

        private static Texture2D RenderCloseUpPreview(GameObject prefabAsset)
        {
            if (prefabAsset == null) return null;

            var renderers = prefabAsset.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return null;

            var preview = new PreviewRenderUtility();
            try
            {
                preview.AddSingleGO(prefabAsset);

                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (maxDim <= 0f) maxDim = 1f;

                float halfFov = preview.camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
                float dist = maxDim / (2f * Mathf.Tan(halfFov)) * 1.05f;

                Vector3 center = bounds.center;
                preview.camera.transform.position = center - Vector3.forward * dist;
                preview.camera.transform.LookAt(center);
                preview.camera.nearClipPlane = 0.001f;
                preview.camera.farClipPlane = dist + maxDim * 2f;

                preview.camera.Render();
                return preview.EndPreview() as Texture2D;
            }
            catch
            {
                return null;
            }
            finally
            {
                preview.Cleanup();
            }
        }

        private bool HasAnyMissingPreview()
        {
            foreach (var (path, _) in PreviewBindings)
            {
                var prop = _so.FindProperty(path);
                if (prop == null || !prop.isArray) continue;

                for (int i = 0; i < prop.arraySize; i++)
                {
                    var go = prop.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                    if (go == null) continue;
                    if (AssetPreview.GetAssetPreview(go) == null && !_previewTexCache.ContainsKey(go))
                        return true;
                }
            }
            return false;
        }

        #endregion

        #region Banner (OG Metadata)

        private void LoadBannerCards()
        {
            _bannerCardStates.Clear();
            _bannerLoading = false;

            var container = _root.Q<VisualElement>("banner-cards");
            if (container == null) return;
            container.Clear();

            var urls = _target.bannerAdUrls;
            if (urls == null || urls.Length == 0)
                urls = DefaultBannerUrls;

            container.style.display = DisplayStyle.Flex;

            foreach (var url in urls)
            {
                if (string.IsNullOrWhiteSpace(url)) continue;

                _bannerCardStates.Add(new BannerCardState { url = url });
                RenderBannerCard(container, _bannerCardStates.Count - 1);
            }

            FetchBannerMetadata();
        }

        private void RenderBannerCard(VisualElement container, int index)
        {
            var state = _bannerCardStates[index];

            var card = new VisualElement();
            card.AddToClassList("banner-card");

            var img = new Image();
            img.AddToClassList("banner-card-image");
            if (state.image != null)
                img.image = state.image;
            card.Add(img);

            var lbl = new Label();
            lbl.AddToClassList("banner-card-label");
            if (state.loaded)
                lbl.text = state.title;
            else if (state.failed)
                lbl.text = "Failed to load";
            else
                lbl.text = "Loading...";
            card.Add(lbl);

            var capturedUrl = state.url;
            card.AddManipulator(new Clickable(() => Application.OpenURL(capturedUrl)));

            container.Add(card);
        }

        private async void FetchBannerMetadata()
        {
            if (_bannerLoading) return;
            _bannerLoading = true;

            var container = _root.Q<VisualElement>("banner-cards");
            if (container == null) { _bannerLoading = false; return; }

            for (int i = 0; i < _bannerCardStates.Count; i++)
            {
                if (_bannerCardStates[i].loaded || _bannerCardStates[i].failed) continue;

                var url = _bannerCardStates[i].url;
                try
                {
                    var (title, imageUrl) = await FetchOgMetadataAsync(url);
                    Texture2D tex = null;

                    if (!string.IsNullOrEmpty(imageUrl))
                        tex = await DownloadImageAsync(imageUrl);

                    _bannerCardStates[i] = new BannerCardState
                    {
                        url = url,
                        title = title ?? url,
                        image = tex,
                        loaded = true,
                        failed = false,
                    };
                }
                catch
                {
                    _bannerCardStates[i] = new BannerCardState
                    {
                        url = url,
                        title = url,
                        loaded = false,
                        failed = true,
                    };
                }

                RefreshBannerCard(container, i);
            }

            _bannerLoading = false;
        }

        private void RefreshBannerCard(VisualElement container, int index)
        {
            if (index >= container.childCount) return;
            var card = container[index];
            if (card == null) return;

            var state = _bannerCardStates[index];

            var img = card.Q<Image>(className: "banner-card-image");
            if (img == null)
            {
                img = card.Query<Image>().First();
            }
            if (img != null && state.image != null)
                img.image = state.image;

            var lbl = card.Q<Label>(className: "banner-card-label");
            if (lbl == null)
            {
                lbl = card.Query<Label>().First();
            }
            if (lbl != null)
            {
                if (state.loaded)
                    lbl.text = state.title;
                else if (state.failed)
                    lbl.text = "Failed";
            }
        }

        private static async Task<(string title, string imageUrl)> FetchOgMetadataAsync(string url)
        {
            using var client = new HttpClient();
            client.Timeout = System.TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

            var response = await client.GetStringAsync(url);
            var html = response;

            string title = ExtractMetaContent(html, "og:title")
                        ?? ExtractMetaContent(html, "twitter:title")
                        ?? ExtractTitleTag(html)
                        ?? url;

            string imageUrl = ExtractMetaContent(html, "og:image")
                           ?? ExtractMetaContent(html, "twitter:image")
                           ?? "";

            if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("//"))
                imageUrl = "https:" + imageUrl;
            else if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("/"))
                imageUrl = new System.Uri(url).GetLeftPart(System.UriPartial.Scheme) + imageUrl;

            return (title, imageUrl);
        }

        private static async Task<Texture2D> DownloadImageAsync(string url)
        {
            using var client = new HttpClient();
            client.Timeout = System.TimeSpan.FromSeconds(10);

            var bytes = await client.GetByteArrayAsync(url);

            var tex = new Texture2D(2, 2);
            if (!tex.LoadImage(bytes))
            {
                Object.DestroyImmediate(tex);
                return null;
            }
            return tex;
        }

        private static string ExtractMetaContent(string html, string property)
        {
            var pattern = $@"<meta[^>]+(?:property|name)=[""']{Regex.Escape(property)}[""'][^>]+content=[""']([^""']+)[""']";
            var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                pattern = $@"<meta[^>]+content=[""']([^""']+)[""'][^>]+(?:property|name)=[""']{Regex.Escape(property)}[""']";
                match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
            }
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string ExtractTitleTag(string html)
        {
            var match = Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        #endregion

        #region Localization

        private void ApplyLocalization(VisualElement root)
        {
            var langNames = LocalizationManager.SupportedLanguageNames;
            for (int i = 0; i < langNames.Length; i++)
            {
                var btn = root.Q<Button>($"lang-btn-{i}");
                if (btn != null) btn.text = langNames[i];
            }

            root.Q<Label>("page0-desc").text = L("step1_description");
            root.Q<Label>("fg0-avatar").text = L("step1_avatar_label");
            SetPropLabel(root, "page0-slot-avatar", L("step1_avatar"));
            root.Q<Label>("fg0-off").text = L("step1_before_clothes_label");
            root.Q<Label>("fg0-color").text = L("step3_color_label");

            root.Q<Label>("page1-desc").text = L("step2_description");
            root.Q<Label>("fg1-parts").text = L("step2_parts_title");
            root.Q<HelpBox>("page1-help").text = L("step2_parts_help");
            root.Q<Label>("label-head").text = L("step2_head_items");
            root.Q<Label>("label-body").text = L("step2_body_items");
            root.Q<Label>("label-hand").text = L("step2_hand_items");
            root.Q<Label>("label-leg").text = L("step2_leg_items");

            root.Q<Label>("page2-desc").text = L("step4_description");
            root.Q<Button>("btn-colabo-shop").text = LC("colabo_shop_button");
            root.Q<HelpBox>("step4-colabo-help").text = LC("colabo_info");
            root.Q<Label>("fg2-fade").text = L("step4_fade_fx_title");

            root.Q<Label>("fg3-basic").text = LC("dev_basic_label");
            root.Q<Label>("fg3-targets").text = LC("dev_target_label");
            root.Q<Label>("fg3-fade-transforms").text = LC("dev_fade_transform_label");
            root.Q<Label>("fg3-materials").text = LC("dev_material_label");
            root.Q<Label>("fg3-gimmick-color").text = LC("dev_gimmick_color_targets");
            root.Q<Label>("fg3-banner").text = "Banner Ad URLs";
        }

        private void SetPropLabel(VisualElement root, string slotName, string label)
        {
            var slot = root.Q<VisualElement>(slotName);
            if (slot == null) return;

            var pf = slot.Q<PropertyField>();
            if (pf != null) pf.label = label;
        }

        private void ReloadAndApplyLocalization()
        {
            LocalizationManager.Load("Metamorphose", _languageCodes[_selectedLanguage]);
            if (_root != null) ApplyLocalization(_root);
        }

        #endregion

        #region Button Callbacks

        private void SetupButtonCallbacks()
        {
            // Theme toggle
            var themeBtn = _root.Q<Button>("theme-toggle");
            if (themeBtn != null)
                themeBtn.clicked += ToggleTheme;

            for (int i = 0; i < _languageCodes.Length; i++)
            {
                int index = i;
                var btn = _root.Q<Button>($"lang-btn-{i}");
                if (btn != null)
                    btn.clicked += () => SwitchLanguage(index);
            }

            _root.Q<Button>("btn-booth").clicked += () => Application.OpenURL(BoothUrl);
            _root.Q<Button>("btn-discord").clicked += () => Application.OpenURL(DiscordUrl);

            _root.Q<Button>("btn-colabo-shop").clicked += () =>
            {
                if (!string.IsNullOrEmpty(_target.colaboShopInfo))
                    Application.OpenURL(_target.colaboShopInfo);
            };

            var btnGenerateAnim = _root.Q<Button>("btn-generate-anim");
            if (btnGenerateAnim != null)
            {
                btnGenerateAnim.clicked += () => GeneratePhysicalAnimations();
            }
        }

        private void GeneratePhysicalAnimations()
        {
            if (_target == null) return;

            if (_target.Avatar == null)
            {
                EditorUtility.DisplayDialog("Error", "Avatar is not set.", "OK");
                return;
            }

            if (_target.Animator == null)
            {
                EditorUtility.DisplayDialog("Error", "Animator is not set in Dev page.", "OK");
                return;
            }

            var controller = _target.Animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Error", "Animator component has no AnimatorController assigned.", "OK");
                return;
            }

            if (_target.OffTargets == null || _target.OffTargets.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "OffTargets (before-transformation clothes) is empty.", "OK");
                return;
            }

            if (_target.Model == null)
            {
                EditorUtility.DisplayDialog("Error", "Model is not set in Dev page.", "OK");
                return;
            }

            string avatarName = _target.Avatar.name;
            string outputFolder = $"Assets/moruton/MetamorphoseAnim/{avatarName}";

            var offTargets = System.Array.FindAll(_target.OffTargets, t => t != null);

            var (enableClip, disableClip) = AnimationBuilder.CreateToggleAnimations(
                _target.Avatar,
                offTargets,
                _target.Model,
                outputFolder,
                "Metamorphose_Enable",
                "Metamorphose_Disable"
            );

            if (enableClip != null)
            {
                AnimationBuilder.ApplyClipToState(controller, "Enable", enableClip);
            }

            if (disableClip != null)
            {
                AnimationBuilder.ApplyClipToState(controller, "Disable", disableClip);
            }

            EditorUtility.DisplayDialog("Success", $"Animations generated as physical assets and assigned to original AnimatorController '{controller.name}'.\nFolder: {outputFolder}", "OK");
        }

        private void SwitchLanguage(int index)
        {
            if (_selectedLanguage == index) return;
            _selectedLanguage = index;
            EditorPrefs.SetInt("MetamorphoseEditor_Language", _selectedLanguage);

            for (int i = 0; i < _languageCodes.Length; i++)
            {
                var btn = _root.Q<Button>($"lang-btn-{i}");
                if (btn != null) btn.EnableInClassList("selected", i == index);
            }

            ReloadAndApplyLocalization();
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

        private void ClearPreviewCache()
        {
            foreach (var tex in _ownedPreviewTextures)
            {
                if (tex != null) Object.DestroyImmediate(tex);
            }
            _ownedPreviewTextures.Clear();
            _previewTexCache.Clear();
        }

        #endregion
    }
}
