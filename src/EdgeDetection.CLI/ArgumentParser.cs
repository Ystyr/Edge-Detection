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
            return config;
        }
    }
}