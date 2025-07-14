using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Veldrid;

namespace EdgeDetection.Core.GPU.Utils
{
    public static class GPUProcessingManager
    {
        static GraphicsDevice _device;
        static ResourceLayout _layout;
        static Dictionary<string, Shader> _shaders;

        public static void Initialize ()
        {
            InitializeGraphicsDevice();
            InitializeCompute();
        }

        public static Image<Rgba32> Process (Image<Rgba32> input, string name)
        {
            throw new NotImplementedException();
        }

        private static void InitializeGraphicsDevice ()
        {

        }

        private static void InitializeCompute ()
        {

        }

        private static (Texture src, Texture result) InitializeMaps (Image<Rgba32> input)
        {
            throw new NotImplementedException();
        }

        private static DeviceBuffer CreateBuffer (uint width, uint height)
        {
            throw new NotImplementedException();
        }

        private static Pipeline CreatePipeline (Shader shader, ResourceLayout layout)
        {
            throw new NotImplementedException();
        }

        private static ResourceSet CreateResourceSet ()
        {
            throw new NotImplementedException();
        }

        private static Image<Rgba32> ExecuteGPGPU (Pipeline pipeline, ResourceSet resources, int width, int height)
        {
            throw new NotImplementedException();
        }

        private static byte[] GetTextureData (Image<Rgba32> input)
        {
            throw new NotImplementedException ();
        }
    }
}
