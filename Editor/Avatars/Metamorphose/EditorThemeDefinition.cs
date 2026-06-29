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
        public Color helpBoxBg;      // HelpBox背景色

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
            helpBoxBg = new Color(0.16f, 0.10f, 0.25f, 1f),
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

            // ★ Rose-Pink palette — ピンク背景と調和する統一感のある配色
            windowBg = new Color(1f, 0.961f, 0.973f, 1f),       // #fff5f8
            windowText = new Color(0.290f, 0.098f, 0.259f, 1f),  // #4a1942
            panelBg = new Color(1f, 0.894f, 0.929f, 1f),         // #ffe4ec
            sidebarBg = new Color(0.988f, 0.839f, 0.894f, 1f),   // #fce4ec
            topbarBg = new Color(1f, 0.894f, 0.929f, 1f),         // #ffe4ec
            elevatedBg = new Color(1f, 1f, 1f, 1f),              // #ffffff
            bannerBg = new Color(0.988f, 0.839f, 0.894f, 1f),   // #fce4ec
            inputBg = new Color(1f, 0.941f, 0.965f, 1f),         // #fff0f4
            hoverBg = new Color(0.988f, 0.894f, 0.925f, 1f),     // #fce4ec
            accent = new Color(0.910f, 0.118f, 0.388f, 1f),      // #e91e63 (rose-pink)
            accentHover = new Color(0.941f, 0.384f, 0.573f, 1f),  // #f06292
            border = new Color(0.973f, 0.733f, 0.816f, 1f),      // #f8bbd0
            textSecondary = new Color(0.533f, 0.055f, 0.310f, 1f), // #880e4f
            textDim = new Color(0.678f, 0.482f, 0.667f, 1f),     // #ad7baa
            helpBoxBg = new Color(1f, 0.973f, 0.882f, 1f),      // #fff8e1
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
