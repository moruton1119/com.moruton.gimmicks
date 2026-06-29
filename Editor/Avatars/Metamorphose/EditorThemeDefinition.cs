using UnityEngine;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// エディターテーマの定義。
    /// UI色もオープニング演出色も、全部この1構造体に集約。
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

        // ═══════════════════════════════════════════
        //  オープニング演出色 — UI色と同じ場所で管理
        // ═══════════════════════════════════════════
        public Color openingBgCenter;       // 演出背景の中心色
        public Color openingBgEdge;         // 演出背景の外縁色
        public Color openingParticleColor;  // 粒子の色
        public Color openingGlowColor;      // グロー色
        public Color openingCircleColor;    // 中央の丸の色
        public Color openingCircleBorder;   // 中央の丸の枠色
        public Color openingTitleColor;     // タイトル文字色
        public Color openingTitleGlow;      // タイトルグロー色
        public Color openingSparkleColor;   // 放射状キラキラの色

        /// <summary>
        /// Moonlight（ダーク・魔法少女）テーマ
        /// </summary>
        public static EditorThemeDefinition Moonlight => new EditorThemeDefinition
        {
            id = "Moonlight",
            displayName = "🌙 Moonlight (Dark)",
            ussClassName = "theme-moonlight",

            // ── UI色 ──
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

            // ── オープニング演出色 ──
            openingBgCenter = new Color(0.12f, 0.06f, 0.22f, 1f),
            openingBgEdge = new Color(0.03f, 0.01f, 0.06f, 1f),
            openingParticleColor = new Color(1f, 0.42f, 0.62f, 1f),
            openingGlowColor = new Color(0.77f, 0.40f, 1f, 0.6f),
            openingCircleColor = new Color(0.14f, 0.08f, 0.24f, 0.9f),
            openingCircleBorder = new Color(1f, 0.42f, 0.62f, 0.8f),
            openingTitleColor = new Color(1f, 0.85f, 1f, 1f),
            openingTitleGlow = new Color(1f, 0.42f, 0.62f, 0.8f),
            openingSparkleColor = new Color(1f, 0.85f, 1f, 1f),
        };

        /// <summary>
        /// Daylight（ライト・魔法少女）テーマ
        /// </summary>
        public static EditorThemeDefinition Daylight => new EditorThemeDefinition
        {
            id = "Daylight",
            displayName = "☀️ Daylight (Light)",
            ussClassName = "theme-daylight",

            // ── UI色: 白基底 × 落ち着いたピンク ──
            windowBg = new Color(1f, 1f, 1f, 1f),
            windowText = new Color(0.25f, 0.231f, 0.227f, 1f),
            panelBg = new Color(0.984f, 0.976f, 0.976f, 1f),
            sidebarBg = new Color(0.965f, 0.953f, 0.957f, 1f),
            topbarBg = new Color(0.984f, 0.976f, 0.976f, 1f),
            elevatedBg = new Color(1f, 1f, 1f, 1f),
            bannerBg = new Color(0.965f, 0.953f, 0.957f, 1f),
            inputBg = new Color(0.988f, 0.984f, 0.984f, 1f),
            hoverBg = new Color(0.965f, 0.953f, 0.957f, 1f),
            accent = new Color(0.847f, 0.525f, 0.608f, 1f),
            accentHover = new Color(0.788f, 0.455f, 0.545f, 1f),
            border = new Color(0.902f, 0.882f, 0.882f, 1f),
            textSecondary = new Color(0.549f, 0.486f, 0.549f, 1f),
            textDim = new Color(0.706f, 0.667f, 0.706f, 1f),
            helpBoxBg = new Color(0.976f, 0.965f, 0.969f, 1f),

            // ── オープニング演出色 ──
            openingBgCenter = new Color(1f, 0.953f, 0.965f, 1f),
            openingBgEdge = new Color(0.965f, 0.902f, 0.929f, 1f),
            openingParticleColor = new Color(0.847f, 0.525f, 0.608f, 1f),
            openingGlowColor = new Color(0.847f, 0.525f, 0.608f, 0.5f),
            openingCircleColor = new Color(1f, 0.976f, 0.984f, 0.95f),
            openingCircleBorder = new Color(0.847f, 0.525f, 0.608f, 0.8f),
            openingTitleColor = new Color(0.4f, 0.235f, 0.290f, 1f),
            openingTitleGlow = new Color(0.847f, 0.525f, 0.608f, 0.6f),
            openingSparkleColor = new Color(0.847f, 0.525f, 0.608f, 1f),
        };

        // ═══════════════════════════════════════════
        //  今後追加するテーマのテンプレート：
        //
        //  public static EditorThemeDefinition Cyber => new EditorThemeDefinition
        //  {
        //      id = "Cyber",
        //      displayName = "🌃 Cyber",
        //      ussClassName = "theme-cyber",
        //      windowBg = ...,
        //      openingBgCenter = ...,
        //      ...（全色フィールドを埋める）
        //  };
        //
        //  → EditorThemeRegistry.RegisterAll() に1行追加するだけ
        // ═══════════════════════════════════════════
    }
}
