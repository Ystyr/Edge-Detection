using EdgeDetection.Core.GPU.Parameters;
using EdgeDetection.Core.GPU.Utils;
using EdgeDetection.Core.Preprocessors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Numerics;

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

            public static Kernel3x3 ToKernel3x3 (int[,] value)
            {
                return new Kernel3x3(
                    new Vector3(value[0, 0], value[0, 1], value[0, 2]),
                    new Vector3(value[1, 0], value[1, 1], value[1, 2]),
                    new Vector3(value[2, 0], value[2, 1], value[2, 2])
                    );
            }
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

        public static Image<Rgba32> Run (Image<Rgba32> input, OperatorType op, bool forceCPU = false, params IPreprocess[] preprocessors)
        {
            var result = ApplyPreprocesing(input, preprocessors, forceCPU);
            var sw = Stopwatch.StartNew();
            result = DetectEdges(result, op, forceCPU);
            sw.Stop();
            Console.WriteLine($"[INFO] Edge Detection took {sw.ElapsedMilliseconds} ms."); 
			return result;
        }

        private static Image<Rgba32> ApplyPreprocesing (Image<Rgba32> input, IPreprocess[] preprocessors, bool forceCPU = false)
        {
            Image<Rgba32> result = input;
            foreach (var item in preprocessors) {
                result = item.Run(result, forceCPU);
            }
            return result;
        }

        private static Image<Rgba32> DetectEdges (Image<Rgba32> input, OperatorType op, bool forceCPU = false)
        {
            try {
                if (forceCPU) {
                    Console.WriteLine($"[INFO] {nameof(DetectEdges)}, forced CPU.");
                    return DetectEdgesCPU(input, op);
                }
                return DetectEdgesGPU(input, op);
            }
            catch (Exception ex) {
                Console.WriteLine($"[WARN] {nameof(DetectEdges)} GPU failed: {ex.Message}, executing CPU fallback");
                return DetectEdgesCPU(input, op);
            }
        }

        private static Image<Rgba32> DetectEdgesGPU (Image<Rgba32> input, OperatorType op)
        {
            var key = "EdgeCompute";
            var kernel = Kernels[op];
            var parameters = new ComputeParams.Edge(
                (uint)input.Width, (uint)input.Height,
                Kernel.ToKernel3x3(kernel.gx), Kernel.ToKernel3x3(kernel.gy)
                );
            return GPUProcessingManager.Process(input, parameters, key);
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
