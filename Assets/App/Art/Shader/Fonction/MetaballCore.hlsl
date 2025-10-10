#ifndef METABALL_CORE_INCLUDED
#define METABALL_CORE_INCLUDED

#include "MetaballData.hlsl"

StructuredBuffer<MetaballData> _MetaballsDataBuffer;
int _MetaballsCount;

float Smin_float(float d1, float d2, float k)
{
    k *= 4.0;
    float x = (d2-d1)/k;
    float g = (x> 1.0) ? x :
              (x<-1.0) ? 0.0 :
              (x*(2.0+x)+1.0)/4.0;
    return d2 - k * g;
    
}

float SdfSphere_float(float3 p, float3 center, float radius)
{
    return length(p - center) - radius;
}

float SdfMap_0_float(float3 p, float eps)
{
    float sumDensity = 0.0;
    float sumRi = 0.0;
    float minDist = 1e20;

    for (int i = 0; i < _MetaballsCount; i++)
    {
        float3 center = _MetaballsDataBuffer[i].Position;
        float radius = _MetaballsDataBuffer[i].Radius;

        float3 d = p - center;
        float r2 = dot(d, d);
        float R2 = radius * radius;

        if (r2 >= R2)
        {
            float r = sqrt(r2);
            minDist = min(minDist, r - radius);
            sumRi += radius;
            continue;
        }

        float r = sqrt(r2);
        float invR = 1.0 / max(radius, eps);
        float x = r * invR;
        float x2 = x * x;
        float x3 = x2 * x;
        sumDensity += 2.0 * x3 - 3.0 * x2 + 1.0;

        minDist = min(minDist, r - radius);
        sumRi += radius;
    }

    float denom = 1.5 * max(sumRi, eps);
    float densityDist = (0.2 - sumDensity) / denom;

    return max(minDist, densityDist);
}

float SdfMap_1_float(float3 p)
{
    float sumDensity = 0.0;
    float sumRi = 0.0;
    float minDist = 100000.0;

    for (int i = 0; i < _MetaballsCount; i++)
    {
        float3 center = _MetaballsDataBuffer[i].Position;
        float radius = _MetaballsDataBuffer[i].Radius;
        float r = length(p - center);
        if (r < radius)
        {
            sumDensity += 2.0 * (r*r*r) / (radius * radius * radius) - 3.0 * (r*r) / (radius * radius) + 1.0;
        }
        minDist = min(minDist, r-radius);
        sumRi += radius;
    }
    return  max(minDist, (0.2 - sumDensity) / (3.0 / 2.0 * sumRi));
}

float SdfMap_2_float(float3 p,float k)
{
    float d = 1e9;
    for (int i = 0; i < _MetaballsCount; i++)
    {
        float3 center = _MetaballsDataBuffer[i].Position;
        float radius = _MetaballsDataBuffer[i].Radius;
        float di = SdfSphere_float(p, center, radius);
        d = Smin_float(d, di, k);
    }
    return d;
    
}


float3 NormalSdfMap_0_float(float3 p,float eps)
{
    float dx = SdfMap_0_float(p + float3(eps, 0, 0),eps) - SdfMap_0_float(p + float3(-eps, 0, 0),eps);
    float dy = SdfMap_0_float(p + float3(0, eps, 0),eps) - SdfMap_0_float(p + float3(0, -eps, 0),eps);
    float dz = SdfMap_0_float(p + float3(0, 0, eps),eps) - SdfMap_0_float(p + float3(0, 0, -eps),eps);
    float3 normal = float3(dx, dy, dz);
    return normalize(normal);
}

float3 NormalSdfMap_2_float(float3 p,float eps, float k)
{
    float dx = SdfMap_2_float(p + float3(eps,0,0),k);
    float dy = SdfMap_2_float(p + float3(0,eps,0),k);
    float dz = SdfMap_2_float(p + float3(0,0,eps),k);
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
    int maxSteps = 20;

    alpha = 0.0;
    
    while (t < maxDist)
    {
        float3 p = rayOrigin + t * rayDir;
        
        float d = SdfMap_1_float(p);
        
        if (d <= eps)
        {
            positionWs = p;
            normalWs = NormalSdfMap_0_float(p, eps);
            viewDir = rayDir;
            alpha = 1.0;
            break;
        }

        t+= d;
        steps++;
    }
    
    #endif
}

#endif