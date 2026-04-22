
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

    #include "Assets/App/Scripts/PathTracer/Ray.hlsl"
    #include "Assets/App/Scripts/PathTracer/Sphere.hlsl"
    #include "Assets/App/Scripts/PathTracer/Random.hlsl"
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
                float4 _ViewParams; // xy: near plane size
                float _RaysPerPixel;
                uint _MaxDepth;
                uint _SpheresCount;
                StructuredBuffer<Sphere> _SpheresBuffer;
            CBUFFER_END

            
            float3 CalculateAtmosphereColor(Ray r)
            {
                float a = 0.5 * (1.0 + r.direction.y);
                return (1.0 - a) * float3(1,1,1) + a * float3(0.5,0.7,1.0);
            }
            
            HitRecord CalculateHitRecord(Ray r)
            {
                HitRecord closestHit = (HitRecord)0;
                closestHit.t = 1.#INF;
                
                for (uint i = 0; i < _SpheresCount; i++)
                {
                    Sphere s = _SpheresBuffer[i];
                    HitRecord rec = s.hit(r);
                    if (rec.hit && rec.t < closestHit.t)
                    {
                        closestHit = rec;
                    }
                }
                
                return closestHit;
            }
            
            float3 TraceRay(Ray r, inout uint state)
            {
                float3 attenuation = 1;
                
                for (uint i = 0; i < _MaxDepth; i++)
                {
                    HitRecord rec = CalculateHitRecord(r);
                    if (rec.hit)
                    {
                        r.origin = rec.p;
                        r.direction = RandomHemisphereDirection(rec.normal, state);
                        attenuation *= rec.material.color;
                    }
                    else
                    {
                        return attenuation * (float3)1;
                    }
                }
                
                return float3(0,0,0);
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
            
            float4 Frag(Varyings input, SamplerState blitsampler) : SV_Target
            {
                uint2 pixelCoord = input.texcoord * _ScreenParams.xy;
                uint pixelIndex = pixelCoord.y * _ScreenParams.x + pixelCoord.x;
                uint state = pixelIndex;
                
                float3 viewPointLS = float3(input.texcoord - 0.5f, 1) * float3(_ViewParams.x, _ViewParams.y, _ProjectionParams.y);
                float3 viewPointWS = mul(unity_CameraToWorld, float4(viewPointLS, 1.0)).xyz;

                Ray r;
                r.origin = _WorldSpaceCameraPos;
                    r.direction = normalize(viewPointWS - r.origin);
                
                float3 color;
                
                for (int i = 0; i < _RaysPerPixel; i++)
                {
                    
                    color += TraceRay(r, state);
                }
                
                color = color/float(_RaysPerPixel);
                
                #ifdef _LINEAR_TO_SRGB_CONVERSION
                color = LinearToSRGB(color);
                #endif
                
                return float4(color,1);
            }
            ENDHLSL
        }
    }
}