using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class RaycastBatchProcessor : MonoBehaviour
{
    public static RaycastBatchProcessor instance { get; private set; }
    
    
    private const int MaxRaycastBatch = 100000;
    private const int MaxHitsPerRaycast = 1;

    NativeArray<RaycastCommand> _raycastCommands;
    NativeArray<RaycastHit> _raycastHits;
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PerformRaycast(RaycastCommandData raycastCommandData , Action<RaycastHit[]> callbacks)
    {
        int rayCount = Math.Min(raycastCommandData.Origin.Length, MaxRaycastBatch);
        QueryParameters queryParameters = new QueryParameters
        {
            layerMask = raycastCommandData.LayerMask,
            hitMultipleFaces = false,
            hitTriggers = raycastCommandData.HitTrigger ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore,
            hitBackfaces = false
        };

        using (_raycastCommands = new NativeArray<RaycastCommand>(rayCount, Allocator.TempJob))
        {
            for (int i = 0; i < rayCount; i++)
            {
                _raycastCommands[i] =  new RaycastCommand(raycastCommandData.Origin[i], raycastCommandData.Direction[i] , queryParameters,raycastCommandData.MaxDistance);
            }
            
            ExecuteRaycast(_raycastCommands, callbacks);
        }
    }

    private void ExecuteRaycast(NativeArray<RaycastCommand> raycastCommands,Action<RaycastHit[]> callbacks)
    {
        using (_raycastHits = new NativeArray<RaycastHit>(raycastCommands.Length, Allocator.TempJob))
        {
            
            JobHandle jobHandle = RaycastCommand.ScheduleBatch(raycastCommands, _raycastHits, (int)(_raycastCommands.Length * 25f / 100f) ,MaxHitsPerRaycast);
            
            jobHandle.Complete();

            if (_raycastHits.Length > 0)
            {
                var hits = _raycastHits.ToArray();
                
                callbacks?.Invoke(hits);
            }
        }
    }

    public struct RaycastCommandData
    {
        public Vector3[] Origin;
        public Vector3[] Direction;
        public float MaxDistance;
        public int LayerMask;
        public bool HitTrigger;
    }
}