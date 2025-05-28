using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class ScreenColorVolumeComponent : VolumeComponent, IPostProcessComponent
{
    
    public EnumParameter<ScreenColorEffect> screenColorEffect = new(ScreenColorEffect.None);
    
        
    public enum ScreenColorEffect
    {
        None = -1,
        InvertColors = 0,
        Sepia,
        Saturate,
    }
    
    public int KernelIndex => (int)screenColorEffect.value;
    
    //By default, isActive returns true if not modified, pass by a function to check if the effect is active.
    public bool IsActive()
    {
        return screenColorEffect.value != ScreenColorEffect.None;
    }
}