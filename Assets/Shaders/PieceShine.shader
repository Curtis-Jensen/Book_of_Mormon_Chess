// Draws a diagonal light band sweeping across a sprite's own silhouette.
// _ShineOffset moves the band from -0.4 to 1.4 along the (u+v) diagonal;
// the sprite's own alpha is used as a mask so the shine never spills outside the artwork.
Shader "Sprites/PieceShine"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ShineColor ("Shine Color", Color) = (1, 1, 1, 1)
        _ShineWidth ("Shine Width", Range(0.02, 1)) = 0.18
        _ShineOffset ("Shine Offset", Float) = -0.4
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _ShineColor;
            float _ShineWidth;
            float _ShineOffset;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed spriteAlpha = tex2D(_MainTex, i.uv).a;

                float diag = (i.uv.x + i.uv.y) * 0.5;
                float band = 1 - smoothstep(0, _ShineWidth, abs(diag - _ShineOffset));

                fixed4 col = _ShineColor * band * spriteAlpha;
                return col;
            }
            ENDCG
        }
    }
}
