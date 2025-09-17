using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LidarRaycastingCpu : MonoBehaviour
{
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    [Header("Settings")]
    [SerializeField] private float delayLidarRaycast = 0.5f;
    [SerializeField] private float distanceSearching = 20;
    [SerializeField] private int density = 10;
    [SerializeField] private int maxVisiblePoints = 1000;
    [SerializeField] private SerializedDictionary<string, Color> colorObjectDetection;
    
    
    [Header("References")]
    [SerializeField] private Mesh meshPrimitive;
    [SerializeField] private Material materialPrimitive;

    //Data Rendering
    private Matrix4x4[] _matricesPointProjected;
    private Color[] _colorsPointProjected;

    private RaycastBatchProcessor.RaycastCommandData _raycastCommandData;
    
    //Internals Counters
    private float _internalCounter;
    private int _matricesPointProjectedIndex;
    
    private void Awake()
    {
        _matricesPointProjected = new Matrix4x4[maxVisiblePoints];
        _colorsPointProjected = new Color[maxVisiblePoints];
        _raycastCommandData = new RaycastBatchProcessor.RaycastCommandData
        {
            Origin = new Vector3[maxVisiblePoints],
            Direction = new Vector3[maxVisiblePoints],
            LayerMask = LayerMask.GetMask("Default"),
            HitTrigger = false,
            MaxDistance = distanceSearching
        };
    }

    private void Update()
    {
        if (_internalCounter >= delayLidarRaycast)
        {
            _internalCounter = 0;
            CalculateLidarPoints();
        }
        
        ShowPoints();
        _internalCounter += Time.deltaTime;
    }
    
    private void CalculateLidarPoints()
    {
        for (int i = 0; i < density; i++)
        {
            var pointW = transform.rotation * Random.insideUnitCircle;
            
            Vector3 pointStart = transform.position + pointW;

            Vector3 pointEnd = pointStart + transform.forward;
            
            _raycastCommandData.Origin[i] = pointStart;
            _raycastCommandData.Direction[i] = (pointEnd - pointStart).normalized;
        }
        
        RaycastBatchProcessor.instance.PerformRaycast(_raycastCommandData, CollectRaycastPoint());
    }

    private Action<RaycastHit[]> CollectRaycastPoint()
    {
        return hits =>
        {
            foreach (var hit in hits)
            {
                if (!hit.collider) continue;
                
                Color colorFromTag = colorObjectDetection.TryGetValue(hit.collider.tag, out var c) ? c : Color.white;
                _colorsPointProjected[_matricesPointProjectedIndex] = colorFromTag;
                
                _matricesPointProjected[_matricesPointProjectedIndex] =
                    Matrix4x4.TRS(hit.point, Quaternion.identity, Vector3.one * 0.1f);
                _matricesPointProjectedIndex = (_matricesPointProjectedIndex + 1) % maxVisiblePoints;
            }
        };
    }


    private void ShowPoints()
    {
        
        
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        for (int i = 0; i < maxVisiblePoints; i++)
        {
            mpb.SetColor(BaseColor, _colorsPointProjected[i]);
            Graphics.DrawMesh(meshPrimitive, _matricesPointProjected[i], materialPrimitive, 0, null, 0, mpb);
        }
        
        // Graphics.DrawMeshInstanced(meshPrimitive, 0, materialPrimitive, _matricesPointProjected);
    }


    private struct PointData
    {
        private Matrix4x4 _matrix;
    }
}
