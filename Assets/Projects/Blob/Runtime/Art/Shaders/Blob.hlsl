#ifndef BLOB_INCLUDED
#define BLOB_INCLUDED

#include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"

struct BlobData
{
    float3 position;
    float3 color;
};

StructuredBuffer<BlobData> _BlobBuffer;
int _BlobCount;

float SDF_Sphere(float3 p, float3 center, float radius)
{
    return length(p - center) - radius;
}

float GetMaxDepth(float4 screenPosition, float3 ray, float3 viewDir)
{
    float4 screenUV = (float4)1;
    float sceneDepth = SHADERGRAPH_SAMPLE_SCENE_DEPTH(screenUV);
    
    ray = ray/dot(ray,viewDir);
    float3 maxRay = sceneDepth * ray;
    float maxDepth = length(maxRay);
    return maxDepth;
}

void SDF_Scene(float3 p, out float dist)
{
    dist = 1e9;
    
    for (int i = 0; i < _BlobCount; i++)
    {
        BlobData data = _BlobBuffer[i];
        float d = SDF_Sphere(p, data.position, 1);
        if (d < dist)
        {
            dist = d;
        }
    }
}

void BlobTrace_float(float3 viewDir, float3 viewPos, float maxStep, float maxDistance, float eps, out float3 color, out bool hit)
{
    float maxDepth = GetMaxDepth(float4(0,0,0,0), viewPos, viewDir);
    
    #if defined(SHADERGRAPH_PREVIEW)
    color = float3(0, 0, 0);
    hit = false;
    return;
    #endif
    
    float distance = 0;
    hit = false;
    color = float3(0, 0, 0);
    int steps = 0;
    
    while (steps < maxStep && distance < maxDistance)
    {
        float3 pos = viewPos + distance * viewDir;
        float dist;
        SDF_Scene(pos, dist);
        distance += dist;
        
        if (dist <= eps)
        {
            hit = true;
            break;
        }
        
        steps++;
    }
}


#endif