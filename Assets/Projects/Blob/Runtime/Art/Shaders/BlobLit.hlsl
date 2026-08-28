#ifndef BLOB_LIGHTING_INCLUDED
#define BLOB_LIGHTING_INCLUDED

#include "BlobCommon.hlsl"

struct BlobTraceResult
{
    float3 color;
    float3 normalWS;
    float3 positionWS;
    float smoothMask;
    float distance;
    float hit;
};

CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    float4 _SpecColor;
    float _Metallic;
    float _Smoothness;

    float _Distortion;
    float _Power;
    float _Scale;

    int _MaxSteps;
    float _Eps;
    float _K;
CBUFFER_END

StructuredBuffer<BlobData> _BlobBuffer;
int _BlobCount;

void SDF_Scene(in float3 p, out float dist)
{
    dist = FLT_MAX;

    for (int i = 0; i < _BlobCount; i++)
    {
        BlobData data = _BlobBuffer[i];
        float d = SDF_Sphere(p, data.position, data.size);
        Smin_Circular(dist, d, _K, dist);
    }
}

void SDF_Scene(in float3 p, out float dist, inout BlobTraceResult data)
{
    data.positionWS = p;
    dist = FLT_MAX;
    SDF_Scene(p, dist);
}

void HitTrace(in float3 p, inout BlobTraceResult data)
{
    data.hit = 1;
    data.color = float3(1, 1, 1);
    SDF_NORMAL_OCTAEDRE(p, _Eps, data.normalWS, SDF_Scene);
}

void BlobTrace(in float3 viewPos, in float3 viewDir, in float2 tMinMax, out BlobTraceResult data)
{
    data = (BlobTraceResult)0;
    RAYMARCHING_LOOP(viewPos, viewDir, tMinMax, data, t, SDF_Scene, HitTrace);
    data.distance = t;
}

#endif
