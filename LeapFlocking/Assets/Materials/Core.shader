Shader "Custom/Core"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _RimColor ("RimColor", Color) = (1,1,1,1)
        _RimPower("RimPower", Range (0.0, 10.0)) = 0.0
        _Alpha("Alpha", Range (0.0, 1.0)) = 0.0
        _Alpha2("Alpha2", Range (0.0, 1.0)) = 1.0
        _Speed("Speed ",Range(0, 1)) = 1
        _Frequency("Frequency ", Range(0, 5)) = 1
        _Amplitude("Amplitude", Range(0, 10)) = 0.5
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
            float3 viewDir : TEXCOORD1;
            float3 normalDir : TEXCOORD2;
        };

        sampler2D _MainTex;
        fixed4 _MainTex_ST;
        float4 _Color;
        float4 _RimColor;
        float _RimPower;
        half _Alpha;
        float _Speed;
        float _Frequency;
        float _Amplitude;
        float _Alpha2;
        float4 _UserPosition;
        int _OnUser = 0;

        v2f vert (appdata v)
        {
            v2f o;
            //move vertex using Perlin Noise
            _Speed = 1/_Speed;
            float wave = perlinNoise(v.uv * _Frequency + _Time.y / _Speed) * _Amplitude / 100;
            v.vertex.xyz += wave * v.normal;

            //add move based on user
            if(_OnUser == 1){
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float distanceToUser = distance(worldPos, _UserPosition.xyz);

                float influence = saturate(1.0 - distanceToUser / 5.0);
                v.vertex.xyz += v.normal * influence * 0.1;
            }
             

            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            float4x4 modelMatrix = unity_ObjectToWorld; //current model matrix
            o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
            o.viewDir = normalize(_WorldSpaceCameraPos - mul(modelMatrix, v.vertex).xyz);
            
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            fixed4 col = tex2D(_MainTex, i.uv) * _Color;

            //color
            float rim = 1.0 - abs(dot(i.viewDir, i.normalDir));
            fixed3 emission = _RimColor.rgb * pow(rim, _RimPower) * _RimPower;
            col.rgb += emission;

            //alpha
            half alpha = 1.0 - (abs(dot(i.viewDir, i.normalDir)));
            alpha = clamp(alpha * _Alpha, 0.1, 1.0);

            alpha *=  _Alpha2;
            col = fixed4(col.rgb, alpha);
            return col;
        }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha 
        LOD 100
        ZWrite OFF

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
