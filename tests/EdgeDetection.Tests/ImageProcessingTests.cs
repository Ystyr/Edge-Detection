using EdgeDetection.Core;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using static EdgeDetection.Core.EdgeDetector;

namespace EdgeDetection.Tests
{
    public class ImageProcessingTests
    {
        string pathToSamples = "../../../../../assets/";

        [Fact]
        public void EdgeDetector_ShouldProcessImageWithoutErrors ()
        {
            var image = new Image<Rgba32>(10, 10);
            image.Mutate(ctx => ctx.BackgroundColor(Color.White));

            var result = EdgeDetector.Run(image, OperatorType.Sobel);

            Assert.NotNull(result);
            Assert.Equal(image.Width, result.Width);
            Assert.Equal(image.Height, result.Height);
        }

        [Fact]
        public void EdgeDetector_ShouldProcessSampleImage ()
        {
            using var image = Image.Load<Rgba32>(pathToSamples + "fishPie.jpg");

            var result = EdgeDetector.Run(image, OperatorType.Sobel);

            Assert.NotNull(result);
            Assert.Equal(image.Width, result.Width);
        }
    }
}
