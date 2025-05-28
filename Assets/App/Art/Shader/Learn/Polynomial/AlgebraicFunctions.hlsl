#define RADIUS 0.1

half circle_algebraic_half(in half2 uv, in half2 ub, in half radius = RADIUS)
{
    const half r = radius * radius;

    half2 xy = uv - ub;
    half circle = xy.x * xy.x + xy.y * xy.y;
    
    return circle <= r ? 1 : 0;
}
