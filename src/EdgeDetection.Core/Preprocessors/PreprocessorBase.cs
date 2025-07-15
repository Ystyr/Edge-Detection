using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.Diagnostics;

namespace EdgeDetection.Core.Preprocessors
{
    public abstract class PreprocessorBase : IPreprocess
    {
        public float Amount { get; private set; }

        protected PreprocessorBase (float amount)
        {
            Amount = amount;
        }

        public Image<Rgba32> Run (Image<Rgba32> input, bool forceCPU = false)
        {
            var sw = Stopwatch.StartNew();
            var result = Process(input, forceCPU);
            sw.Stop();
            Console.WriteLine($"[INFO] Preprocessing {GetType()} took {sw.ElapsedMilliseconds} ms.");
            return result;
        }

        private Image<Rgba32> Process (Image<Rgba32> input, bool forceCPU = false)
        {
            try {
                if (forceCPU) {
                    Console.WriteLine($"[INFO] {(GetType())}.{nameof(Process)}, forced CPU.");
                    return RunCPU(input);
                }
                return RunGPU(input);
            }
            catch (Exception ex) {
                Console.WriteLine($"[WARN] {GetType()} GPU failed: {ex.Message}, executing CPU fallback");
                return RunCPU(input);
            }
        }

        protected abstract Image<Rgba32> RunCPU (Image<Rgba32> input);

        protected abstract Image<Rgba32> RunGPU (Image<Rgba32> input);
    }
}
