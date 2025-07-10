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

        static readonly Kernel Sobel = new Kernel {
            gx = new int[,] {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
            },
            gy = new int[,] {
            {  1,  2,  1 },
            {  0,  0,  0 },
            { -1, -2, -1 }
            }
        };

        public static Image<Rgba32> Run (Image<Rgba32> input, OperatorType op)
        {
            try {
                return DetectEdgesGPU(input, op);
            }
            catch (Exception ex) {
                Console.WriteLine($"[WARN] GPU failed: {ex.Message}, CPU fallback");
                return DetectEdgesCPU(input, op);
            }
        }

        private static Image<Rgba32> DetectEdgesGPU (Image<Rgba32> input, OperatorType op)
        {
            throw new NotImplementedException();
        }

        private static Image<Rgba32> DetectEdgesCPU (Image<Rgba32> input, OperatorType op)
        {
            int width = input.Width;
            int height = input.Height;
            var output = new Image<Rgba32>(width, height);
            var kernel = Sobel;

            for (int y = 1; y < height - 1; y++) {
                for (int x = 1; x < width - 1; x++) {
                    int sumX = 0;
                    int sumY = 0;

                    for (int j = -1; j <= 1; j++) {
                        for (int i = -1; i <= 1; i++) {
                            var pixel = input[x + i, y + j];
                            int intensity = (int)(0.299f * pixel.R + 0.587f * pixel.G + 0.114f * pixel.B);
                            sumX += intensity * kernel.gx[j + 1, i + 1];
                            sumY += intensity * kernel.gy[j + 1, i + 1];
                        }
                    }

                    int magnitude = (int)Math.Sqrt(sumX * sumX + sumY * sumY);
                    byte edge = (byte)Math.Clamp(magnitude, 0, 255);

                    output[x, y] = new Rgba32(edge, edge, edge, 255);
                }
            }

            return output;
        }
    }
}
