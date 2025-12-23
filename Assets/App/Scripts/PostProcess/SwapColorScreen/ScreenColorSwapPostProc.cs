using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class ScreenColorSwapPostProc : ScriptableRendererFeature
{
    [FormerlySerializedAs("injectionPoint")]
    [Header("Settings")]
    [SerializeField] private RenderPassEvent m_InjectionPoint = RenderPassEvent.AfterRenderingTransparents;
    [FormerlySerializedAs("computeShaderEffect")] [SerializeField] private ComputeShader m_ComputeShaderEffect;
    
    private ScreenColorSwapPass m_ScreenColorSwapPass;
    
    public override void Create()
    {
        m_ScreenColorSwapPass = new ScreenColorSwapPass
        {
            renderPassEvent = m_InjectionPoint
        };
        
        //Setup function used to pass any data/dependencies to the render pass.
        m_ScreenColorSwapPass.Setup(m_ComputeShaderEffect);
    }

    //Inject multiple render passes into the renderer with some context.
    //Rendering data contains all the information needed to render the scene.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            
            renderer.EnqueuePass(m_ScreenColorSwapPass);
        }
            
    }
}