using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RigidbodyRandomForceController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ForceMode m_ForceMode;
    [SerializeField] private float m_Force;
    [Header("References")]
    [SerializeField] private Rigidbody m_Rb;

    private void Awake()
    {
        if (!m_Rb) m_Rb = GetComponent<Rigidbody>();
    }

    private void Start() => m_Rb.AddForce(new Vector3(Random.value, Random.value, Random.value) * m_Force, m_ForceMode);
}
