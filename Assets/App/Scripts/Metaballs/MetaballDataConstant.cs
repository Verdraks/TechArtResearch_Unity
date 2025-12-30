using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

public static class MetaballDataConstant
{
    public static readonly int S_MetaballDataBufferMat = Shader.PropertyToID("_MetaballDataBuffer");
    public static readonly int S_MetaballCountMat = Shader.PropertyToID("_MetaballCount");

    
    [VFXType(VFXTypeAttribute.Usage.GraphicsBuffer), StructLayout(LayoutKind.Sequential)]
    public struct MetaballData
    {
        public Vector3 Position;
        public float Radius;
    }
}