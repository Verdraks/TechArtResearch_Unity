Shader "Hidden/AccumulationTracer"
{   
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL

        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "AccumulationTracer"

            HLSLPROGRAM
            

            CBUFFER_START(UnityPerMaterial)
                Texture2D<float4> _CurrentFrame;
                Texture2D<float4> _PreviousFrame;
                uint _FrameIndex;
            CBUFFER_END


            sampler sampler_CurrentFrame;
            sampler sampler_PreviousFrame;

            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag (Varyings input) : SV_Target
            {
                float4 currentColor = _CurrentFrame.Sample(sampler_CurrentFrame, input.texCoord);
                float4 previousColor = _PreviousFrame.Sample(sampler_PreviousFrame, input.texCoord);

                float weightCurrent = 1.0 / (_FrameIndex + 1);

                float4 color = lerp(previousColor, currentColor, weightCurrent);
                return color;
            }
            
            ENDHLSL
        }
    }
}
