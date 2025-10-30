Shader "Custom/SH_TexturePainter"
{
    Properties
    {   
        _MainTex ("Main Texture", 2D) = "white" {}
        _UvIslandsTex ("UV Islands Texture", 2D) = "white" {}
        _PrepareUvIslands ("Prepare UV Islands", Float) = 1.0
        _UvOffset ("UV Offset", Float) = 0
        _PainterColor ("Painter Color", Color) = (0,0,0,0)
        _PainterPosition ("Painter Position", Vector) = (0,0,0,0)
        _Radius ("Radius", Float) = 1
        _Hardness ("Hardness", Float) = 1
        _Strength ("Strength", Float) = 1
    }

    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        
            BlendOp Add
            Blend One One
        
            Cull Off ZWrite Off ZTest Off
            
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
                float _PrepareUvIslands;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float4 uvHCS = float4(0,0,0,1);

                uvHCS.xy = float2(1.0,_ProjectionParams.x) * (IN.uvLightmap * float2(2.0,2.0) - float2(1.0,1.0));

                OUT.positionHCS = uvHCS;
                
                OUT.uv = IN.uvLightmap;
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
                if (_PrepareUvIslands > 0)
                {
                    return half4(0.0,0.0,1.0,1.0);
                }
                else
                {
                    float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, IN.uv);
                    float m = mask(IN.positonWS, _PainterPosition, _Radius, _Hardness);
                    float edge = m * _Strength;
                    return lerp(col, _PainterColor, edge);
                }
            }
            ENDHLSL
        }

        Pass 
        {
            
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"  }
            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #pragma fragment frag
            #pragma vertex vert

            #pragma target 3.0

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            
            TEXTURE2D(_UvIslandsTex);
            SAMPLER(sampler_UvIslandsTex);

            
            const float UvOffset = 0.5f;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 offsets[8] = {
                    float2(-UvOffset, 0), float2(UvOffset, 0), float2(0, UvOffset), float2(0, -UvOffset),
                    float2(-UvOffset, UvOffset), float2(UvOffset, UvOffset), float2(UvOffset, -UvOffset),
                    float2(-UvOffset, -UvOffset)
                };
                
				float2 uv = IN.uv;
                
				float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,uv);
				float4 island = SAMPLE_TEXTURE2D(_UvIslandsTex, sampler_UvIslandsTex,uv);

                if(island.z < 1)
                {
                    float4 extendedColor = color;
                    
                    for	(int i = 0; i < 8; i++){
                        float2 currentUV = uv + offsets[i] * _MainTex_TexelSize.xy;
                        float4 offsettedColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, currentUV);
                        extendedColor = max(offsettedColor, extendedColor);
                    }
                    color = extendedColor;
                }
                
				return color;
            }
            
            ENDHLSL

        }
    }
}
