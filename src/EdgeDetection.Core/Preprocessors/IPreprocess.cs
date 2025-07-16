using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace EdgeDetection.Core.Preprocessors
{
    /// <summary>
    /// Contract for image preprocessing operations
    /// </summary>
    public interface IPreprocess
    {
        Image<Rgba32> Run (Image<Rgba32> input, bool forceCPU = false);
    }
}
