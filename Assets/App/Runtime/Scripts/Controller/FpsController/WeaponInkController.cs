using System;
using UnityEngine;
using UnityEngine.VFX;

namespace Controller.FpsController
{
    public class WeaponInkController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool m_UseOldParticleSystem = true;
    
        [Header("References")]
        [SerializeField] private InputHandler m_InputHandler;
        [SerializeField] private ParticleSystem m_ParticleSystem;
        [SerializeField] private VisualEffect m_VisualEffect;

        private bool m_IsFiring;

        private void Start() => DisableFire();

        private void Update() => HandleFire();

        private void HandleFire()
        {
            if (!m_InputHandler.AttackPressed)
            {
                if (!m_IsFiring) return;
                DisableFire();
                m_IsFiring = false;
            }
            else
            {
                if (m_IsFiring) return;
                EnableFire();
                m_IsFiring = true;
            }
        }


        private void EnableFire()
        {
            if (m_UseOldParticleSystem)
                m_ParticleSystem.Play();
            else m_VisualEffect.Play();
        }

        private void DisableFire()
        {
            if (m_UseOldParticleSystem)
                m_ParticleSystem.Stop();
            else  m_VisualEffect.Stop();
        }
    }
}