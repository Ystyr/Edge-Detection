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

    float4 color = InputImage.Load(int3(x, y, 0));

    color.rgb = (color.rgb - 0.5) * Amount + 0.5;

    OutputImage[uint2(x, y)] = saturate(color);
}
