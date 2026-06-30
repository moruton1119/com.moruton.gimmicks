using UnityEditor;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    // Theme制御・オープニング演出
    public partial class MetamorphoseWindow
    {
        private MagicalOpeningEffect _openingEffect;

        #region Theme

        /// <summary>
        /// PrefabのThemeSettingに従って実際のテーマを決定する。
        /// Autoの場合はEditorPrefs（直前の手動切替）を使用。
        /// </summary>
        private void ResolveTheme()
        {
            if (_target == null) return;

            string prefabTheme = _target.ThemeSetting;
            if (!string.IsNullOrEmpty(prefabTheme))
            {
                _currentThemeId = prefabTheme;
            }
            else
            {
                _currentThemeId = EditorPrefs.GetString("MetamorphoseEditor_ThemeId", "Moonlight");
            }
        }

        private EditorThemeDefinition GetCurrentTheme()
        {
            return EditorThemeRegistry.GetTheme(_currentThemeId);
        }

        /// <summary>
        /// テーマを適用する。
        /// 色の制御は全てUSS（CSS変数）に任せる。
        /// C#側は「テーマクラスの付与」だけ行う。
        /// </summary>
        private void ApplyTheme()
        {
            if (_root == null) return;

            var theme = GetCurrentTheme();

            var appElement = _root.Q<VisualElement>("app");

            // 全テーマクラスを削除
            foreach (var t in EditorThemeRegistry.Themes)
            {
                _root.RemoveFromClassList(t.ussClassName);
                rootVisualElement.RemoveFromClassList(t.ussClassName);
                if (appElement != null) appElement.RemoveFromClassList(t.ussClassName);
            }

            // 現在のテーマクラスを付与（USSのCSS変数が切り替わる）
            _root.AddToClassList(theme.ussClassName);
            rootVisualElement.AddToClassList(theme.ussClassName);
            if (appElement != null) appElement.AddToClassList(theme.ussClassName);
        }

        #endregion

        #region Opening Animation

        /// <summary>
        /// ウィンドウを開いた時のオープニング演出。
        /// </summary>
        private void PlayOpeningAnimation()
        {
            if (_root == null || _hasPlayedOpening) return;
            _hasPlayedOpening = true;

            var theme = GetCurrentTheme();
            _openingEffect = new MagicalOpeningEffect(
                MagicalOpeningEffect.FromDefinition(theme),
                onComplete: () =>
                {
                    _openingEffect = null;
                });

            _openingEffect.AddTitleLabel();
            _root.Add(_openingEffect);

            EditorApplication.delayCall += () =>
            {
                _openingEffect?.Play();
            };
        }

        #endregion
    }
}
