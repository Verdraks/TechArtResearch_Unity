Shader "Custom/Grass"
{
    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
            
            Tags { "LightMode" = "UniversalForward" }
            
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 3.5
            
            #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            struct GrassData
            {
                float3 position;
            };

            StructuredBuffer<GrassData> grassDataBuffer;

            Varyings vert(Attributes v, uint instanceID : SV_InstanceID)
            {
                Varyings o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                
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