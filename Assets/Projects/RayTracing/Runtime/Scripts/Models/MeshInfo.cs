using System.Runtime.InteropServices;

namespace RayTracing.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshInfo
    {
        public MaterialInfo mat;
        public uint firstTriangleIndex;
        public uint trianglesCount;
    }
}