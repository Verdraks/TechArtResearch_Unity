using System.Runtime.InteropServices;
using UnityEngine;

namespace Blob.Runtime
{
    [ExecuteInEditMode]
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
                    position = transform.position
                };
                return blobData;
            }
        }
    }
    
    [System.Serializable, StructLayout(LayoutKind.Sequential)]
    public struct BlobData
    {
        public Vector3 position;
        public Vector3 color;
    }
}