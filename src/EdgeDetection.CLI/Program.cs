using EdgeDetection.Core.GPU.Utils;

namespace EdgeDetection.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            GPUProcessingManager.Initialize(); /// Critical GPU setup

            if (args.Length == 0) {
                /// Interactive mode for user-friendly testing
                Console.WriteLine("EdgeDetection CLI - Interactive Mode");
                Console.WriteLine("------------------------------------");
                Console.Write("Choose operator (sobel/prewitt): ");
                string op = Console.ReadLine();
                Console.Write("Enter optional preprocessing steps (space-separated): ");
                string preprocess = Console.ReadLine();
                Console.Write("Forse CPU (y/n):");
                string forceCPU = Console.ReadLine();
                string preprocessFilename = preprocess?.Length > 0 ? $"_{preprocess?.Replace(' ', '_')}" : "";
                Console.Write("Enter source image path:");
                string inputPath = Console.ReadLine();
                Console.Write("Enter output file path:");
                string outputPath = Console.ReadLine();

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

            /// Standard command-line processing
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
