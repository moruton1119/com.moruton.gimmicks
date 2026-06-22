using System;
using UnityEditor;
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
        private const string UssPath = "Packages/com.moruton.gimmicks/Editor/Avatars/Metamorphose/MetamorphoseEditor.uss";

        private readonly string[] languageCodes = { "ja", "en", "ko", "it", "es" };

        private VisualElement _root;
        private int _selectedLanguage;
        private Color _lastGimmickColor;

        // Localization helpers
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

            // ギミック色の変更を監視
            var script = (PrettyCureMirror)target;
            if (script.gimmickColor != _lastGimmickColor)
            {
                _lastGimmickColor = script.gimmickColor;
                MetamorphoseSetupService.ApplyGimmickColor(script);
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();

            // UXML/USS 読み込み
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            if (visualTree == null)
            {
                _root.Add(new Label($"Error: Could not load {UxmlPath}"));
                return _root;
            }
            var clone = visualTree.CloneTree();
            _root.Add(clone);

            // USS（UXMLのStyle srcからも読まれるが、念のため）
            // var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath);
            // if (styleSheet != null) clone.styleSheets.Add(styleSheet);

            // SerializedObject バインディング
            clone.Bind(serializedObject);

            // Header (IMGUI wrapped)
            var headerContainer = clone.Q<VisualElement>("header-container");
            headerContainer.Add(new IMGUIContainer(() => MorutonAvatarPackageEditorHelper.DrawHeader()));

            // ローカライゼーション適用
            ApplyLocalization(clone);

            // ステップトグル
            SetupStepToggles(clone);

            // ボタン コールバック
            SetupButtonCallbacks(clone);

            // 初期プレビュー
            UpdateAllPreviews(clone);

            return _root;
        }

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
            SetPropLabel(root, "pf-avatar", L("step1_avatar"));
            SetPropLabel(root, "pf-model", L("step1_model"));
            SetPropLabel(root, "pf-animator", L("step1_animator"));
            root.Q<Label>("step1-off-label").text = L("step1_before_clothes_label");

            // Step 2
            root.Q<Label>("step2-desc").text = L("step2_description");
            root.Q<Label>("step2-unpack-label").text = L("step2_unpack_title");
            root.Q<HelpBox>("step2-unpack-help").text = L("step2_unpack_help");
            root.Q<Button>("btn-unpack").text = L("step2_unpack_button");
            root.Q<Label>("step2-parts-label").text = L("step2_parts_title");
            root.Q<HelpBox>("step2-parts-help").text = L("step2_parts_help");

            root.Q<Label>("label-head-target").text = L("step2_head_target");
            root.Q<Label>("label-head-items").text = L("step2_head_items");
            root.Q<Label>("label-body-target").text = L("step2_body_target");
            root.Q<Label>("label-body-items").text = L("step2_body_items");
            root.Q<Label>("label-hand-target").text = L("step2_hand_target");
            root.Q<Label>("label-hand-items").text = L("step2_hand_items");
            root.Q<Label>("label-leg-target").text = L("step2_leg_target");
            root.Q<Label>("label-leg-items").text = L("step2_leg_items");

            // Step 3
            root.Q<Label>("step3-desc").text = L("step3_description");
            SetPropLabel(root, "pf-gimmickColor", L("step3_color_label"));

            // Step 4
            root.Q<Label>("step4-desc").text = L("step4_description");
            root.Q<Button>("btn-colabo-shop").text = LC("colabo_shop_button");
            root.Q<HelpBox>("step4-colabo-help").text = LC("colabo_info");
            root.Q<Label>("step4-onepiece-label").text = L("step4_onepiece_title");
            SetPropLabel(root, "pf-onePiece", L("step4_onepiece_label"));
            SetPropLabel(root, "pf-colaboFBX", "FBX");
            root.Q<Label>("step4-additem-label").text = L("step4_additional_item_title");
            SetPropLabel(root, "pf-colaboItemTarget", L("step4_additional_item_target"));
            SetPropLabel(root, "pf-colaboItem", L("step4_additional_item"));
            root.Q<Label>("step4-fade-label").text = L("step4_fade_fx_title");

            // Setup Button
            root.Q<Button>("btn-setup").text = LC("setup_button");
        }

        private void SetPropLabel(VisualElement root, string name, string label)
        {
            var pf = root.Q<PropertyField>(name);
            if (pf != null) pf.label = label;
        }

        private void ReloadAndApplyLocalization()
        {
            LocalizationManager.Load("PrettyCureMirror", languageCodes[_selectedLanguage]);
            var clone = _root?.Q<VisualElement>("root");
            if (clone != null) ApplyLocalization(clone);
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

            // Unpack
            root.Q<Button>("btn-unpack").clicked += () =>
            {
                var itemToUnpack = serializedObject.FindProperty("itemToUnpack").objectReferenceValue as GameObject;
                MetamorphoseSetupService.UnpackPrefab(itemToUnpack);
            };

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

            // Developer
            root.Q<Button>("btn-gen-anim").clicked += () =>
            {
                MetamorphoseSetupService.GenerateAnimations((PrettyCureMirror)target);
            };

            root.Q<Button>("btn-reprocess").clicked += () =>
            {
                MetamorphoseSetupService.ExecuteFullSetup((PrettyCureMirror)target);
                serializedObject.Update();
                Repaint();
            };
        }

        private void SwitchLanguage(int index)
        {
            if (_selectedLanguage == index) return;
            _selectedLanguage = index;
            EditorPrefs.SetInt("MetamorphoseEditor_Language", _selectedLanguage);

            // ボタンUI更新
            for (int i = 0; i < languageCodes.Length; i++)
            {
                var btn = _root.Q<Button>($"lang-btn-{i}");
                if (btn != null) btn.EnableInClassList("selected", i == index);
            }

            // テキスト更新
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
