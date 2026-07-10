Shader "moruton/Package/Effect/VanishShader" {
    Properties {
        [Header(Surface Appearance)]
        _Color ("表面の色 (Surface Tint)", Color) = (0.3, 0.5, 1.0, 0.08)
        _RimColor ("縁の発光色 (Edge Glow)", Color) = (0.2, 0.6, 1.0, 1.0)
        _RimPower ("縁の鋭さ (Glow Sharpness)", Range(0.5, 10.0)) = 3.0
        _RimIntensity ("縁の強さ (Glow Intensity)", Range(0.0, 5.0)) = 1.5

        [Header(Shimmer)]
        _ShimmerSpeed ("揺らめき速度", Range(0.0, 10.0)) = 1.5
        _ShimmerAmount ("揺らめきの大きさ", Range(0.0, 0.3)) = 0.05

        [Header(Depth Mask Settings)]
        [Enum(Cull Back,2,Cull Front,1,Cull Off,0)] _MaskCull ("マスクの Cull モード", Float) = 2
        [Toggle(_DISABLE_MASK)] _DisableMask ("デプスマスクを無効化 (テスト用)", Float) = 0

        [Header(Render Settings)]
        [Enum(AlphaBlend,10,Additive,1)] _BlendMode ("ブレンドモード", Float) = 10
    }

    // ============================================================
    //  SubShader: Depth Mask + Semi-transparent
    //  Queue=Geometry-1 で真っ先に深度を書き込み、
    //  以降の不透明オブジェクト（アバター等）を深度テストで弾く。
    // ============================================================
    SubShader {
        Tags {
            "Queue" = "Geometry-1"
            "RenderType" = "TransparentCutout"
            "IgnoreProjector" = "True"
        }
        LOD 100

        // ---- Pass 1: Depth Mask ----
        // 色は描かず深度のみ書き込む。この深度より奥にある物体は描画されない。
        Pass {
            Name "DEPTH_MASK"
            ColorMask 0
            ZWrite On
            Cull [_MaskCull]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _DISABLE_MASK
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
            #if defined(_DISABLE_MASK)
                discard; // マスク無効時は描画しない = 深度も書かない
            #endif
                return 0;
            }
            ENDCG
        }

        // ---- Pass 2: Semi-transparent Surface ----
        // フレネル縁発光 + 微細な揺らめきを持つ半透明サーフェス
        Pass {
            Name "SEMITRANSPARENT"
            Blend SrcAlpha [_BlendMode]
            ZWrite Off
            Cull Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldViewDir : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            fixed4 _Color;
            fixed4 _RimColor;
            float _RimPower;
            float _RimIntensity;
            float _ShimmerSpeed;
            float _ShimmerAmount;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldViewDir = normalize(UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex)));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                // 1. Fresnel（視線と法線の角度で縁を光らせる）
                float rim = 1.0 - saturate(dot(normalize(i.worldNormal), normalize(i.worldViewDir)));
                rim = pow(rim, _RimPower);

                // 2. 揺らめき（プロシージャル・テクスチャ不要）
                float shimmer = sin(i.uv.y * 18.0 + _Time.y * _ShimmerSpeed) *
                                cos(i.uv.x * 14.0 - _Time.y * _ShimmerSpeed * 0.7);
                shimmer = shimmer * 0.5 + 0.5; // 0〜1にリマップ

                // 3. 合成
                fixed4 col = _Color;
                col.rgb += _RimColor.rgb * rim * _RimIntensity;
                col.a = saturate(_Color.a + rim * _RimIntensity + shimmer * _ShimmerAmount);

                return col;
            }
            ENDCG
        }
    }

    // ============================================================
    //  フォールバック（モバイル等でシェーダーが対応していない場合）
    // ============================================================
    FallBack "Transparent/Diffuse"
}
