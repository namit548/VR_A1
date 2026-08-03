Shader "BotWork/MagicTrailAdditive"
{
    Properties
    {
        [HDR]_Tint ("Tint", Color) = (0.4, 0.9, 2.0, 1)
        [HDR]_CoreColor ("Core Color", Color) = (1.5, 1.2, 3.0, 1)
        _MainTex ("Trail Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        _Intensity ("Intensity", Range(0, 10)) = 2.5
        _Softness ("Edge Softness", Range(0.1, 8)) = 2.0
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.12
        _MainScrollX ("Main Scroll X", Range(-5, 5)) = -1.5
        _NoiseScrollX ("Noise Scroll X", Range(-5, 5)) = -0.8
        _NoiseScrollY ("Noise Scroll Y", Range(-5, 5)) = 0.25
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend One One
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float4 _Tint;
            float4 _CoreColor;
            float _Intensity;
            float _Softness;
            float _NoiseStrength;
            float _MainScrollX;
            float _NoiseScrollX;
            float _NoiseScrollY;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.noiseUV = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 mainUV = i.uv;
                float2 noiseUV = i.noiseUV;

                mainUV.x += _Time.y * _MainScrollX;
                noiseUV += float2(_NoiseScrollX, _NoiseScrollY) * _Time.y;

                float noise = tex2D(_NoiseTex, noiseUV).r;
                mainUV.y += (noise - 0.5) * _NoiseStrength;

                fixed4 trailTex = tex2D(_MainTex, mainUV);

                float cross = abs(i.uv.y * 2.0 - 1.0);
                float edgeFade = pow(saturate(1.0 - cross), _Softness);

                float headBoost = saturate(1.15 - i.uv.x);
                headBoost = lerp(1.0, 1.6, headBoost);

                fixed4 combined = trailTex * i.color;
                fixed3 color = combined.rgb * _Tint.rgb;
                color += _CoreColor.rgb * edgeFade * combined.a;
                color *= edgeFade * headBoost * _Intensity;

                float alpha = combined.a * edgeFade;
                return fixed4(color, alpha);
            }
            ENDCG
        }
    }

    FallBack Off
}
