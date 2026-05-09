#ifndef RAY_INCLUDED
#define RAY_INCLUDED

struct Ray
{
    float3 origin;
    float3 direction;
    
    float3 RayAt(half t)
    {
        return origin + t * direction;
    }
};

#endif