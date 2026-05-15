using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RayTracing.Runtime
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct MaterialInfo
    {
        public Vector3 albedo;
        public float emissiveStrength;
        public Vector3 emissive;
        [Range(0,1)]public float metallicFuzz;
        public float refractiveIndex;
        public MaterialType materialType;
    }
    
    public enum MaterialType
    {
        Diffuse = 0,
        Metallic,
        Dielectric
    }
}