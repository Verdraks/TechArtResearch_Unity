using System.Runtime.InteropServices;

namespace PathTracer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshInfo
    {
        public MaterialInfo mat;
        public uint firstTriangleIndex;
        public uint trianglesCount;
    }
}