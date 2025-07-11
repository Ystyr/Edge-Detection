using System;

namespace EdgeDetection.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine("EdgeDetection CLI - Interactive Mode");
                Console.WriteLine("------------------------------------");
                Console.Write("Choose operator (sobel/prewitt): ");
                string op = Console.ReadLine();
                string inputPath = "../../../../../assets/fishPie.jpg";
                string outputPath = $"../../../../../assets/fishPie_{op}.jpg";

                /// Simulate command-line args
                args = new string[] {
                    "--input", inputPath,
                    "--output", outputPath,
                    "--operator", op.ToLower()
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
