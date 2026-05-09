using System;
using System.Collections.Generic;
using UnityEngine;

namespace PathTracer
{
    [ExecuteInEditMode]
    public class PathTracingSphereRenderer : MonoBehaviour
    {
        public static readonly List<PathTracingSphereRenderer> Elements = new(capacity:1024);
        
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
            Elements.Add(this);
        }
        
        private void OnValidate()
        {
            m_Data.center = transform.position;
        }
        
        private void OnDisable()
        {
            Elements.Remove(this);
        }
    }
}