using System.Runtime.InteropServices;
using UnityEngine;

namespace RayTracing.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Triangle
    {
        public Vector3 v0;
        public Vector3 v1;
        public Vector3 v2;
    }
}