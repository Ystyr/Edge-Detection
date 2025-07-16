using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using EdgeDetection.Core.GPU.Parameters;
using EdgeDetection.Core.GPU.Utils;

namespace EdgeDetection.Core.Preprocessors
{
    /// <summary>
    /// Applies contrast ajustments using ImageSharp (CPU) or custom compute shader (GPU)
    /// </summary>
    public class ContrastPreprocessor : PreprocessorBase
    {
        public ContrastPreprocessor (float amount = .5f) : base(amount)
        {
        }

        protected override Image<Rgba32> RunCPU (Image<Rgba32> input)
        {
            var output = input.Clone();
            output.Mutate(ctx => ctx.Contrast(Amount));
            return output;
        }

        protected override Image<Rgba32> RunGPU (Image<Rgba32> input)
        {
            var key = "ContrastCompute";
            var parameters = new ComputeParams.FloatController(
                (uint)input.Width, (uint)input.Height, Amount
                );
            return GPUProcessingManager.Process(input, parameters, key);
        }
    }
}
