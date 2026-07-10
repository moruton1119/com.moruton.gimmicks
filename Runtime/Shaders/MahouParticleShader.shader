Shader "moruton/Package/Particle/MahouParticleShader" {
    Properties {
        [Header(Main Texture)]
        _MainTex ("パーティクル本体テクスチャ", 2D) = "white" {}
        [HDR] _Color ("本体の色 (HDR)", Color) = (1, 1, 1, 1)
        _MainScale ("本体テクスチャの拡大率", Range(0.1, 3.0)) = 1.0

        [Header(Glow Texture)]
        _GlowTex ("ふんわり発光テクスチャ (下層)", 2D) = "black" {}
        [HDR] _GlowColor ("発光の色 (HDR)", Color) = (1, 1, 1, 1)
        _GlowIntensity ("発光の強さ", Range(0, 5)) = 1.0
        _GlowScale ("発光テクスチャの拡大率", Range(0.1, 3.0)) = 1.0

        [Header(Render Settings)]
        [Enum(Additive,1,AlphaBlend,10)] _BlendMode ("ブレンドモード (1=加算, 10=アルファ)", Float) = 1
        _Cutoff ("黒透明の閾値 (これ以下を透過)", Range(0, 1)) = 0.01
    }
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Particle" }
        Blend SrcAlpha [_BlendMode]
        Cull Off
        Lighting Off
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 glowUV : TEXCOORD1;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _MainScale;

            sampler2D _GlowTex;
            float4 _GlowTex_TexelSize;
            float4 _GlowColor;
            float _GlowIntensity;
            float _GlowScale;

            float _Cutoff;

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = (v.uv - 0.5) / _MainScale + 0.5;
                o.glowUV = (v.uv - 0.5) / _GlowScale + 0.5;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float2 clampedMainUV = clamp(i.uv, 0.0, 1.0);
                fixed4 mainTex = tex2D(_MainTex, clampedMainUV);

                float2 clampedGlowUV = clamp(i.glowUV, 0.0, 1.0);
                fixed4 glowTex = tex2D(_GlowTex, clampedGlowUV);

                float mainLum = Luminance(mainTex.rgb);
                float mainAlpha = saturate(max(mainTex.a, mainLum));

                fixed3 mainRGB = mainTex.rgb * _Color.rgb * i.color.rgb * mainAlpha;
                float glowLuminance = Luminance(glowTex.rgb);
                fixed3 glowRGB = glowTex.rgb * _GlowColor.rgb * _GlowColor.a * _GlowIntensity * glowLuminance;

                float3 finalRGB = glowRGB + mainRGB;
                float finalAlpha = saturate(max(glowLuminance * _GlowColor.a * _GlowIntensity, mainAlpha * _Color.a * i.color.a));

                clip(finalAlpha - _Cutoff);

                return fixed4(finalRGB, finalAlpha * i.color.a);
            }
            ENDCG
        }
    }
}
