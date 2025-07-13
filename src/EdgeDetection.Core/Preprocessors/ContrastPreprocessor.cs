using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;

namespace EdgeDetection.Core.Preprocessors
{
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
            throw new NotImplementedException();
        }
    }
}
