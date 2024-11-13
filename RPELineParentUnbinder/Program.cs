using System;
using Newtonsoft.Json;
using RPELineParentUnbinder;

Console.WriteLine("Please enter the path to the JSON file:");
Console.WriteLine("请输入JSON文件的路径：");
string? jsonPath = Console.ReadLine();
if (string.IsNullOrEmpty(jsonPath))
{
    Console.WriteLine("Path cannot be empty.");
    Console.WriteLine("路径不能为空。");
    return;
}
//请手动输入谱面时长（单位：拍）
Console.WriteLine("Please enter the duration of the chart in beats:");
Console.WriteLine("请输入谱面的时长（单位：拍）：");
if (!float.TryParse(Console.ReadLine(), out float chartDuration))
{
    Console.WriteLine("Invalid input.");
    Console.WriteLine("无效的输入。");
    return;
}
//给你一次确认的机会，如果你把拍数太长了，你的内存会不会炸我不好说
Console.WriteLine("Are you sure you want to continue? (Y/N)");
Console.WriteLine("你确定要继续吗？（Y/N）");
if (Console.ReadLine()?.ToLower() != "y")
{
    return;
}
// 读取，并反序列化为Chart对象
string json = File.ReadAllText(jsonPath);
Chart chart = JsonConvert.DeserializeObject<Chart>(json);

var eventLayers = new EventLayers();
//询问精度，并在询问时推荐默认值为8
Console.WriteLine("Please enter the precision (8 recommended)");
Console.WriteLine("请输入精度（推荐默认值为8）：");
if (!int.TryParse(Console.ReadLine(), out int precision))
{
    Console.WriteLine("Invalid input.");
    Console.WriteLine("无效的输入。");
    return;
}
if (precision <= 0)
{
    Console.WriteLine("Invalid input.");
    Console.WriteLine("无效的输入。");
    return;
}
// 每拍带有缓动的事件等分为n份
float segmentLength = 1f / precision;
for (int i = 0; i < chart.judgeLineList.Count; i++)
{
    if (chart.judgeLineList[i].father != -1)
    {
        
        var newEventLayer = new EventLayer();
        newEventLayer.index = i;
        //从0拍开始，硬算到最后一拍
        for (float beat = 0; beat < chartDuration; beat += segmentLength)
        {
            var newXEvent = new Event
            {
                start = (float)chart.judgeLineList.GetLineXYPos(i, beat).Item1,
                end = (float)chart.judgeLineList.GetLineXYPos(i, beat + segmentLength).Item1,
                startTime = BeatConverter.BeatToRPEBeat(beat).ToList(),
                endTime = BeatConverter.BeatToRPEBeat(beat + segmentLength).ToList(),
            };
            var newYEvent = new Event
            {
                start = (float)chart.judgeLineList.GetLineXYPos(i, beat).Item2,
                end = (float)chart.judgeLineList.GetLineXYPos(i, beat + segmentLength).Item2,
                startTime = BeatConverter.BeatToRPEBeat(beat).ToList(),
                endTime = BeatConverter.BeatToRPEBeat(beat + segmentLength).ToList(),
            };
            
            
            // 覆写层级
            newEventLayer.moveXEvents.Add(newXEvent);
            newEventLayer.moveYEvents.Add(newYEvent);
            
            //分别检查添加的两个事件是否头尾数值相同，如果相同，检查上一个事件的结束值是否与这个事件的开始值相同，如果相同，删除这个事件
            if (newEventLayer.moveXEvents.Last().start == newEventLayer.moveXEvents.Last().end)
            {
                if (newEventLayer.moveXEvents.Last().start == newEventLayer.moveXEvents[^1].end)
                {
                    //删除这个事件
                    newEventLayer.moveXEvents.RemoveAt(newEventLayer.moveXEvents.Count - 1);
                }
            }
            //同上
            if (newEventLayer.moveYEvents.Last().start == newEventLayer.moveYEvents.Last().end)
            {
                if (newEventLayer.moveYEvents.Last().start == newEventLayer.moveYEvents[^1].end)
                {
                    newEventLayer.moveYEvents.RemoveAt(newEventLayer.moveYEvents.Count - 1);
                }
            }
        }
        //检查层级两个事件的第0拍是否有事件，如果没有，添加一个segmentLength长度的事件
        if (newEventLayer.moveXEvents.Count == 0 || newEventLayer.moveXEvents.First().GetStartBeat() != 0)
        {
            //在列表的第一个位置插入一个事件
            newEventLayer.moveXEvents.Insert(0, new Event
            {
                start = (float)chart.judgeLineList.GetLineXYPos(i, 0).Item1,
                end = (float)chart.judgeLineList.GetLineXYPos(i, 0 + segmentLength).Item1,
                startTime = new List<int> { 0, 0, 1 },
                endTime = BeatConverter.BeatToRPEBeat(segmentLength).ToList(),
            });
        }
        if (newEventLayer.moveYEvents.Count == 0 || newEventLayer.moveYEvents.First().GetStartBeat() != 0)
        {
            newEventLayer.moveYEvents.Insert(0, new Event
            {
                start = (float)chart.judgeLineList.GetLineXYPos(i, 0).Item2,
                end = (float)chart.judgeLineList.GetLineXYPos(i, 0 + segmentLength).Item2,
                startTime = new List<int> { 0, 0, 1 },
                endTime = BeatConverter.BeatToRPEBeat(segmentLength).ToList(),
            });
        }
        eventLayers.Add(newEventLayer);
    }
}

//使用dynamic再次读取原本的Json文件，并逐个替换事件层级
dynamic chartDynamic = JsonConvert.DeserializeObject<dynamic>(json);
for (int i = 0; i < eventLayers.Count; i++)
{
    // 通过index找到对应所属线
    int index = eventLayers[i].index ?? -1;
    // 来吧伙计。
    for (int j = 0; j < chartDynamic["judgeLineList"][index]["eventLayers"].Count; j++)
    {
        if (j == 0)
        {
            chartDynamic["judgeLineList"][index]["eventLayers"][j]["moveXEvents"] = 
                JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(eventLayers[i].moveXEvents));
            chartDynamic["judgeLineList"][index]["eventLayers"][j]["moveYEvents"] = 
                JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(eventLayers[i].moveYEvents));
            continue;
        }

        chartDynamic["judgeLineList"][index]["eventLayers"][j]["moveXEvents"] = null;
        chartDynamic["judgeLineList"][index]["eventLayers"][j]["moveYEvents"] = null;
    }
    
    chartDynamic["judgeLineList"][index]["father"] = -1;
}
//生成chart.json文件，输出被修改的Json文件
File.WriteAllText("chart.json", JsonConvert.SerializeObject(chartDynamic));
File.WriteAllText("rawchart.json", JsonConvert.SerializeObject(eventLayers, Formatting.Indented));



/*
static List<Event> SplitEvent(Event theEvent, float segmentLength = 0.125f)
{
    float startBeat = theEvent.startTime[0] + (float)theEvent.startTime[1] / theEvent.startTime[2];
    float endBeat = theEvent.endTime[0] + (float)theEvent.endTime[1] / theEvent.endTime[2];

    float totalLength = endBeat - startBeat;

    List<Event> segments = new();

    int numberOfSegments = (int)(totalLength / segmentLength);
    double remainder = totalLength % segmentLength;

    double totalTime = endBeat - startBeat;
    double timePerSegment = totalTime / totalLength * segmentLength;
    double remainderTime = totalTime / totalLength * remainder;

    double currentTime = startBeat;
    for (int i = 0; i < numberOfSegments; i++)
    {
        double segmentEndTime = currentTime + timePerSegment;
        //segments.Add(((float)currentTime, (float)segmentEndTime));
        float start = theEvent.GetValueAtBeat((float)currentTime);
        float end = theEvent.GetValueAtBeat((float)segmentEndTime);
        segments.Add(new Event
        {
            bezier = 0,
            bezierPoints = new List<double> { 0.0, 0.0, 0.0, 0.0 },
            easingLeft = 0.0f,
            easingRight = 1.0f,
            easingType = 1,
            end = end,
            endTime = BeatConverter.BeatToRPEBeat((float)segmentEndTime).ToList(),
            linkgroup = 0,
            start = start,
            startTime = BeatConverter.BeatToRPEBeat((float)currentTime).ToList()
        });
        currentTime = segmentEndTime;
    }

    if (remainder > 0)
    {
        double segmentEndTime = currentTime + remainderTime;
        float start = theEvent.GetValueAtBeat((float)currentTime);
        float end = theEvent.GetValueAtBeat((float)segmentEndTime);
        segments.Add(new Event
        {
            bezier = 0,
            bezierPoints = new List<double> { 0.0, 0.0, 0.0, 0.0 },
            easingLeft = 0.0f,
            easingRight = 1.0f,
            easingType = 1,
            end = end,
            endTime = BeatConverter.BeatToRPEBeat((float)segmentEndTime).ToList(),
            linkgroup = 0,
            start = start,
            startTime = BeatConverter.BeatToRPEBeat((float)currentTime).ToList()
        });
    }

    return segments;
}
*/