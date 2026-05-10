#ifndef HITRECORD_INCLUDED
#define HITRECORD_INCLUDED

#include "./Ray.hlsl"
#include "./MaterialInfo.hlsl"

struct HitRecord
{
    MaterialInfo material;
    float3 p;
    float t;
    float3 normal;
    bool frontFace;
    bool hit;
    
    void SetFaceNormal(Ray r, float3 outwardNormal)
    {
        frontFace = dot(r.direction, outwardNormal) < 0;
        normal = frontFace ? outwardNormal : -outwardNormal;
    }
};

#endif
