using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 変身ギミック (Metamorphose) の Inspector。
    /// UI Toolkit (UXML/USS) で構築。
    /// </summary>
    [CustomEditor(typeof(PrettyCureMirror))]
    public class MetamorphoseEditor : UnityEditor.Editor
    {
        private const string UxmlPath = "Packages/com.moruton.gimmicks/Editor/Avatars/Metamorphose/MetamorphoseEditor.uxml";

        private readonly string[] languageCodes = { "ja", "en", "ko", "it", "es" };

        private VisualElement _root;
        private int _selectedLanguage;
        private Color _lastGimmickColor;
        private float _previewRefreshTime = -1f;

        // Step フィールド (ユーザー向け) — slot-{path} にPropertyFieldを生成
        private static readonly string[] FieldPaths =
        {
            "avatar",
            "model",
            "animator",
            "offTargets",
            "headItems",
            "bodyItems",
            "handItems",
            "legItems",
            "gimmickColor",
            "fadeHeadItems",
            "fadeBodyItems",
            "fadeArmItems",
            "fadeLegItems",
        };

        // プレビュー対象のマッピング (propertyPath → preview要素名)
        private static readonly (string path, string preview)[] PreviewBindings =
        {
            ("headItems", "head-preview"),
            ("bodyItems", "body-preview"),
            ("handItems", "hand-preview"),
            ("legItems", "leg-preview"),
        };

        // Developer Mode フィールド (開発者向け)
        private static readonly (string path, string label, string slot)[] DevFieldBindings =
        {
            ("dummyImage", "Dummy Image", "slot-dummyImage"),
            ("colaboShopTex", "Colabo Shop Tex", "slot-colaboShopTex"),
            ("colaboShopInfo", "Colabo Shop Info", "slot-colaboShopInfo"),
            ("model", "Model", "slot-devmodel"),
            ("animator", "Animator", "slot-devanimator"),
            ("headTarget", "Head Target", "slot-devHeadTarget"),
            ("bodyTarget", "Body Target", "slot-devBodyTarget"),
            ("handTarget", "Hand Target", "slot-devHandTarget"),
            ("legTarget", "Leg Target", "slot-devLegTarget"),
            ("onePiece", "OnePiece", "slot-devOnePiece"),
            ("colaboItemTarget", "Colabo Item Target", "slot-devColaboItemTarget"),
            ("colaboItem", "Colabo Item", "slot-devColaboItem"),
            ("colaboFBX", "Colabo FBX", "slot-devColaboFBX"),
            ("fadeHead", "Fade Head", "slot-devFadeHead"),
            ("fadeBody", "Fade Body", "slot-devFadeBody"),
            ("fadeArm", "Fade Arm", "slot-devFadeArm"),
            ("fadeLeg", "Fade Leg", "slot-devFadeLeg"),
            ("fadeHeadMaterial", "Head Material", "slot-devFadeHeadMaterial"),
            ("fadeBodyMaterial", "Body Material", "slot-devFadeBodyMaterial"),
            ("fadeArmMaterial", "Arm Material", "slot-devFadeArmMaterial"),
            ("fadeLegMaterial", "Leg Material", "slot-devFadeLegMaterial"),
            ("gimmickCollar", "Gimmick Collar", "slot-gimmickCollar"),
        };

        private string L(string key) => LocalizationManager.Get("PrettyCureMirror", key);
        private string LC(string key) => LocalizationManager.GetCommon(key);

        #region Lifecycle

        private void OnEnable()
        {
            _selectedLanguage = EditorPrefs.GetInt("MetamorphoseEditor_Language", 0);
            LocalizationManager.Load("PrettyCureMirror", languageCodes[_selectedLanguage]);

            if (target != null)
            {
                var script = (PrettyCureMirror)target;
                script.AutoAssignAvatarAndAnimatorIfEmpty();
                _lastGimmickColor = script.gimmickColor;
            }

            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDestroy()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (target == null) return;

            var script = (PrettyCureMirror)target;
            if (script.gimmickColor != _lastGimmickColor)
            {
                _lastGimmickColor = script.gimmickColor;
                MetamorphoseSetupService.ApplyGimmickColor(script);
            }

            if (_previewRefreshTime > 0f && Time.realtimeSinceStartup >= _previewRefreshTime)
            {
                UpdateAllPreviews();
                if (!HasAnyMissingPreview())
                    _previewRefreshTime = -1f;
                else
                    _previewRefreshTime = Time.realtimeSinceStartup + 0.2f;
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            try
            {
                return BuildInspector();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MetamorphoseEditor] CreateInspectorGUI failed: {e}");
                var fallback = new VisualElement();
                var label = new Label("MetamorphoseEditor error - check Console");
                label.style.color = Color.red;
                fallback.Add(label);
                return fallback;
            }
        }

        private VisualElement BuildInspector()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (visualTree == null)
                return new Label($"Error: Could not load {UxmlPath}");

            _root = visualTree.CloneTree();

            CreatePropertyFields();
            RegisterPreviewCallbacks();

            // Header (IMGUI wrapped)
            var headerContainer = _root.Q<VisualElement>("header-container");
            if (headerContainer != null)
                headerContainer.Add(new IMGUIContainer(() => MorutonAvatarPackageEditorHelper.DrawHeader()));

            ApplyLocalization(_root);
            SetupStepToggles(_root);
            SetupPartToggles(_root);
            SetupButtonCallbacks(_root);

            _root.Bind(serializedObject);
            UpdateAllPreviews();
            if (HasAnyMissingPreview())
                _previewRefreshTime = Time.realtimeSinceStartup + 0.15f;

            return _root;
        }

        #endregion

        #region PropertyField Generation

        private void CreatePropertyFields()
        {
            // ユーザー向けフィールド
            foreach (var path in FieldPaths)
            {
                var slot = _root.Q<VisualElement>($"slot-{path}");
                if (slot == null) continue;

                var pf = new PropertyField { bindingPath = path };
                slot.Add(pf);
            }

            // Developer Mode フィールド
            foreach (var (path, label, slotName) in DevFieldBindings)
            {
                var slotEl = _root.Q<VisualElement>(slotName);
                if (slotEl == null) continue;

                var pf = new PropertyField { bindingPath = path, label = label };
                slotEl.Add(pf);
            }
        }

        #endregion

        #region Preview

        /// <summary>
        /// プレビュー対象のPropertyFieldに変更コールバックを登録。
        /// slot名から直接PropertyFieldを取得する（インデックス不使用）。
        /// </summary>
        private void RegisterPreviewCallbacks()
        {
            foreach (var (path, previewName) in PreviewBindings)
            {
                var slot = _root.Q<VisualElement>($"slot-{path}");
                if (slot == null) continue;

                var pf = slot.Q<PropertyField>();
                if (pf == null) continue;

                var capturedPath = path;
                var capturedPreview = previewName;

                pf.RegisterValueChangeCallback(evt =>
                {
                    UpdatePartPreview(capturedPreview, capturedPath);
                    _previewRefreshTime = Time.realtimeSinceStartup + 0.15f;
                });
            }
        }

        /// <summary>
        /// 全プレビューを初期化。
        /// </summary>
        private void UpdateAllPreviews()
        {
            foreach (var (path, previewName) in PreviewBindings)
            {
                UpdatePartPreview(previewName, path);
            }

            // コラボ画像
            var script = (PrettyCureMirror)target;
            var colaboContainer = _root.Q<VisualElement>("colabo-image-container");
            if (colaboContainer != null)
            {
                colaboContainer.Clear();
                if (script.colaboShopTex != null)
                {
                    var img = new Image { image = script.colaboShopTex };
                    img.AddToClassList("colabo-shop-image");
                    colaboContainer.Add(img);
                }
            }
        }

        /// <summary>
        /// 指定プロパティ配列からプレビューを生成。
        /// </summary>
        private void UpdatePartPreview(string previewElementName, string propertyPath)
        {
            var container = _root.Q<VisualElement>(previewElementName);
            if (container == null) return;

            container.Clear();

            var prop = serializedObject.FindProperty(propertyPath);
            if (prop == null || !prop.isArray) return;

            int count = Mathf.Min(prop.arraySize, 4);
            for (int i = 0; i < count; i++)
            {
                var go = prop.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                if (go == null) continue;

                var previewTex = AssetPreview.GetAssetPreview(go);
                if (previewTex == null)
                    previewTex = AssetPreview.GetMiniThumbnail(go);

                var img = new Image();
                if (previewTex != null)
                    img.image = previewTex;
                img.AddToClassList("preview-thumb");
                container.Add(img);
            }

            if (prop.arraySize > count)
            {
                var more = new Label($"+{prop.arraySize - count} more...");
                more.AddToClassList("preview-more");
                container.Add(more);
            }
        }

        private bool HasAnyMissingPreview()
        {
            foreach (var (path, _) in PreviewBindings)
            {
                var prop = serializedObject.FindProperty(path);
                if (prop == null || !prop.isArray) continue;

                for (int i = 0; i < prop.arraySize; i++)
                {
                    var go = prop.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                    if (go != null && AssetPreview.GetAssetPreview(go) == null)
                        return true;
                }
            }
            return false;
        }

        #endregion

        #region Localization

        private void ApplyLocalization(VisualElement root)
        {
            // 言語ボタン
            var langNames = LocalizationManager.SupportedLanguageNames;
            for (int i = 0; i < langNames.Length; i++)
            {
                var btn = root.Q<Button>($"lang-btn-{i}");
                if (btn != null) btn.text = langNames[i];
            }

            // Step トグル
            root.Q<Button>("step1-toggle").text = $"▼ {L("step1_title")}";
            root.Q<Button>("step2-toggle").text = $"▶ {L("step2_title")}";
            root.Q<Button>("step3-toggle").text = $"▶ {L("step3_title")}";
            root.Q<Button>("step4-toggle").text = $"▶ {L("step4_title")}";

            // Step 1
            root.Q<Label>("step1-desc").text = L("step1_description");
            root.Q<Label>("step1-avatar-label").text = L("step1_avatar_label");
            SetPropLabel(root, "slot-avatar", L("step1_avatar"));
            SetPropLabel(root, "slot-model", L("step1_model"));
            SetPropLabel(root, "slot-animator", L("step1_animator"));
            root.Q<Label>("step1-off-label").text = L("step1_before_clothes_label");

            // Step 2
            root.Q<Label>("step2-desc").text = L("step2_description");
            root.Q<Label>("step2-parts-label").text = L("step2_parts_title");
            root.Q<HelpBox>("step2-parts-help").text = L("step2_parts_help");

            root.Q<Label>("label-head-items").text = L("step2_head_items");
            root.Q<Label>("label-body-items").text = L("step2_body_items");
            root.Q<Label>("label-hand-items").text = L("step2_hand_items");
            root.Q<Label>("label-leg-items").text = L("step2_leg_items");

            // Step 3
            root.Q<Label>("step3-desc").text = L("step3_description");
            SetPropLabel(root, "slot-gimmickColor", L("step3_color_label"));

            // Step 4
            root.Q<Label>("step4-desc").text = L("step4_description");
            root.Q<Button>("btn-colabo-shop").text = LC("colabo_shop_button");
            root.Q<HelpBox>("step4-colabo-help").text = LC("colabo_info");

            root.Q<Label>("step4-fade-label").text = L("step4_fade_fx_title");

            // Developer Mode labels
            root.Q<Label>("dev-basic-label").text = LC("dev_basic_label");
            root.Q<Label>("dev-target-label").text = LC("dev_target_label");
            root.Q<Label>("dev-material-label").text = LC("dev_material_label");
            root.Q<Label>("dev-gimmick-color-label").text = LC("dev_gimmick_color_targets");
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
            LocalizationManager.Load("PrettyCureMirror", languageCodes[_selectedLanguage]);
            if (_root != null) ApplyLocalization(_root);
        }

        #endregion

        #region Step Toggles

        private void SetupStepToggles(VisualElement root)
        {
            SetupToggle(root, "step1-toggle", "step1-body");
            SetupToggle(root, "step2-toggle", "step2-body");
            SetupToggle(root, "step3-toggle", "step3-body");
            SetupToggle(root, "step4-toggle", "step4-body");
            SetupToggle(root, "devmode-toggle", "devmode-body");
        }

        private void SetupToggle(VisualElement root, string toggleName, string bodyName)
        {
            var toggle = root.Q<Button>(toggleName);
            var body = root.Q<VisualElement>(bodyName);
            if (toggle == null || body == null) return;

            toggle.clicked += () =>
            {
                bool isExpanded = !body.ClassListContains("collapsed");
                if (isExpanded)
                {
                    body.AddToClassList("collapsed");
                    toggle.text = toggle.text.Replace("▼", "▶");
                }
                else
                {
                    body.RemoveFromClassList("collapsed");
                    toggle.text = toggle.text.Replace("▶", "▼");
                }
            };
        }

        #endregion

        #region Part Toggles (Developer Mode)

        private void SetupPartToggles(VisualElement root)
        {
            SetupPartToggle(root, "devmode-toggle-headItems", "section-headItems", "Head");
            SetupPartToggle(root, "devmode-toggle-bodyItems", "section-bodyItems", "Body");
            SetupPartToggle(root, "devmode-toggle-handItems", "section-handItems", "Hand");
            SetupPartToggle(root, "devmode-toggle-legItems", "section-legItems", "Leg");
            SetupPartToggle(root, "devmode-toggle-fadeHeadItems", "section-fadeHeadItems", "F-Head");
            SetupPartToggle(root, "devmode-toggle-fadeBodyItems", "section-fadeBodyItems", "F-Body");
            SetupPartToggle(root, "devmode-toggle-fadeArmItems", "section-fadeArmItems", "F-Arm");
            SetupPartToggle(root, "devmode-toggle-fadeLegItems", "section-fadeLegItems", "F-Leg");
        }

        private void SetupPartToggle(VisualElement root, string toggleName, string rowName, string displayName)
        {
            var toggle = root.Q<Button>(toggleName);
            var row = root.Q<VisualElement>(rowName);
            if (toggle == null || row == null) return;

            string prefsKey = $"MetamorphoseEditor_Show_{rowName}";
            bool isVisible = EditorPrefs.GetBool(prefsKey, true);
            ApplyPartToggleVisual(toggle, row, displayName, isVisible);

            toggle.clicked += () =>
            {
                isVisible = !isVisible;
                EditorPrefs.SetBool(prefsKey, isVisible);
                ApplyPartToggleVisual(toggle, row, displayName, isVisible);
            };
        }

        private void ApplyPartToggleVisual(Button toggle, VisualElement row, string displayName, bool isVisible)
        {
            toggle.text = $"{displayName}: {(isVisible ? "ON" : "OFF")}";
            toggle.EnableInClassList("part-toggle-off", !isVisible);
            row.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        #endregion

        #region Button Callbacks

        private void SetupButtonCallbacks(VisualElement root)
        {
            // 言語切替
            for (int i = 0; i < languageCodes.Length; i++)
            {
                int index = i;
                var btn = root.Q<Button>($"lang-btn-{i}");
                if (btn != null)
                    btn.clicked += () => SwitchLanguage(index);
            }

            // Colabo Shop
            root.Q<Button>("btn-colabo-shop").clicked += () =>
            {
                var script = (PrettyCureMirror)target;
                if (!string.IsNullOrEmpty(script.colaboShopInfo))
                    Application.OpenURL(script.colaboShopInfo);
            };
        }

        private void SwitchLanguage(int index)
        {
            if (_selectedLanguage == index) return;
            _selectedLanguage = index;
            EditorPrefs.SetInt("MetamorphoseEditor_Language", _selectedLanguage);

            for (int i = 0; i < languageCodes.Length; i++)
            {
                var btn = _root.Q<Button>($"lang-btn-{i}");
                if (btn != null) btn.EnableInClassList("selected", i == index);
            }

            ReloadAndApplyLocalization();
        }

        #endregion
    }
}
