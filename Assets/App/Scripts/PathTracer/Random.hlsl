#ifndef RANDOM_INCLUDED
#define RANDOM_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//https://stackoverflow.com/questions/5825680/code-to-generate-gaussian-normally-distributed-random-numbers-in-ruby/6178290#6178290
float GaussianRandomNormalDistribution(inout uint state)
{
    float theta = 2 * PI * GenerateHashedRandomFloat(state);
    float rho = sqrt(-2 * log(GenerateHashedRandomFloat(state)));
    return rho * cos(theta);
}

float3 RandomDirection(inout uint seed)
{
    float x = GenerateHashedRandomFloat(seed);
    float y = GenerateHashedRandomFloat(seed);
    float z = GenerateHashedRandomFloat(seed);
    
    return SafeNormalize(float3(x, y, z));
}

float3 RandomHemisphereDirection(float3 normal, inout uint state)
{
    float3 direction = RandomDirection(state);
    return direction * sign(dot(normal, direction));
}

#endif
