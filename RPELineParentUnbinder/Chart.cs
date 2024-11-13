using Newtonsoft.Json;

namespace RPELineParentUnbinder;

[JsonObject]
public class Chart
{
    public JudgeLineList judgeLineList { get; set; }
}

public class JudgeLineList : List<judgeLine>
{
    public Tuple<float, float> GetLineXYPos(int index, float beat)
    {
        //从eventLayers中获取当前时间的位置
        var xPos = this[index].eventLayers.GetXAtBeat(beat);
        var yPos = this[index].eventLayers.GetYAtBeat(beat);
        if (this[index].father != -1)
        {
            var fatherPos = GetLineXYPos(this[index].father, beat);
            var fatherAngle = this[this[index].father].eventLayers.GetAngleAtBeat(beat);
            //通过重载，计算相对位置
            return GetLinePos(fatherPos.Item1, fatherPos.Item2, fatherAngle, xPos, yPos);
        }

        return new(xPos, yPos);
    }

    public static Tuple<float, float> GetLinePos(float fatherLineX, float fatherLineY, float angleDegrees, float lineX, float lineY)
    {
        // 将角度转换为弧度
        float angleRadians = angleDegrees * (float)Math.PI / 180f;

        // 计算旋转后的坐标
        float rotatedX = lineX * (float)Math.Cos(angleRadians) + lineY * (float)Math.Sin(angleRadians);
        float rotatedY = -lineX * (float)Math.Sin(angleRadians) + lineY * (float)Math.Cos(angleRadians);

        // 计算绝对坐标
        float absoluteX = fatherLineX + rotatedX;
        float absoluteY = fatherLineY + rotatedY;

        return new(absoluteX, absoluteY);
    }
}

public class judgeLine
{
    public int father = -1;
    public EventLayers eventLayers;
}

public class Events : List<Event>
{
    public float GetValueAtBeat(float t)
    {
        Event previousChange = null;

        for (int i = 0; i < Count; i++)
        {
            var theEvent = this[i];
            if (t >= theEvent.GetStartBeat() && t <= theEvent.GetEndBeat())
            {
                return theEvent.GetValueAtBeat(t);
            }

            if (t <= theEvent.GetStartBeat())
            {
                break;
            }

            previousChange = theEvent;
        }
        
        return previousChange?.end ?? 0;
    }
}

public class EventLayers : List<EventLayer>
{
    public float GetYAtBeat(float beat) =>
        this.Sum(eventLayer => eventLayer.moveYEvents.GetValueAtBeat(beat));

    public float GetXAtBeat(float beat) =>
        this.Sum(eventLayer => eventLayer.moveXEvents.GetValueAtBeat(beat));

    public float GetAngleAtBeat(float beat) =>
        this.Sum(eventLayer => eventLayer.rotateEvents.GetValueAtBeat(beat));
}

public class EventLayer
{
    public int? index;
    public Events moveXEvents = new();
    public Events moveYEvents = new();
    public Events rotateEvents = new();
}

[JsonObject]
public class Event
{
    public float start { get; set; }
    public float end { get; set; }
    public List<int> startTime { get; set; }
    public List<int> endTime { get; set; }
    public int bezier { get; set; } = 0;
    public List<double> bezierPoints { get; set; } = new() { 0.0, 0.0, 0.0, 0.0 };
    public float easingLeft { get; set; } = 0.0f;
    public float easingRight { get; set; } = 1.0f;
    public int easingType { get; set; } = 1;
    public int linkgroup { get; set; } = 0;

    public float GetValueAtBeat(float beat)
    {
        float startBeat = startTime[0] + (float)startTime[1] / startTime[2];
        float endBeat = endTime[0] + (float)endTime[1] / endTime[2];
        //获得这个拍在这个事件的时间轴上的位置
        float t = (beat - startBeat) / (endBeat - startBeat);
        //获得当前拍的值
        float easedBeat = Easing.Evaluate(easingType, easingLeft, easingRight, t);
        //插值
        return Easing.Lerp(start, end, easedBeat);
    }

    public float GetStartBeat()
    {
        return startTime[0] + (float)startTime[1] / startTime[2];
    }

    public float GetEndBeat()
    {
        return endTime[0] + (float)endTime[1] / endTime[2];
    }
}

public static class BeatConverter
{
    public static int[] BeatToRPEBeat(float beat)
    {
        int RPEBeat0 = (int)Math.Floor(beat);
        double fractionalPart = beat - RPEBeat0;
        int maxDenominator = 10000;
        int RPEBeat1, RPEBeat2;

        FractionalApproximation(fractionalPart, maxDenominator, out RPEBeat1, out RPEBeat2);

        return new[] { RPEBeat0, RPEBeat1, RPEBeat2 };
    }

    private static void FractionalApproximation(double x, int maxDenominator, out int numerator, out int denominator)
    {
        if (x == 0)
        {
            numerator = 0;
            denominator = 1;
            return;
        }

        int sign = x < 0 ? -1 : 1;
        x = Math.Abs(x);

        int n = 0, d = 1;
        int n1 = 1, d1 = 0;
        int n2 = 0, d2 = 1;

        double fraction = x;
        while (d <= maxDenominator)
        {
            int a = (int)Math.Floor(fraction);
            double newFraction = fraction - a;

            n = a * n1 + n2;
            d = a * d1 + d2;

            if (d > maxDenominator)
                break;

            n2 = n1;
            d2 = d1;
            n1 = n;
            d1 = d;

            if (newFraction < 1e-10)
                break;

            fraction = 1.0 / newFraction;
        }

        numerator = n * sign;
        denominator = d;
    }
}