﻿using System;

namespace RPELineParentUnbinder;

public static class Easing
{
    // Delegate for easing functions
    public delegate float EasingFunction(float t);

    // Linear
    public static float Linear(float t) => t;

    // Quadratic
    public static float EaseInQuad(float t) => t * t;
    public static float EaseOutQuad(float t) => t * (2 - t);
    public static float EaseInOutQuad(float t) =>
        t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

    // Cubic
    public static float EaseInCubic(float t) => t * t * t;
    public static float EaseOutCubic(float t)
    {
        t--;
        return t * t * t + 1;
    }
    public static float EaseInOutCubic(float t) =>
        t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

    // Quartic
    public static float EaseInQuart(float t) => t * t * t * t;
    public static float EaseOutQuart(float t)
    {
        t--;
        return 1 - t * t * t * t;
    }
    public static float EaseInOutQuart(float t) =>
        t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;

    // Quintic
    public static float EaseInQuint(float t) => t * t * t * t * t;
    public static float EaseOutQuint(float t)
    {
        t--;
        return t * t * t * t * t + 1;
    }
    public static float EaseInOutQuint(float t) =>
        t < 0.5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;

    // Sine
    public static float EaseInSine(float t) =>
        (float)(1 - Math.Cos(t * Math.PI / 2));
    public static float EaseOutSine(float t) =>
        (float)Math.Sin(t * Math.PI / 2);
    public static float EaseInOutSine(float t) =>
        (float)(-0.5 * (Math.Cos(Math.PI * t) - 1));

    // Exponential
    public static float EaseInExpo(float t) =>
        (float)(t == 0 ? 0 : Math.Pow(2, 10 * (t - 1)));
    public static float EaseOutExpo(float t) =>
        (float)(t == 1 ? 1 : 1 - Math.Pow(2, -10 * t));
    public static float EaseInOutExpo(float t)
    {
        if (t == 0 || t == 1) return t;
        return t < 0.5f
            ? (float)(0.5 * Math.Pow(2, 20 * t - 10))
            : (float)(1 - 0.5 * Math.Pow(2, -20 * t + 10));
    }

    // Circular
    public static float EaseInCirc(float t) =>
        (float)(1 - Math.Sqrt(1 - t * t));
    public static float EaseOutCirc(float t) =>
        (float)Math.Sqrt(1 - (--t) * t);
    public static float EaseInOutCirc(float t) =>
        t < 0.5f
            ? (float)(0.5 * (1 - Math.Sqrt(1 - 4 * t * t)))
            : (float)(0.5 * (Math.Sqrt(1 - 4 * (--t) * t) + 1));

    // Back
    public static float EaseInBack(float t)
    {
        const float s = 1.70158f;
        return t * t * ((s + 1) * t - s);
    }
    public static float EaseOutBack(float t)
    {
        const float s = 1.70158f;
        t--;
        return t * t * ((s + 1) * t + s) + 1;
    }
    public static float EaseInOutBack(float t)
    {
        const float s = 1.70158f * 1.525f;
        t *= 2;
        if (t < 1)
            return 0.5f * (t * t * ((s + 1) * t - s));
        t -= 2;
        return 0.5f * (t * t * ((s + 1) * t + s) + 2);
    }

    // Elastic
    public static float EaseInElastic(float t)
    {
        if (t == 0 || t == 1) return t;
        return (float)(-Math.Pow(2, 10 * (t - 1)) *
            Math.Sin((t - 1.1f) * 5 * Math.PI));
    }
    public static float EaseOutElastic(float t)
    {
        if (t == 0 || t == 1) return t;
        return (float)(Math.Pow(2, -10 * t) *
            Math.Sin((t - 0.1f) * 5 * Math.PI) + 1);
    }
    public static float EaseInOutElastic(float t)
    {
        if (t == 0 || t == 1) return t;
        t *= 2;
        if (t < 1)
            return (float)(-0.5 * Math.Pow(2, 10 * (t - 1)) *
                Math.Sin((t - 1.1f) * 5 * Math.PI));
        t--;
        return (float)(Math.Pow(2, -10 * t) *
            Math.Sin((t - 0.1f) * 5 * Math.PI) * 0.5f + 1);
    }

    // Bounce
    public static float EaseInBounce(float t) =>
        1 - EaseOutBounce(1 - t);
    public static float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;
        if (t < 1 / d1)
            return n1 * t * t;
        else if (t < 2 / d1)
        {
            t -= 1.5f / d1;
            return n1 * t * t + 0.75f;
        }
        else if (t < 2.5f / d1)
        {
            t -= 2.25f / d1;
            return n1 * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }
    public static float EaseInOutBounce(float t) =>
        t < 0.5f
            ? (1 - EaseOutBounce(1 - 2 * t)) * 0.5f
            : (EaseOutBounce(2 * t - 1) + 1) * 0.5f;

    // Method to evaluate easing between any start and end point
    public static float Evaluate(EasingFunction function, float start, float end, float t)
    {
        float value = function(t);
        return start + (end - start) * value;
    }
    //重载，使用int指定对应的EasingFunction
    public static float Evaluate(int easingType, float start, float end, float t)
    {
        EasingFunction function = easingType switch
        {
            1 => Linear,
            2 => EaseOutSine,
            3 => EaseInSine,
            4 => EaseOutQuad,
            5 => EaseInQuad,
            6 => EaseInOutSine,
            7 => EaseInOutQuad,
            8 => EaseOutCubic,
            9 => EaseInCubic,
            10 => EaseOutQuart,
            11 => EaseInQuart,
            12 => EaseInOutCubic,
            13 => EaseInOutQuart,
            14 => EaseOutQuint,
            15 => EaseInQuint,
            16 => EaseOutExpo,
            17 => EaseInExpo,
            18 => EaseOutCirc,
            19 => EaseInCirc,
            20 => EaseOutBack,
            21 => EaseInBack,
            22 => EaseInOutCirc,
            23 => EaseInOutBack,
            24 => EaseOutElastic,
            25 => EaseInElastic,
            26 => EaseOutBounce,
            27 => EaseInBounce,
            28 => EaseInOutBounce,
            _ => Linear,
        };
        return Evaluate(function, start, end, t);
    }
    
    public static float Lerp(float start, float end, float t) =>
        start + (end - start) * t;
}
/*
1	Linear	-
2	Out Sine	-
3	In Sine	-
4	Out Quad	-
5	In Quad	-
6	In Out Sine	-
7	In Out Quad	-
8	Out Cubic	-
9	In Cubic	-
10	Out Quart	-
11	In Quart	-
12	In Out Cubic	-
13	In Out Quart	-
14	Out Quint	-
15	In Quint	-
16	Out Expo	-
17	In Expo	-
18	Out Circ	-
19	In Circ	-
20	Out Back	-
21	In Back	-
22	In Out Circ	-
23	In Out Back	-
24	Out Elastic	-
25	In Elastic	-
26	Out Bounce	-
27	In Bounce	-
28	In Out Bounce	-
 */