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
// 每拍带有缓动的事件等分为8份线性缓动事件
for (int i = 0; i < chart.judgeLineList.Count; i++)
{
    if (chart.judgeLineList[i].father != -1)
    {
        var newEventLayer = new EventLayer();
        newEventLayer.index = i;
        //从0拍开始，硬算到最后一拍，每拍等分为8份，所以每拍的时间为0.125
        for (float beat = 0; beat < chartDuration; beat += 0.125f)
        {
            // 覆写层级，从头到尾使用持续0.125拍的线性缓动事件，开始时间为当前拍，结束时间为下一0.125拍，开始数值结束数值使用GetLineXYPos方法获取
            newEventLayer.moveXEvents.Add(new Event
            {
                start = chart.judgeLineList.GetLineXYPos(i, beat).Item1,
                end = chart.judgeLineList.GetLineXYPos(i, beat + 0.125f).Item1,
                startTime = BeatConverter.BeatToRPEBeat(beat).ToList(),
                endTime = BeatConverter.BeatToRPEBeat(beat + 0.125f).ToList(),
                bezier = 0,
                bezierPoints = new List<double> { 0.0, 0.0, 0.0, 0.0 },
                easingLeft = 0.0f,
                easingRight = 1.0f,
                easingType = 1,
                linkgroup = 0
            });
          
            newEventLayer.moveYEvents.Add(new Event
            {
                start = chart.judgeLineList.GetLineXYPos(i, beat).Item2,
                end = chart.judgeLineList.GetLineXYPos(i, beat + 0.125f).Item2,
                startTime = BeatConverter.BeatToRPEBeat(beat).ToList(),
                endTime = BeatConverter.BeatToRPEBeat(beat + 0.125f).ToList(),
                bezier = 0,
                bezierPoints = new List<double> { 0.0, 0.0, 0.0, 0.0 },
                easingLeft = 0.0f,
                easingRight = 1.0f,
                easingType = 1,
                linkgroup = 0
            });
        }
        /*
        // 遍历所有事件层级，再遍历每个事件层级的事件，使用SplitEvent方法将事件等分，添加到新的事件层级中
        foreach (var eventLayer in chart.judgeLineList[i].eventLayers)
        {
            var newEvents = new Events();
            foreach (var rpeEvent in eventLayer.moveXEvents)
            {
                var splitEvents = SplitEvent(rpeEvent);
                foreach (var splitEvent in splitEvents)
                {
                    splitEvent.start = chart.judgeLineList.GetLineXYPos(i,
                        splitEvent.startTime[0] + (float)splitEvent.startTime[1] / splitEvent.startTime[2]).Item1;
                    splitEvent.end = chart.judgeLineList.GetLineXYPos(i,
                        splitEvent.endTime[0] + (float)splitEvent.endTime[1] / splitEvent.endTime[2]).Item1;
                    newEvents.Add(splitEvent);
                }
            }

            foreach (var newEvent in newEvents)
            {
                newEventLayer.moveXEvents.Add(newEvent);
            }

            newEvents.Clear();
            foreach (var rpeEvent in eventLayer.moveYEvents)
            {
                var splitEvents = SplitEvent(rpeEvent);
                foreach (var splitEvent in splitEvents)
                {
                    splitEvent.start = chart.judgeLineList.GetLineXYPos(i,
                        splitEvent.startTime[0] + (float)splitEvent.startTime[1] / splitEvent.startTime[2]).Item1;
                    splitEvent.end = chart.judgeLineList.GetLineXYPos(i,
                        splitEvent.endTime[0] + (float)splitEvent.endTime[1] / splitEvent.endTime[2]).Item1;
                    newEvents.Add(splitEvent);
                }
            }

            foreach (var newEvent in newEvents)
            {
                newEventLayer.moveYEvents.Add(newEvent);
            }
        }
        */
        eventLayers.Add(newEventLayer);
        /*
        var moveXEvents = chart.judgeLineList[i].eventLayers[1].moveXEvents;
        var moveYEvents = chart.judgeLineList[i].eventLayers[1].moveYEvents;
        var newXEvents = new Events();
        var newYEvents = new Events();
        //遍历所有事件
        for (int j = 0; j < moveXEvents.Count - 1; j++)
        {
            var newEvents = SplitEvent(moveXEvents[i]);
            for (int k = 0; k < newEvents.Count - 1; k++)
            {
                newXEvents.Add(newEvents[j]);
            }
        }

        for (int j = 0; j < moveYEvents.Count - 1; j++)
        {
            var newEvents = SplitEvent(moveXEvents[i]);
            for (int k = 0; k < newEvents.Count - 1; k++)
            {
                newYEvents.Add(newEvents[j]);
            }
        }

        // 新的XY事件均遍历一次对方的事件列表
        for (int j = 0; j < newXEvents.Count; j++)
        {
            bool found = false;
            for (int k = 0; k < newYEvents.Count; k++)
            {
                if (newXEvents[j].startTime.SequenceEqual(newYEvents[k].startTime) &&
                    newXEvents[j].endTime.SequenceEqual(newYEvents[k].endTime))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var previousEvent = newYEvents.LastOrDefault(e => e.endTime.SequenceEqual(newXEvents[j].startTime));
                if (previousEvent != null)
                {
                    newYEvents.Add(new Event
                    {
                        start = previousEvent.end,
                        end = previousEvent.end,
                        startTime = new List<int>(newXEvents[j].startTime),
                        endTime = new List<int>(newXEvents[j].endTime),
                        bezier = 0,
                        bezierPoints = new List<double> { 0.0, 0.0, 0.0, 0.0 },
                        easingLeft = 0.0f,
                        easingRight = 1.0f,
                        easingType = 1,
                        linkgroup = 0
                    });
                }
            }
        }

        for (int j = 0; j < newYEvents.Count; j++)
        {
            bool found = false;
            for (int k = 0; k < newXEvents.Count; k++)
            {
                if (newYEvents[j].startTime.SequenceEqual(newXEvents[k].startTime) &&
                    newYEvents[j].endTime.SequenceEqual(newXEvents[k].endTime))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var previousEvent = newXEvents.LastOrDefault(e => e.endTime.SequenceEqual(newYEvents[j].startTime));
                if (previousEvent != null)
                {
                    newXEvents.Add(new Event
                    {
                        start = previousEvent.end,
                        end = previousEvent.end,
                        startTime = new List<int>(newYEvents[j].startTime),
                        endTime = new List<int>(newYEvents[j].endTime),
                        bezier = 0,
                        bezierPoints = new List<double> { 0.0, 0.0, 0.0, 0.0 },
                        easingLeft = 0.0f,
                        easingRight = 1.0f,
                        easingType = 1,
                        linkgroup = 0
                    });
                }
            }
        }
        */
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
File.WriteAllText("chart.json", JsonConvert.SerializeObject(chartDynamic, Formatting.Indented));
File.WriteAllText("rawchart.json", JsonConvert.SerializeObject(eventLayers, Formatting.Indented));




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