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

    float3 c = InputImage.Load(int3(x, y, 0)).rgb;

    float3 n = InputImage.Load(int3(x, clamp(int(y - 1), 0, Height - 1), 0)).rgb;
    float3 s = InputImage.Load(int3(x, clamp(int(y + 1), 0, Height - 1), 0)).rgb;
    float3 e = InputImage.Load(int3(clamp(int(x + 1), 0, Width - 1), y, 0)).rgb;
    float3 w = InputImage.Load(int3(clamp(int(x - 1), 0, Width - 1), y, 0)).rgb;

    float3 result = (5.0 * c - n - s - e - w) * Amount + c * (1.0 - Amount);

    OutputImage[uint2(x, y)] = float4(saturate(result), 1.0);
}