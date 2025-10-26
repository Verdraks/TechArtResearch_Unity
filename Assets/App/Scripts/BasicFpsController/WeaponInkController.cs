using System;
using UnityEngine;

public class WeaponInkController : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] private InputHandler m_InputHandler;
    [SerializeField] private ParticleSystem m_ParticleSystem;

    private bool m_IsFiring;

    private void Awake()
    {
        m_ParticleSystem.Stop();
    }

    private void Update() => HandleFire();

    private void HandleFire()
    {
        if (!m_InputHandler.AttackPressed)
        {
            if (!m_IsFiring) return;
            m_ParticleSystem.Stop();
            m_IsFiring = false;
        }
        else
        {
            if (m_IsFiring) return;
            m_ParticleSystem.Play();
            m_IsFiring = true;
        }
        
        
    }
}
