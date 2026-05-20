#ifndef BLOB_INCLUDED
#define BLOB_INCLUDED

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

void BlobTrace_float(float3 viewDir, float3 viewPos, float maxDepth, float maxDistance, float eps, out float3 color, out bool hit)
{
    #if defined(SHADERGRAPH_PREVIEW)
    color = float3(0, 0, 0);
    hit = false;
    return;
    #endif
    
    float t = 0;
    hit = false;
    color = float3(0, 0, 0);
    int steps = 0;
    
    while (steps < maxDepth && t < maxDistance)
    {
        float3 pos = viewPos + t * viewDir;
        float dist;
        SDF_Scene(pos, dist);
        t += dist;
        
        if (dist <= eps)
        {
            hit = true;
            break;
        }
        
        steps++;
    }
}


#endif