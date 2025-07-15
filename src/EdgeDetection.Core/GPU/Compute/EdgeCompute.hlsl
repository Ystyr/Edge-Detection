struct kernel
{
    float4 r0, r1, r2;
};

cbuffer Params : register(b0)
{
    uint Width;
    uint Height;
    kernel HKernel;
    kernel VKernel;
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
    
    const float3 kernelX[3] = {
        HKernel.r0.xyz, HKernel.r1.xyz, HKernel.r2.xyz
    };

    const float3 kernelY[3] = {
        VKernel.r0.xyz, VKernel.r1.xyz, VKernel.r2.xyz
    };

    float3 sumX = float3(0, 0, 0);
    float3 sumY = float3(0, 0, 0);
    
    [unroll]
    for (int j = -1; j <= 1; j++)
    {
        [unroll]
        for (int i = -1; i <= 1; i++)
        {
            int2 coord = int2(x + i, y + j);
            coord = clamp(coord, int2(0, 0), int2(Width - 1, Height - 1));

            float3 sample = InputImage.Load(int3(coord, 0)).rgb;

            sumX += sample * kernelX[j + 1][i + 1];
            sumY += sample * kernelY[j + 1][i + 1];
        }
    }

    float mag = length(sumX + sumY);
    mag = saturate(mag);

    OutputImage[uint2(x, y)] = float4(mag, mag, mag, 1.0);
}