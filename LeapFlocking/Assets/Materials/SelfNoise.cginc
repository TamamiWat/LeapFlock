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