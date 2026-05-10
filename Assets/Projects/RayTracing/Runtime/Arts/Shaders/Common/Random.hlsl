#ifndef RANDOM_INCLUDED
#define RANDOM_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


// PCG (permuted congruential generator).
// www.pcg-random.org
// www.shadertoy.com/view/XlGcRh
uint NextRandom(inout uint state)
{
    state = state * 747796405 + 2891336453;
    uint result = ((state >> ((state >> 28) + 4)) ^ state) * 277803737;
    result = (result >> 22) ^ result;
    return result;
}

float RandomValue(inout uint state)
{
    return NextRandom(state) / 4294967295.0; // 2^32 - 1
}

//https://stackoverflow.com/questions/5825680/code-to-generate-gaussian-normally-distributed-random-numbers-in-ruby/6178290#6178290
float GaussianRandomNormalDistribution(inout uint state)
{
    float theta = 2 * PI * RandomValue(state);
    float rho = sqrt(-2 * log(RandomValue(state)));
    return rho * cos(theta);
}

float3 RandomDirection(inout uint seed)
{
    float x = GaussianRandomNormalDistribution(seed);
    float y = GaussianRandomNormalDistribution(seed);
    float z = GaussianRandomNormalDistribution(seed);

    return SafeNormalize(float3(x, y, z));
}

float3 RandomHemisphereDirection(float3 normal, inout uint state)
{
    float3 direction = RandomDirection(state);
    return direction * sign(dot(normal, direction));
}

float2 RandomPointInCircle(inout uint state)
{
    float angle = RandomValue(state) * 2 * PI;
    float2 pointInCircle = float2(cos(angle), sin(angle));
    return pointInCircle * SafeSqrt(RandomValue(state));
}

#endif
