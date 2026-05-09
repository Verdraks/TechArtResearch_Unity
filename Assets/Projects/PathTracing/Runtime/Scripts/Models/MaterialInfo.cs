using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PathTracer
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct MaterialInfo
    {
        public Color albedo;
        public Vector3 emission;
        public float emissionStrength;
    }
}