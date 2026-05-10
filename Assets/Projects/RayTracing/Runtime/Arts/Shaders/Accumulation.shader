Shader "RayTracing/Accumulation"
{   
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL

        Tags { "RenderType"="Opaque" }
        ZWrite Off Cull Off ZTest Always
        Pass
        {
            Name "Accumulation"

            HLSLPROGRAM
            
            CBUFFER_START(UnityPerMaterial)
                Texture2D _CurrentFrame;
                Texture2D _PreviousFrame;
                uint _FrameIndex;
            CBUFFER_END

            #pragma vertex VertTris
            #pragma fragment Frag

            Varyings VertTris(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);

                output.positionCS = pos;
                output.texcoord = uv;

                return output;
            }
            
            float4 Frag (Varyings input) : SV_Target
            {
                float4 previousColor = SAMPLE_TEXTURE2D(_PreviousFrame, sampler_PointClamp, input.texcoord);
                float4 currentColor = SAMPLE_TEXTURE2D(_CurrentFrame, sampler_PointClamp ,input.texcoord);
                
                float weight = 1.0 / (_FrameIndex + 1);

                float4 color = lerp(previousColor, currentColor, weight);
                return color;
            }
            
            ENDHLSL
        }
    }
}
