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

        // ═══════════════════════════════════════════
        //  UI色定義 — 全テーマ色をここに集約
        // ═══════════════════════════════════════════
        public Color windowBg;       // _root / app の背景
        public Color windowText;     // メイン文字色
        public Color panelBg;        // content-panel, pages-container
        public Color sidebarBg;      // sidebar
        public Color topbarBg;       // topbar
        public Color elevatedBg;     // part-card, field-group
        public Color bannerBg;       // banner
        public Color inputBg;        // 入力欄
        public Color hoverBg;        // ホバー状態
        public Color accent;         // アクセント色
        public Color accentHover;    // アクセント ホバー
        public Color border;         // 境界線
        public Color textSecondary;  // 補助文字色
        public Color textDim;        // 薄い文字色

        /// <summary>
        /// Moonlight（ダーク・魔法少女）テーマ
        /// </summary>
        public static EditorThemeDefinition Moonlight => new EditorThemeDefinition
        {
            id = "Moonlight",
            displayName = "🌙 Moonlight (Dark)",
            ussClassName = "theme-moonlight",
            openingPalette = MagicalOpeningEffect.ThemePalette.Moonlight,

            windowBg = new Color(0.102f, 0.055f, 0.180f, 1f),
            windowText = new Color(0.941f, 0.902f, 1f, 1f),
            panelBg = new Color(0.14f, 0.085f, 0.22f, 1f),
            sidebarBg = new Color(0.075f, 0.035f, 0.12f, 1f),
            topbarBg = new Color(0.14f, 0.085f, 0.22f, 1f),
            elevatedBg = new Color(0.14f, 0.085f, 0.22f, 1f),
            bannerBg = new Color(0.06f, 0.027f, 0.10f, 1f),
            inputBg = new Color(0.11f, 0.067f, 0.19f, 1f),
            hoverBg = new Color(0.18f, 0.114f, 0.29f, 1f),
            accent = new Color(1f, 0.42f, 0.62f, 1f),
            accentHover = new Color(1f, 0.56f, 0.72f, 1f),
            border = new Color(0.227f, 0.149f, 0.345f, 1f),
            textSecondary = new Color(0.769f, 0.722f, 0.878f, 1f),
            textDim = new Color(0.478f, 0.416f, 0.62f, 1f),
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

            windowBg = new Color(1f, 0.941f, 0.961f, 1f),
            windowText = new Color(0.290f, 0.188f, 0.251f, 1f),
            panelBg = new Color(1f, 0.882f, 0.929f, 1f),
            sidebarBg = new Color(1f, 0.839f, 0.894f, 1f),
            topbarBg = new Color(1f, 0.882f, 0.929f, 1f),
            elevatedBg = new Color(1f, 1f, 1f, 1f),
            bannerBg = new Color(1f, 0.882f, 0.929f, 1f),
            inputBg = new Color(1f, 0.965f, 0.976f, 1f),
            hoverBg = new Color(0.992f, 0.941f, 0.961f, 1f),
            accent = new Color(0.831f, 0.627f, 0.090f, 1f),
            accentHover = new Color(0.941f, 0.690f, 0.125f, 1f),
            border = new Color(0.941f, 0.831f, 0.886f, 1f),
            textSecondary = new Color(0.427f, 0.298f, 0.490f, 1f),
            textDim = new Color(0.620f, 0.494f, 0.682f, 1f),
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
        //      windowBg = ...,
        //      windowText = ...,
        //      ...（全色フィールドを埋める）
        //  };
        //
        //  → EditorThemeRegistry.RegisterAll() に1行追加するだけ
        // ═══════════════════════════════════════════
    }
}
