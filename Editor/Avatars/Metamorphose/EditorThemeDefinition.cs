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
            openingBgCenter = new Color(0.30f, 0.18f, 0.45f, 1f), // #4d2e73 中心は明るい紫
            openingBgEdge = new Color(0.06f, 0.02f, 0.10f, 1f),    // #0f051a 外縁は暗い
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

            // ── UI色: 魔法少女ライト ── パステルピンク × ラベンダー紫
            windowBg = new Color(1f, 0.969f, 0.973f, 1f),       // #fff7f8 ほんのりピンク白
            windowText = new Color(0.290f, 0.098f, 0.259f, 1f),  // #4a1942 魔法少女ピンク紫
            panelBg = new Color(1f, 0.941f, 0.961f, 1f),         // #fff0f5 lavenderblush
            sidebarBg = new Color(1f, 0.894f, 0.929f, 1f),       // #ffe4ed パステルピンク
            topbarBg = new Color(1f, 0.941f, 0.961f, 1f),         // #fff0f5
            elevatedBg = new Color(1f, 1f, 1f, 1f),              // #ffffff 純白カード
            bannerBg = new Color(1f, 0.894f, 0.929f, 1f),       // #ffe4ed
            inputBg = new Color(1f, 0.965f, 0.976f, 1f),         // #fff7f9
            hoverBg = new Color(1f, 0.910f, 0.941f, 1f),         // #ffe8f0 ホバーで少し濃いピンク
            accent = new Color(0.910f, 0.118f, 0.388f, 1f),      // #e91e63 鮮やかローズピンク
            accentHover = new Color(0.757f, 0.094f, 0.353f, 1f),  // #c2185b ディープピンク
            border = new Color(0.973f, 0.733f, 0.816f, 1f),      // #f8bbd0 ピンク系境界線
            textSecondary = new Color(0.380f, 0.149f, 0.290f, 1f), // #61264a 深いピンク紫（視認性UP）
            textDim = new Color(0.451f, 0.275f, 0.380f, 1f),     // #734661 ディープラベンダー（視認性UP）
            helpBoxBg = new Color(0.988f, 0.894f, 0.925f, 1f),   // #fce4ec ピンク50

            // ── オープニング演出色: 中心が明るく光る ──
            openingBgCenter = new Color(0.953f, 0.882f, 0.933f, 1f), // #f3e1ee 中心は明るいラベンダー白
            openingBgEdge = new Color(0.820f, 0.620f, 0.710f, 1f),    // #d19eb5 外縁は濃いピンク
            openingParticleColor = new Color(0.910f, 0.118f, 0.388f, 1f), // #e91e63
            openingGlowColor = new Color(1f, 0.753f, 0.827f, 0.5f), // #ffc0d3 明るいピンクグロー
            openingCircleColor = new Color(1f, 1f, 1f, 0.95f),   // #ffffff 中心の丸も白く光る
            openingCircleBorder = new Color(0.910f, 0.118f, 0.388f, 0.8f),
            openingTitleColor = new Color(0.447f, 0.125f, 0.290f, 1f), // #72204a
            openingTitleGlow = new Color(1f, 0.753f, 0.827f, 0.7f), // #ffc0d3 明るいグロー
            openingSparkleColor = new Color(1f, 0.753f, 0.827f, 1f), // #ffc0d3
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
