using EdgeDetection.Core;
using EdgeDetection.Core.Preprocessors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EdgeDetection.CLI
{
    /// <summary>
    /// Coordinates image processing pipeline from configuration to output
    /// </summary>
    public static class ConsoleApp
    {
        public static void Run (CliConfig config)
        {
            /// Convert CLI strings to strongly-typed enums and processors
            var opereator = Enum.Parse<EdgeDetector.OperatorType>(config.Operator, true);
            var preprocessors = CreatePreprocessors(config.PreprocessSteps);
            using var image = Image.Load<Rgba32>(config.InputPath);
            using var result = EdgeDetector.Run(
                image, opereator, 
                !config.UseGpu, /// GPU toggle inversion (forceCPU param)
                preprocessors
                );
            result.Save(config.OutputPath);
        }

        /// <summary>
        /// Factory method mapping CLI tokens to preprocessor instances with preset intensities
        /// </summary>
        private static IPreprocess[] CreatePreprocessors (List<string> data)
        {
            var preprocessors = new IPreprocess[data.Count()];
            for (int i = 0; i < preprocessors.Length; i++) {
                switch (data[i]) {
                    case "blur": preprocessors[i] = new BlurPreprocessor(1.5f); break;
                    case "sharpen": preprocessors[i] = new SharpenPreprocessor(1.5f); break;
                    case "contrast": preprocessors[i] = new ContrastPreprocessor(2.5f); break;
                    default: break;
                }
            }

            return preprocessors;
        }
    }
}
