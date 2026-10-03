// UI shader for the Title of Liberty flag: ripples the cloth in the wind and draws it semi-transparent.
// The flag art is a single opaque quad (pole + cloth + baked background), so the wave is done by
// offsetting UVs in the fragment stage rather than moving vertices. Displacement is zero at
// _PoleU (the pole edge) and grows toward the free end, so the pole stays still.
Shader "UI/FlagWave"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Alpha ("Alpha", Range(0, 1)) = 0.65
        _PoleU ("Pole Edge (U)", Range(0, 1)) = 0.27
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.05)) = 0.015
        _WaveFrequency ("Wave Frequency", Float) = 9
        _WaveSpeed ("Wave Speed", Float) = 2.5

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; float4 worldPos : TEXCOORD1; };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;
            float _Alpha, _PoleU, _WaveAmplitude, _WaveFrequency, _WaveSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPos = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 0 at the pole, 1 at the right edge; squared so the ripple builds up gently.
                float along = saturate((i.uv.x - _PoleU) / (1 - _PoleU));
                float strength = along * along;
                float phase = i.uv.x * _WaveFrequency - _Time.y * _WaveSpeed;

                float2 uv = i.uv;
                uv.y += sin(phase) * _WaveAmplitude * strength;
                uv.x += cos(phase * 0.7) * _WaveAmplitude * 0.4 * strength;

                fixed4 col = tex2D(_MainTex, uv) * i.color;
                // Subtle shading so the folds read as cloth rather than a plain smear.
                col.rgb *= 1 - cos(phase) * 0.06 * strength;
                col.a *= _Alpha;
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                return col;
            }
            ENDCG
        }
    }
}
