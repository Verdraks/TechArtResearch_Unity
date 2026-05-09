using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathTracer
{
    [ExecuteInEditMode]
    public class PathTracingMeshRenderer : MonoBehaviour
    {
        public static readonly List<PathTracingMeshRenderer> Elements = new(capacity:1024);
        
        [Header("Settings")]
        [SerializeField] private MaterialInfo m_Data;
        
        [Header("References")]
        [SerializeField] private MeshFilter m_MeshFilter;
        
        private Triangle[] m_LocalTriangles;
        private Triangle[] m_WorldTriangles;
        private int m_TrianglesCount;
        
        public MeshInfo MeshInfo
        {
            get
            {
                MeshInfo meshInfo = new MeshInfo
                {
                    mat = m_Data,
                    trianglesCount = (uint)m_TrianglesCount,
                };
                
                return meshInfo;
            }
        }
        
        public Triangle[] Triangles
        {
            get
            {
                if (m_LocalTriangles == null)
                {
                    int trisCount = m_MeshFilter.sharedMesh.triangles.Length/3;
                    m_LocalTriangles = new Triangle[trisCount];
                    Vector3[] vertices = m_MeshFilter.sharedMesh.vertices;
                    for (int i = 0; i < trisCount; i++)
                    {
                        m_LocalTriangles[i] = new Triangle
                        {
                            v0 = vertices[m_MeshFilter.sharedMesh.triangles[i*3]],
                            v1 = vertices[m_MeshFilter.sharedMesh.triangles[1 + i*3]],
                            v2 = vertices[m_MeshFilter.sharedMesh.triangles[2 + i*3]],
                        };
                    }
                }
                
                if (m_WorldTriangles == null)
                {
                    m_WorldTriangles = new Triangle[m_LocalTriangles.Length];
                }
                
                Array.Clear(m_WorldTriangles, 0, m_WorldTriangles.Length);
                for (var i = 0; i < m_LocalTriangles.Length; i++)
                {
                    m_WorldTriangles[i] = LocalToWorld(m_LocalTriangles[i]);
                }

                return m_WorldTriangles;
            }
        }

        private Triangle LocalToWorld(Triangle localTriangle)
        {
            Triangle worldTriangle = new Triangle
            {
                v0 = transform.TransformPoint(localTriangle.v0),
                v1 = transform.TransformPoint(localTriangle.v1),
                v2 = transform.TransformPoint(localTriangle.v2)
            };

            return worldTriangle;
        }
        
        private void OnEnable()
        {
            Elements.Add(this);
        }

        private void OnValidate()
        {
            m_TrianglesCount = m_MeshFilter.sharedMesh.triangles.Length/3;
        }

        private void OnDisable()
        {
            Elements.Remove(this);
            m_LocalTriangles = null;
            m_WorldTriangles = null;
        }
    }
}