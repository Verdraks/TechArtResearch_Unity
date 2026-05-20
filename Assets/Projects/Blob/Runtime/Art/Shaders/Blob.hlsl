#ifndef BLOB_INCLUDED
#define BLOB_INCLUDED

struct BlobData
{
    float3 position;
    float3 color;
};

StructuredBuffer<BlobData> _BlobData;
int _BlobCount;

float SDF_Sphere(float3 p, float radius)
{
    return length(p) - radius;
}

void SDF_Scene(out float dist)
{
    dist = 1.#INF;
    
    for (int i = 0; i < _BlobCount; i++)
    {
        BlobData data = _BlobData[i];
        float d = SDF_Sphere(data.position, 1);
        if (d < dist)
        {
            dist = d;
        }
    }
}

void BlobTrace_float(float3 viewDir, float3 viewPos, float maxDepth, float maxDistance, float eps, out float3 color, out float hit)
{
    #if SHADERGRAPH_PREVIEW
    color = float3(0, 0, 0);
    hit = 0.0;
        return;
    #endif
    
    float t = 0;
    hit = 0.0;
    color = float3(0, 0, 0);
    
    for (int i = 0; i < maxDepth; i++)
    {
        float3 pos = viewPos + t * viewDir;
        float dist;
        SDF_Scene(dist);
        t += dist;
        
        if (dist < eps)
        {
            hit = 1.0;
            break;
        }
        
        if (t > maxDistance)
        {
            break;
        }
    }
}


#endif