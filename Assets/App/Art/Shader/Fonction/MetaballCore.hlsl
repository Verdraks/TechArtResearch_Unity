#ifndef METABALL_CORE_INCLUDED
#define METABALL_CORE_INCLUDED

#include "MetaballData.hlsl"

StructuredBuffer<MetaballData> _MetaballsDataBuffer;
int _MetaballsCount;

float GetDistanceMetaball_float(float3 p)
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

float3 CalculateNormalMetaball_float(float3 from)
{
    float delta = 10e-5;
    float3 normal = float3(
        GetDistanceMetaball_float(from + float3(delta, 0, 0)) - GetDistanceMetaball_float(from + float3(-delta, 0, 0)),
        GetDistanceMetaball_float(from + float3(0, delta, 0)) - GetDistanceMetaball_float(from + float3(-0, -delta, 0)),
        GetDistanceMetaball_float(from + float3(0, 0, delta)) - GetDistanceMetaball_float(from + float3(0, 0, -delta))
    );
    return normalize(normal);
}

void SphereTraceMetaballs_float(float3 worldPosition, float3 viewPosition, out float alpha, out float3 normalWs)
{
    #if defined(SHADERGRAPH_PREVIEW)
    alpha = 1.0;
    normalWs = float3(0,0,0);
    #else
    
    float maxDist = 100.0;
    float threshold = 0.00001;
    float t = 0.0;
    int numSteps = 0;
    
    half3 viewDir = normalize( worldPosition - viewPosition );

    
    while (t < maxDist)
    {
        float3 from = viewPosition + t * viewDir;

        //Work, data buffer filled
        // float d = GetDistanceSphere_float(from, _MetaballsDataBuffer[0].Position, _MetaballsDataBuffer[0].Radius);

        float d = GetDistanceMetaball_float(from);
        
        if (d <= threshold * t)
        {
            alpha = 1.0;
            normalWs = CalculateNormalMetaball_float(from);
            break;
        }

        t+= d;
        numSteps++;
    }
    
    #endif
}

#endif