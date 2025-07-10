using EdgeDetection.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace EdgeDetection.CLI
{
    public static class ConsoleApp
    {
        public static void Run (CliConfig config)
        {
            using var image = Image.Load<Rgba32>(config.InputPath);
            using var result = EdgeDetector.Run(image, Enum.Parse<EdgeDetector.OperatorType>(config.Operator, true));
            result.Save(config.OutputPath);
        }
    }
}
