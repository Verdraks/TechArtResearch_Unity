Shader "Custom/SH_TexturePainter"
{
    Properties
    {   
        _MainTex ("Main Texture", 2D) = "white" {}
        _PainterColor ("Painter Color", Color) = (0,0,0,0)
        _PainterPosition ("Painter Position", Vector) = (0,0,0,0)
        _Radius ("Radius", Float) = 1
        _Hardness ("Hardness", Float) = 1
        _Strength ("Strength", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Cull Off ZWrite Off ZTest Off
        
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uvLightmap : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positonWS : TEXCOORD1;
                float2 uv : TEXCOORD0;
            };


            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _PainterColor;
                float3 _PainterPosition;
                float _Radius;
                float _Hardness;
                float _Strength;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float4 uvHCS = float4(0,0,0,1);

                uvHCS.xy = float2(1.0,_ProjectionParams.x) * (IN.uvLightmap * float2(2.0,2.0) - float2(1.0,1.0));

                OUT.positionHCS = uvHCS;
                
                OUT.uv = IN.uv;
                OUT.positonWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }


            float mask(float3 p, float3 center, float radius, float hardness)
            {
                float m = distance(p, center);
                return 1-smoothstep(radius * hardness, radius , m);
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, IN.uv);
                float m = mask(IN.positonWS, _PainterPosition, _Radius, _Hardness);
                float edge = m * _Strength;
                return lerp(col, _PainterColor, edge);
            }
            
            ENDHLSL
        }
    }
}
