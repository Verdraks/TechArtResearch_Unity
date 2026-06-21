Shader "Custom/BlobFullscreen"
{
    Properties
    {
        _MaxSteps("MaxSteps", Int) = 100
        _Eps("Precision", Range(0.00001, 0.1)) = 0.001
        _K("Thickness", Float) = 0.01
    }
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        
        
        ENDHLSL

        Tags
        {
            "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent"
        }

        ZWrite On ZTest Off Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            Name "BlobFullscreen"

            Tags
            {
                "LightMode" = "UniversalForward"
            }
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            
            // Unity defined keywords
            #pragma multi_compile_fragment _ DEBUG_DISPLAY

            #pragma enable_cbuffer
            #pragma editor_sync_compilation

            struct BlobData
            {
                float3 position;
                float3 color;
                float size;
            };

            CBUFFER_START(UnityPerMaterial)
                int _MaxSteps;
                float _Eps;
                float _K;
            CBUFFER_END

            StructuredBuffer<BlobData> _BlobBuffer;
            int _BlobCount;


            struct BlobTraceResult
            {
                float3 color;
                float3 normalWS;
                float3 positionWS;
                float smoothMask;
                float distance;
                float hit;
            };

            void Smin_Circular(in float a, in float b, in float k, out float d, out float h)
            {
                const float b2 = 13.0 / 4.0 - 4.0 * sqrt(0.5);
                const float b3 = 3.0 / 4.0 - 1.0 * sqrt(0.5);

                k *= 1.0 / (1.0 - sqrt(0.5));
                h = max(k - abs(a - b), 0.0) / k;
                d = min(a, b) - k * h * h * (h * b3 * (h - 4.0) + b2);
            }

            void Smin_Polynomial(in float a, in float b, in float k, out float d, out float h)
            {
                h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
                d = lerp(b, a, h) - k * h * (1.0 - h);
            }

            float SDF_Sphere(float3 p, float3 center, float radius)
            {
                return length(p - center) - radius;
            }

            void SDF_Scene(in float3 p, inout float dist, out float mask)
            {
                mask = 0.0f;

                for (int i = 0; i < _BlobCount; i++)
                {
                    BlobData data = _BlobBuffer[i];
                    float d = SDF_Sphere(p, data.position, data.size);
                    float m;
                    Smin_Circular(dist, d, _K, dist, m);
                    mask = max(m, mask);
                }
            }

            void SDF_Scene(in float3 p, out float dist)
            {
                float mask = 0.0f;
                dist = FLT_MAX;
                SDF_Scene(p, dist, mask);
            }

            void SDF_Normal_Tetraedre(in float3 p, out float3 normal)
            {
                float h = max(FLT_EPS, _Eps * 1.5f);

                float3 e1 = float3(h, -h, -h);
                float3 e2 = float3(-h, -h, h);
                float3 e3 = float3(-h, h, -h);
                float3 e4 = float3(h, h, h);

                float de1, de2, de3, de4;

                SDF_Scene(p + e1, de1);
                SDF_Scene(p + e2, de2);
                SDF_Scene(p + e3, de3);
                SDF_Scene(p + e4, de4);

                float ddx = de1 - de2 - de3 + de4;
                float ddy = -de1 - de2 + de3 + de4;
                float ddz = -de1 + de2 - de3 + de4;

                normal = normalize(float3(ddx, ddy, ddz));
            }

            void SDF_Normal_Octaedre(in float3 p, out float3 normal)
            {
                float h = max(FLT_EPS, _Eps*1.5f);

                float d_right, d_left, d_up, d_down, d_front, d_back;
                SDF_Scene(p + float3(h, 0, 0), d_right);
                SDF_Scene(p + float3(-h, 0, 0), d_left);
                SDF_Scene(p + float3(0, h, 0), d_up);
                SDF_Scene(p + float3(0, -h, 0), d_down);
                SDF_Scene(p + float3(0, 0, h), d_front);
                SDF_Scene(p + float3(0, 0, -h), d_back);

                float pente_x = (d_right - d_left) / (2 * h);
                float pente_y = (d_up - d_down) / (2 * h);
                float pente_z = (d_front - d_back) / (2 * h);
                normal = normalize(float3(pente_x, pente_y, pente_z));
            }

            void BlobTrace(in float3 viewDir, in float3 viewPos, in float2 tMinMax, out BlobTraceResult data)
            {
                data = (BlobTraceResult)0;

                float dist;
                float distanceMarched = tMinMax.x;

                UNITY_LOOP
                for (int steps = 0; steps < _MaxSteps; steps++)
                {
                    float3 pos = viewPos + distanceMarched * viewDir;
                    data.positionWS = pos;
                    dist = tMinMax.y;
                    SDF_Scene(pos, dist, data.smoothMask);
                    distanceMarched += dist;

                    if (distanceMarched > tMinMax.y)
                    {
                        break;
                    }

                    if (dist <= _Eps)
                    {
                        data.hit = 1;
                        data.color = float3(1, 1, 1);
                        SDF_Normal_Octaedre(pos, data.normalWS);
                        break;
                    }
                }

                data.distance = distanceMarched;
            }

            float2 GetTMinMax(Varyings input)
            {
                float2 tMinMax;
                tMinMax.x = _ProjectionParams.y;

                float rawSceneDepth = SampleSceneDepth(input.texcoord);

                float3 worldPos = ComputeWorldSpacePosition(input.texcoord, rawSceneDepth, unity_MatrixInvVP);

                float distanceCamWorld = distance(_WorldSpaceCameraPos, worldPos);

                tMinMax.y = min(_ProjectionParams.z, distanceCamWorld);

                return tMinMax;
            }

            float3 ComputeWorldSpacePosition(float2 uv)
            {
                float2 ndc = uv * 2.0f - 1.0f;
                float4 clip = float4(ndc, UNITY_NEAR_CLIP_VALUE, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                clip.y = -clip.y;
                #endif

                return ComputeWorldSpacePosition(clip, unity_MatrixInvVP);
            }

            half4 Frag(Varyings input, out float depth : SV_Depth) : SV_Target
            {
                float3 fragPosWS = ComputeWorldSpacePosition(input.texcoord);

                float3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(fragPosWS) * -1.0f;
                float3 camPosWS = GetCameraPositionWS();
                float2 tMinMax = GetTMinMax(input);

                BlobTraceResult result;

                BlobTrace(viewDirectionWS, camPosWS, tMinMax, result);


                if (result.hit == 0)
                {
                    depth = 0;
                    return 0;
                }

                float4 positionCS = TransformWorldToHClip(result.positionWS);
                depth = positionCS.z / positionCS.w;
                
                InputData inputData = (InputData)0;
                SurfaceData surfaceData = (SurfaceData)0;

                inputData.positionWS = result.positionWS;
                inputData.normalWS = NormalizeNormalPerPixel(result.normalWS);
                inputData.viewDirectionWS = viewDirectionWS * -1;
                inputData.fogCoord = 0;
                inputData.vertexLighting = 0;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(positionCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(inputData.normalizedScreenSpaceUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw)
                inputData.shadowCoord = TransformWorldToShadowCoord(result.positionWS);
                inputData.bakedGI = SampleSH(result.normalWS);
                
                surfaceData.normalTS = half3(0,0,1);
                surfaceData.albedo = half3(1, 1, 1);
                surfaceData.emission = 0.0;
                surfaceData.metallic = 1.0;
                surfaceData.specular = 0.0;
                surfaceData.smoothness = 1.0;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = 1;
                
                half4 color = 0;
                color = UniversalFragmentBlinnPhong(inputData, surfaceData);
                
                // color = lerp(color, 1, result.smoothMask);
                
                // half4 color = UniversalFragmentPBR(inputData, surfaceData);
                return color;
            }
            ENDHLSL
        }
    }
}