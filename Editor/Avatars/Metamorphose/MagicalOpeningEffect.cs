using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 魔法少女風オープニング演出エレメント。
    /// グラデーション背景 + 浮遊粒子 + グロータイトル + バイネット。
    /// generateVisualContent で直接メッシュ描画する。
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
            public float phase;
            public float twinkleSpeed;
        }

        // ═══════════════════════════════════════════
        //  状態
        // ═══════════════════════════════════════════
        private ThemePalette _palette;
        private readonly List<Particle> _particles = new List<Particle>();
        private float _elapsed;
        private float _lastTime;
        private float _totalDuration = 2.5f;
        private bool _isPlaying;
        private System.Action _onComplete;

        private Label _titleLabel;

        // アニメーションフェーズ
        private const float PhaseFadeIn = 0.4f;
        private const float PhaseHold = 1.2f;

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
            // Overflow.Hidden is not available in Unity 2022.3 UIToolkit
            // style.overflowHidden is the equivalent but let's just not set it

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
            if (_isPlaying) return;
            _isPlaying = true;
            _elapsed = 0f;
            _lastTime = Time.realtimeSinceStartup;
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

            // EditorApplication.timeDelta doesn't exist in Unity 2022.3
            // Use Time.realtimeSinceStartup for delta calculation
            float dt = Time.realtimeSinceStartup - _lastTime;
            _lastTime = Time.realtimeSinceStartup;
            _elapsed += dt;

            // パーティクル更新
            for (int i = 0; i < _particles.Count; i++)
            {
                var p = _particles[i];
                p.position += p.velocity * dt;
                p.phase += p.twinkleSpeed * dt;

                if (p.position.y > 1.05f)
                {
                    p.position.y = -0.05f;
                    p.position.x = Random.value;
                }
                if (p.position.x > 1.05f) p.position.x -= 1.1f;
                if (p.position.x < -0.05f) p.position.x += 1.1f;

                _particles[i] = p;
            }

            // タイトルラベルの opacity を更新（generateVisualContent 内ではなくここで）
            if (_titleLabel != null)
            {
                var (_, _, titleAlpha) = GetPhaseValues();
                _titleLabel.style.opacity = titleAlpha;
            }

            if (_elapsed >= _totalDuration)
            {
                Stop();
                return;
            }

            MarkDirtyRepaint();
        }

        // ═══════════════════════════════════════════
        //  フェーズ計算
        // ═══════════════════════════════════════════
        private (float globalAlpha, float titleScale, float titleAlpha) GetPhaseValues()
        {
            if (_elapsed < PhaseFadeIn)
            {
                float t = _elapsed / PhaseFadeIn;
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
                float eased = t * t;
                return (1f - eased, 1f + eased * 0.1f, 1f - eased);
            }
        }

        // ═══════════════════════════════════════════
        //  描画メイン
        // ═══════════════════════════════════════════
        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var (globalAlpha, _, _) = GetPhaseValues();

            float width = contentRect.width;
            float height = contentRect.height;

            if (width <= 0 || height <= 0) return;

            DrawGradientBackground(ctx, width, height, globalAlpha);
            DrawCenterGlow(ctx, width, height, globalAlpha);
            DrawParticles(ctx, width, height, globalAlpha);
            DrawVignette(ctx, width, height, globalAlpha);
            DrawTitleGlow(ctx, width, height);
        }

        // ── グラデーション背景 ──
        private void DrawGradientBackground(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            var mesh = ctx.Allocate(4, 6);

            Color center = _palette.bgCenter;
            Color edge = _palette.bgEdge;
            center.a *= alpha;
            edge.a *= alpha;

            mesh.SetNextVertex(new Vertex { position = new Vector3(0, 0, 0), tint = edge });
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, 0, 0), tint = edge });
            mesh.SetNextVertex(new Vertex { position = new Vector3(0, h, 0), tint = center });
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, h, 0), tint = center });

            mesh.SetNextIndex((ushort)0); mesh.SetNextIndex((ushort)1); mesh.SetNextIndex((ushort)2);
            mesh.SetNextIndex((ushort)1); mesh.SetNextIndex((ushort)3); mesh.SetNextIndex((ushort)2);
        }

        // ── 中心グロー ──
        private void DrawCenterGlow(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float radius = Mathf.Min(w, h) * 0.6f;

            int layers = 5;
            var mesh = ctx.Allocate(4 * layers, 6 * layers);

            for (int i = layers - 1; i >= 0; i--)
            {
                float t = (float)i / layers;
                float r = radius * (1f - t * 0.7f);

                Color c = _palette.glowColor;
                c.a = c.a * alpha * (1f - t) * 0.3f;

                mesh.SetNextVertex(new Vertex { position = new Vector3(cx - r, cy - r, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(cx + r, cy - r, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(cx - r, cy + r, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(cx + r, cy + r, 0), tint = c });

                ushort baseIdx = (ushort)((layers - 1 - i) * 4);
                mesh.SetNextIndex(baseIdx);
                mesh.SetNextIndex((ushort)(baseIdx + 1));
                mesh.SetNextIndex((ushort)(baseIdx + 2));
                mesh.SetNextIndex((ushort)(baseIdx + 1));
                mesh.SetNextIndex((ushort)(baseIdx + 3));
                mesh.SetNextIndex((ushort)(baseIdx + 2));
            }
        }

        // ── パーティクル ──
        private void DrawParticles(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            int count = _particles.Count;
            var mesh = ctx.Allocate(4 * count, 6 * count);

            for (int i = 0; i < count; i++)
            {
                var p = _particles[i];

                float twinkle = Mathf.Sin(p.phase) * 0.5f + 0.5f;
                float opacity = p.baseOpacity * twinkle * alpha;
                float s = p.size;

                float px = p.position.x * w;
                float py = p.position.y * h;

                Color c = _palette.particleColor;
                c.a = opacity;

                mesh.SetNextVertex(new Vertex { position = new Vector3(px, py - s, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(px + s, py, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(px, py + s, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(px - s, py, 0), tint = c });

                ushort baseIdx = (ushort)(i * 4);
                mesh.SetNextIndex(baseIdx);
                mesh.SetNextIndex((ushort)(baseIdx + 1));
                mesh.SetNextIndex((ushort)(baseIdx + 2));
                mesh.SetNextIndex(baseIdx);
                mesh.SetNextIndex((ushort)(baseIdx + 2));
                mesh.SetNextIndex((ushort)(baseIdx + 3));
            }
        }

        // ── バイネット ──
        private void DrawVignette(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            var mesh = ctx.Allocate(4, 6);

            Color dark = new Color(0, 0, 0, 0.5f * alpha);

            mesh.SetNextVertex(new Vertex { position = new Vector3(0, 0, 0), tint = dark });
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, 0, 0), tint = dark });
            mesh.SetNextVertex(new Vertex { position = new Vector3(0, h, 0), tint = dark });
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, h, 0), tint = dark });

            mesh.SetNextIndex((ushort)0); mesh.SetNextIndex((ushort)1); mesh.SetNextIndex((ushort)2);
            mesh.SetNextIndex((ushort)1); mesh.SetNextIndex((ushort)3); mesh.SetNextIndex((ushort)2);
        }

        // ── タイトルグロー背景 ──
        private void DrawTitleGlow(MeshGenerationContext ctx, float w, float h)
        {
            var (_, titleScale, titleAlpha) = GetPhaseValues();

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

            mesh.SetNextIndex((ushort)0); mesh.SetNextIndex((ushort)1); mesh.SetNextIndex((ushort)2);
            mesh.SetNextIndex((ushort)1); mesh.SetNextIndex((ushort)3); mesh.SetNextIndex((ushort)2);
        }

        // ═══════════════════════════════════════════
        //  タイトルラベル
        // ═══════════════════════════════════════════
        public void AddTitleLabel()
        {
            _titleLabel = new Label(_palette.titleText);
            _titleLabel.style.position = Position.Absolute;
            _titleLabel.style.left = 0;
            _titleLabel.style.right = 0;
            _titleLabel.style.top = 0;
            _titleLabel.style.bottom = 0;
            _titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _titleLabel.style.fontSize = 26;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.color = _palette.titleColor;
            _titleLabel.pickingMode = PickingMode.Ignore;

            Add(_titleLabel);
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
