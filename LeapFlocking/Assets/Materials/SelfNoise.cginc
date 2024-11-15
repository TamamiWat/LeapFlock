float hash( float n )
{
    return frac(sin(n)*43758.5453);
}

float2 random2(float2 st)
{
    st = float2(dot(st, float2(127.1, 311.7)),
		dot(st, float2(269.5, 183.3)));
	return -1.0 + 2.0 * frac(sin(st) * 43758.5453123);
}

float random(float2 st)
{
    return frac(sin(dot(st, float2(12.9898,78.233))) * 43758.5453);
}

float perlinNoise(float2 st)
{
    float2 p = floor(st);
    float2 f = frac(st);
    float2 u = f*f*(3.0-2.0*f);

    float v00 = random2(p+fixed2(0,0));
    float v10 = random2(p+fixed2(1,0));
    float v01 = random2(p+fixed2(0,1));
    float v11 = random2(p+fixed2(1,1));

    return lerp( lerp( dot( v00, f - fixed2(0,0) ), dot( v10, f - fixed2(1,0) ), u.x ),
                         lerp( dot( v01, f - fixed2(0,1) ), dot( v11, f - fixed2(1,1) ), u.x ), 
                         u.y)+0.5f;

}

float cellularNoise(float2 st, float scale, float speed, float contrast) 
{
    // Scale
    st *= scale;

    // Tile the space
    float2 ist = floor(st);
    float2 fst = frac(st);

    float distance = 5;

    for (int y = -1; y <= 1; y++)
        for (int x = -1; x <= 1; x++)
        {
            float2 neighbor = float2(x, y);
            float2 p = 0.5 + 0.5 * sin(_Time.y*speed + 6.2831 * random2(ist + neighbor));
            float2 diff = neighbor + p - fst;
            distance = min(distance, length(diff));
        }

    float color = distance * contrast;

    return color;
}

float simplex3DNoise(float3 c)
{
    float3 p = floor(c);
    float3 f = frac(c);

    f       = f*f*(3.0-2.0*f);
    float n = p.x + p.y*57.0 + 113.0*p.z;
    return lerp(lerp(lerp( hash(n+0.0), hash(n+1.0),f.x),
                lerp( hash(n+57.0), hash(n+58.0),f.x),f.y),
                    lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
                        lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);

}

float3 mod289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
float2 mod289(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
float3 permute(float3 x) { return mod289((x * 34.0 + 1.0) * x); }

float simplex2DNoise(float2 v)
{
    const float4 C = float4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                            0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                            -0.577350269189626,  // -1.0 + 2.0 * C.x
                            0.024390243902439); // 1.0 / 41.0

    float2 i = floor(v + dot(v, C.yy));
    float2 x0 = v - i + dot(i, C.xx);

    float2 i1;
    i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    float2 x1 = x0.xy + C.xx - i1;
    float2 x2 = x0.xy + C.zz;

    i = mod289(i);
    float3 p = permute(
        permute(i.y + float3(0.0, i1.y, 1.0))
        + i.x + float3(0.0, i1.x, 1.0));

    float3 m = max(0.5 - float3(dot(x0, x0), dot(x1, x1), dot(x2, x2)), 0.0);
    m = m * m;
    m = m * m;

    float3 x = 2.0 * frac(p * C.www) - 1.0;
    float3 h = abs(x) - 0.5;
    float3 ox = floor(x + 0.5);
    float3 a0 = x - ox;

    m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);

    float3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * float2(x1.x, x2.x) + h.yz * float2(x1.y, x2.y);
    return 130.0 * dot(m, g);
}

float Curve(float src, float factor)
{
    return src - (src - src * src) * -factor;
}

float fbm(float2 p, int octaves, float frequency, float amplitude, float time)
{
    float value = 0.0;
    //float amplitude = 0.5;
    float e = 2.0;
    for (int i = 0; i < octaves; ++i)
    {
        value += amplitude * perlinNoise(p * frequency);
        amplitude *= 0.5;
        p = p * e;
        e *= 0.95;
    }
    return value;
}

float voronoi(float2 uv, int squareNum, float distance, float time)
{
    uv *= squareNum;
    
    float2 i_uv = floor(uv);
    float2 f_uv = frac(uv);
    float2 p_min;
    
    for (int y = -1; y < 1; y++)
        for (int x = -1; x <= 1; x++)
        {
            float2 neighbor = float2(x, y);
            
            float2 p = 0.5 + 0.5 * sin(time + 6.2831 * random2(i_uv + neighbor));
            
            float2 diff = neighbor + p - f_uv;
            
            if(distance > length(diff))
            {
                distance = length(diff);
                p_min = p;

            }
            
        }
    
    p_min.x += sin(time);
    p_min.y += cos(time);
    
    return fixed4(p_min.x, p_min.y, p_min.x + p_min.y, 1);

}

float advancedSimplexNoise(float3 c, int harmonics, float harmonicsSpread, float3 offset)
{
    float noise = 0.0;
    float amplitude = 1.0;
    float frequency = 1.0;

    c += offset;

    for (int i = 0; i < harmonics; i++)
    {
        noise += simplex3DNoise(c * frequency) * amplitude;

        frequency *= harmonicsSpread;
        amplitude *= 0.5;
    }

    return noise;
}