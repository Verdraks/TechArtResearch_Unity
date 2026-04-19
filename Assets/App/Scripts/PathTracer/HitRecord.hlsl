#ifndef HITRECORD_INCLUDED
#define HITRECORD_INCLUDED

#include "./Ray.hlsl"
#include "./Material.hlsl"

struct HitRecord
{
    float3 p;
    float3 normal;
    float t;
    bool frontFace;
    bool hit;
    Material material;
    
    void SetFaceNormal(Ray r, float3 outwardNormal)
    {
        frontFace = dot(r.direction, outwardNormal) < 0;
        normal = frontFace ? outwardNormal : -outwardNormal;
    }
};

#endif
