using System;
using System.Collections.Generic;

namespace EdgeDetection.CLI
{
    /// <summary>
    /// Configuration container for CLI arguments with safe defaults
    /// </summary>
    public class CliConfig
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public string Operator { get; set; } = "sobel";
        public List<string> PreprocessSteps { get; set; } = new();
        public bool UseGpu { get; set; } = true;
    }

    /// <summary>
    /// Command-line parser with validation and error handling
    /// </summary>
    public static class ArgumentParser
    {
        public static CliConfig ParseArgs(string[] args)
        {
            var config = new CliConfig();
            
            /// Sequential argument processing with lookahead
            for (int i = 0; i < args.Length; i++) {
                switch (args[i]) {
                    case "--input":
                        config.InputPath = args[++i].Trim();
                        break;
                    case "--output":
                        config.OutputPath = args[++i].Trim();
                        break;
                    case "--operator":
                        /// Validation for supported operators
                        config.Operator = args[++i].ToLower().Trim();
                        if (config.Operator != "sobel" && config.Operator != "prewitt")
                            throw new ArgumentException("Invalid operator: must be 'sobel' or 'prewitt'");
                        break;
                    case "--preprocess":
                        /// Supports space-separated preprocessing steps
                        string preprocessLine = args[++i].Trim();
                        if (preprocessLine != null || preprocessLine.Length > 0) {
                            string[] preprocessNames = preprocessLine.Split(' ');
                            config.PreprocessSteps.AddRange(preprocessNames);
                        }
                        break;
                    case "--force-cpu":
                        /// GPU opt-out
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