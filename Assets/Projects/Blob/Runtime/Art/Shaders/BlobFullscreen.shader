Shader "Custom/BlobFullscreen"
{
    Properties
    {
        _MaxSteps("MaxSteps", Int) = 100
        _Eps("Precision", Range(0.000001, 0.1)) = 0.001
        _K("Thickness", Range(0.0001, 0.5)) = 0.01
    }
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include  "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL

        Tags
        {
            "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent"
        }

        ZWrite Off ZTest Off Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            Name "BlobFullscreen"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma enable_cbuffer
            #pragma editor_sync_compilation

            struct BlobData
            {
                float3 position;
                float3 color;
            };

            CBUFFER_START(UnityPerMaterial)
                int _MaxSteps;
                float _Eps;
                float _K;
            CBUFFER_END

            StructuredBuffer<BlobData> _BlobBuffer;
            int _BlobCount;

            float SMin_float(float a, float b, float k)
            {
                const float b2 = 13.0 / 4.0 - 4.0 * sqrt(0.5);
                const float b3 = 3.0 / 4.0 - 1.0 * sqrt(0.5);

                k *= 1.0 / (1.0 - sqrt(0.5));
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - k * h * h * (h * b3 * (h - 4.0) + b2);
            }

            float SDF_Sphere(float3 p, float3 center, float radius)
            {
                return length(p - center) - radius;
            }

            void SDF_Scene(float3 p, out float dist)
            {
                dist = FLT_MAX;

                for (int i = 0; i < _BlobCount; i++)
                {
                    BlobData data = _BlobBuffer[i];
                    float d = SDF_Sphere(p, data.position, 1);
                    dist = SMin_float(dist, d, _K);
                }
            }

            void BlobTrace(in float3 viewDir, in float3 viewPos, in float2 tMinMax, out float3 color, out float hit,
                           out float t)
            {
                float distance = tMinMax.x;
                hit = 0;
                color = float3(1, 1, 1);
                int steps = 0;

                while (steps < _MaxSteps && distance <= tMinMax.y)
                {
                    float3 pos = viewPos + distance * viewDir;
                    float dist;
                    SDF_Scene(pos, dist);
                    distance += dist;

                    if (dist <= _Eps)
                    {
                        hit = 1;
                        break;
                    }

                    steps++;
                }

                t = distance;
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

            float4 Frag(Varyings input) : SV_Target
            {
                float3 fragPosWS = ComputeWorldSpacePosition(input.texcoord); 
                
                float3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(fragPosWS) * -1.0f;
                float3 camPosWS = GetCameraPositionWS();
                float2 tMinMax = GetTMinMax(input);

                float hit = false;
                float t = 0.0f;
                float3 color = 0;

                BlobTrace(viewDirectionWS, camPosWS, tMinMax, color, hit, t);

                return float4(color,hit);
            }
            ENDHLSL
        }
    }
}