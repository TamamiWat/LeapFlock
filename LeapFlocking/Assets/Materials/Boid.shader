Shader "Custom/Boid"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Speed("Animation Speed ",Range(0, 5)) = 1
        _Frequency("Frequency ", Range(0, 5)) = 1
        _Amplitude("Amplitude", Range(0, 10)) = 0.5
        _SimplexBias("Simplex Amplitude", Range(0, 2)) = 1.0
        _Harmonics("Harmonics", Range(1, 10)) = 1
        _HarmonicsSpread ("Harmonics Spread", Range(0, 2)) = 1
        _Offset ("Offset", Color) = (0, 0, 0, 0)
        _Scale ("Noise Scale", Float) = 5.0
        _MinBrightness ("Minimum Brightness", Range(0, 1)) = 0.1
    }

    CGINCLUDE
        #include "UnityCG.cginc"
        #include "SelfNoise.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
	        float3 normal : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        sampler2D _MainTex;
        fixed4 _MainTex_ST;
        float _Speed;
        float _Amplitude;
        float _Frequency;
        int _Octaves;
        float4 _Color;
        float _SimplexBias;
        int _Harmonics;
        float _HarmonicsSpread;
        float4 _Offset;
        float _Scale;
        float _MinBrightness;



        v2f vert (appdata v)
        {
            v2f o;
            float noise = perlinNoise(v.uv * _Frequency + _Time.y * _Speed) * _Amplitude / 100;
            v.vertex.xyz += noise*v.normal;

            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }

        // fixed4 frag(v2f i) : SV_Target
        // {
        //     float3 position = float3(i.uv, _Time.y * _Speed) + _Offset.xyz;

        //     // RGBに異なるノイズを適用
        //     float noiseR = advancedSimplexNoise(position + float3(0.1, 0.0, 0.0), _Harmonics, _HarmonicsSpread, _Offset);
        //     float noiseG = advancedSimplexNoise(position + float3(0.0, 0.1, 0.0), _Harmonics, _HarmonicsSpread, _Offset);
        //     float noiseB = advancedSimplexNoise(position + float3(0.0, 0.0, 0.1), _Harmonics, _HarmonicsSpread, _Offset);

        //     // [-1, 1] を [0, 1] にスケール
        //     noiseR = noiseR * 0.5 + 0.5;
        //     noiseG = noiseG * 0.5 + 0.5;
        //     noiseB = noiseB * 0.5 + 0.5;

        //     // ノイズ値をRGBとして使用
        //     fixed4 col = fixed4(noiseR, noiseG, noiseB, 1.0);

        //     // カラー補正
        //     col *= _Color;

        //     return col;
        // }
        fixed4 frag(v2f i) : SV_Target
        {
            // UV座標にスケールとアニメーションオフセットを適用
            //float2 uv = i.uv * _Scale + _Offset.xy + _Time.y * _Speed;
            float2 st = i.uv;
            int channel = (int)(2.0*i.uv.x);
            st = _Scale * st + _Time*_Speed;

            // 各チャンネルに異なるSimplex Noiseを適用
            float r = simplex2DNoise(st);
            float g = simplex2DNoise(st + float2(13.0, 17.0)); // オフセットで異なるパターン
            //float b = simplex2DNoise(uv + float2(21.0, 29.0));

            r = max(r * 0.5 + 0.5, _MinBrightness);
            g = max(g * 0.5 + 0.5, _MinBrightness);
            //b = max(b * 0.5 + 0.5, _MinBrightness);

            // ノイズを [0, 1] にスケール
            r = r * 0.5 + 0.5;
            g = g * 0.5 + 0.5;
            //b = b * 0.5 + 0.5;

            float3 c = lerp(r, g, channel);

            // ノイズをRGBに適用し、ベースカラーで乗算
            fixed4 col = fixed4(c, 1.0) * _Color;

            return col;
        }
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
