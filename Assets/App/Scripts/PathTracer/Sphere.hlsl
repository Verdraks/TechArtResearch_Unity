#ifndef SPHERE_INCLUDED
#define SPHERE_INCLUDED

#include "./Ray.hlsl"
#include "./HitRecord.hlsl"
#include "./Material.hlsl"

struct Sphere
{
    
    HitRecord hit(Ray r)
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
        
        if (root < 0)
        {
            rec.hit = false;
            return rec;
        }
        
        rec.t = root;
        rec.p = r.RayAt(rec.t);
        float3 outwardNormal = (rec.p - center) / radius;
        rec.SetFaceNormal(r, outwardNormal);
        rec.material = material;
        
        rec.hit = true;
        return rec;
    }
    
    float3 center;
    float radius;
    Material material;
    
};


#endif
