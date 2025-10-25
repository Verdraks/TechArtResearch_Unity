Shader "Custom/SH_DebugUv"
{
    Properties
    {
        [KeywordEnum(UvSpace, WorldSpace, LocalSpace, ClipSpace, TextureSpace)] _Type("Type", Float) = 0
        [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _TYPE_UVSPACE _TYPE_WORLDSPACE _TYPE_LOCALSPACE _TYPE_CLIPSPACE _TYPE_TEXTURESPACE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
                float2 uvCS : TEXCOORD3;
                float2 uvTS : TEXCOORD4;
            };
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                OUT.uvCS = IN.uv * 2 - 1;

                OUT.uvTS = TRANSFORM_TEX(IN.uv, _MainTex);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                #if defined(_TYPE_UVSPACE)
                    return half4(IN.uv, 0, 1);
                #elif defined(_TYPE_WORLDSPACE)
                    return half4(IN.positionWS, 1);
                #elif defined(_TYPE_CLIPSPACE)
                    return half4(IN.uvCS, 0, 1);
                #elif defined(_TYPE_LOCALSPACE)
                    return half4(IN.positionOS , 1);
                #elif defined (_TYPE_TEXTURESPACE)
                return half4(IN.uvTS,0,1);
                #else
                    return half4(0,0,0,1);
                #endif
            }
            ENDHLSL
        }
    }
}
