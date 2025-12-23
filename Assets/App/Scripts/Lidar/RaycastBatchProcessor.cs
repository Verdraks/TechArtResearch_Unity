using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class RaycastBatchProcessor : MonoBehaviour
{
    public static RaycastBatchProcessor Instance { get; private set; }
    
    
    private const int k_MaxRaycastBatch = 100000;
    private const int k_MaxHitsPerRaycast = 1;

    NativeArray<RaycastCommand> m_RaycastCommands;
    NativeArray<RaycastHit> m_RaycastHits;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PerformRaycast(RaycastCommandData raycastCommandData , Action<RaycastHit[]> callbacks)
    {
        int rayCount = Math.Min(raycastCommandData.Origin.Length, k_MaxRaycastBatch);
        QueryParameters queryParameters = new QueryParameters
        {
            layerMask = raycastCommandData.LayerMask,
            hitMultipleFaces = false,
            hitTriggers = raycastCommandData.HitTrigger ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore,
            hitBackfaces = false
        };

        using (m_RaycastCommands = new NativeArray<RaycastCommand>(rayCount, Allocator.TempJob))
        {
            for (int i = 0; i < rayCount; i++)
            {
                m_RaycastCommands[i] =  new RaycastCommand(raycastCommandData.Origin[i], raycastCommandData.Direction[i] , queryParameters,raycastCommandData.MaxDistance);
            }
            
            ExecuteRaycast(m_RaycastCommands, callbacks);
        }
    }

    private void ExecuteRaycast(NativeArray<RaycastCommand> raycastCommands,Action<RaycastHit[]> callbacks)
    {
        using (m_RaycastHits = new NativeArray<RaycastHit>(raycastCommands.Length, Allocator.TempJob))
        {
            
            JobHandle jobHandle = RaycastCommand.ScheduleBatch(raycastCommands, m_RaycastHits, (int)(m_RaycastCommands.Length * 25f / 100f) ,k_MaxHitsPerRaycast);
            
            jobHandle.Complete();

            if (m_RaycastHits.Length > 0)
            {
                var hits = m_RaycastHits.ToArray();
                
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