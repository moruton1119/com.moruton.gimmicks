using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    // 多言語対応
    public partial class MetamorphoseWindow
    {
        #region Localization

        private void ApplyLocalization(VisualElement root)
        {
            var langNames = LocalizationManager.SupportedLanguageNames;
            for (int i = 0; i < langNames.Length; i++)
            {
                var btn = root.Q<Button>($"lang-btn-{i}");
                if (btn != null) btn.text = langNames[i];
            }

            // メインページのラベル
            root.Q<Label>("fg-off-title").text = L("step1_before_clothes_label");
            root.Q<Label>("fg-after-title").text = L("step2_parts_title");
            root.Q<HelpBox>("page1-help").text = L("step2_parts_help");
            root.Q<Label>("label-head").text = L("step2_head_items");
            root.Q<Label>("label-body").text = L("step2_body_items");
            root.Q<Label>("label-hand").text = L("step2_hand_items");
            root.Q<Label>("label-leg").text = L("step2_leg_items");
            root.Q<Label>("fg-color-title").text = L("step3_color_label");
            root.Q<Label>("fg-avatar-title").text = L("step1_avatar_label");
            SetPropLabel(root, "page0-slot-avatar", L("step1_avatar"));

            // コラボページのラベル
            root.Q<Label>("fg-colabo-title").text = L("step4_description");
            var colaboShopBtn = root.Q<Button>("btn-colabo-shop");
            if (colaboShopBtn != null) colaboShopBtn.text = LC("colabo_shop_button");
            var colaboHelp = root.Q<HelpBox>("colabo-help");
            if (colaboHelp != null) colaboHelp.text = LC("colabo_info");
            root.Q<Label>("fg-fade-title").text = L("step4_fade_fx_title");

            // Dev ページのラベル
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
    }
}
