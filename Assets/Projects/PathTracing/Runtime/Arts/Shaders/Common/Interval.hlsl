#ifndef INTERVAL_INCLUDED
#define INTERVAL_INCLUDED

struct Interval
{
    float min;
    float max;
    
    bool Surrounds(float x)
    {
        return x > min && x < max;
    }
    
    bool Contains(float x)
    {
        return x >= min && x <= max;
    }
};

#endif
