using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RayTracing.Runtime
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Sphere
    {
        public MaterialInfo material;
        [HideInInspector] public Vector3 center;
        public float radius;
    }
}