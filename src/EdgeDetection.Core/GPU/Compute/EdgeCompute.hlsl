cbuffer Params : register(b0)
{
    uint Width;
    uint Height;
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

    float3 sobelX[3] =
    {
        float3(-1, 0, 1),
        float3(-2, 0, 2),
        float3(-1, 0, 1)
    };

    float3 sobelY[3] =
    {
        float3(1, 2, 1),
        float3(0, 0, 0),
        float3(-1, -2, -1)
    };

    float3 sumX = float3(0, 0, 0);
    float3 sumY = float3(0, 0, 0);

    for (int j = -1; j <= 1; j++)
    {
        for (int i = -1; i <= 1; i++)
        {
            int2 coord = int2(x + i, y + j);
            coord = clamp(coord, int2(0, 0), int2(Width - 1, Height - 1));

            float3 sample = InputImage.Load(int3(coord, 0)).rgb;

            sumX += sample * sobelX[j + 1][i + 1];
            sumY += sample * sobelY[j + 1][i + 1];
        }
    }

    float mag = length(sumX) + length(sumY);
    mag = saturate(mag);

    OutputImage[uint2(x, y)] = float4(mag, mag, mag, 1.0);
}