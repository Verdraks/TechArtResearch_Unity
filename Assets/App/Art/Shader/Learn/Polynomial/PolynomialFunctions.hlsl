half tangent_polynomial_half(in half2 point_a, in half2 point_b)
{
    half x = point_b.x- point_a.x;
    half y = point_b.y - point_a.y;
    return y/x;
}


half linear_polynomial_half(in half2 uv, in half m, in half b, in half u)
{
    half fx = uv.y;
    half x = uv.x;

    half f = m * (x - u)+ b;
    fx -= f;
    
    return  fx > 0.0 ? 1.0 : 0.0;
}

half segment_polynomial_half(in half2 uv, in half2 point_a, in half2 point_b)
{
    half fx = uv.y;
    half x = uv.x;
    half m = tangent_polynomial_half(point_a,point_b);
    half f = m*(x - point_a.x)+ point_a.y;
    fx -= f;
    return  fx > 0.0 ? 1.0 : 0.0;   
}

half quadratic_polynomial_half(in half2 uv, in half a, in half b, in half c , in half u)
{
    half fx = uv.y;
    half x = uv.x - u;

    half f = a*x*x + b*x + c;
    fx -= f;
    return fx > 0.0 ? 1.0 : 0.0;
}


