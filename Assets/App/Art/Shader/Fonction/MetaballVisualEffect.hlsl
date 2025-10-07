#define METABALL_DATA_INCLUDED 1
#include "MetaballData.hlsl"

void SetMetaballData(inout VFXAttributes attributes, RWStructuredBuffer<MetaballData> metaballsDataBuffer)
{
    uint index = attributes.particleIndexInStrip; 
    float3 pos = attributes.position;
    float size = attributes.size;

    MetaballData data = {
        pos.x, pos.y, pos.z,
        size,
        attributes.color.r, attributes.color.g, attributes.color.b
    };
    
    metaballsDataBuffer[index] = data;
}