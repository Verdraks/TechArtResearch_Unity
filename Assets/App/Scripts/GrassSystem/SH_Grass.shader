Shader "Custom/Grass"
{
    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Unlit"  "IgnoreProector" = "True" }
            
            ZWrite On
            Cull Front
            
            
            HLSLINCLUDE
            struct GrassData
            {
                float3 position;
            };

            StructuredBuffer<GrassData> grassDataBuffer;
            ENDHLSL

            HLSLPROGRAM
            
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            
            #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            
            #pragma multi_compile_instancing
            #pragma  instancing_options renderinglayer
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"


            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float4 color : COLOR;
            };
            
            Varyings vert(Attributes v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                uint instanceID = GetIndirectInstanceID(svInstanceID);

                
                // Get grass position from buffer and apply to world position
                float3 grassPos = grassDataBuffer[instanceID].position;
                float3 posOS = v.positionOS.xyz + grassPos;
                float3 wpos = TransformObjectToWorld(posOS);
                o.positionHCS = TransformWorldToHClip(wpos);
                o.color = float4(0.2, 0.8, 0.2, 1.0);  // Green color for visibility
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                return float4(1.0, 1.0, 1.0, 1.0);  // White color for visibility
            }
            ENDHLSL
        }
    }
}