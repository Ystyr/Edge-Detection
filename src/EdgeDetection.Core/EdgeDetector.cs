using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EdgeDetection.Core
{
    public static class EdgeDetector
    {
        public enum OperatorType
        {
            Sobel,
            Prewitt
        }

        public record Kernel
        {
            public int[,] gx;
            public int[,] gy;
        }

        public static Image<Rgba32> Run (Image<Rgba32> input, OperatorType op)
        {
            throw new NotImplementedException ();
        }

        private static Image<Rgba32> DetectEdgesGPU (Image<Rgba32> input, OperatorType op)
        {
            throw new NotImplementedException();
        }

        private static Image<Rgba32> DetectEdgesCPU (Image<Rgba32> input, OperatorType op)
        {
            throw new NotImplementedException();
        }
    }
}
