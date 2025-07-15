using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Veldrid;
using System.Runtime.InteropServices;

namespace EdgeDetection.Core.GPU.Utils
{
    public static class GPUProcessingManager
    {
        const uint NumThread = 16;

        static GraphicsDevice _device;
        static ResourceLayout _layout;
        static CommandList _commandList;
        static Dictionary<string, Shader> _shaders;

        public static void Initialize ()
        {
            InitializeGraphicsDevice();
            InitializeCompute();
            InitializeLayout();
            _commandList = _device.ResourceFactory.CreateCommandList();
        }

        public static Image<Rgba32> Process (Image<Rgba32> input, string key)
        {
            uint width = (uint)input.Width;
            uint height = (uint)input.Height;
            var shader = _shaders[key];
            var pipeline = CreatePipeline(shader, _layout);
            var buffer = CreateBuffer(width, height);
            var maps = GetMaps(input);
            var resources = CreateResourceSet(buffer, maps.src, maps.result);
            var texBytes = GetTextureBytes(input);
            
            _device.UpdateTexture(
                maps.src, texBytes, 0, 0, 0, width, height, 1, 0, 0
                );

            return ExecuteGPUProgram(pipeline, resources, maps.result, width, height);
        }

        private static void InitializeGraphicsDevice ()
        {
            var options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: null,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferStandardClipSpaceYDirection: false,
                preferDepthRangeZeroToOne: true
            );

            _device = GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions());
        }

        private static void InitializeCompute ()
        {
            string workingDirectory = Environment.CurrentDirectory;
            string rootDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;
            string folderPath = rootDirectory + "\\EdgeDetection.Core\\GPU\\Compute\\";
            Shader LoadAndCreate (string filename)
            {
                string shaderPath = Path.Combine(folderPath, filename + ".cso");
                byte[] shaderBytes = File.ReadAllBytes(shaderPath);
                var shaderDesc = new ShaderDescription(
                    ShaderStages.Compute, shaderBytes, "CSMain"
                );
                return _device.ResourceFactory.CreateShader(shaderDesc);
            }
            string[] filenames = Directory.GetFiles(folderPath, "*.cso", SearchOption.TopDirectoryOnly)
                .Select(s => Path.GetFileNameWithoutExtension(s))
                .ToArray();
            _shaders = new Dictionary<string, Shader>();
            foreach (var name in filenames) {
                _shaders.Add(name.Split('.')[0], LoadAndCreate(name));
            }
        }

        private static void InitializeLayout ()
        {
            _layout = _device.ResourceFactory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Params", ResourceKind.UniformBuffer, ShaderStages.Compute),
                new ResourceLayoutElementDescription("InputImage", ResourceKind.TextureReadOnly, ShaderStages.Compute),
                new ResourceLayoutElementDescription("OutputImage", ResourceKind.TextureReadWrite, ShaderStages.Compute)
            ));
        }

        private static (Texture src, Texture result) GetMaps (Image<Rgba32> input)
        {
            var factory = _device.ResourceFactory;
            var texDesc = TextureDescription.Texture2D(
                (uint)input.Width,
                (uint)input.Height,
                mipLevels: 1,
                arrayLayers: 1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled | TextureUsage.Storage
            );

            var srcTex = factory.CreateTexture(texDesc);
            var resultTex = factory.CreateTexture(texDesc);

            return (srcTex, resultTex);
        }

        private static DeviceBuffer CreateBuffer (uint width, uint height)
        {
            var buffer = _device.ResourceFactory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));
            _device.UpdateBuffer(buffer, 0, BitConverter.GetBytes(width));
            _device.UpdateBuffer(buffer, 4, BitConverter.GetBytes(height));

            return buffer;
        }

        private static Pipeline CreatePipeline (Shader shader, ResourceLayout layout)
        {
            return _device.ResourceFactory.CreateComputePipeline(
                new ComputePipelineDescription(
                    shader, layout, NumThread, NumThread, 1
                    )
                );
        }

        private static ResourceSet CreateResourceSet (DeviceBuffer buffer, Texture input, Texture output)
        {
            var factory = _device.ResourceFactory;
            var inputView = factory.CreateTextureView(input);
            var outputView = factory.CreateTextureView(output);
            var resourceSet = factory.CreateResourceSet(
                new ResourceSetDescription(
                    _layout, buffer, inputView, outputView
                    )
                );

            return resourceSet;
        }

        private static Image<Rgba32> ExecuteGPUProgram (
            Pipeline pipeline, ResourceSet resources, Texture destination, 
            uint width, uint height)
        {
            var factory = _device.ResourceFactory;
            uint size = width * height * 4;
            var numthread = (float)NumThread;
            var readbackBuffer = factory.CreateBuffer(new BufferDescription(size, BufferUsage.Staging));
            var stagingTex = factory.CreateTexture(TextureDescription.Texture2D(
                width, height, mipLevels: 1, arrayLayers: 1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Staging
            ));
            
            _commandList.Begin();
            _commandList.SetPipeline(pipeline);
            _commandList.SetComputeResourceSet(0, resources);
            _commandList.Dispatch((uint)Math.Ceiling(width / numthread), (uint)Math.Ceiling(height / numthread), 1);
            _commandList.CopyTexture(destination, stagingTex);
            _commandList.End();

            _device.SubmitCommands(_commandList);
            _device.WaitForIdle();

            _commandList.Dispose();

            var map = _device.Map(stagingTex, MapMode.Read);

            _device.Unmap(stagingTex);

            return CreateImage(map, (int)width, (int)height);
        }

        private static byte[] GetTextureBytes (Image<Rgba32> input)
        {
            int width = input.Width;
            int height = input.Height;
            byte[] data = new byte[width * height * 4];

            if (input.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixelMemory)) {
                /// Fast path: contiguous buffer, reinterpret as byte[]
                return MemoryMarshal.AsBytes(pixelMemory.Span).ToArray();
            }

            /// Safe fallback: row-by-row copy
            input.ProcessPixelRows(accessor => {
                for (int y = 0; y < accessor.Height; y++) {
                    var row = accessor.GetRowSpan(y);
                    var rowBytes = MemoryMarshal.AsBytes(row);

                    Buffer.BlockCopy(rowBytes.ToArray(), 0, data, y * width * 4, width * 4);
                }
            });

            return data;
        }

        private static Image<Rgba32> CreateImage (MappedResource map, int width, int height)
        {
            int rowPitch = (int)map.RowPitch;
            byte[] rowBytes = new byte[rowPitch];
            byte[] pixelBytes = new byte[width * height * 4];

            /// Copy row-by-row contagiously
            for (int y = 0; y < height; y++) {
                var rowPtr = IntPtr.Add(map.Data, y * rowPitch);
                Marshal.Copy(rowPtr, rowBytes, 0, rowPitch);
                Buffer.BlockCopy(rowBytes, 0, pixelBytes, y * width * 4, width * 4);
            }
            return Image.LoadPixelData<Rgba32>(pixelBytes, width, height);
        }
    }
}
