using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PathTracer
{
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Sphere
    {
        [HideInInspector] public Vector3 center;
        public float radius;
        public MaterialInfo material;
    }
}