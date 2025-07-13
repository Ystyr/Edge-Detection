using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace EdgeDetection.Core.Preprocessors
{
    public interface IPreprocess
    {
        Image<Rgba32> Run (Image<Rgba32> input);
    }
}
