#ifndef BLOB_COMMON_INCLUDED
#define BLOB_COMMON_INCLUDED

struct BlobData
{
    float3 position;
    float3 color;
    float size;
};

void Smin_Circular(in float a, in float b, in float k, out float d)
{
    const float b2 = 13.0 / 4.0 - 4.0 * sqrt(0.5);
    const float b3 = 3.0 / 4.0 - 1.0 * sqrt(0.5);

    k *= 1.0 / (1.0 - sqrt(0.5));
    float h = max(k - abs(a - b), 0.0) / k;
    d = min(a, b) - k * h * h * (h * b3 * (h - 4.0) + b2);
}

void Smin_Polynomial(in float a, in float b, in float k, out float d)
{
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    d = lerp(b, a, h) - k * h * (1.0 - h);
}

float SDF_Sphere(float3 p, float3 center, float radius)
{
    return length(p - center) - radius;
}

#define SDF_NORMAL_THETRAEDRE(p, eps, normal, SDF_SCENE) \
    float h = max(FLT_EPS, eps * 1.5f); \
    \
    float3 e1 = float3(h, -h, -h); \
    float3 e2 = float3(-h, -h, h); \
    float3 e3 = float3(-h, h, -h); \
    float3 e4 = float3(h, h, h); \
    \
    float de1, de2, de3, de4; \
    \
    SDF_SCENE(p + e1, de1); \
    SDF_SCENE(p + e2, de2);\
    SDF_SCENE(p + e3, de3); \
    SDF_SCENE(p + e4, de4); \
    \
    float ddx = de1 - de2 - de3 + de4; \
    float ddy = -de1 - de2 + de3 + de4; \
    float ddz = -de1 + de2 - de3 + de4; \
    \
    normal = normalize(float3(ddx, ddy, ddz));

#define SDF_NORMAL_OCTAEDRE(p, eps, normal, SDF_SCENE) \
    float h = max(FLT_EPS, eps * 1.5f); \
    \
    float d_right, d_left, d_up, d_down, d_front, d_back; \
    SDF_Scene(p + float3(h, 0, 0), d_right); \
    SDF_Scene(p + float3(-h, 0, 0), d_left); \
    SDF_Scene(p + float3(0, h, 0), d_up); \
    SDF_Scene(p + float3(0, -h, 0), d_down); \
    SDF_Scene(p + float3(0, 0, h), d_front); \
    SDF_Scene(p + float3(0, 0, -h), d_back); \
    \
    float pente_x = (d_right - d_left) / (2 * h); \
    float pente_y = (d_up - d_down) / (2 * h); \
    float pente_z = (d_front - d_back) / (2 * h); \
    normal = normalize(float3(pente_x, pente_y, pente_z));

#define RAYMARCHING_LOOP(ro, rd, tMinMax, data, t, SDF_SCENE, HIT) \
    float d = 0.0; \
    float t = (tMinMax).x; \
    UNITY_LOOP \
    for (int i = 0; i < _MaxSteps; i++) \
    { \
        float3 p = (ro) + (rd) * t; \
        SDF_SCENE(p, d, data); \
        t += d; \
        if (d < _Eps) \
        { \
            HIT(p,data); \
            break; \
        } \
        if (t >= (tMinMax).y) \
        { \
            break; \
        } \
    }

#endif
