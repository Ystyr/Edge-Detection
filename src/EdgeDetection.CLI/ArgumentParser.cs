using System;
using System.Collections.Generic;

namespace EdgeDetection.CLI
{
    public class CliConfig
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public string Operator { get; set; } = "sobel";
        public List<string> PreprocessSteps { get; set; } = new();
        public bool UseGpu { get; set; } = true;
    }

    public static class ArgumentParser
    {
        public static CliConfig ParseArgs(string[] args)
        {
            var config = new CliConfig();

            for (int i = 0; i < args.Length; i++) {
                switch (args[i]) {
                    case "--input":
                        config.InputPath = args[++i].Trim();
                        break;
                    case "--output":
                        config.OutputPath = args[++i].Trim();
                        break;
                    case "--operator":
                        config.Operator = args[++i].ToLower().Trim();
                        if (config.Operator != "sobel" && config.Operator != "prewitt")
                            throw new ArgumentException("Invalid operator: must be 'sobel' or 'prewitt'");
                        break;
                    case "--preprocess":
                        config.PreprocessSteps.AddRange(args[++i].Split(' '));
                        break;
                    case "--force-cpu":
                        config.UseGpu = false;
                        break;
                    default:
                        throw new ArgumentException($"Unknown argument: {args[i]}");
                }
            }

            if (string.IsNullOrEmpty(config.InputPath) || string.IsNullOrEmpty(config.OutputPath))
                throw new ArgumentException("Missing required --input or --output");

            return config;
        }
    }
}