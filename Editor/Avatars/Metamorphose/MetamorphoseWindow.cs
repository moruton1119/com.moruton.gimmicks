using System.Collections.Generic;
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

        private const int PreviewTexSize = 128;
        private const int TotalPages = 5;

        private readonly string[] _languageCodes = { "ja", "en", "ko", "it", "es" };

        private PrettyCureMirror _target;
        private SerializedObject _so;
        private VisualElement _root;
        private bool _uiBuilt;
        private int _selectedLanguage;
        private int _currentPage;
        private Color _lastGimmickColor;
        private float _previewRefreshTime = -1f;
        private readonly Dictionary<Object, Texture2D> _previewTexCache = new();
        private readonly HashSet<Texture2D> _ownedPreviewTextures = new();

        private static readonly string[][] PageFieldPaths =
        {
            new[] { "avatar", "model", "animator", "offTargets" },
            new[] { "headItems", "bodyItems", "handItems", "legItems" },
            new[] { "gimmickColor" },
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
            ("dummyImage", "page4-slot-dummyImage"),
            ("colaboShopTex", "page4-slot-colaboShopTex"),
            ("colaboShopInfo", "page4-slot-colaboShopInfo"),
            ("model", "page4-slot-model"),
            ("animator", "page4-slot-animator"),
            ("headTarget", "page4-slot-devHeadTarget"),
            ("bodyTarget", "page4-slot-devBodyTarget"),
            ("handTarget", "page4-slot-devHandTarget"),
            ("legTarget", "page4-slot-devLegTarget"),
            ("onePiece", "page4-slot-devOnePiece"),
            ("colaboItemTarget", "page4-slot-devColaboItemTarget"),
            ("colaboItem", "page4-slot-devColaboItem"),
            ("colaboFBX", "page4-slot-devColaboFBX"),
            ("fadeHead", "page4-slot-devFadeHead"),
            ("fadeBody", "page4-slot-devFadeBody"),
            ("fadeArm", "page4-slot-devFadeArm"),
            ("fadeLeg", "page4-slot-devFadeLeg"),
            ("fadeHeadMaterial", "page4-slot-devFadeHeadMaterial"),
            ("fadeBodyMaterial", "page4-slot-devFadeBodyMaterial"),
            ("fadeArmMaterial", "page4-slot-devFadeArmMaterial"),
            ("fadeLegMaterial", "page4-slot-devFadeLegMaterial"),
            ("gimmickCollar", "page4-slot-gimmickCollar"),
        };

        private static readonly (string label, string url, string colorClass)[] BannerCards =
        {
            ("Booth Store", "https://moruton.booth.pm/", "blue"),
            ("Discord", "https://discord.gg/GHJwmyTcfX", "purple"),
            ("Item Randomizer", "", "green"),
            ("Setup Helper", "", "orange"),
        };

        private string L(string key) => LocalizationManager.Get("PrettyCureMirror", key);
        private string LC(string key) => LocalizationManager.GetCommon(key);

        #region Public

        public static void Show(PrettyCureMirror target)
        {
            if (target == null) return;
            var window = GetWindow<MetamorphoseWindow>("Metamorphose Setup");
            window.minSize = new Vector2(520, 560);
            window.SetTarget(target);
        }

        public void SetTarget(PrettyCureMirror target)
        {
            _target = target;
            _so = target != null ? new SerializedObject(target) : null;

            if (target != null)
            {
                target.AutoAssignAvatarAndAnimatorIfEmpty();
                _lastGimmickColor = target.gimmickColor;
            }

            _uiBuilt = false;
            _currentPage = 0;
            ClearPreviewCache();
            Repaint();
        }

        #endregion

        #region Lifecycle

        private void OnEnable()
        {
            _selectedLanguage = EditorPrefs.GetInt("MetamorphoseEditor_Language", 0);
            LocalizationManager.Load("PrettyCureMirror", _languageCodes[_selectedLanguage]);
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            ClearPreviewCache();
        }

        public void CreateGUI()
        {
            if (_target == null || _so == null)
            {
                rootElement.Clear();
                var label = new Label("No target assigned.");
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                rootElement.Add(label);
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
                rootElement.Clear();
                rootElement.Add(new Label($"Error: Could not load {UxmlPath}"));
                return;
            }

            _root = visualTree.CloneTree();

            CreatePropertyFields();
            RegisterPreviewCallbacks();
            SetupNavigation();
            SetupButtonCallbacks();
            CreateBannerCards();
            ApplyLocalization(_root);

            _root.Bind(_so);
            UpdateAllPreviews();

            if (HasAnyMissingPreview())
                _previewRefreshTime = Time.realtimeSinceStartup + 0.15f;

            var shopSection = _root.Q<VisualElement>("colabo-shop-section");
            if (shopSection != null)
            {
                bool hasShop = _target.colaboShopTex != null || !string.IsNullOrEmpty(_target.colaboShopInfo);
                shopSection.style.display = hasShop ? DisplayStyle.Flex : DisplayStyle.None;
            }

            rootElement.Clear();
            rootElement.Add(_root);
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
                var instance = preview.AddSingleGO(prefabAsset);
                if (instance == null) return null;

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

                var rect = new Rect(0, 0, PreviewTexSize, PreviewTexSize);
                preview.Render(rect, true, true);
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

        #region Banner

        private void CreateBannerCards()
        {
            var container = _root.Q<VisualElement>("banner-cards");
            if (container == null) return;
            container.Clear();

            foreach (var (label, url, colorClass) in BannerCards)
            {
                var card = new VisualElement();
                card.AddToClassList("banner-card");
                card.AddToClassList(colorClass);

                var lbl = new Label(label);
                card.Add(lbl);

                if (!string.IsNullOrEmpty(url))
                {
                    var capturedUrl = url;
                    card.AddManipulator(new Clickable(() => Application.OpenURL(capturedUrl)));
                    card.style.cursorStyle = CursorStyle.Link;
                }

                container.Add(card);
            }
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

            root.Q<Button>("nav-0").text = $"① {L("step1_title")}";
            root.Q<Button>("nav-1").text = $"② {L("step2_title")}";
            root.Q<Button>("nav-2").text = $"③ {L("step3_title")}";
            root.Q<Button>("nav-3").text = $"④ {L("step4_title")}";
            root.Q<Button>("nav-4").text = "Dev Mode";

            root.Q<Label>("page0-desc").text = L("step1_description");
            root.Q<Label>("fg0-avatar").text = L("step1_avatar_label");
            SetPropLabel(root, "page0-slot-avatar", L("step1_avatar"));
            SetPropLabel(root, "page0-slot-model", L("step1_model"));
            SetPropLabel(root, "page0-slot-animator", L("step1_animator"));
            root.Q<Label>("fg0-off").text = L("step1_before_clothes_label");

            root.Q<Label>("page1-desc").text = L("step2_description");
            root.Q<Label>("fg1-parts").text = L("step2_parts_title");
            root.Q<HelpBox>("page1-help").text = L("step2_parts_help");
            root.Q<Label>("label-head").text = L("step2_head_items");
            root.Q<Label>("label-body").text = L("step2_body_items");
            root.Q<Label>("label-hand").text = L("step2_hand_items");
            root.Q<Label>("label-leg").text = L("step2_leg_items");

            root.Q<Label>("page2-desc").text = L("step3_description");
            root.Q<Label>("fg2-color").text = L("step3_color_label");

            root.Q<Label>("page3-desc").text = L("step4_description");
            root.Q<Button>("btn-colabo-shop").text = LC("colabo_shop_button");
            root.Q<HelpBox>("step4-colabo-help").text = LC("colabo_info");
            root.Q<Label>("fg3-fade").text = L("step4_fade_fx_title");

            root.Q<Label>("fg4-basic").text = LC("dev_basic_label");
            root.Q<Label>("fg4-targets").text = LC("dev_target_label");
            root.Q<Label>("fg4-fade-transforms").text = LC("dev_fade_transform_label");
            root.Q<Label>("fg4-materials").text = LC("dev_material_label");
            root.Q<Label>("fg4-gimmick-color").text = LC("dev_gimmick_color_targets");
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
            LocalizationManager.Load("PrettyCureMirror", _languageCodes[_selectedLanguage]);
            if (_root != null) ApplyLocalization(_root);
        }

        #endregion

        #region Button Callbacks

        private void SetupButtonCallbacks()
        {
            for (int i = 0; i < _languageCodes.Length; i++)
            {
                int index = i;
                var btn = _root.Q<Button>($"lang-btn-{i}");
                if (btn != null)
                    btn.clicked += () => SwitchLanguage(index);
            }

            _root.Q<Button>("btn-colabo-shop").clicked += () =>
            {
                if (!string.IsNullOrEmpty(_target.colaboShopInfo))
                    Application.OpenURL(_target.colaboShopInfo);
            };
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
