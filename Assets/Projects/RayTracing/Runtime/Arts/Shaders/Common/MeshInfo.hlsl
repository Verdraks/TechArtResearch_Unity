#ifndef MESH_INCLUDED
#define MESH_INCLUDED

#include "./Interval.hlsl"
#include "./Ray.hlsl"
#include "./HitRecord.hlsl"
#include "./MaterialInfo.hlsl"

struct Triangle
{
    float3 posA, posB, posC;
    
    //https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
    //https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution.html
    //https://stackoverflow.com/questions/42740765/intersection-between-line-and-triangle-in-3d/42752998#42752998
    HitRecord CalculateRayHit(Ray r, Interval interval)
    {
        HitRecord rec;
        float3 edgeAB = posB - posA;
        float3 edgeAC = posC - posA;
        float3 normal = cross(edgeAB, edgeAC);
        
        float det = -dot(r.direction, normal);
        
        if (abs(det) < FLT_EPS)
        {
            rec.hit = false;
            return rec;
        }
        
        float detInv = 1.0f / det;
      
        float3 ao = r.origin - posA;
        float3 dirAo = cross(ao, r.direction);
        
        float u = dot(dirAo, edgeAC) * detInv;
        if (u < -FLT_EPS || u - 1 > FLT_EPS)
        {
            rec.hit = false;
            return rec;
        }
        
        float v = -dot(dirAo, edgeAB) * detInv;
        if (v < -FLT_EPS || u + v - 1 > FLT_EPS)
        {
            rec.hit = false;
            return rec;
        }
        
        float t = dot(ao, normal) * detInv;
        if (interval.Surrounds(t) == false)
        {
            rec.hit = false;
            return rec;
        }
        
        rec.hit = true;
        rec.t = t;
        rec.p = r.RayAt(t);
        rec.SetFaceNormal(r, normalize(normal));
        return rec;
    }
};

struct MeshInfo
{
    MaterialInfo mat;
    uint firstTriangleIndex;
    uint trianglesCount;
};

#endif