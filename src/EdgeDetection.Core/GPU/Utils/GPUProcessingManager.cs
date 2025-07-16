using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Veldrid;
using System.Runtime.InteropServices;

namespace EdgeDetection.Core.GPU.Utils
{
    /// <summary>
    /// Manages GPU compute pipelines: shader loading, resource setup, dispatch & readback.
    /// </summary>
    public static class GPUProcessingManager
    {
        const uint NumThread = 16; /// Matches [numthreads(16,16,1)] in HLSL

        static GraphicsDevice _device;
        static ResourceLayout _layout;
        static Dictionary<string, Shader> _shaders;

        /// <summary>
        /// Must be called once before any Process() calls.
        /// </summary>
        public static void Initialize ()
        {
            InitializeGraphicsDevice();
            InitializeCompute();
            InitializeLayout();
        }

        /// <summary>
        /// Runs the named compute shader (by key) on the input image, using TParams as the uniform buffer.
        /// </summary>
        public static Image<Rgba32> Process<TParams> (Image<Rgba32> input, TParams parameters, string key)
            where TParams : unmanaged
        {
            uint width = (uint)input.Width;
            uint height = (uint)input.Height;
            
            /// Lookup precompiled shader & create pipeline
            var shader = _shaders[key];
            /// Lookup precompiled shader & create pipeline
            var pipeline = CreatePipeline(shader, _layout);
            /// Create & fill uniform buffer (auto‑aligned to 16 bytes)
            var buffer = CreateBuffer(parameters);
            /// Allocate textures for input/output
            var maps = GetMaps(input);
            /// Bind uniforms + textures
            var resources = CreateResourceSet(buffer, maps.src, maps.result);
            /// Upload pixel data to GPU
            var texBytes = GetTextureBytes(input);
            _device.UpdateTexture(
                maps.src, texBytes, 0, 0, 0, 
                width, height, 1, 0, 0
                );
            /// Dispatch and read back into a new Image<Rgba32>
            return ExecuteGPUProgram(
                pipeline, resources, maps.result, 
                width, height
                );
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
            /// Headless D3D11 for compute-only
            _device = GraphicsDevice.CreateD3D11(new GraphicsDeviceOptions());
        }

        private static void InitializeCompute ()
        {
            /// Load all .cso files under GPU/Compute
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
            /// Uniform + input texture + output texture
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

        private static DeviceBuffer CreateBuffer<TParams> (TParams parameters)
            where TParams : unmanaged
        {
            /// Compute size of TParams and round up to 16-byte boundary
            uint rawSize = (uint)Marshal.SizeOf<TParams>();
            uint alignedSize = ((rawSize + 15) / 16) * 16;
            
            var buffer = _device.ResourceFactory.CreateBuffer(
                new BufferDescription(alignedSize, BufferUsage.UniformBuffer)
                );

            /// Upload entire struct at offset 0
            _device.UpdateBuffer(buffer, 0, ref parameters);

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
            /// Record & dispatch
            var commandList = _device.ResourceFactory.CreateCommandList();
            commandList.Begin();
            commandList.SetPipeline(pipeline);
            commandList.SetComputeResourceSet(0, resources);
            commandList.Dispatch((uint)Math.Ceiling(width / numthread), (uint)Math.Ceiling(height / numthread), 1);
            commandList.CopyTexture(destination, stagingTex);
            commandList.End();

            _device.SubmitCommands(commandList);
            _device.WaitForIdle();

            commandList.Dispose();

            /// Map & convert to Image<Rgba32>
            var map = _device.Map(stagingTex, MapMode.Read);
            var result = CreateImage(map, (int)width, (int)height);
            _device.Unmap(stagingTex);

            return result;
        }

        /// <summary>
        /// Fast path: if memory is contiguous, reinterpret as bytes.
        /// Otherwise, safe row-by-row copy.
        /// </summary>
        private static byte[] GetTextureBytes (Image<Rgba32> input)
        {
            int width = input.Width;
            int height = input.Height;
            byte[] data = new byte[width * height * 4];

            if (input.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> pixelMemory)) {
                return MemoryMarshal.AsBytes(pixelMemory.Span).ToArray();
            }

            input.ProcessPixelRows(accessor => {
                for (int y = 0; y < accessor.Height; y++) {
                    var row = accessor.GetRowSpan(y);
                    var rowBytes = MemoryMarshal.AsBytes(row);

                    Buffer.BlockCopy(rowBytes.ToArray(), 0, data, y * width * 4, width * 4);
                }
            });

            return data;
        }

        /// <summary>
        /// Copy readback data row-by-row into a flat pixel array, then load into Image.
        /// </summary>
        private static Image<Rgba32> CreateImage (MappedResource map, int width, int height)
        {
            int rowPitch = (int)map.RowPitch;
            byte[] rowBytes = new byte[rowPitch];
            byte[] pixelBytes = new byte[width * height * 4];

            for (int y = 0; y < height; y++) {
                var rowPtr = IntPtr.Add(map.Data, y * rowPitch);
                Marshal.Copy(rowPtr, rowBytes, 0, rowPitch);
                /// Only copy the first width*4 bytes of each row
                Buffer.BlockCopy(rowBytes, 0, pixelBytes, y * width * 4, width * 4);
            }
            return Image.LoadPixelData<Rgba32>(pixelBytes, width, height);
        }
    }
}
