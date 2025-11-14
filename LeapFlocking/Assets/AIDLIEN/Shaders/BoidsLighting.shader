Shader "Custom/BoidsLighting"
{
    Properties
    {
        _Texture ("Texture", 2D) = "white"{}

        _FresnelPower       ("Fresnel Power",       Range(0,8))  = 3
        _IridescentStrength ("Iridescent Strength", Range(0,5))  = 2
        _Saturation         ("Base Saturation",     Range(0,1))  = 0.9
        _Value              ("Base Value",          Range(0,2))  = 1.2
        _Brightness ("Brightness", Range(0,5)) = 1
        _HueMin             ("Hue Min",             Range(0,1))  = 0.55
        _HueMax             ("Hue Max",             Range(0,1))  = 0.8
    }

    CGINCLUDE
        #include "UnityCG.cginc"
        #include "CustomFunction.cginc"   // hsv2rgb が入ってる想定
        #include "Lighting.cginc"

        struct BoidData {
            float3 velocity;
            float3 position;
            float4 color;
            float3 scale;
        };

        StructuredBuffer<BoidData> _BoidDataBuffer;

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv     : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float2 uv     : TEXCOORD0;
            float3 normal : TEXCOORD1;
            float3 wpos   : TEXCOORD2;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;

        float _IridescentStrength;
        float _FresnelPower;
        float _Saturation;
        float _Value;
        float _Brightness;
        float _HueMin;
        float _HueMax;

        v2f vert(appdata v, uint id : SV_InstanceID)
        {   
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);

            BoidData boidData = _BoidDataBuffer[id];
            float3 pos   = boidData.position;
            float3 scale = boidData.scale;

            // ---- スケール（半径はXY、長さはZ） ----
            float radius  = scale.x;
            float speed   = length(boidData.velocity);
            float baseLen = scale.z;
            float stretch = 1.5;                          // 速度に応じた伸び
            float len     = baseLen * (1.0 + speed * stretch);

            float4x4 o2w = (float4x4)0;
            o2w._11_22_33_44 = float4(radius, radius, len, 1.0);

            // ---- 進行方向に向ける回転 ----
            float rotY = atan2(boidData.velocity.x, boidData.velocity.z);
            float rotX = -asin(boidData.velocity.y /
                               (length(boidData.velocity.xyz) + 1e-8));

            float4x4 rotMatrix = ConvertEulerToRotateMatrix(float3(rotX, rotY, 0));
            o2w = mul(rotMatrix, o2w);

            // ---- 平行移動 ----
            o2w._14_24_34 += pos.xyz;

            // ワールド座標と法線
            float4 worldPos  = mul(o2w, v.vertex);
            float3 worldNorm = normalize(mul(o2w, float4(v.normal, 0)).xyz);

            UNITY_TRANSFER_INSTANCE_ID(v, o);
            o.vertex = UnityObjectToClipPos(worldPos);
            o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
            UNITY_TRANSFER_FOG(o, o.vertex);
            o.normal = worldNorm;
            o.wpos   = worldPos.xyz;
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            float3 N = normalize(i.normal);
            float3 V = normalize(_WorldSpaceCameraPos.xyz - i.wpos);
            float3 L = normalize(_WorldSpaceLightPos0.xyz);

            float ndotl = saturate(dot(N, L));
            float ndotv = saturate(dot(N, V));

            // フレネル
            float fresnel = pow(1.0 - ndotv, _FresnelPower);

            // ★ 生の hue（0〜1でぐるっと一周）※偏光の「動き」はここで作る
            float rawHue = frac(ndotl * 2.0 + fresnel * 3.0);

            // ★ 生の hue を [_HueMin, _HueMax] に押し込む
            float hue = lerp(_HueMin, _HueMax, rawHue);

            float3 base = hsv2rgb(float3(hue, _Saturation, _Value));

            // 棒の根本〜先端グラデ
            float t = saturate(i.uv.y); // 0 = 根本, 1 = 先端

            // 先端側だけ少し色相をずらす（+0.1 は好みで）
            float tipHue  = frac(hue + 0.1);
            float3 tipColor  = hsv2rgb(float3(tipHue, 1.0, 1.0));
            float3 baseColor = base;
            float3 gradColor = lerp(baseColor, tipColor, t);

            // 簡単な拡散＋スペキュラ
            float diff = ndotl;
            float3 H   = normalize(L + V);
            float spec = pow(saturate(dot(N, H)), 64.0);

            float3 color =
                gradColor * (0.1 + 0.9 * diff) +   // ライティング
                spec * _IridescentStrength;        // ハイライト

            color *= _Brightness;

            return float4(color, 1.0);
        }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            ENDCG
        }
    }
}
