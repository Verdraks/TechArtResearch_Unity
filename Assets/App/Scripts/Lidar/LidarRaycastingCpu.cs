using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LidarRaycastingCpu : MonoBehaviour
{
    private static readonly int s_BaseColor = Shader.PropertyToID("_BaseColor");

    [Header("Settings")]
    [SerializeField] private float m_DelayLidarRaycast = 0.5f;
    [SerializeField] private float m_DistanceSearching = 20;
    [SerializeField] private int m_Density = 10;
    [SerializeField] private int m_MaxVisiblePoints = 1000;
    [SerializeField] private SerializedDictionary<string, Color> m_ColorObjectDetection;
    
    
    [Header("References")]
    [SerializeField] private Mesh m_MeshPrimitive;
    [SerializeField] private Material m_MaterialPrimitive;

    //Data Rendering
    private Matrix4x4[] m_MatricesPointProjected;
    private Color[] m_ColorsPointProjected;

    private RaycastBatchProcessor.RaycastCommandData m_RaycastCommandData;
    
    //Internals Counters
    private float m_InternalCounter;
    private int m_MatricesPointProjectedIndex;
    
    private void Awake()
    {
        m_MatricesPointProjected = new Matrix4x4[m_MaxVisiblePoints];
        m_ColorsPointProjected = new Color[m_MaxVisiblePoints];
        m_RaycastCommandData = new RaycastBatchProcessor.RaycastCommandData
        {
            Origin = new Vector3[m_MaxVisiblePoints],
            Direction = new Vector3[m_MaxVisiblePoints],
            LayerMask = LayerMask.GetMask("Default"),
            HitTrigger = false,
            MaxDistance = m_DistanceSearching
        };
    }

    private void Update()
    {
        if (m_InternalCounter >= m_DelayLidarRaycast)
        {
            m_InternalCounter = 0;
            CalculateLidarPoints();
        }
        
        ShowPoints();
        m_InternalCounter += Time.deltaTime;
    }
    
    private void CalculateLidarPoints()
    {
        for (int i = 0; i < m_Density; i++)
        {
            var pointW = transform.rotation * Random.insideUnitCircle;
            
            Vector3 pointStart = transform.position + pointW;

            Vector3 pointEnd = pointStart + transform.forward;
            
            m_RaycastCommandData.Origin[i] = pointStart;
            m_RaycastCommandData.Direction[i] = (pointEnd - pointStart).normalized;
        }
        
        RaycastBatchProcessor.Instance.PerformRaycast(m_RaycastCommandData, CollectRaycastPoint());
    }

    private Action<RaycastHit[]> CollectRaycastPoint()
    {
        return hits =>
        {
            foreach (var hit in hits)
            {
                if (!hit.collider) continue;
                
                Color colorFromTag = m_ColorObjectDetection.TryGetValue(hit.collider.tag, out Color c) ? c : Color.white;
                m_ColorsPointProjected[m_MatricesPointProjectedIndex] = colorFromTag;
                
                m_MatricesPointProjected[m_MatricesPointProjectedIndex] =
                    Matrix4x4.TRS(hit.point, Quaternion.identity, Vector3.one * 0.1f);
                m_MatricesPointProjectedIndex = (m_MatricesPointProjectedIndex + 1) % m_MaxVisiblePoints;
            }
        };
    }


    private void ShowPoints()
    {
        
        
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        for (int i = 0; i < m_MaxVisiblePoints; i++)
        {
            mpb.SetColor(s_BaseColor, m_ColorsPointProjected[i]);
            Graphics.DrawMesh(m_MeshPrimitive, m_MatricesPointProjected[i], m_MaterialPrimitive, 0, null, 0, mpb);
        }
        
        // Graphics.DrawMeshInstanced(meshPrimitive, 0, materialPrimitive, _matricesPointProjected);
    }


    private struct PointData
    {
        private Matrix4x4 m_Matrix;
    }
}
