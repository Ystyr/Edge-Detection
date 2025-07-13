using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace EdgeDetection.Core.Preprocessors
{
    public class BlurPreprocessor : PreprocessorBase
    {
        public BlurPreprocessor (float amount = .5f) : base(amount)
        {
        }

        protected override Image<Rgba32> RunCPU (Image<Rgba32> input)
        {
            var output = input.Clone();
            output.Mutate(ctx => ctx.GaussianBlur(Amount)); 
            return output;
        }

        protected override Image<Rgba32> RunGPU (Image<Rgba32> input)
        {
            throw new NotImplementedException();
        }
    }
}
