#ifndef BRDF_INCLUDED
#define BRDF_INCLUDED

#include "./HitRecord.hlsl"
#include "./Ray.hlsl"
#include "./Random.hlsl"

float Reflectance(float cosine, float reflectionIndex)
{
    float r0 = (1 - reflectionIndex) / (1 + reflectionIndex);
    r0 = r0 * r0;
    return r0 + (1 - r0) * pow((1 - cosine), 5);
}

bool ScatterDiffuse(inout uint state, inout Ray r, in HitRecord rec, inout float3 attenuation, out Ray scatteredRay)
{
    float3 scatteredDirection = rec.normal + RandomDirection(state);
    scatteredRay = (Ray)0;
    scatteredRay.origin = rec.p;
    scatteredRay.direction = scatteredDirection;
    attenuation = rec.material.albedo;
    return true;
}

bool ScatterMetal(inout uint state,inout Ray r, in HitRecord rec, inout float3 attenuation, out Ray scatteredRay)
{
    float3 reflectedRay = reflect(r.direction, rec.normal);
    reflectedRay = normalize(reflectedRay) + RandomDirection(state) * rec.material.metallicFuzz;
    scatteredRay = (Ray)0;
    scatteredRay.origin = rec.p;
    scatteredRay.direction = reflectedRay;
    return dot(scatteredRay.direction, rec.normal) > 0;
}

bool ScatterDielectric(inout uint state, inout Ray r, in HitRecord rec, inout float3 attenuation, out Ray scatteredRay)
{
    attenuation = float3(1.0f, 1.0f, 1.0f);
    float ri =  rec.frontFace ? (1.0f / rec.material.refractionIndex) : rec.material.refractionIndex;
    
    float3 normalizeDir = normalize(r.direction);
    
    float cosTheta = min(dot(-normalizeDir, rec.normal), 1.0f);
    float sinTheta = sqrt(1.0f - cosTheta * cosTheta);
    
    float3 dirRay;
    if (ri * sinTheta > 1.0 || Reflectance(cosTheta, ri) > RandomValue(state))
    {
        
        dirRay = reflect(normalizeDir, rec.normal);
    }
    else
    {
        dirRay = refract(normalizeDir, rec.normal, ri);
    }
    scatteredRay = (Ray)0;
    scatteredRay.origin = rec.p;
    scatteredRay.direction = dirRay;
    return true;
}

bool Scatter(inout uint state, inout Ray r, in HitRecord rec, inout float3 attenuation, out Ray scattered)
{
    if (rec.material.materialType == MaterialType::Diffuse)
    {
        return ScatterDiffuse(state, r, rec, attenuation, scattered);
    }
    if (rec.material.materialType == MaterialType::Metallic)
    {
        return ScatterMetal(state, r, rec, attenuation, scattered);
    }
    
    if (rec.material.materialType == MaterialType::Dielectric)
    {
        return ScatterDielectric(state, r, rec, attenuation, scattered);
    }
    
    return false;
}

#endif