using EdgeDetection.Core.GPU.Utils;
using System;

namespace EdgeDetection.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            GPUProcessingManager.Initialize();

            if (args.Length == 0) {
                Console.WriteLine("EdgeDetection CLI - Interactive Mode");
                Console.WriteLine("------------------------------------");
                Console.Write("Choose operator (sobel/prewitt): ");
                string op = Console.ReadLine();
                Console.Write("Select preprocessors: ");
                string preprocess = Console.ReadLine();
                Console.Write("Forse CPU (y/n):");
                string forceCPU = Console.ReadLine();
                string preprocessFilename = preprocess?.Length > 0? $"_{preprocess?.Replace(' ', '_')}" : "";
                string inputPath = "../../../../../assets/Ros.jpg";
                string outputPath = $"../../../../../assets/Ros_{op}{preprocessFilename}.jpg";

                /// Simulate command-line args
                var arguments = new List<string> {
                    "--input", inputPath,
                    "--output", outputPath,
                    "--operator", op.ToLower(),
                };
                if (preprocess != null && preprocess.Length > 0) {
                    arguments.Add("--preprocess");
                    arguments.Add(preprocess.ToLower());
                }
                if (forceCPU != null && forceCPU == "y") {
                    arguments.Add($"--force-cpu");
                }

                ConsoleApp.Run(ArgumentParser.ParseArgs(arguments.ToArray()));

                return;
            }

            try {
                var config = ArgumentParser.ParseArgs(args);
                ConsoleApp.Run(config);
            }
            catch (ArgumentException ex) {
                Console.WriteLine($"[ERROR] {ex.Message}");
            }
        }
    }
}
