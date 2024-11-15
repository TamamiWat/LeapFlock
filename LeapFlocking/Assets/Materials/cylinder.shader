Shader "Custom/cylinder"
{
    Properties
    {
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveFrequency ("Wave Frequency", Float) = 2.0
        _Amplitude ("Amplitude", Float) = 0.2
        _FadeStart ("Fade Start (Y)", Float) = 0.0
        _FadeEnd ("Fade End (Y)", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "SelfNoise.cginc"

            // ユーザーが設定可能なプロパティ
            float _WaveSpeed;
            float _WaveFrequency;
            float _Amplitude;
            float _FadeStart;
            float _FadeEnd;


            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            // 頂点シェーダー
            v2f vert(appdata v)
            {
                v2f o;
                float3 worldPos = v.vertex.xyz;

                // Y軸位置に基づいて波を計算
                float wave = sin(worldPos.y * _WaveFrequency + _Time.y * _WaveSpeed) * _Amplitude;

                // フェード処理
                float fadeFactor = saturate((worldPos.y - _FadeStart) / (_FadeEnd - _FadeStart));

                // 頂点位置の変形（X方向に波を適用）
                worldPos.x += wave * fadeFactor;

                o.pos = UnityObjectToClipPos(float4(worldPos, 1.0));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // シンプルな白色
                return float4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
}