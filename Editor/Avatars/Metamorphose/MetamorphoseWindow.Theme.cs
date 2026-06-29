using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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
            if (!string.IsNullOrEmpty(prefabTheme) && prefabTheme != "Auto")
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

        private void ApplyTheme()
        {
            if (_root == null) return;

            var theme = GetCurrentTheme();
            bool isDaylight = theme.id == "Daylight";

            // ★ テーマクラスを付与（USSの細かい styling 用）
            var appElement = _root.Q<VisualElement>("app");

            foreach (var t in EditorThemeRegistry.Themes)
            {
                _root.RemoveFromClassList(t.ussClassName);
                rootVisualElement.RemoveFromClassList(t.ussClassName);
                if (appElement != null) appElement.RemoveFromClassList(t.ussClassName);
            }

            _root.AddToClassList(theme.ussClassName);
            rootVisualElement.AddToClassList(theme.ussClassName);
            if (appElement != null) appElement.AddToClassList(theme.ussClassName);

            _root.EnableInClassList("light-theme", isDaylight);
            rootVisualElement.EnableInClassList("light-theme", isDaylight);

            // ★ 色は全部 EditorThemeDefinition から読む（唯一のソース）
            _root.style.backgroundColor = theme.windowBg;
            _root.style.color = theme.windowText;

            if (appElement != null)
            {
                appElement.style.backgroundColor = theme.windowBg;
                appElement.style.color = theme.windowText;
            }

            SetBg(_root, "content-panel", theme.panelBg);
            SetBg(_root, "topbar", theme.topbarBg);
            SetBg(_root, "sidebar", theme.sidebarBg);
            SetBg(_root, "pages-container", theme.panelBg);
            SetBg(_root, "banner", theme.bannerBg);

            var cards = _root.Query<VisualElement>(className: "part-card").ToList();
            foreach (var card in cards)
                card.style.backgroundColor = theme.elevatedBg;

            var groups = _root.Query<VisualElement>(className: "field-group").ToList();
            foreach (var g in groups)
                g.style.backgroundColor = theme.elevatedBg;

            var toggle = _root.Q<Button>("theme-toggle");
            if (toggle != null)
                toggle.text = isDaylight ? "☀️" : "🌙";

            // ★ Unity標準要素の色をC#から直接上書き（USS cascadeの限界を回避）
            OverrideUnityBuiltInColors(_root, theme);
        }

        /// <summary>
        /// Unity標準のHelpBox, TextField, ObjectField, Toggle等の色を
        /// C#インラインスタイルで強制的にテーマに合わせる。
        /// USS cascadeだけではUnityビルトインUSSに負けるため。
        /// </summary>
        private void OverrideUnityBuiltInColors(VisualElement root, EditorThemeDefinition theme)
        {
            // HelpBox — Unity標準の背景が暗いので明示的に上書き
            var helpBoxes = root.Query<HelpBox>().ToList();
            foreach (var hb in helpBoxes)
            {
                hb.style.backgroundColor = theme.helpBoxBg;
                hb.style.color = theme.textSecondary;
                hb.style.borderLeftWidth = 3f;
                hb.style.borderLeftColor = theme.accent;
                hb.style.borderTopWidth = 0f;
                hb.style.borderRightWidth = 0f;
                hb.style.borderBottomWidth = 0f;
                hb.style.paddingLeft = 8f;
                hb.style.paddingTop = 6f;
                hb.style.paddingBottom = 6f;
                hb.style.paddingRight = 8f;
                hb.style.marginTop = 4f;
                hb.style.marginBottom = 10f;
                hb.style.borderTopLeftRadius = 4f;
                hb.style.borderTopRightRadius = 4f;
                hb.style.borderBottomLeftRadius = 4f;
                hb.style.borderBottomRightRadius = 4f;
            }

            // TextField / ObjectField の入力欄
            var inputs = root.Query(className: "unity-base-field__input").ToList();
            foreach (var input in inputs)
            {
                input.style.backgroundColor = theme.inputBg;
                input.style.borderBottomColor = theme.border;
                input.style.borderTopColor = theme.border;
                input.style.borderLeftColor = theme.border;
                input.style.borderRightColor = theme.border;
            }

            // PropertyField のラベル
            var labels = root.Query(className: "unity-property-field__label").ToList();
            foreach (var label in labels)
            {
                label.style.color = theme.textSecondary;
            }

            // Foldout ヘッダー
            var foldouts = root.Query<Foldout>().ToList();
            foreach (var foldout in foldouts)
            {
                foldout.style.color = theme.windowText;
            }

            // ScrollView の背景
            var scrollViews = root.Query<ScrollView>().ToList();
            foreach (var sv in scrollViews)
            {
                sv.style.backgroundColor = new Color(0, 0, 0, 0);
            }

            // 配列・リスト要素（USS cascadeが届かないことがあるのでC#でも確実に）
            var listItems = root.Query(className: "unity-list-view__reorderable-item").ToList();
            foreach (var item in listItems)
            {
                item.style.backgroundColor = theme.elevatedBg;
                item.style.borderTopColor = theme.border;
                item.style.borderBottomColor = theme.border;
                item.style.borderLeftColor = theme.border;
                item.style.borderRightColor = theme.border;
            }

            // 配列要素のドラッグハンドル
            var dragHandles = root.Query(className: "unity-list-view__reorderable-item__drag-handle").ToList();
            foreach (var handle in dragHandles)
            {
                handle.style.backgroundColor = theme.border;
            }

            // リスト全体の背景
            var listViews = root.Query<ListView>().ToList();
            foreach (var lv in listViews)
            {
                lv.style.backgroundColor = theme.inputBg;
            }
        }

        private void SetBg(VisualElement root, string elementName, Color color)
        {
            var el = root.Q<VisualElement>(elementName);
            if (el != null) el.style.backgroundColor = color;
        }

        private void ToggleTheme()
        {
            _currentThemeId = _currentThemeId == "Daylight" ? "Moonlight" : "Daylight";
            EditorPrefs.SetString("MetamorphoseEditor_ThemeId", _currentThemeId);

            if (_target != null)
            {
                Undo.RecordObject(_target, "Change Editor Theme");
                _so.Update();
                var prop = _so.FindProperty("editorTheme");
                if (prop != null)
                {
                    prop.stringValue = _currentThemeId;
                    _so.ApplyModifiedProperties();
                }
            }

            ApplyTheme();
        }

        #endregion

        #region Opening Animation

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
            var theme = GetCurrentTheme();
            _openingEffect = new MagicalOpeningEffect(
                MagicalOpeningEffect.FromDefinition(theme),
                onComplete: () =>
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
    }
}
