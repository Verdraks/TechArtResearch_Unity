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

            #pragma vertex Vert
            #pragma fragment Frag

            float4 Frag (Varyings input) : SV_Target
            {
                float4 previousColor = SAMPLE_TEXTURE2D(_PreviousFrame, sampler_LinearClamp, input.texcoord);
                float4 currentColor = SAMPLE_TEXTURE2D(_CurrentFrame, sampler_LinearClamp, input.texcoord);

                float weight = 1.0 / (_FrameIndex + 1);

                float4 color = lerp(previousColor, currentColor, weight);
                return color;
            }
            
            ENDHLSL
        }
    }
}
