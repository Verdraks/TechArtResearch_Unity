using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RayTracing.Runtime
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct MaterialInfo
    {
        public Color albedo;
        public Vector3 emissive;
        public float emissiveStrength;
    }
}