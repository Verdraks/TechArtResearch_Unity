#ifndef BLOB_INCLUDED
#define BLOB_INCLUDED

struct BlobData
{
    float3 position;
    float3 color;
};

StructuredBuffer<BlobData> _BlobBuffer;
int _BlobCount;

float SMin_float(float a, float b, float k)
{
    k *= 1.0 / (1.0 - sqrt(0.5));
    float h = max(k - abs(a - b), 0.0) / k;
    const float b2 = 13.0 / 4.0 - 4.0 * sqrt(0.5);
    const float b3 = 3.0 / 4.0 - 1.0 * sqrt(0.5);
    return min(a, b) - k * h * h * (h * b3 * (h - 4.0) + b2);
}

float SDF_Sphere(float3 p, float3 center, float radius)
{
    return length(p - center) - radius;
}

void SDF_Scene(float3 p, out float dist)
{
    dist = 1.#INF;
    
    for (int i = 0; i < _BlobCount; i++)
    {
        BlobData data = _BlobBuffer[i];
        float d = SDF_Sphere(p, data.position, 1);
        dist = SMin_float(dist, d, 0.1);
    }
}

/// @param maxDepth Depth in LinearEye
void BlobTrace_float(float3 viewDir, float3 viewPos, float maxStep, float2 rangeView, float eps, out float3 color, out bool hit, out float t)
{
    #if defined(SHADERGRAPH_PREVIEW)
    color = float3(0, 0, 0);
    hit = false;
    t = 0;
    return;
    #endif
    
    float distance = rangeView.x;
    hit = false;
    color = float3(0, 0, 0);
    int steps = 0;
    
    while (steps < maxStep && distance <= rangeView.y)
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

    t = distance;
}
#endif