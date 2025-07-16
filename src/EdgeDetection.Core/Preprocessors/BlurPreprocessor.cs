using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using EdgeDetection.Core.GPU.Parameters;
using EdgeDetection.Core.GPU.Utils;

namespace EdgeDetection.Core.Preprocessors
{
    /// <summary>
    /// Applies Gaussian blur using ImageSharp (CPU) or custom compute shader (GPU)
    /// </summary>
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
            var key = "BlurCompute";
            var parameters = new ComputeParams.FloatController(
                (uint)input.Width, (uint)input.Height, Amount
                );
            return GPUProcessingManager.Process(input, parameters, key);
        }
    }
}
