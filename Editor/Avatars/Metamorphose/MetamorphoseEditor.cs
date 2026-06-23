using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 変身ギミック (PrettyCureMirror) の Inspector。
    /// UI Toolkit (UXML/USS) で構築。ロジックは MetamorphoseSetupService に委譲。
    /// 見た目を変えるなら USS、レイアウトを変えるなら UXML、処理を変えるなら SetupService。
    /// </summary>
    [CustomEditor(typeof(PrettyCureMirror))]
    public class MetamorphoseEditor : UnityEditor.Editor
    {
        private const string UxmlPath = "Packages/com.moruton.gimmicks/Editor/Avatars/Metamorphose/MetamorphoseEditor.uxml";

        private readonly string[] languageCodes = { "ja", "en", "ko", "it", "es" };

        private VisualElement _root;
        private int _selectedLanguage;
        private Color _lastGimmickColor;

        // Step フィールド (ユーザー向け)
        private static readonly (string path, string label)[] FieldBindings = new (string, string)[]
        {
            ("avatar", "Avatar Root"),
            ("model", "Model"),
            ("animator", "Animator"),
            ("offTargets", "Off Targets"),
            ("headItems", "Head Items"),
            ("bodyItems", "Body Items"),
            ("handItems", "Hand Items"),
            ("legItems", "Leg Items"),
            ("gimmickColor", "Gimmick Color"),
            ("fadeHeadItems", "Fade Head Items"),
            ("fadeBodyItems", "Fade Body Items"),
            ("fadeArmItems", "Fade Arm Items"),
            ("fadeLegItems", "Fade Leg Items"),
        };

        // Developer Mode フィールド (開発者向け)
        private static readonly (string path, string label, string slot)[] DevFieldBindings = new (string, string, string)[]
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

        private void OnEnable()
        {
            _selectedLanguage = EditorPrefs.GetInt("MetamorphoseEditor_Language", 0);
            LocalizationManager.Load("PrettyCureMirror", languageCodes[_selectedLanguage]);

            if (target != null)
                _lastGimmickColor = ((PrettyCureMirror)target).gimmickColor;

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
                // フォールバック: デフォルトInspectorを返す
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
            {
                return new Label($"Error: Could not load {UxmlPath}");
            }

            _root = visualTree.CloneTree();

            // PropertyField をスロットに手動生成
            CreatePropertyFields();

            // Header (IMGUI wrapped)
            var headerContainer = _root.Q<VisualElement>("header-container");
            headerContainer.Add(new IMGUIContainer(() => MorutonAvatarPackageEditorHelper.DrawHeader()));

            // ローカライゼーション
            ApplyLocalization(_root);

            // ステップトグル
            SetupStepToggles(_root);

            // ボタン コールバック
            SetupPartToggles(_root);
            SetupButtonCallbacks(_root);

            // Bind
            _root.Bind(serializedObject);

            // 初期プレビュー
            UpdateAllPreviews(_root);

            return _root;
        }

        #region PropertyField Generation

        private void CreatePropertyFields()
        {
            foreach (var (path, label) in FieldBindings)
            {
                var slot = _root.Q<VisualElement>($"slot-{path}");
                if (slot == null) continue;

                var pf = new PropertyField { bindingPath = path };
                if (label != null) pf.label = label;
                slot.Add(pf);
            }

            // Developer Mode フィールド
            foreach (var (path, label, slot) in DevFieldBindings)
            {
                var slotEl = _root.Q<VisualElement>(slot);
                if (slotEl == null) continue;

                var pf = new PropertyField { bindingPath = path, label = label };
                slotEl.Add(pf);
            }
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

            // Setup Button
            root.Q<Button>("btn-setup").text = LC("setup_button");

            // Developer Mode labels
            root.Q<Label>("dev-basic-label").text = LC("dev_basic_label");
            root.Q<Label>("dev-target-label").text = LC("dev_target_label");
            root.Q<Label>("dev-material-label").text = LC("dev_material_label");
            root.Q<Label>("dev-gimmick-color-label").text = LC("dev_gimmick_color_targets");
        }

        private void SetPropLabel(VisualElement root, string slotName, string label)
        {
            var slot = root.Q<VisualElement>(slotName);
            if (slot != null)
            {
                var pf = slot.Query<PropertyField>().First();
                if (pf != null) pf.label = label;
            }
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
                    toggle.RemoveFromClassList("expanded");
                    toggle.text = toggle.text.Replace("▼", "▶");
                }
                else
                {
                    body.RemoveFromClassList("collapsed");
                    toggle.AddToClassList("expanded");
                    toggle.text = toggle.text.Replace("▶", "▼");
                }
            };
        }

        #endregion

        #region Part Toggles (Step 2 & 3)

        private void SetupPartToggles(VisualElement root)
        {
            // Developer Mode のトグルで Step 2/4 のセクション全体を表示/非表示
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
            if (toggle == null || row == null)
            {
                Debug.LogWarning($"[MetamorphoseEditor] Toggle not found: {toggleName} -> {rowName}");
                return;
            }

            string prefsKey = $"MetamorphoseEditor_Show_{rowName}";
            bool isVisible = EditorPrefs.GetBool(prefsKey, true);

            Debug.Log($"[MetamorphoseEditor] Toggle setup: {toggleName} -> {rowName} ({displayName}) initial: {isVisible}");

            ApplyPartToggleVisual(toggle, row, displayName, isVisible);

            toggle.clicked += () =>
            {
                isVisible = !isVisible;
                EditorPrefs.SetBool(prefsKey, isVisible);
                Debug.Log($"[MetamorphoseEditor] Toggle clicked: {toggleName} -> {rowName} now: {isVisible}");
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
                {
                    btn.clicked += () => SwitchLanguage(index);
                }
            }

            // Colabo Shop
            root.Q<Button>("btn-colabo-shop").clicked += () =>
            {
                var script = (PrettyCureMirror)target;
                if (!string.IsNullOrEmpty(script.colaboShopInfo))
                    Application.OpenURL(script.colaboShopInfo);
            };

            // Setup
            root.Q<Button>("btn-setup").clicked += () =>
            {
                MetamorphoseSetupService.ExecuteFullSetup((PrettyCureMirror)target);
                serializedObject.Update();
                Repaint();
            };

            // Developer buttons removed (no longer needed)
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

        #region Previews

        private void UpdateAllPreviews(VisualElement root)
        {
            var script = (PrettyCureMirror)target;

            UpdatePartPreview(root, "head-preview", script.headItems);
            UpdatePartPreview(root, "body-preview", script.bodyItems);
            UpdatePartPreview(root, "hand-preview", script.handItems);
            UpdatePartPreview(root, "leg-preview", script.legItems);

            // コラボ画像
            var colaboContainer = root.Q<VisualElement>("colabo-image-container");
            colaboContainer.Clear();
            if (script.colaboShopTex != null)
            {
                var img = new Image { image = script.colaboShopTex };
                img.AddToClassList("colabo-shop-image");
                colaboContainer.Add(img);
            }
        }

        private void UpdatePartPreview(VisualElement root, string containerName, GameObject[] items)
        {
            var container = root.Q<VisualElement>(containerName);
            if (container == null) return;

            container.Clear();

            if (items == null || items.Length == 0) return;

            int maxShow = Mathf.Min(items.Length, 4);
            for (int i = 0; i < maxShow; i++)
            {
                if (items[i] == null) continue;
                var preview = AssetPreview.GetAssetPreview(items[i]);
                if (preview != null)
                {
                    var img = new Image { image = preview };
                    img.AddToClassList("preview-thumb");
                    container.Add(img);
                }
            }

            if (items.Length > maxShow)
            {
                var label = new Label($"+{items.Length - maxShow} more...");
                label.AddToClassList("preview-more");
                container.Add(label);
            }
        }

        #endregion
    }
}
