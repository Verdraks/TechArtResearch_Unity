using System.Collections.Generic;
using UnityEngine;

namespace RayTracing.Runtime
{
    [ExecuteInEditMode]
    public class RayTracingSphereRenderer : MonoBehaviour
    {
        public static readonly List<RayTracingSphereRenderer> ELEMENTS = new(capacity:1024);
        
        [Header("Settings")]
        [SerializeField] private Sphere m_Data;

        public Sphere Data
        {
            get
            {
                OnValidate();
                return m_Data;
            }
        }

        private void OnEnable()
        {
            ELEMENTS.Add(this);
        }
        
        private void OnValidate()
        {
            m_Data.center = transform.position;
        }
        
        private void OnDisable()
        {
            ELEMENTS.Remove(this);
        }
    }
}