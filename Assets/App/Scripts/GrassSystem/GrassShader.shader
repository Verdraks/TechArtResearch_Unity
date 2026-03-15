Shader "ExampleShader"
{
    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
            
            
            ZWrite Off
            Cull Front
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma  target  2.0
            
            #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv : TEXCOORD0;
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
                
                float3 wpos = TransformObjectToWorld(v.positionOS );
                o.positionHCS = TransformWorldToHClip(wpos);
                o.color = float4(1, 0, 0, 1);
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
}