Shader "Custom/Blob"
{
    Properties
    {

        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5

        _Distortion("Distortion",Float) = 1.0
        _Power("Power",Float) = 1.0
        _Scale("Scale",Float) = 1.0

        _MaxSteps("MaxSteps", Int) = 100
        _Eps("Precision", Range(0.00001, 0.1)) = 0.001
        _K("Thickness", Float) = 0.01
        _Cull("Cull", Float) = 2.0
    }
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        ENDHLSL

        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"

            ZWrite On
            ZTest Off
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            // Unity defined keywords
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fragment _ LIGHTMAP_BICUBIC_SAMPLING
            #pragma multi_compile_fragment _ REFLECTION_PROBE_ROTATION
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fragment _ DEBUG_DISPLAY

            #include "BlobLit.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float2 positionNDC : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct Results
            {
                half4 color : SV_Target;
                float depth : SV_Depth;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4 positionCS = TransformObjectToHClip(input.positionOS);
                output.positionCS = positionCS;
                output.positionWS = TransformObjectToWorld(input.positionOS);
                output.positionNDC = ComputeNormalizedDeviceCoordinates(positionCS);
                return output;
            }

            float2 GetTMinMax(float2 uv)
            {
                float2 tMinMax;
                tMinMax.x = _ProjectionParams.y;

                float rawSceneDepth = SampleSceneDepth(uv);

                float depth = LinearEyeDepth(rawSceneDepth, _ZBufferParams);
                tMinMax.y = depth;

                return tMinMax;
            }

            half ApproxThickness(float3 normalWS, float3 lDir, float radius)
            {
                float NdotL = dot(normalWS, lDir);
                // Aux bords (NdotL proche de 0 ou négatif vu de l'autre côté), la matière traversée est fine
                float edgeFactor = 1.0 - saturate(abs(NdotL));
                return radius * (1.0 - edgeFactor * 0.8); // radius = épaisseur typique du blob
            }

            half3 SubsurfaceScaterring(half3 vDir, half3 normalWS, Light l)
            {
                half3 lDir = l.direction;
                float thickness = ApproxThickness(vDir, lDir, 1);
                half3 h = SafeNormalize(lDir + normalWS * _Distortion);
                half vDoth = pow(saturate(dot(vDir, -h)), _Power) * _Scale;
                float absorption = exp(-thickness * 1);
                half3 i = vDoth * l.distanceAttenuation;
                half3 lighting = i * l.color * absorption;
                return lighting;
            }

            half3 SubsurfaceScaterring(half3 vDir, half3 normalWS, half3 positionWS, InputData inputData)
            {

                half3 lighting = half3(0, 0, 0);
                lighting += SubsurfaceScaterring(vDir, normalWS, GetMainLight());


                #if defined(_ADDITIONAL_LIGHTS)

                #if USE_CLUSTER_LIGHT_LOOP
                UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                    Light additionalLight = GetAdditionalLight(lightIndex, positionWS, half4(1,1,1,1));
                    lighting += SubsurfaceScaterring(vDir, normalWS, additionalLight);
                }
                #endif

                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light additionalLight = GetAdditionalLight(lightIndex, positionWS, half4(1,1,1,1));
                    lighting += SubsurfaceScaterring(vDir, normalWS, additionalLight);
                LIGHT_LOOP_END
                
                #endif

                return lighting;
            }

            void Frag(Varyings input, out Results output)
            {
                output = (Results)0;

                float2 uv = input.positionCS / _ScaledScreenParams.xy;
                float3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS) * -1.0f;
                float3 camPosWS = GetCameraPositionWS();
                float2 tMinMax = GetTMinMax(uv);

                BlobTraceResult result;

                BlobTrace(camPosWS, viewDirectionWS, tMinMax, result);

                if (result.hit == 0)
                {
                    discard;
                }

                InputData inputData = (InputData)0;
                SurfaceData surfaceData = (SurfaceData)0;

                inputData.positionWS = result.positionWS;
                inputData.normalWS = NormalizeNormalPerPixel(result.normalWS);
                inputData.viewDirectionWS = viewDirectionWS * -1;
                inputData.fogCoord = 0;
                inputData.vertexLighting = 0;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(
                    inputData.normalizedScreenSpaceUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw)
                inputData.shadowCoord = TransformWorldToShadowCoord(result.positionWS);
                inputData.bakedGI = SampleSH(result.normalWS);

                surfaceData.normalTS = half3(0, 0, 1);

                surfaceData.albedo = _BaseColor;
                surfaceData.specular = _SpecColor;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;

                surfaceData.occlusion = 1.0;
                surfaceData.alpha = 1;

                half4 pbr = UniversalFragmentPBR(inputData, surfaceData);
                half3 sssColor = SubsurfaceScaterring(viewDirectionWS * -1, result.normalWS, result.positionWS, inputData);

                output.depth = result.distance;
                output.color = half4(pbr.rgb + sssColor, pbr.a);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}