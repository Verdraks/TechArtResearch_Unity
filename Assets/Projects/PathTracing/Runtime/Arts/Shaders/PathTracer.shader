
Shader "Hidden/PathTracer"
{
    HLSLINCLUDE
    #pragma target 4.5
    #pragma editor_sync_compilation
    
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    // // DebuggingFullscreen.hlsl for URP debug draw
    // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/DebuggingFullscreen.hlsl"
    // // Color.hlsl for color space conversion
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

    #include "./Common/Ray.hlsl"
    #include "./Common/Sphere.hlsl"
    #include "./Common/Random.hlsl"
    #include "./Common/Interval.hlsl"
    #include "./Common/MeshInfo.hlsl"
    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalRenderPipeline"
        }

        Pass
        {
            Cull Off ZWrite Off ZTest Always
            Name "Path Tracer"

            HLSLPROGRAM
            #pragma vertex VertTris
            #pragma fragment Frag

            CBUFFER_START(UnityPerMaterial)
                // x = near clip plane width
                // y = near clip plane height
                // z = near clip plane distance
                // w = far clip plane distance
                float4 _ViewParams;
                float _DefocusStrength;
                float _DivergeStrength;
                uint _FrameIndex;
                uint _RaysPerPixel;
                uint _MaxDepth;
                uint _SpheresCount;
                uint _MeshesCount;
                StructuredBuffer<Sphere> _SpheresBuffer;
                StructuredBuffer<Triangle> _TrianglesBuffer;
                StructuredBuffer<MeshInfo> _MeshesBuffer;
            CBUFFER_END
            
            float3 CalculateAtmosphereColor(Ray r)
            {
                float a = 0.5 * (1.0 + r.direction.y);
                return (1.0 - a) * float3(1.0,1.0,1.0) + a * float3(0.5,0.7,1.0);
            }
            
            HitRecord CalculateHitRecord(Ray r, Interval interval)
            {
                HitRecord closestHit = (HitRecord)0;
                closestHit.t = 1.#INF;
                
                for (uint i = 0; i < _SpheresCount; i++)
                {
                    Sphere s = _SpheresBuffer[i];
                    HitRecord rec = s.CalculateRayHit(r, interval);
                    if (rec.hit && rec.t < closestHit.t)
                    {
                        closestHit = rec;
                    }
                }
                
                for (uint i = 0; i < _MeshesCount; i++)
                {
                    MeshInfo m = _MeshesBuffer[i];
                    
                    for (uint j = 0; j < m.trianglesCount; j++)
                    {
                        int triangleIndex = m.firstTriangleIndex + j;
                        Triangle t = _TrianglesBuffer[triangleIndex];
                        HitRecord rec = t.CalculateRayHit(r, interval);
                        if (rec.hit && rec.t < closestHit.t)
                        {
                            closestHit = rec;
                            closestHit.material = m.mat;
                        }
                    }
                }
                
                return closestHit;
            }
            
            float3 TraceRay(Ray r, Interval interval, inout uint state)
            {
                float3 rayColor = (float3)1;
                float3 emission = (float3)0;
                
                for (uint i = 0; i < _MaxDepth; i++)
                {
                    HitRecord rec = CalculateHitRecord(r, interval);
                    if (rec.hit)
                    {
                        r.origin = rec.p;
                        r.direction = rec.normal + RandomDirection(state);
                        
                        float3 emittedLight = rec.material.emission * rec.material.emissionStrength;
                        emission += emittedLight * rayColor;
                        
                        // float lightIntensity = dot(rec.normal, r.direction);
                        rayColor *= rec.material.albedo.xyz;
                    }
                    else
                    {
                        emission += rayColor * CalculateAtmosphereColor(r);
                        break;
                    }
                }
                
                return emission;
            }

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
            

            float4 Frag(Varyings input) : SV_Target
            {
                uint2 pixelCoord = input.texcoord * _ScreenParams.xy;
                uint pixelIndex = pixelCoord.y * _ScreenParams.x + pixelCoord.x;
                uint state = pixelIndex + _FrameIndex * 719393;
                
                float3 viewPointLS = float3(input.texcoord - 0.5f, 1) * float3(_ViewParams.x, _ViewParams.y, _ProjectionParams.y);
                float3 viewPointWS = mul(unity_CameraToWorld, float4(viewPointLS, 1.0)).xyz;

                float3 camRight = unity_CameraToWorld._m00_m10_m20;
                float3 camUp = unity_CameraToWorld._m01_m11_m21;
                
                Ray r;
                float3 color;
                Interval interval;
                interval.min = _ViewParams.z;
                interval.max = _ViewParams.w;
                
                for (uint i = 0; i < _RaysPerPixel; i++)
                {
                    float2 defocusOffset = RandomPointInCircle(state) * _DefocusStrength / _ScreenParams.x;
                    r.origin = _WorldSpaceCameraPos + camRight * defocusOffset.x + camUp * defocusOffset.y;
                    
                    float2 offset = RandomPointInCircle(state) * _DivergeStrength / _ScreenParams.x;;
                    float3 offsetFocalPoint = viewPointWS + camRight * offset.x + camUp * offset.y;
                    r.direction = normalize(offsetFocalPoint - r.origin);
                    
                    color += TraceRay(r, interval, state);
                }
                
                color = color/float(_RaysPerPixel);
                
                return float4(color,1);
            }
            ENDHLSL
        }
    }
}