#ifndef METABALL_CORE_INCLUDED
#define METABALL_CORE_INCLUDED

#include "MetaballData.hlsl"

StructuredBuffer<MetaballData> _MetaballDataBuffer;
int _MetaballCount;

float Smin_float(float a, float b, float k)
{
    k *= 1.0 / (1.0 - sqrt(0.5));
    float h = max(k - abs(a - b), 0.0) / k;
    const float b2 = 13.0 / 4.0 - 4.0 * sqrt(0.5);
    const float b3 = 3.0 / 4.0 - 1.0 * sqrt(0.5);
    return min(a, b) - k * h * h * (h * b3 * (h - 4.0) + b2);
    
}

float SdfSphere_float(float3 p, float3 center, float radius)
{
    return length(p - center) - radius;
}

float SdfMap_float(float3 p,float k)
{
    float d = 1e9;
    for (int i = 0; i < _MetaballCount; i++)
    {
        float3 center = _MetaballDataBuffer[i].Position;
        float radius = _MetaballDataBuffer[i].Radius;
        float di = SdfSphere_float(p, center, radius);

        float kScaled = max(k * radius, 1e-6);
        
        d = Smin_float(d, di, kScaled);
    }
    return d;
    
}

float3 NormalSdfMap_float(float3 p,float eps, float k)
{
    float dx = SdfMap_float(p + float3(eps,0,0),k) - SdfMap_float(p + float3(-eps,0,0),k);
    float dy = SdfMap_float(p + float3(0,eps,0),k) - SdfMap_float(p + float3(0,-eps,0),k);
    float dz = SdfMap_float(p + float3(0,0,eps),k) - SdfMap_float(p + float3(0,0,-eps),k);
    float3 normal = normalize(float3(dx, dy, dz));
    return normal;
}

void SphereTraceMetaballs_float(float k, float eps, float3 rayOrigin, float3 rayDir, out float3 positionWs,out float3 normalWs, out float3 viewDir, out float alpha)
{
    #if defined(SHADERGRAPH_PREVIEW)
    positionWs = float3(0,0,0);
    normalWs = float3(0,0,0);
    viewDir = float3(0,0,0);
    alpha = 1.0;
    #else
    
    float maxDist = 100.0;
    float t = 0.0;
    int steps = 0;
    int maxSteps = 64;

    alpha = 0.0;
    
    while (t < maxDist && steps < maxSteps)
    {
        float3 p = rayOrigin + t * rayDir;
        
        float d = SdfMap_float(p, k);
        
        if (d <= eps)
        {
            positionWs = p;
            normalWs = NormalSdfMap_float(p, eps, k);
            viewDir = rayDir;
            alpha = 1.0;
            break;
        }

        t+= max(d,eps * 0.5f);
        steps++;
    }
    
    #endif
}

#endif