using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moruton.Gimmicks.Editor
{
    /// <summary>
    /// 魔法少女風オープニング演出エレメント。
    /// グラデーション背景 + 中央の丸 + 放射状キラキラ + 浮遊粒子 + グロータイトル + バイネット。
    /// </summary>
    public class MagicalOpeningEffect : VisualElement
    {
        // ═══════════════════════════════════════════
        //  テーマ別カラー
        //  ※ 色の定義は EditorThemeDefinition 側で一元管理。
        //    この構造体は受け取った色を使うだけの入れ物。
        // ═══════════════════════════════════════════
        public struct ThemePalette
        {
            public Color bgCenter;
            public Color bgEdge;
            public Color particleColor;
            public Color glowColor;
            public Color circleColor;
            public Color circleBorderColor;
            public Color titleColor;
            public Color titleGlow;
            public Color sparkleColor;
            public string titleText;
        }

        /// <summary>
        /// EditorThemeDefinition から ThemePalette を生成。
        /// 色の定義は EditorThemeDefinition にしかないので、
        /// ここで変換するだけ。
        /// </summary>
        public static ThemePalette FromDefinition(EditorThemeDefinition theme)
        {
            return new ThemePalette
            {
                bgCenter = theme.openingBgCenter,
                bgEdge = theme.openingBgEdge,
                particleColor = theme.openingParticleColor,
                glowColor = theme.openingGlowColor,
                circleColor = theme.openingCircleColor,
                circleBorderColor = theme.openingCircleBorder,
                titleColor = theme.openingTitleColor,
                titleGlow = theme.openingTitleGlow,
                sparkleColor = theme.openingSparkleColor,
                titleText = "Metamorphose",
            };
        }

        // ═══════════════════════════════════════════
        //  パーティクル（浮遊用）
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
        //  放射状キラキラ（丸から外へ飛ぶ）
        // ═══════════════════════════════════════════
        private struct Sparkle
        {
            public float angle;        // 飛ぶ方向（ラジアン）
            public float distance;     // 中心からの距離（0〜1、1=画面端）
            public float speed;        // 飛ぶ速度
            public float size;         // 大きさ
            public float opacity;      // 透明度
            public float life;         // 0〜1（1で消える）
        }

        // ═══════════════════════════════════════════
        //  状態
        // ═══════════════════════════════════════════
        private ThemePalette _palette;
        private readonly List<Particle> _particles = new List<Particle>();
        private readonly List<Sparkle> _sparkles = new List<Sparkle>();
        private float _elapsed;
        private float _lastTime;
        private float _totalDuration = 2.8f;
        private bool _isPlaying;
        private System.Action _onComplete;
        private Label _titleLabel;

        private const float PhaseFadeIn = 0.4f;
        private const float PhaseHold = 1.4f;

        // ═══════════════════════════════════════════
        //  初期化
        // ═══════════════════════════════════════════
        public MagicalOpeningEffect(MagicalOpeningEffect.ThemePalette palette, System.Action onComplete = null)
        {
            _palette = palette;
            _onComplete = onComplete;

            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;
            style.flexGrow = 1;
            pickingMode = PickingMode.Ignore;

            generateVisualContent += OnGenerateVisualContent;

            GenerateParticles(30);
            GenerateSparkles(24);
        }

        private void GenerateParticles(int count)
        {
            _particles.Clear();
            for (int i = 0; i < count; i++)
            {
                _particles.Add(new Particle
                {
                    position = new Vector2(Random.value, Random.value),
                    velocity = new Vector2(
                        (Random.value - 0.5f) * 0.02f,
                        (Random.value * 0.5f + 0.3f) * 0.015f
                    ),
                    size = Random.value * 3f + 1.5f,
                    baseOpacity = Random.value * 0.5f + 0.3f,
                    phase = Random.value * Mathf.PI * 2f,
                    twinkleSpeed = Random.value * 3f + 1.5f,
                });
            }
        }

        private void GenerateSparkles(int count)
        {
            _sparkles.Clear();
            for (int i = 0; i < count; i++)
            {
                float angle = (float)i / count * Mathf.PI * 2f + Random.Range(-0.1f, 0.1f);
                _sparkles.Add(new Sparkle
                {
                    angle = angle,
                    distance = 0f,
                    speed = Random.Range(0.15f, 0.3f),
                    size = Random.Range(3f, 6f),
                    opacity = Random.Range(0.5f, 1f),
                    life = 0f,
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
            generateVisualContent -= OnGenerateVisualContent; // ★ コールバック解除
            _onComplete?.Invoke();
            RemoveFromHierarchy();
        }

        private void Tick()
        {
            if (!_isPlaying) return;

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

            // 放射状キラキラ更新（フェードイン完了後から飛び始める）
            if (_elapsed > PhaseFadeIn * 0.5f)
            {
                for (int i = 0; i < _sparkles.Count; i++)
                {
                    var s = _sparkles[i];
                    s.distance += s.speed * dt;
                    s.life = Mathf.Clamp01(s.distance / 1.2f);
                    _sparkles[i] = s;
                }
            }

            // タイトルラベルの opacity 更新
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
            // ★ 再生中以外は描画しない（安全ガード）
            if (!_isPlaying) return;

            var (globalAlpha, titleScale, _) = GetPhaseValues();

            float width = contentRect.width;
            float height = contentRect.height;

            if (width <= 0 || height <= 0) return;

            // ── 1. グラデーション背景 ──
            DrawGradientBackground(ctx, width, height, globalAlpha);

            // ── 2. 中心のグロー ──
            DrawCenterGlow(ctx, width, height, globalAlpha);

            // ── 3. 放射状キラキラ（丸の外側に飛ぶ） ──
            DrawRadialSparkles(ctx, width, height, globalAlpha);

            // ── 4. 浮遊粒子 ──
            DrawParticles(ctx, width, height, globalAlpha);

            // ── 5. 中央の丸（タイトルの背景） ──
            DrawTitleCircle(ctx, width, height, titleScale, globalAlpha);

            // ── 6. バイネット ──
            DrawVignette(ctx, width, height, globalAlpha);
        }

        // ── グラデーション背景 ──
        private void DrawGradientBackground(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            // 3段グラデーション: 上端(edge) → 中央(center/明るい) → 下端(edge)
            // これで「中心が明るく光る」放射状っぽい見た目になる
            var mesh = ctx.Allocate(8, 12);

            Color center = _palette.bgCenter;
            Color edge = _palette.bgEdge;
            center.a *= alpha;
            edge.a *= alpha;

            float midY = h * 0.5f;

            // 上半分: edge → center
            mesh.SetNextVertex(new Vertex { position = new Vector3(0, 0, 0), tint = edge });      // 0: 左上
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, 0, 0), tint = edge });      // 1: 右上
            mesh.SetNextVertex(new Vertex { position = new Vector3(0, midY, 0), tint = center }); // 2: 左中央
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, midY, 0), tint = center }); // 3: 右中央

            // 下半分: center → edge
            mesh.SetNextVertex(new Vertex { position = new Vector3(0, midY, 0), tint = center }); // 4: 左中央
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, midY, 0), tint = center }); // 5: 右中央
            mesh.SetNextVertex(new Vertex { position = new Vector3(0, h, 0), tint = edge });      // 6: 左下
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, h, 0), tint = edge });      // 7: 右下

            // 上半分の三角形
            mesh.SetNextIndex((ushort)0); mesh.SetNextIndex((ushort)1); mesh.SetNextIndex((ushort)2);
            mesh.SetNextIndex((ushort)1); mesh.SetNextIndex((ushort)3); mesh.SetNextIndex((ushort)2);

            // 下半分の三角形
            mesh.SetNextIndex((ushort)4); mesh.SetNextIndex((ushort)5); mesh.SetNextIndex((ushort)6);
            mesh.SetNextIndex((ushort)5); mesh.SetNextIndex((ushort)7); mesh.SetNextIndex((ushort)6);
        }

        // ── 中心グロー ──
        private void DrawCenterGlow(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float radius = Mathf.Min(w, h) * 0.5f;

            int layers = 5;
            var mesh = ctx.Allocate(4 * layers, 6 * layers);

            for (int i = layers - 1; i >= 0; i--)
            {
                float t = (float)i / layers;
                float r = radius * (1f - t * 0.6f);

                Color c = _palette.glowColor;
                c.a = c.a * alpha * (1f - t) * 0.25f;

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

        // ── 放射状キラキラ（丸から外側に飛ぶ） ──
        private void DrawRadialSparkles(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float minDim = Mathf.Min(w, h);
            float circleRadius = minDim * 0.12f; // 丸の半径
            float maxRadius = minDim * 0.5f;     // 飛んでいく最大距離

            // ★ 全sparkleを必ず描画する（スキップしない）
            // 透明度0でも頂点を書き込むことでallocate数と一致させる
            int count = _sparkles.Count;
            if (count == 0) return;

            var mesh = ctx.Allocate(4 * count, 6 * count);

            for (int i = 0; i < count; i++)
            {
                var s = _sparkles[i];

                // 描画データを計算（条件に関わらず必ず頂点を書く）
                float dist, fadeOpacity, sz;
                if (s.life >= 1f || s.distance <= 0.01f)
                {
                    // 非表示の時は透明にする（頂点は書く）
                    fadeOpacity = 0f;
                    dist = 0f;
                    sz = 0f;
                }
                else
                {
                    dist = circleRadius + s.distance * (maxRadius - circleRadius);
                    fadeOpacity = s.opacity * (1f - s.life) * alpha;
                    sz = s.size * (1f - s.life * 0.3f);
                }

                float px = cx + Mathf.Cos(s.angle) * dist;
                float py = cy + Mathf.Sin(s.angle) * dist;

                Color c = _palette.sparkleColor;
                c.a = fadeOpacity;

                mesh.SetNextVertex(new Vertex { position = new Vector3(px, py - sz, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(px + sz, py, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(px, py + sz, 0), tint = c });
                mesh.SetNextVertex(new Vertex { position = new Vector3(px - sz, py, 0), tint = c });

                ushort bi = (ushort)(i * 4);
                mesh.SetNextIndex(bi);
                mesh.SetNextIndex((ushort)(bi + 1));
                mesh.SetNextIndex((ushort)(bi + 2));
                mesh.SetNextIndex(bi);
                mesh.SetNextIndex((ushort)(bi + 2));
                mesh.SetNextIndex((ushort)(bi + 3));
            }
        }

        // ── 中央の丸（タイトル背景） ──
        private void DrawTitleCircle(MeshGenerationContext ctx, float w, float h, float scale, float alpha)
        {
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float minDim = Mathf.Min(w, h);
            float radius = minDim * 0.12f * scale;

            // 丸の背景（円形に近づけるため多角形で描画）
            int segments = 24;
            // 外側のボーダー用に少し大きめの円も描く
            float borderRadius = radius * 1.08f;

            // ボーダー（外側の円）
            var borderMesh = ctx.Allocate(segments, (segments - 2) * 3);
            Color borderColor = _palette.circleBorderColor;
            borderColor.a *= alpha;

            for (int i = 0; i < segments; i++)
            {
                float a = (float)i / segments * Mathf.PI * 2f;
                borderMesh.SetNextVertex(new Vertex
                {
                    position = new Vector3(cx + Mathf.Cos(a) * borderRadius, cy + Mathf.Sin(a) * borderRadius, 0),
                    tint = borderColor
                });
            }
            for (int i = 0; i < segments - 2; i++)
            {
                borderMesh.SetNextIndex((ushort)0);
                borderMesh.SetNextIndex((ushort)(i + 1));
                borderMesh.SetNextIndex((ushort)(i + 2));
            }

            // 内側の円（背景）
            var innerMesh = ctx.Allocate(segments, (segments - 2) * 3);
            Color innerColor = _palette.circleColor;
            innerColor.a *= alpha;

            for (int i = 0; i < segments; i++)
            {
                float a = (float)i / segments * Mathf.PI * 2f;
                innerMesh.SetNextVertex(new Vertex
                {
                    position = new Vector3(cx + Mathf.Cos(a) * radius, cy + Mathf.Sin(a) * radius, 0),
                    tint = innerColor
                });
            }
            for (int i = 0; i < segments - 2; i++)
            {
                innerMesh.SetNextIndex((ushort)0);
                innerMesh.SetNextIndex((ushort)(i + 1));
                innerMesh.SetNextIndex((ushort)(i + 2));
            }
        }

        // ── 浮遊粒子 ──
        private void DrawParticles(MeshGenerationContext ctx, float w, float h, float alpha)
        {
            int count = _particles.Count;
            var mesh = ctx.Allocate(4 * count, 6 * count);

            for (int i = 0; i < count; i++)
            {
                var p = _particles[i];

                float twinkle = Mathf.Sin(p.phase) * 0.5f + 0.5f;
                float opacity = p.baseOpacity * twinkle * alpha * 0.6f;
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
            // Daylight（ライトテーマ）ではバイネットを描画しない
            // （暗い膜がかかって見えるため）
            if (_palette.bgCenter.r > 0.7f && _palette.bgCenter.g > 0.7f)
                return;

            var mesh = ctx.Allocate(4, 6);

            Color dark = new Color(0, 0, 0, 0.4f * alpha);

            mesh.SetNextVertex(new Vertex { position = new Vector3(0, 0, 0), tint = dark });
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, 0, 0), tint = dark });
            mesh.SetNextVertex(new Vertex { position = new Vector3(0, h, 0), tint = dark });
            mesh.SetNextVertex(new Vertex { position = new Vector3(w, h, 0), tint = dark });

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
            _titleLabel.style.fontSize = 18;
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
            generateVisualContent -= OnGenerateVisualContent; // ★ コールバック確実に解除
        }
    }
}
