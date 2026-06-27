using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// エディターテーマの定義。
    /// 新しいテーマを追加する時はこの構造体を量産するだけ。
    /// </summary>
    [System.Serializable]
    public struct EditorThemeDefinition
    {
        [Tooltip("テーマの識別子（半角英数）")]
        public string id;

        [Tooltip("DevページのDropdownに表示される名前")]
        public string displayName;

        [Tooltip("USSクラス名（例: theme-moonlight）")]
        public string ussClassName;

        [Tooltip("オープニング演出の色")]
        public MagicalOpeningEffect.ThemePalette openingPalette;

        [Tooltip("ウィンドウの背景色")]
        public Color windowBackground;

        [Tooltip("ウィンドウの文字色")]
        public Color windowText;

        /// <summary>
        /// Moonlight（ダーク・魔法少女）テーマ
        /// </summary>
        public static EditorThemeDefinition Moonlight => new EditorThemeDefinition
        {
            id = "Moonlight",
            displayName = "🌙 Moonlight (Dark)",
            ussClassName = "theme-moonlight",
            openingPalette = MagicalOpeningEffect.ThemePalette.Moonlight,
            windowBackground = new Color(0.102f, 0.055f, 0.180f, 1f),
            windowText = new Color(0.941f, 0.902f, 1f, 1f),
        };

        /// <summary>
        /// Daylight（ライト・魔法少女）テーマ
        /// </summary>
        public static EditorThemeDefinition Daylight => new EditorThemeDefinition
        {
            id = "Daylight",
            displayName = "☀️ Daylight (Light)",
            ussClassName = "theme-daylight",
            openingPalette = MagicalOpeningEffect.ThemePalette.Daylight,
            windowBackground = new Color(1f, 0.94f, 0.96f, 1f),
            windowText = new Color(0.545f, 0.412f, 0.078f, 1f),
        };

        // ═══════════════════════════════════════════
        //  今後追加するテーマのテンプレート：
        //
        //  public static EditorThemeDefinition Cyber => new EditorThemeDefinition
        //  {
        //      id = "Cyber",
        //      displayName = "🌃 Cyber",
        //      ussClassName = "theme-cyber",
        //      openingPalette = new MagicalOpeningEffect.ThemePalette { ... },
        //      windowBackground = new Color(0f, 0f, 0.1f, 1f),
        //      windowText = new Color(0f, 1f, 1f, 1f),
        //  };
        //
        //  そして Theme_Cyber.uss を作成して .theme-cyber の色を定義する
        // ═══════════════════════════════════════════
    }
}
