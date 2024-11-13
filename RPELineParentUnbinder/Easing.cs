using System;

namespace RPELineParentUnbinder;

public static class Easing
{
    // Delegate for easing functions
    public delegate double EasingFunction(double t);

    // Linear
    public static double Linear(double t) => t;

    // Quadratic
    public static double EaseInQuad(double t) => t * t;
    public static double EaseOutQuad(double t) => t * (2 - t);
    public static double EaseInOutQuad(double t) =>
        t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;

    // Cubic
    public static double EaseInCubic(double t) => t * t * t;
    public static double EaseOutCubic(double t)
    {
        t--;
        return t * t * t + 1;
    }
    public static double EaseInOutCubic(double t) =>
        t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

    // Quartic
    public static double EaseInQuart(double t) => t * t * t * t;
    public static double EaseOutQuart(double t)
    {
        t--;
        return 1 - t * t * t * t;
    }
    public static double EaseInOutQuart(double t) =>
        t < 0.5 ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;

    // Quintic
    public static double EaseInQuint(double t) => t * t * t * t * t;
    public static double EaseOutQuint(double t)
    {
        t--;
        return t * t * t * t * t + 1;
    }
    public static double EaseInOutQuint(double t) =>
        t < 0.5 ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;

    // Sine
    public static double EaseInSine(double t) =>
        1 - Math.Cos(t * Math.PI / 2);
    public static double EaseOutSine(double t) =>
        Math.Sin(t * Math.PI / 2);
    public static double EaseInOutSine(double t) =>
        -0.5 * (Math.Cos(Math.PI * t) - 1);

    // Exponential
    public static double EaseInExpo(double t) =>
        t == 0 ? 0 : Math.Pow(2, 10 * (t - 1));
    public static double EaseOutExpo(double t) =>
        t == 1 ? 1 : 1 - Math.Pow(2, -10 * t);
    public static double EaseInOutExpo(double t)
    {
        if (t == 0 || t == 1) return t;
        return t < 0.5
            ? 0.5 * Math.Pow(2, 20 * t - 10)
            : 1 - 0.5 * Math.Pow(2, -20 * t + 10);
    }

    // Circular
    public static double EaseInCirc(double t) =>
        1 - Math.Sqrt(1 - t * t);
    public static double EaseOutCirc(double t) =>
        Math.Sqrt(1 - (--t) * t);
    public static double EaseInOutCirc(double t) =>
        t < 0.5
            ? 0.5 * (1 - Math.Sqrt(1 - 4 * t * t))
            : 0.5 * (Math.Sqrt(1 - 4 * (--t) * t) + 1);

    // Back
    public static double EaseInBack(double t)
    {
        const double s = 1.70158;
        return t * t * ((s + 1) * t - s);
    }
    public static double EaseOutBack(double t)
    {
        const double s = 1.70158;
        t--;
        return t * t * ((s + 1) * t + s) + 1;
    }
    public static double EaseInOutBack(double t)
    {
        const double s = 1.70158 * 1.525;
        t *= 2;
        if (t < 1)
            return 0.5 * (t * t * ((s + 1) * t - s));
        t -= 2;
        return 0.5 * (t * t * ((s + 1) * t + s) + 2);
    }

    // Elastic
    public static double EaseInElastic(double t)
    {
        if (t == 0 || t == 1) return t;
        return -Math.Pow(2, 10 * (t - 1)) *
            Math.Sin((t - 1.1) * 5 * Math.PI);
    }
    public static double EaseOutElastic(double t)
    {
        if (t == 0 || t == 1) return t;
        return Math.Pow(2, -10 * t) *
            Math.Sin((t - 0.1) * 5 * Math.PI) + 1;
    }
    public static double EaseInOutElastic(double t)
    {
        if (t == 0 || t == 1) return t;
        t *= 2;
        if (t < 1)
            return -0.5 * Math.Pow(2, 10 * (t - 1)) *
                Math.Sin((t - 1.1) * 5 * Math.PI);
        t--;
        return Math.Pow(2, -10 * t) *
            Math.Sin((t - 0.1) * 5 * Math.PI) * 0.5 + 1;
    }

    // Bounce
    public static double EaseInBounce(double t) =>
        1 - EaseOutBounce(1 - t);
    public static double EaseOutBounce(double t)
    {
        const double n1 = 7.5625;
        const double d1 = 2.75;
        if (t < 1 / d1)
            return n1 * t * t;
        else if (t < 2 / d1)
        {
            t -= 1.5 / d1;
            return n1 * t * t + 0.75;
        }
        else if (t < 2.5 / d1)
        {
            t -= 2.25 / d1;
            return n1 * t * t + 0.9375;
        }
        else
        {
            t -= 2.625 / d1;
            return n1 * t * t + 0.984375;
        }
    }
    public static double EaseInOutBounce(double t) =>
        t < 0.5
            ? (1 - EaseOutBounce(1 - 2 * t)) * 0.5
            : (EaseOutBounce(2 * t - 1) + 1) * 0.5;

    // Method to evaluate easing between any start and end point
    public static double Evaluate(EasingFunction function, double start, double end, double t)
    {
        double scaledT = start + t * (end - start);
        return function(scaledT);
    }
    // Overload, using int to specify the corresponding EasingFunction
    public static double Evaluate(int easingType, double start, double end, double t)
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

    public static double Lerp(double start, double end, double t) =>
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