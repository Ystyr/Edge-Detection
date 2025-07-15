cbuffer Params : register(b0)
{
    uint Width;
    uint Height;
    float Amount;
};

Texture2D<float4> InputImage : register(t0);
RWTexture2D<float4> OutputImage : register(u0);

[numthreads(16, 16, 1)]
void CSMain(uint3 DTid : SV_DispatchThreadID)
{
    uint x = DTid.x;
    uint y = DTid.y;

    if (x >= Width || y >= Height)
        return;

    float3 sum = float3(0, 0, 0);

    [unroll]
    for (int j = -1; j <= 1; j++)
    {
        [unroll]
        for (int i = -1; i <= 1; i++)
        {
            int2 coord = int2(clamp(int(x) + i, 0, Width - 1), clamp(int(y) + j, 0, Height - 1));
            float3 sample = InputImage.Load(int3(coord, 0)).rgb;

            float weight = 1.0;
            if (i == 0 || j == 0)
                weight = 2.0;
            if (i == 0 && j == 0)
                weight = 4.0;

            sum += sample * weight;
        }
    }

    sum /= 16.0;

    float3 c = InputImage.Load(int3(x, y, 0)).rgb;
    float3 result = lerp(c, sum, Amount);

    OutputImage[uint2(x, y)] = float4(saturate(result), 1.0);
}