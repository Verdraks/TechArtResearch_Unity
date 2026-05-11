#ifndef MATERIAL_INCLUDED
#define MATERIAL_INCLUDED

// #include "./HitRecord.hlsl"
//
// enum MaterialType
// {
//     Diffuse,
//     Metal,
//     Glass,
// };

struct MaterialInfo
{
    float4 albedo;
    float3 emissive;
    float emissiveStrength;
    // MaterialType type;

    // bool Scatter(inout Ray ray, in HitRecord rec, inout float3 attenuation, out Ray scattered)
    // {
    //     switch (type)
    //     {
    //         case Diffuse:
    //             return ScatterDiffuse( ray, rec, attenuation, scattered);
    //         default: 
    //             return true;
    //     }
    // }
    //
    // bool ScatterDiffuse(inout Ray r, in HitRecord rec, inout float3 attenuation, out Ray scattered)
    // {
    //     
    // }
    //
    // float3 GetEmissive()
    // {
    //     return emissive * emissiveStrength;
    // }
    //
    // float3 GetAlbedo()
    // {
    //     
    // }
};



#endif
