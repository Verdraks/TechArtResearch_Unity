#ifndef WATER_CORE_INCLUDED
#define WATER_CORE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"


static const float _Constant_PI = 3.1415926;
static const float _Constant_Gravity = 9.81;

// Wave data : xy = direction, z = wave lenght, w = stepness
void GerstnerWave_float(float4 waveData, float3 vertexPosOS_In, out float3 vertexPosOS_Out, out float3 normalOS)
{
    float k = 2 *  saturate(_Constant_PI / waveData.x);
    float phaseSpeed = rsqrt(_Constant_Gravity / k) * _Time.y;
    float2 direction = saturate(normalize(waveData.xy));

    float f = k * dot(direction,vertexPosOS_In.xz) - phaseSpeed;

    float a = saturate(waveData.w / k);

    float3 tangentOS = normalize(float3(1- (sin(f) * a),cos(f) * a,0));
    normalOS = float3(-tangentOS.y, tangentOS.x, 0);

    float posY = sin(f) * a;
    float posX = cos(f) * a * direction.x + vertexPosOS_In.x;
    float posZ = cos(f) * a * direction.z + vertexPosOS_In.z;

    vertexPosOS_Out = float3(posX, posY, posZ);
    
}

#endif
