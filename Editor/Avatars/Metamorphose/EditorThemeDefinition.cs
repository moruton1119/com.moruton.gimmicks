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
            openingBgCenter = new Color(1f, 0.965f, 0.976f, 1f), // #fef7f9 中心はパっと明るいピンク白
            openingBgEdge = new Color(0.910f, 0.706f, 0.780f, 1f),    // #e8b4c7 外縁ははっきり濃いピンク
            openingParticleColor = new Color(0.910f, 0.118f, 0.388f, 1f), // #e91e63
            openingGlowColor = new Color(1f, 0.753f, 0.827f, 0.5f), // #ffc0d3 明るいピンクグロー
            openingCircleColor = new Color(1f, 1f, 1f, 0.95f),   // #ffffff 中心の丸も白く光る
            openingCircleBorder = new Color(0.910f, 0.118f, 0.388f, 0.8f),
            openingTitleColor = new Color(0.447f, 0.125f, 0.290f, 1f), // #72204a
            openingTitleGlow = new Color(1f, 0.753f, 0.827f, 0.7f), // #ffc0d3 明るいグロー
            openingSparkleColor = new Color(1f, 0.753f, 0.827f, 1f), // #ffc0d3
        };

        // ═══════════════════════════════════════════
        //  Cyber（サイバーパンク）テーマ
        // ═══════════════════════════════════════════
        public static EditorThemeDefinition Cyber => new EditorThemeDefinition
        {
            id = "Cyber",
            displayName = "Cyber",
            ussClassName = "theme-cyber",

            // ── UI色: シアン × マゼンダ × ダーク ──
            windowBg = new Color(0.039f, 0.039f, 0.059f, 1f),       // #0a0a0f
            windowText = new Color(0.376f, 0.929f, 0.937f, 1f),     // #60edef シアン
            panelBg = new Color(0.059f, 0.059f, 0.082f, 1f),        // #0f0f15
            sidebarBg = new Color(0.020f, 0.020f, 0.035f, 1f),      // #050509
            topbarBg = new Color(0.059f, 0.059f, 0.082f, 1f),       // #0f0f15
            elevatedBg = new Color(0.078f, 0.078f, 0.110f, 1f),     // #14141c
            bannerBg = new Color(0.020f, 0.020f, 0.035f, 1f),      // #050509
            inputBg = new Color(0.047f, 0.047f, 0.067f, 1f),        // #0c0c11
            hoverBg = new Color(0.110f, 0.110f, 0.149f, 1f),        // #1c1c26
            accent = new Color(0.000f, 0.882f, 0.961f, 1f),         // #00e1f5 ネオンシアン
            accentHover = new Color(0.000f, 0.741f, 0.831f, 1f),    // #00b8d4
            border = new Color(0.000f, 0.443f, 0.502f, 1f),         // #007180
            textSecondary = new Color(0.882f, 0.149f, 0.882f, 1f),  // #e026e0 ネオンマゼンダ
            textDim = new Color(0.502f, 0.502f, 0.643f, 1f),        // #8080a4
            helpBoxBg = new Color(0.059f, 0.039f, 0.078f, 1f),     // #0f0a14

            // ── オープニング演出色 ──
            openingBgCenter = new Color(0.110f, 0.110f, 0.180f, 1f), // #1c1c2e
            openingBgEdge = new Color(0.012f, 0.012f, 0.024f, 1f),   // #030309
            openingParticleColor = new Color(0.000f, 0.882f, 0.961f, 1f),
            openingGlowColor = new Color(0.882f, 0.149f, 0.882f, 0.5f),
            openingCircleColor = new Color(0.039f, 0.078f, 0.110f, 0.9f),
            openingCircleBorder = new Color(0.000f, 0.882f, 0.961f, 0.8f),
            openingTitleColor = new Color(0.000f, 0.882f, 0.961f, 1f),
            openingTitleGlow = new Color(0.882f, 0.149f, 0.882f, 0.7f),
            openingSparkleColor = new Color(0.000f, 0.882f, 0.961f, 1f),
        };

        // ═══════════════════════════════════════════
        //  Wizard（魔法使い）テーマ
        // ═══════════════════════════════════════════
        public static EditorThemeDefinition Wizard => new EditorThemeDefinition
        {
            id = "Wizard",
            displayName = "Wizard",
            ussClassName = "theme-wizard",

            // ── UI色: 水色 × シルバー × アイスブルー ──
            windowBg = new Color(0.090f, 0.137f, 0.165f, 1f),       // #17231a...いや #17232a 深い青緑
            windowText = new Color(0.831f, 0.929f, 0.965f, 1f),     // #d4edf6 氷色文字
            panelBg = new Color(0.110f, 0.157f, 0.188f, 1f),        // #1c2830
            sidebarBg = new Color(0.063f, 0.102f, 0.129f, 1f),      // #101a21
            topbarBg = new Color(0.110f, 0.157f, 0.188f, 1f),       // #1c2830
            elevatedBg = new Color(0.141f, 0.192f, 0.224f, 1f),     // #243139 カード
            bannerBg = new Color(0.063f, 0.102f, 0.129f, 1f),      // #101a21
            inputBg = new Color(0.082f, 0.125f, 0.153f, 1f),        // #152027
            hoverBg = new Color(0.176f, 0.235f, 0.271f, 1f),        // #2d3c45
            accent = new Color(0.000f, 0.690f, 0.937f, 1f),         // #00b0ef 水色
            accentHover = new Color(0.000f, 0.580f, 0.831f, 1f),    // #0094d4
            border = new Color(0.000f, 0.345f, 0.467f, 1f),         // #005877
            textSecondary = new Color(0.400f, 0.737f, 0.816f, 1f),  // #66bcd0
            textDim = new Color(0.502f, 0.620f, 0.690f, 1f),        // #809eb0
            helpBoxBg = new Color(0.078f, 0.118f, 0.145f, 1f),     // #141e25

            // ── オープニング演出色: 氷の魔法 ──
            openingBgCenter = new Color(0.180f, 0.247f, 0.290f, 1f), // #2e3f4a 中心は明るい青
            openingBgEdge = new Color(0.031f, 0.059f, 0.078f, 1f),   // #080f14 外縁は暗い
            openingParticleColor = new Color(0.000f, 0.690f, 0.937f, 1f), // 水色粒子
            openingGlowColor = new Color(0.400f, 0.737f, 0.816f, 0.5f),    // 氷グロー
            openingCircleColor = new Color(0.110f, 0.176f, 0.208f, 0.9f),
            openingCircleBorder = new Color(0.000f, 0.690f, 0.937f, 0.8f),
            openingTitleColor = new Color(0.831f, 0.929f, 0.965f, 1f),     // 氷色
            openingTitleGlow = new Color(0.000f, 0.690f, 0.937f, 0.7f),   // 水色グロー
            openingSparkleColor = new Color(0.749f, 0.882f, 0.957f, 1f),  // #bfe1f4
        };
    }
}
