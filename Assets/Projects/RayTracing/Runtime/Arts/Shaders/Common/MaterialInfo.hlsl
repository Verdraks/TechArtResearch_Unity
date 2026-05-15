#ifndef MATERIAL_INCLUDED
#define MATERIAL_INCLUDED

struct MaterialInfo
{
    float3 albedo;
    float emissiveStrength;
    float3 emissive;
    float metallicFuzz;
    float refractionIndex;
    int materialType;
    
    float3 GetEmissive()
    {
        return emissive * emissiveStrength;
    }
};

struct MaterialType
{
    static const int Diffuse = 0;
    static const int Metallic = 1;
    static const int Dielectric = 2;
};
#endif
