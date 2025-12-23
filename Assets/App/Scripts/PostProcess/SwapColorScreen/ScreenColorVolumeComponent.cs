using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[Serializable]
public class ScreenColorVolumeComponent : VolumeComponent, IPostProcessComponent
{
    
    public EnumParameter<ScreenColorEffect> ScreenColor = new(ScreenColorEffect.None);
    
        
    public enum ScreenColorEffect
    {
        None = -1,
        InvertColors = 0,
        Sepia,
        Saturate,
    }
    
    public int KernelIndex => (int)ScreenColor.value;
    
    //By default, isActive returns true if not modified, pass by a function to check if the effect is active.
    public bool IsActive()
    {
        return ScreenColor.value != ScreenColorEffect.None;
    }
}