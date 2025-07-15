using Xunit;
using EdgeDetection.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static EdgeDetection.Core.EdgeDetector;

namespace EdgeDetection.Tests
{
    public class OperatorSelectionTests
    {
        [Fact]
        public void Run_ShouldSelectSobelKernel ()
        {
            var input = new Image<Rgba32>(10, 10);
            var result = EdgeDetector.Run(input, OperatorType.Sobel);

            Assert.NotNull(result);
            Assert.Equal(input.Width, result.Width);
            Assert.Equal(input.Height, result.Height);
        }

        [Fact]
        public void Run_ShouldSelectPrewittKernel ()
        {
            var input = new Image<Rgba32>(10, 10);
            var result = EdgeDetector.Run(input, OperatorType.Prewitt);

            Assert.NotNull(result);
            Assert.Equal(input.Width, result.Width);
            Assert.Equal(input.Height, result.Height);
        }
    }
}