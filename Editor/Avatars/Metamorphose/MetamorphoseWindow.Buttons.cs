using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    // ボタンコールバック・アニメーション生成
    public partial class MetamorphoseWindow
    {
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
            _root.Q<Button>("btn-x").clicked += () => Application.OpenURL(XUrl);
            _root.Q<Button>("btn-note").clicked += () => Application.OpenURL(NoteUrl);
            _root.Q<Button>("btn-discord").clicked += () => Application.OpenURL(DiscordUrl);

            var colaboShopBtn = _root.Q<Button>("btn-colabo-shop");
            if (colaboShopBtn != null)
            {
                colaboShopBtn.clicked += () =>
                {
                    if (!string.IsNullOrEmpty(_target.colaboShopInfo))
                        Application.OpenURL(_target.colaboShopInfo);
                };
            }

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

            var controller = _target.Animator.runtimeAnimatorController as AnimatorController;
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
    }
}
