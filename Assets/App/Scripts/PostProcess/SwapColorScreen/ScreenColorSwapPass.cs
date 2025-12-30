using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;


public class ScreenColorSwapPass : ScriptableRenderPass
{
    private ComputeShader m_ComputeShaderEffect;
    private const string k_PassName = "ScreenColorSwapPass";
    
    class PassDataCompute
    {
        public ComputeShader ComputeShader;
        public Vector2Int DispatchSize;
        public TextureHandle Source; 
        public TextureHandle SourceModified;
        public int KernelIndex = 0;
    }

    class PassDataRaster
    {
        public TextureHandle SourceModified;
    }

    public void Setup(ComputeShader computeShader)
    {
        m_ComputeShaderEffect = computeShader;
    }
    
    
    //Entry point for the render graph, add any passes you need for your effect here.
    // <param name="frameData"> Container of all data URP, Dictionary</param>
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        
        var urpData = frameData.Get<UniversalResourceData>();
        
        //Check if write in the backbuffer, and pass if it (write to later)
        if (urpData.isActiveTargetBackBuffer)
            return;
        
        
        //Get data from the VolumeManager, for post-processing effects.
        var screenColorVolumeComponent = VolumeManager.instance.stack.GetComponent<ScreenColorVolumeComponent>();

        
        if (!screenColorVolumeComponent.IsActive())
        {
            return;
        }
        
        var cameraTextureTarget = urpData.activeColorTexture;

        var descSourceTexture = cameraTextureTarget.GetDescriptor(renderGraph);
        descSourceTexture.enableRandomWrite = true;
        descSourceTexture.name = "SourceTextureModified";


        var sourceTextureModified = renderGraph.CreateTexture(descSourceTexture);
        
        //Compute Pass => Use for perform any compute operations
        using (var builder = renderGraph.AddComputePass(k_PassName + "_Compute",out PassDataCompute passData))
        {
            builder.UseTexture(cameraTextureTarget, AccessFlags.Read);
            builder.UseTexture(sourceTextureModified, AccessFlags.Write);
            
            passData.Source = cameraTextureTarget;
            passData.SourceModified = sourceTextureModified;
            passData.ComputeShader = m_ComputeShaderEffect;
            passData.DispatchSize = new Vector2Int(
                Mathf.CeilToInt(descSourceTexture.width),
                Mathf.CeilToInt(descSourceTexture.height));
            passData.KernelIndex = screenColorVolumeComponent.KernelIndex;
            
            //Call function to execute the compute pass
            builder.SetRenderFunc((PassDataCompute data, ComputeGraphContext context) => ExecuteComputePass(data, context));
        }
        
        //Raster Pass => Use for perform any raster operations (post-processing, etc.)
        using (var builder = renderGraph.AddRasterRenderPass(k_PassName + "_Raster", out PassDataRaster passDataRaster))
        {
            builder.UseTexture(sourceTextureModified, AccessFlags.Read);
            
            //Declare by SetRenderAttachment for Blit operations => cmd.SetRenderTarget()
            builder.SetRenderAttachment(cameraTextureTarget, 0, AccessFlags.Write);
            
            passDataRaster.SourceModified = sourceTextureModified;
            
            builder.SetRenderFunc((PassDataRaster data, RasterGraphContext context) => ExecuteRasterPass(data, context));
        }
    }
    
    
    // Execute passes, called by SetRenderFunc lambda, static function to avoid capturing the context and have weird behaviour.
    static void ExecuteComputePass(PassDataCompute data, ComputeGraphContext context)
    {
        context.cmd.SetComputeTextureParam(data.ComputeShader, data.KernelIndex, "SourceTexture", data.Source);
        context.cmd.SetComputeTextureParam(data.ComputeShader, data.KernelIndex, "Result",data.SourceModified);
        
        context.cmd.DispatchCompute(data.ComputeShader, data.KernelIndex,data.DispatchSize.x/8,data.DispatchSize.y/8,1);
    }
    
    static void ExecuteRasterPass(PassDataRaster data, RasterGraphContext context)
    {
        Blitter.BlitTexture2D(context.cmd, data.SourceModified, new Vector4(1,1,0,0), 0,false);
    }
}