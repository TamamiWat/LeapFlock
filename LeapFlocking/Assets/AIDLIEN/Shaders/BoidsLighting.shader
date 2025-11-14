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
        _HueShiftSpeed ("Hue Shift Speed", Range(-5,5)) = 0.5

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
        float _HueShiftSpeed;

        v2f vert(appdata v, uint id : SV_InstanceID)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);

            BoidData boidData = _BoidDataBuffer[id];
            float3 pos   = boidData.position;
            float3 scale = boidData.scale;

            // ----- カプセルの太さ＆長さ -----
            // カプセルは「Y 方向に長い」前提
            float radius  = scale.x;
            float speed   = length(boidData.velocity);
            float baseLen = scale.z;
            float stretch = 1.5;
            float len     = baseLen * (1.0 + speed * stretch);

            // スケール行列（XY:太さ, Y:長さ）
            float4x4 S = (float4x4)0;
            S._11_22_33_44 = float4(radius, len, radius, 1.0);

            // ----- ローカルY軸を velocity に合わせる回転行列 -----
            float3 dir = boidData.velocity;
            dir = (length(dir) > 1e-6) ? normalize(dir) : float3(0,1,0); // 速度0対策

            // dir を「新しい Y 軸」にしたい
            float3 newY = dir;

            // newY と平行でない適当なベクトルを用意
            float3 tmp = (abs(newY.y) > 0.9) ? float3(1,0,0) : float3(0,1,0);

            // newX, newZ を直交ベクトルとして作る
            float3 newX = normalize(cross(tmp, newY));
            float3 newZ = cross(newY, newX);

            float4x4 R = (float4x4)0;
            R._11_21_31_41 = float4(newX, 0);
            R._12_22_32_42 = float4(newY, 0); // ★ ここが進行方向（Y軸）
            R._13_23_33_43 = float4(newZ, 0);
            R._14_24_34_44 = float4(0,0,0,1);

            // ----- 平行移動 -----
            float4x4 T = (float4x4)0;
            T._11_22_33_44 = float4(1,1,1,1);
            T._14_24_34    = pos;

            // 最終的なオブジェクト→ワールド行列
            float4x4 o2w = mul(T, mul(R, S));

            float4 worldPos  = mul(o2w, v.vertex);
            float3 worldNorm = normalize(mul(o2w, float4(v.normal, 0)).xyz);

            UNITY_TRANSFER_INSTANCE_ID(v, o);
            o.vertex = UnityObjectToClipPos(worldPos);
            o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
            UNITY_TRANSFER_FOG(o, o.vertex);

            // lighting 無印版なら normal / wpos が不要なら消してOK
            // Lighting 版なら v2f に normal, wpos を入れる
            o.normal = worldNorm;
            o.wpos   = worldPos.xyz;

            return o;
        }

        // fixed4 frag(v2f i) : SV_Target
        // {
        //     // float3 N = normalize(i.normal);
        //     // float3 V = normalize(_WorldSpaceCameraPos.xyz - i.wpos);
        //     // float3 L = normalize(_WorldSpaceLightPos0.xyz);

        //     // float ndotl = saturate(dot(N, L));
        //     // float ndotv = saturate(dot(N, V));

        //     // // フレネル
        //     // float fresnel = pow(1.0 - ndotv, _FresnelPower);

        //     // // ★ 生の hue（0〜1でぐるっと一周）※偏光の「動き」はここで作る
        //     // float rawHue = frac(ndotl * 2.0 + fresnel * 3.0);

        //     // // ★ 生の hue を [_HueMin, _HueMax] に押し込む
        //     // float hue = lerp(_HueMin, _HueMax, rawHue);

        //     // float3 base = hsv2rgb(float3(hue, _Saturation, _Value));

        //     // // 棒の根本〜先端グラデ
        //     // float t = saturate(i.uv.y); // 0 = 根本, 1 = 先端

        //     // // 先端側だけ少し色相をずらす（+0.1 は好みで）
        //     // float tipHue  = frac(hue + 0.1);
        //     // float3 tipColor  = hsv2rgb(float3(tipHue, 1.0, 1.0));
        //     // float3 baseColor = base;
        //     // float3 gradColor = lerp(baseColor, tipColor, t);

        //     // // 簡単な拡散＋スペキュラ
        //     // float diff = ndotl;
        //     // float3 H   = normalize(L + V);
        //     // float spec = pow(saturate(dot(N, H)), 64.0);

        //     // float3 color =
        //     //     gradColor * (0.1 + 0.9 * diff) +   // ライティング
        //     //     spec * _IridescentStrength;        // ハイライト

        //     // color *= _Brightness;

        //     // return float4(color, 1.0);
        //     // --- 法線 → 色相（虹色帯） ---
        //     float3 N = normalize(i.normal);

        //     // atan2 で方向を色相に変換
        //     float hue = atan2(N.x, N.z) / (2 * UNITY_PI);
        //     hue = frac(hue); // 0〜1に正規化

        //     // HSVで虹色
        //     float3 base = hsv2rgb(float3(hue, 1.0, 1.0));

        //     // --- フレネルで偏光感を追加 ---
        //     float3 V = normalize(_WorldSpaceCameraPos.xyz - i.wpos);
        //     float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);
        //     base *= (0.6 + fresnel * 0.6);

        //     // --- カプセルの先端グラデ ---
        //     float t = saturate(i.uv.y);
        //     float3 tipColor = hsv2rgb(float3(frac(hue + 0.1), 1.0, 1.0));
        //     float3 gradColor = lerp(base, tipColor, t);

        //     // --- 最終カラー ---
        //     float brightness = 1.3;
        //     return float4(gradColor * brightness, 1.0);

        // }

        fixed4 frag(v2f i) : SV_Target
        {
            float3 N = normalize(i.normal);

            // ベースの方向→色相
            float baseHue = atan2(N.x, N.z) / (2 * UNITY_PI);
            baseHue = frac(baseHue);

            // ★ 時間で Hue をシフト
            // _Time.y は「経過秒」に相当
            float timeOffset = _Time.y * _HueShiftSpeed;
            float hue = frac(baseHue + timeOffset);

            // HSVで虹色
            float3 base = hsv2rgb(float3(hue, 1.0, 1.0));

            // --- フレネルで偏光感を追加 ---
            float3 V = normalize(_WorldSpaceCameraPos.xyz - i.wpos);
            float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);
            base *= (0.6 + fresnel * 0.6);

            // --- カプセルの先端グラデ ---
            float t = saturate(i.uv.y);
            float3 tipColor = hsv2rgb(float3(frac(hue + 0.1), 1.0, 1.0));
            float3 gradColor = lerp(base, tipColor, t);

            float brightness = 1.3;
            return float4(gradColor * brightness, 1.0);
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
