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
                string op = "sobel";//Console.ReadLine();
                Console.Write("Select preprocessors: ");
                string preprocess = "blur";// Console.ReadLine();
                string inputPath = "../../../../../assets/Ros.jpg";
                string outputPath = $"../../../../../assets/Ros_{op}_{preprocess.Replace(' ', '_')}.jpg";

                /// Simulate command-line args
                args = new string[] {
                    "--input", inputPath,
                    "--output", outputPath,
                    "--operator", op.ToLower(),
                    "--preprocess", preprocess.ToLower()
                };
                ConsoleApp.Run(ArgumentParser.ParseArgs(args));

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
