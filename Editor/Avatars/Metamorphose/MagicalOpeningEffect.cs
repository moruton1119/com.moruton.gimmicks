using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 魔法少女風オープニング演出エレメント。
    /// グラデーション背景 + 浮遊粒子 + グロータイトル + バイネット。
    /// generateVisualContent で直接メッシュ描画する（CSSの限界を超える）。
    /// </summary>
    public class MagicalOpeningEffect : VisualElement
    {
        // ═══════════════════════════════════════════
        //  テーマ別カラー
        // ═══════════════════════════════════════════
        public struct ThemePalette
        {
            public Color bgCenter;
            public Color bgEdge;
            public Color particleColor;
            public Color glowColor;
            public Color titleColor;
            public Color titleGlow;
            public string titleText;

            public static ThemePalette Moonlight => new ThemePalette
            {
                bgCenter = new Color(0.12f, 0.06f, 0.22f, 1f),
                bgEdge = new Color(0.03f, 0.01f, 0.06f, 1f),
                particleColor = new Color(1f, 0.42f, 0.62f, 1f),
                glowColor = new Color(0.77f, 0.40f, 1f, 0.6f),
                titleColor = new Color(1f, 0.85f, 1f, 1f),
                titleGlow = new Color(1f, 0.42f, 0.62f, 0.8f),
                titleText = "✦ Metamorphose ✦",
            };

            public static ThemePalette Daylight => new ThemePalette
            {
                bgCenter = new Color(1f, 0.94f, 0.97f, 1f),
                bgEdge = new Color(0.95f, 0.82f, 0.90f, 1f),
                particleColor = new Color(0.91f, 0.12f, 0.39f, 1f),
                glowColor = new Color(1f, 0.66f, 0.15f, 0.5f),
                titleColor = new Color(0.29f, 0.10f, 0.26f, 1f),
                titleGlow = new Color(0.91f, 0.12f, 0.39f, 0.7f),
                titleText = "✦ Metamorphose ✦",
            };
        }

        // ═══════════════════════════════════════════
        //  パーティクル
        // ═══════════════════════════════════════════
        private struct Particle
        {
            public Vector2 position;
            public Vector2 velocity;
            public float size;
            public float baseOpacity;
            public float phase;     // twinkle 用
            public float twinkleSpeed;
        }

        // ═══════════════════════════════════════════
        //  状態
        // ═══════════════════════════════════════════
        private ThemePalette _palette;
        private readonly List<Particle> _particles = new List<Particle>();
        private float _elapsed;
        private float _totalDuration = 2.5f;
        private bool _isPlaying;
        private bool _isDone;
        private System.Action _onComplete;

        // アニメーションフェーズ
        private const float PhaseFadeIn = 0.4f;
        private const float PhaseHold = 1.2f;
        // 残りがフェードアウト

        // ═══════════════════════════════════════════
        //  初期化
        // ═══════════════════════════════════════════
        public MagicalOpeningEffect(bool isLightTheme, System.Action onComplete = null)
        {
            _palette = isLightTheme ? ThemePalette.Daylight : ThemePalette.Moonlight;
            _onComplete = onComplete;

            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;
            style.flexGrow = 1;
            pickingMode = PickingMode.Ignore;
            overflow = Overflow.Hidden;

            // generateVisualContent に描画コールバックを登録
            generateVisualContent += OnGenerateVisualContent;

            GenerateParticles(40);
        }

        private void GenerateParticles(int count)
        {
            _particles.Clear();
            var rng = new System.Random();

            for (int i = 0; i < count; i++)
            {
                _particles.Add(new Particle
                {
                    position = new Vector2(
                        (float)rng.NextDouble(),
                        (float)rng.NextDouble()
                    ),
                    velocity = new Vector2(
                        ((float)rng.NextDouble() - 0.5f) * 0.02f,
                        ((float)rng.NextDouble() * 0.5f + 0.3f) * 0.015f
                    ),
                    size = (float)rng.NextDouble() * 3f + 1.5f,
                    baseOpacity = (float)rng.NextDouble() * 0.5f + 0.3f,
                    phase = (float)rng.NextDouble() * Mathf.PI * 2f,
                    twinkleSpeed = (float)rng.NextDouble() * 3f + 1.5f,
                });
            }
        }

        // ═══════════════════════════════════════════
        //  再生制御
        // ═══════════════════════════════════════════
        public void Play()
        {
            if (_isPlaying || _isDone) return;
            _isPlaying = true;
            _elapsed = 0f;
            EditorApplication.update += Tick;
            MarkDirtyRepaint();
        }

        public void Stop()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            EditorApplication.update -= Tick;
            _onComplete?.Invoke();
            RemoveFromHierarchy();
        }

        private void Tick()
        {
            if (!_isPlaying) return;

            _elapsed += (float)EditorApplication.timeDelta;

            // パーティクル更新
            float dt = (float)EditorApplication.timeDelta;
            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                p.position += p.velocity * dt;
                p.phase += p.twinkleSpeed * dt;

                // 上に抜けたら下に戻る
                if (p.position.y > 1.05f)
                {
                    p.position.y = -0.05f;
                    p.position.x = Random.value;
                }
                // 横のラッピング
                if (p.position.x > 1.05f) p.position.x -= 1.1f;
                if (p.position.x < -0.05f) p.position.x += 1.1f;

                _particles[i] = p;
            }

            // 終了判定
            if (_elapsed >= _totalDuration)
            {
                Stop();
                return;
            }

            MarkDirtyRepaint();
        }

        // ═══════════════════════════════════════════
        //  現在のフェーズ取得
        // ═══════════════════════════════════════════
        private (float globalAlpha, float titleScale, float titleAlpha) GetPhaseValues()
        {
            if (_elapsed < PhaseFadeIn)
            {
                float t = _elapsed / PhaseFadeIn;
                // ease-out
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                return (eased, 0.5f + eased * 0.5f, eased);
            }
            else if (_elapsed < PhaseFadeIn + PhaseHold)
            {
                return (1f, 1f, 1f);
            }
            else
            {
                float remaining = _totalDuration - PhaseFadeIn - PhaseHold;
                float t = (_elapsed - PhaseFadeIn - PhaseHold) / remaining;
                // ease-in
                float eased = t * t;
                return (1f - eased, 1f + eased * 0.1f, 1f - eased);
            }
        }

        // ═══════════════════════════════════════════
        //  描画 — generateVisualContent コールバック
        // ═══════════════════════════════════════════
        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var (globalAlpha, _, _) = GetPhaseValues();

            float width = contentRect.width;
            float height = contentRect.height;

            if (width <= 0 || height <= 0) return;

            // ── 1. グラデーション背景 ──
            DrawGradientBackground(ctx, width, height, globalAlpha);

            // ── 2. 中心のグロー ──
            DrawCenterGlow(ctx, width, height, globalAlpha);

            // ── 3. パーティクル ──
            DrawParticles(ctx, width, height, globalAlpha);

            // ── 4. バイネット（四隅が暗い） ──
            DrawVignette(ctx, width, height, globalAlpha);

            // ── 5. タイトル ──
            DrawTitle(ctx, width, height);
        }

        // ── グラデーション背景 ──
        private void DrawGradientBackground(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            var mesh = ctx.Allocate(4, 6);

            // 中心が明るく、外周が暗い
            Color center = _palette.bgCenter;
            Color edge = _palette.bgEdge;
            center.a *= alpha;
            edge.a *= alpha;

            // 4頂点（左下→右下→左上→右上）
            mesh.SetNextVertex(new Vertex { position = new Vector3(0, 0, 0), tint = edge });
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, 0, 0), tint = edge });
            mesh.SetNextVertex(new Vertex { position = new Vector3(0, h, 0), tint = center });
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, h, 0), tint = center });

            mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(2);
            mesh.SetNextIndex(1); mesh.SetNextIndex(3); mesh.SetNextIndex(2);
        }

        // ── 中心のグロー（モヤッとした円） ──
        private void DrawCenterGlow(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float radius = Mathf.Min(w, h) * 0.6f;

            // 複数の重なり合う半透明四角形でグローを表現
            int layers = 5;
            var mesh = ctx.Allocate(4 * layers, 6 * layers);

            for (int i = layers - 1; i >= 0; i--)
            {
                float t = (float)i / layers;
                float r = radius * (1f - t * 0.7f);
                float a = _palette.glowColor.a * alpha * (1f - t) * 0.3f;

                Color c = _palette.glowColor;
                c.a = a;

                mesh.SetNextVertex(new Vertex { position = new Vector3(cx - r, cy - r, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(cx + r, cy - r, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(cx - r, cy + r, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(cx + r, cy + r, 0), tint = c });

                int baseIdx = (layers - 1 - i) * 4;
                mesh.SetNextIndex(baseIdx); mesh.SetNextIndex(baseIdx + 1); mesh.SetNextIndex(baseIdx + 2);
                mesh.SetNextIndex(baseIdx + 1); mesh.SetNextIndex(baseIdx + 3); mesh.SetNextIndex(baseIdx + 2);
            }
        }

        // ── パーティクル（キラキラ） ──
        private void DrawParticles(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            // 各パーティクルを小さい四角形（ダイヤ型）として描画
            // 4頂点×粒子数
            int count = _particles.Count;
            var mesh = ctx.Allocate(4 * count, 6 * count);

            for (int i = 0; i < count; i++)
            {
                var p = _particles[i];

                float twinkle = (Mathf.Sin(p.phase) * 0.5f + 0.5f);
                float opacity = p.baseOpacity * twinkle * alpha;
                float size = p.size;

                // ダイヤ型にするために45度回転
                float px = p.position.x * w;
                float py = p.position.y * h;
                float s = size;

                Color c = _palette.particleColor;
                c.a = opacity;

                // ダイヤ型の4頂点
                mesh.SetNextVertex(new Vertex { position = new Vector3(px, py - s, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(px + s, py, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(px, py + s, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(px - s, py, 0), tint = c });

                int baseIdx = i * 4;
                mesh.SetNextIndex(baseIdx); mesh.SetNextIndex(baseIdx + 1); mesh.SetNextIndex(baseIdx + 2);
                mesh.SetNextIndex(baseIdx); mesh.SetNextIndex(baseIdx + 2); mesh.SetNextIndex(baseIdx + 3);
            }
        }

        // ── バイネット（四隅が暗いオーバーレイ） ──
        private void DrawVignette(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float maxDist = Mathf.Max(w, h) * 0.7f;

            // 8分割の扇形でバイネットを近似（簡易版）
            // 実際は4頂点のグラデーションで代用
            var mesh = ctx.Allocate(4, 6);

            Color transparent = new Color(0, 0, 0, 0);
            Color dark = new Color(0, 0, 0, 0.5f * alpha);

            mesh.SetNextVertex(new Vertex { position = new Vector3(0, 0, 0), tint = dark });
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, 0, 0), tint = dark });
            mesh.SetNextVertex(new Vertex { position = new Vector3(0, h, 0), tint = dark });
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, h, 0), tint = dark });

            // 内側に向かって透明になるグラデーションは簡易的に四隅を暗くする
            // より正確にするには複数メッシュが必要だが、雰囲気重視で簡略化
            mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(2);
            mesh.SetNextIndex(1); mesh.SetNextIndex(3); mesh.SetNextIndex(2);
        }

        // ── タイトルテキスト ──
        private void DrawTitle(MeshGenerationContext ctx, float w, float h)
        {
            var (_, titleScale, titleAlpha) = GetPhaseValues();

            // UIToolkit の描画コンテキストではテキストを直接描画できないので、
            // 子の Label を使う（VisualElement を重ねる方式）
            // → この描画はタイトルの「グロー背景」のみ
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float glowSize = Mathf.Min(w, h) * 0.15f * titleScale;

            var mesh = ctx.Allocate(4, 6);

            Color glow = _palette.titleGlow;
            glow.a *= titleAlpha * 0.4f;

            mesh.SetNextVertex(new Vertex { position = new Vector3(cx - glowSize, cy - glowSize * 0.4f, 0), tint = glow });
            mesh.SetNextVertex(new Vertex { position = new Vector3(cx + glowSize, cy - glowSize * 0.4f, 0), tint = glow });
            mesh.SetNextVertex(new Vertex { position = new Vector3(cx - glowSize, cy + glowSize * 0.4f, 0), tint = glow });
            mesh.SetNextVertex(new Vertex { position = new Vector3(cx + glowSize, cy + glowSize * 0.4f, 0), tint = glow });

            mesh.SetNextIndex(0); mesh.SetNextIndex(1); mesh.SetNextIndex(2);
            mesh.SetNextIndex(1); mesh.SetNextIndex(3); mesh.SetNextIndex(2);
        }

        // ═══════════════════════════════════════════
        //  タイトルラベル（テキストは子要素として配置）
        // ═══════════════════════════════════════════
        public void AddTitleLabel()
        {
            var label = new Label(_palette.titleText);
            label.style.position = Position.Absolute;
            label.style.left = 0;
            label.style.right = 0;
            label.style.top = 0;
            label.style.bottom = 0;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.fontSize = 26;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = _palette.titleColor;

            // アニメーションでopacityを更新するためのコールバック
            label.generateVisualContent += _ =>
            {
                var (_, _, titleAlpha) = GetPhaseValues();
                label.style.opacity = titleAlpha;
            };

            Add(label);
        }

        // ═══════════════════════════════════════════
        //  クリーンアップ
        // ═══════════════════════════════════════════
        public void Cleanup()
        {
            if (_isPlaying)
            {
                _isPlaying = false;
                EditorApplication.update -= Tick;
            }
        }
    }
}
