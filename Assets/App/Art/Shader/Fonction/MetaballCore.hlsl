#ifndef METABALL_CORE_INCLUDED
#define METABALL_CORE_INCLUDED

#include "MetaballData.hlsl"

StructuredBuffer<MetaballData> _MetaballsDataBuffer;
int _MetaballsCount;

float GetDistanceMetaball_float(float3 p)
{
    float sumDensity = 0.0;
    float sumRi = 0.0;
    float minDist = 1000000.0;

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

float GetDistanceSphere_float(float3 p, float3 center, float radius)
{
    return length(p - center) - radius;
}


void SphereTraceMetaballs_float(float3 worldPosition, float3 viewPosition, out float alpha)
{
    #if defined(SHADERGRAPH_PREVIEW)
    alpha = 1.0;
    #else
    
    float maxDist = 100.0;
    float threshold = 0.0001;
    float t = 0.0;
    int numSteps = 0;
    
    half3 viewDir = normalize( worldPosition - viewPosition );

    for (int i = 0; i < _MetaballsCount; i++)
    {
        while (t < maxDist)
        {
            float minDist = 1000000.0;
            float3 from = viewPosition + t * viewDir;

            float d = 0.0;
            //Work, data buffer filled
            d = GetDistanceSphere_float(from, _MetaballsDataBuffer[i].Position, _MetaballsDataBuffer[i].Radius);

            //Dont work
            // d = GetDistanceMetaball_float(from);
        

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
    }
    
    #endif
}

#endif