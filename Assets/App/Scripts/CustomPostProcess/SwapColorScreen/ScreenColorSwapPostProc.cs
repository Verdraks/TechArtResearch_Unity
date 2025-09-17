using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenColorSwapPostProc : ScriptableRendererFeature
{
    [Header("Settings")]
    [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;
    [SerializeField] private ComputeShader computeShaderEffect;
    
    private ScreenColorSwapPass _screenColorSwapPass;
    
    public override void Create()
    {
        _screenColorSwapPass = new ScreenColorSwapPass
        {
            renderPassEvent = injectionPoint
        };
        
        //Setup function used to pass any data/dependencies to the render pass.
        _screenColorSwapPass.Setup(computeShaderEffect);
    }

    //Inject multiple render passes into the renderer with some context.
    //Rendering data contains all the information needed to render the scene.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            
            renderer.EnqueuePass(_screenColorSwapPass);
        }
            
    }
}