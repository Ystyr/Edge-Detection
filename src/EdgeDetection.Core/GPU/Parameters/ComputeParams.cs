using System.Numerics;
using System.Runtime.InteropServices;

namespace EdgeDetection.Core.GPU.Parameters
{
    /// <summary>
    /// 3x3 convolution kernel with padding for GPU memory alignment
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Kernel3x3
    {
        public Vector4 r0, r1, r2; /// 'Vector4.W' element ensures 16-byte alignment

        public Kernel3x3 (Vector3 r0, Vector3 r1, Vector3 r2)
        {
            this.r0 = new Vector4(r0, 0); 
            this.r1 = new Vector4(r1, 0); 
            this.r2 = new Vector4(r2, 0);
        }
    }

    public static class ComputeParams
    {
        /// <summary>
        /// Base dimensions struct
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Base
        {
            public uint width;
            public uint height;
            public Base (uint width, uint height)
            {
                this.width = width;
                this.height = height;
            }
        }

        /// <summary>
        /// Parameters for shaders controlled by a single float value (e.g., intensity)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct FloatController
        {
            public uint width;
            public uint height;
            public float amount;
            public FloatController (uint width, uint height, float amount)
            {
                this.width = width;
                this.height = height;
                this.amount = amount;
            }
        }

        /// <summary>
        /// Edge detection parameters with explicit padding for GPU memory alignment.
        /// Contains horizontal and vertical convolution kernels.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Edge
        {
            public uint width;
            public uint height;
            public ulong padding; /// Explicit padding for 16-byte alignment

            public Kernel3x3 vKernel;
            public Kernel3x3 hKernel;

            public Edge (uint width, uint height, Kernel3x3 hKernel, Kernel3x3 vKernel)
            {
                this.width = width;
                this.height = height;
                this.hKernel = hKernel;
                this.vKernel = vKernel;
            }
        }
    }
}
