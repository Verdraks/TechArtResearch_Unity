using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RayTracing.Runtime
{
    public class PathTracerFrameData : ContextItem
    {
        public TextureHandle pathTracerTexture = TextureHandle.nullHandle;
        public TextureHandle supportTexture = TextureHandle.nullHandle;
        public int frameIndex;

        public override void Reset()
        {
            pathTracerTexture = TextureHandle.nullHandle;
            frameIndex = 0;
        }
    }
}
