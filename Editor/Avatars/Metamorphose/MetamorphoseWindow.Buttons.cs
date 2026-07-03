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
