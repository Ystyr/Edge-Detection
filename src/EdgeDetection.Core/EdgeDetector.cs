using EdgeDetection.Core.Preprocessors;
using SharpGen.Runtime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using static System.Net.Mime.MediaTypeNames;

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

        static readonly Kernel Prewitt = new Kernel {
            gx = new int[,] {
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0, 1 }
            },
            gy = new int[,] {
            {  1,  1,  1 },
            {  0,  0,  0 },
            { -1, -1, -1 }
            }
        };

        static readonly Dictionary<OperatorType, Kernel> Kernels = new Dictionary<OperatorType, Kernel>() {
            { OperatorType.Sobel, Sobel}, { OperatorType.Prewitt, Prewitt}
        };

        public static Image<Rgba32> Run (Image<Rgba32> input, OperatorType op, params IPreprocess[] preprocessors)
        {
            var result = ApplyPreprocesing(input, preprocessors);
            var sw = Stopwatch.StartNew();
            result = DetectEdges(result, op);
            sw.Stop();
            Console.WriteLine($"[INFO] Edge Detection took {sw.ElapsedMilliseconds} ms."); 
			return result;
        }

        private static Image<Rgba32> ApplyPreprocesing (Image<Rgba32> input, IPreprocess[] preprocessors)
        {
            Image<Rgba32> result = input;
            foreach (var item in preprocessors) {
                result = item.Run(result);
            }
            return result;
        }

        private static Image<Rgba32> DetectEdges (Image<Rgba32> input, OperatorType op)
        {
            try {
                return DetectEdgesGPU(input, op);
            }
            catch (Exception ex) {
                Console.WriteLine($"[WARN] {nameof(DetectEdges)} GPU failed: {ex.Message}, executing CPU fallback");
                return DetectEdgesCPU(input, op);
            }
        }

        private static Image<Rgba32> DetectEdgesGPU (Image<Rgba32> input, OperatorType op)
        {
            throw new NotImplementedException();
        }

        private static Image<Rgba32> DetectEdgesCPU (Image<Rgba32> input, OperatorType op)
        {
            /// Convert to grayscale using luminance formula
            int GetIntensity (Rgba32 pixel) => (int)(
                0.299f * pixel.R + 0.587f * pixel.G + 0.114f * pixel.B
                );
            
            int width = input.Width;
            int height = input.Height;
            var output = new Image<Rgba32>(width, height);
            var kernel = Kernels[op];

            for (int y = 1; y < height - 1; y++) {
                for (int x = 1; x < width - 1; x++) {
                    int sumX = 0;
                    int sumY = 0;
                    for (int j = -1; j <= 1; j++) {
                        for (int i = -1; i <= 1; i++) {
                            var pixel = input[x + i, y + j];
                            int intensity = GetIntensity(pixel);
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
