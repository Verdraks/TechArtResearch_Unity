#ifndef SPHERE_INCLUDED
#define SPHERE_INCLUDED

#include "./HitRecord.hlsl"
#include "./Interval.hlsl"
#include "./MaterialInfo.hlsl"
#include "./Ray.hlsl"

struct Sphere
{
    float3 center;
    float radius;
    MaterialInfo material;
    
    HitRecord CalculateRayHit(Ray r, Interval interval)
    {
        HitRecord rec = (HitRecord)0;
        
        float3 oc = center - r.origin;
        float a = Length2(r.direction);
        float half_b = dot(oc, r.direction);
        float c = Length2(oc) - radius * radius;
        float discriminant = half_b * half_b - a * c;
        
        if (discriminant < 0)
        {
            rec.hit = false;
            return rec;
        }
        
        float sqrt_discriminant = sqrt(discriminant);
        
        float root = (half_b - sqrt_discriminant) / a;
        
        if (interval.Surrounds(root) == false)
        {
            root = (half_b + sqrt_discriminant) / a;
        
            if (interval.Surrounds(root) == false)
            {
                rec.hit = false;
                return rec;
            }
        }
        
        rec.t = root;
        rec.p = r.RayAt(rec.t);
        float3 outwardNormal = (rec.p - center) / radius;
        rec.SetFaceNormal(r, outwardNormal);
        rec.material = material;
        
        rec.hit = true;
        return rec;
    }
};
#endif
