float4x4 ConvertEulerToRotateMatrix(float3 angles)
{
    //X:Pitycos
    float pcos = cos(angles.x);
    float psin = sin(angles.x);
    //Y:Yaw
    float ycos = cos(angles.y);
    float ysin = sin(angles.y);
    //Z:Roll
    float rcos = cos(angles.z);
    float rsin = sin(angles.z);

    return float4x4(
        ycos * rcos + ysin * psin * rsin, -ycos * rsin + ysin * psin * rcos, ysin * pcos, 0,
        pcos * rsin, pcos * rcos, -psin, 0,
        -ysin * rcos + ycos * psin * rsin, ysin * rsin + ycos * psin * rcos, ycos * pcos, 0,
        0, 0, 0, 1
    );
}

float2 random2(float2 st, float seed)
{
    float2 s = float2(dot(st, float2(127.1, 311.7)) + seed, dot(st, float2(269.5, 183.3)) + seed);
    return -1.0 + 2.0 * frac(sin(s) * 43758.5453123);
    // -1 < result < 1
}

float gradientNoise(float2 st, float seed)
{
    float2 i = floor(st);
    float2 f = frac(st);

    float2 u = f*f*(3.0-2.0*f);
    return lerp( lerp( dot( random2(i + float2(0.0,0.0), seed ), f - float2(0.0,0.0) ),
                                 dot( random2(i + float2(1.0,0.0), seed ), f - float2(1.0,0.0) ), u.x),
                            lerp( dot( random2(i + float2(0.0,1.0), seed ), f - float2(0.0,1.0) ),
                                 dot( random2(i + float2(1.0,1.0), seed ), f - float2(1.0,1.0) ), u.x), u.y);

}

float rand1(float n)
{
    return frac(sin(n) * 43758.5453);
}

float rand1dynamic(float n, float _Speed, float _RandomSize, float time)
{
    //if want to change dynamic
    //input _Time in UnityCG.cginc
    return _RandomSize * frac(sin(n) * 43758.5453 * _Speed * time);
}

float random(float2 st)
{
    return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
}

float3 rgb2hsv(float3 rgb)
{
    float3 hsv;

    float maxRGB = max(rgb.r, max(rgb.g, rgb.b));
    float minRGB = min(rgb.r, min(rgb.g, rgb.b));

    float delta = maxRGB - minRGB;

    //Value
    hsv.z = maxRGB;

    //Saturation
    if(maxRGB != 0.0)
    {
        hsv.y = delta / maxRGB;
    }
    else
    {
        hsv.y = 0.0;
    }

    //Hue
    if(hsv.y > 0.0)
    {
        if(rgb.r == maxRGB)
        {
            hsv.x = (rgb.g - rgb.b) / delta;
        }
        else if(rgb.g == maxRGB)
        {
            hsv.x = 2 + (rgb.b - rgb.r) / delta;            
        }
        else
        {
            hsv.x = 4 + (rgb.r - rgb.g) / delta;
        }

        hsv.x /= 6.0;
        if(hsv.x < 0)
        {
            hsv.x += 1.0;
        }
    }

    return hsv;
}

float3 hsv2rgb(float3 hsv)
{
    float3 rgb;

    if(hsv.y == 0)
    {
        rgb.r = rgb.g = rgb.b = hsv.z; 
    }
    else
    {
        hsv.x *= 6.0;
        float i = floor (hsv.x);
        float f = hsv.x - i;
        float a = hsv.z * (1 - hsv.y);
        float b = hsv.z * (1 - (hsv.y * f));
        float c = hsv.z * (1 - (hsv.y * (1 - f)));

        if( i < 1 ) {
            rgb.r = hsv.z;
            rgb.g = c;
            rgb.b = a;
        } else if( i < 2 ) {
            rgb.r = b;
            rgb.g = hsv.z;
            rgb.b = a;
        } else if( i < 3 ) {
            rgb.r = a;
            rgb.g = hsv.z;
            rgb.b = c;
        } else if( i < 4 ) {
            rgb.r = a;
            rgb.g = b;
            rgb.b = hsv.z;
        } else if( i < 5 ) {
            rgb.r = c;
            rgb.g = a;
            rgb.b = hsv.z;
        } else {
            rgb.r = hsv.z;
            rgb.g = a;
            rgb.b = b;
        }
    }

    return rgb;
}

float valueNoise(float2 p){
    float2 n = floor(p);
    float2 f = frac(p);

    float a = random(n);
    float b = random(n + float2(1.0, 0.0));
    float c = random(n + float2(0.0, 1.0));
    float d = random(n + float2(1.0, 1.0));

    float2 u = f*f*(3.0 - 2.0*f);
    return lerp(a, b, u.x) + (c-a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

float fbm(float2 st){

    float val = -0.2;
    float amp = 0.428;
    float2 shift = float2(-0.42, -0.44);
    float2x2 rot = float2x2(cos(1.388), sin(2.068),
                            -sin(0.58), cos(1.228));
    for (int i = 0; i < 6; ++i){
        val += amp * valueNoise(st);
        st = mul(rot, st) * 2.5 + shift;
        amp *= 0.788;
    }

    return val;
}
