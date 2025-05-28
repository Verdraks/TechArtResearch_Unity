#define PI 3.14159265359

half sinusoidal_half (half x, half a, half b, half c)
{
    const half u = 0.5;
    return a * sin((x + c) * b) * u + u;
}

half sine_wave_half (half2 uv, half a, half b, half c)
{
    half fx = uv.y;
    half x = uv.x;
    half f = sinusoidal_half(x, a, b, c);
    fx -= f;
    return fx > 0 ? 0.0 : 1.0;
}

half tan_wave_half (half2 uv, half a, half b, half c)
{
    half fx = uv.y;
    half x = uv.x;
    const half px = 0.5;
    half py = sinusoidal_half(px, a, b, c);
    half m = a * tan(PI / 4.0 * cos((px + c) * b)) * (b / 2.0);
    half f = m * (x - px) + py;
    fx -= f;
    return fx > 0 ? 1.0 : 0.0;
}

void trigonometric_operators_half (in half2 uv,
in half a, in half b, in half c, out half Sin, out
half Tan, out half Py)
{
    Sin = sine_wave_half(uv, a, b, c);
    Tan = tan_wave_half(uv, a, b, c);
    Py = sinusoidal_half(0.5, a, b, c);
}