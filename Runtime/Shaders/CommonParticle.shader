Shader "moruton/Package/Particle/ComonParticleShader"
{
    Properties
    {
        [Header(Main Settings)]
        _Color ("Base Color", Color) = (1,1,1,1)
        _Brightness ("Overall Brightness", Float) = 1.0

        [Header(Texture 1 Settings)]
        [NoScaleOffset] _MainTex ("Texture 1", 2D) = "white" {}
        _MainTex_ST ("Tiling/Offset 1", Vector) = (1,1,0,0)
        _ScrollX1 ("Scroll X1", Float) = 0
        _ScrollY1 ("Scroll Y1", Float) = 0
        [Toggle] _UseEmission1 ("Enable Emission 1", Float) = 0
        [HDR] _EmissionColor1 ("Emission Color 1", Color) = (1,1,1,1)

        [Header(Texture 2 Settings)]
        [NoScaleOffset] _SubTex1 ("Texture 2", 2D) = "black" {}
        _SubTex1_ST ("Tiling/Offset 2", Vector) = (1,1,0,0)
        _ScrollX2 ("Scroll X2", Float) = 0
        _ScrollY2 ("Scroll Y2", Float) = 0
        [Toggle] _UseEmission2 ("Enable Emission 2", Float) = 0
        [HDR] _EmissionColor2 ("Emission Color 2", Color) = (1,1,1,1)

        [Header(Texture 3 Settings)]
        [NoScaleOffset] _SubTex2 ("Texture 3", 2D) = "black" {}
        _SubTex2_ST ("Tiling/Offset 3", Vector) = (1,1,0,0)
        _ScrollX3 ("Scroll X3", Float) = 0
        _ScrollY3 ("Scroll Y3", Float) = 0
        [Toggle] _UseEmission3 ("Enable Emission 3", Float) = 0
        [HDR] _EmissionColor3 ("Emission Color 3", Color) = (1,1,1,1)

        [Header(Visible Mask Settings)]
        [Enum(Tiling,0,Scale,1)] _VisibleMaskMode ("Visible Mask Mode", Float) = 0
        _VisibleMaskTex ("Visible Mask (White=Show)", 2D) = "white" {}
        _VisibleMaskTex_ST ("Tiling/Offset VM", Vector) = (1,1,0,0)
        _VisibleMaskScale ("Visible Mask Scale", Range(0.01, 5)) = 1.0
        _VisibleMaskStrength ("Visible Mask Strength", Range(0, 1)) = 1.0

        [Header(Hide Mask Settings)]
        [Enum(Tiling,0,Scale,1)] _HideMaskMode ("Hide Mask Mode", Float) = 0
        _HideMaskTex ("Hide Mask (White=Hide)", 2D) = "black" {}
        _HideMaskTex_ST ("Tiling/Offset HM", Vector) = (1,1,0,0)
        _HideMaskScale ("Hide Mask Scale", Range(0.01, 5)) = 1.0
        _HideMaskStrength ("Hide Mask Strength", Range(0, 1)) = 1.0

        [Header(Rendering Options)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
        [Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            Cull [_Cull]
            ZWrite [_ZWrite]
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex, _SubTex1, _SubTex2, _VisibleMaskTex, _HideMaskTex;
            float4 _MainTex_ST, _SubTex1_ST, _SubTex2_ST, _VisibleMaskTex_ST, _HideMaskTex_ST;
            fixed4 _Color;
            float _Brightness;
            float _ScrollX1, _ScrollY1, _ScrollX2, _ScrollY2, _ScrollX3, _ScrollY3;
            
            float _UseEmission1, _UseEmission2, _UseEmission3;
            fixed4 _EmissionColor1, _EmissionColor2, _EmissionColor3;

            float _VisibleMaskMode, _HideMaskMode;
            float _VisibleMaskScale, _HideMaskScale;
            float _VisibleMaskStrength, _HideMaskStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 : TEXCOORD2;
                float2 uv_visible_mask : TEXCOORD3;
                float2 uv_hide_mask : TEXCOORD4;
                UNITY_FOG_COORDS(5)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv1 = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.uv, _SubTex1);
                o.uv3 = TRANSFORM_TEX(v.uv, _SubTex2);

                if (_VisibleMaskMode == 0) // Tiling
                {
                    o.uv_visible_mask = TRANSFORM_TEX(v.uv, _VisibleMaskTex);
                }
                else // Scale
                {
                    o.uv_visible_mask = (v.uv - 0.5) * _VisibleMaskScale + 0.5;
                }

                if (_HideMaskMode == 0) // Tiling
                {
                    o.uv_hide_mask = TRANSFORM_TEX(v.uv, _HideMaskTex);
                }
                else // Scale
                {
                    o.uv_hide_mask = (v.uv - 0.5) * _HideMaskScale + 0.5;
                }
                
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex1 = tex2D(_MainTex, i.uv1 + float2(_ScrollX1, _ScrollY1) * _Time.y);
                fixed tex2 = tex2D(_SubTex1, i.uv2 + float2(_ScrollX2, _ScrollY2) * _Time.y).r;
                fixed tex3 = tex2D(_SubTex2, i.uv3 + float2(_ScrollX3, _ScrollY3) * _Time.y).r;

                fixed visibleMaskValue = tex2D(_VisibleMaskTex, i.uv_visible_mask).r;
                if (_VisibleMaskMode == 1) // Scale Mode
                {
                    visibleMaskValue = all(saturate(i.uv_visible_mask) == i.uv_visible_mask) ? visibleMaskValue : 0;
                }
                fixed visibleMaskEffect = lerp(1.0, visibleMaskValue, _VisibleMaskStrength);

                fixed hideMaskValue = tex2D(_HideMaskTex, i.uv_hide_mask).r;
                if (_HideMaskMode == 1) // Scale Mode
                {
                    hideMaskValue = all(saturate(i.uv_hide_mask) == i.uv_hide_mask) ? hideMaskValue : 0;
                }
                fixed hideMaskEffect = lerp(1.0, 1.0 - hideMaskValue, _HideMaskStrength);

                fixed3 finalTex1_rgb = tex1.rgb * visibleMaskEffect * hideMaskEffect;
                fixed finalTex1_a = tex1.a * visibleMaskEffect * hideMaskEffect;
                fixed finalTex2 = tex2 * visibleMaskEffect * hideMaskEffect;
                fixed finalTex3 = tex3 * visibleMaskEffect * hideMaskEffect;
                
                fixed3 baseColor = 
                    (1 - _UseEmission1) * finalTex1_rgb * _Color.rgb +
                    (1 - _UseEmission2) * finalTex2 * _Color.rgb +
                    (1 - _UseEmission3) * finalTex3 * _Color.rgb;

                fixed3 emissionColor = 
                    _UseEmission1 * finalTex1_rgb * _EmissionColor1.rgb +
                    _UseEmission2 * finalTex2 * _EmissionColor2.rgb +
                    _UseEmission3 * finalTex3 * _EmissionColor3.rgb;

                fixed3 finalColor = (baseColor + emissionColor) * _Brightness * i.color.rgb;

                fixed totalEmissionForAlpha = (finalTex1_a + finalTex2 + finalTex3);
                fixed finalAlpha = saturate(totalEmissionForAlpha) * _Color.a * i.color.a;

                finalColor *= finalAlpha;

                fixed4 col = fixed4(finalColor, finalAlpha);
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}
