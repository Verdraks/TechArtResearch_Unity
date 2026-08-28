using System;
using System.Buffers;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Blob.Runtime
{
    [ExecuteInEditMode, SelectionBase]
    public class BlobRenderer : MonoBehaviour
    {
        [SerializeField] private BlobData _blobData;

        public BlobData BlobData
        {
            get
            {
                BlobData blobData = new BlobData
                {
                    color = _blobData.color,
                    position = transform.position,
                    radius = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z) * 0.5f
                };
                return blobData;
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.crimson;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
    
    [System.Serializable, StructLayout(LayoutKind.Sequential)]
    public struct BlobData
    {
        [HideInInspector]public Vector3 position;
        public Vector3 color;
        [HideInInspector] public float radius;
    }
}