#ifndef METABALL_CORE_INCLUDED
#define METABALL_CORE_INCLUDED

#include "MetaballData.hlsl"

StructuredBuffer<MetaballData> _MetaballsDataBuffer;
int _MetaballsCount;

void GetDistanceMetaball_float(float3 p, out float distance)
{
    float sumDensity = 0.0;
    float sumRi = 0.0;
    float minDist = 1000000.0;

    for (int i = 0; i < _MetaballsCount; i++)
    {
        float3 center = _MetaballsDataBuffer[i].Position;
        float radius = 1;
        float r = length(p - center);
        if (r < radius)
        {
            sumDensity += 2.0 * (r*r*r) / (radius * radius * radius) - 3.0 * (r*r) / (radius * radius) + 1.0;
        }
        minDist = min(minDist, r-radius);
        sumRi += radius;
    }
    distance = max(minDist, (0.2 - sumDensity) / (3.0 / 2.0 * sumRi));
}

void SphereTraceMetaballs_float(float3 worldPosition, float3 viewPosition, out float alpha)
{
    #if defined(SHADERGRAPH_PREVIEW)
    alpha = 1.0;
    #else

    
        float3 c = float3(0,0,0);
        float dist = length(worldPosition - c);
        alpha = saturate(1.0 - dist / 2.0); // fade autour du premier metaball
        return;
    
    float maxDist = 100.0;
    float threshold = 0.0001;
    float t = 0.0;
    int numSteps = 0;
    
    half3 viewDir = normalize(viewPosition - worldPosition);

    while (t < maxDist)
    {
        float minDist = 1000000.0;
        float3 from = viewPosition + t * viewDir;
        float d = 0.0;
        GetDistanceMetaball_float(from,d);

        if (d < minDist)
        {
            minDist = d;
        }

        if (minDist <= threshold * t)
        {
            alpha = 1.0;
            break;
        }

        t+= minDist;
        numSteps++;
    }
    
    #endif
}

#endif