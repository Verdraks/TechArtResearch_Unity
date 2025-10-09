#define METABALL_DATA_INCLUDED 1
#include "MetaballData.hlsl"

void SetMetaballData(inout VFXAttributes attributes, RWStructuredBuffer<MetaballData> metaballsDataBuffer)
{
    uint index = attributes.particleIdClamped; 
    float3 pos = attributes.position;
    float size = attributes.size;

    MetaballData data = {
        pos.x, pos.y, pos.z,
        size,
    };
    
    metaballsDataBuffer[index] = data;
}