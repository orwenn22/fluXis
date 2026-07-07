using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Map;

namespace fluXis.Utils;

public class SnapIndices
{
    private static int[] snaps { get; } = { 48, 24, 16, 12, 8, 6, 4, 3 };

    private Dictionary<int, int> snapIndices;
    private MapInfo map;

    public SnapIndices(MapInfo map)
    {
        snapIndices = new Dictionary<int, int>();
        this.map = map;
    }

    public int GetSnapIndex(double time, bool dynamicAdd = false)
    {
        if (snapIndices.TryGetValue((int)time, out int i))
            return i;

        var closest = snapIndices.Keys.MinBy(k => Math.Abs(k - time));

        // allow a 10ms margin of error for snapping
        // when dynamically adding, we might not want to snap to some existing snap point (which might happen often on high bpm),
        // so reduce that margin to 2ms in that case
        if (Math.Abs(closest - time) <= (dynamicAdd ? 2 : 10) && snapIndices.TryGetValue(closest, out i))
            return i;

        // nothing found
        if (dynamicAdd == false || map == null)
            return -1;

        // add it
        return AddSnapAtTime((int)time);
    }

    public void InitSnapIndices()
    {
        snapIndices.Clear();

        // shouldn't happen but just in case
        if (map.TimingPoints == null || map.TimingPoints.Count == 0) return;

        foreach (var hitObject in map.HitObjects)
        {
            var time = (int)hitObject.Time;
            var endTime = (int)hitObject.EndTime;

            AddSnapAtTime(time);
            AddSnapAtTime(endTime);
        }
    }

    public int AddSnapAtTime(int time)
    {
        int index = getIndex(time);
        if (!snapIndices.ContainsKey(time))
            snapIndices.Add(time, index);
        return index;
    }

    private int getIndex(int time)
    {
        var tp = map.GetTimingPoint(time);
        var diff = time - tp.Time;
        var idx = Math.Round(snaps[0] * diff / tp.MsPerBeat, MidpointRounding.AwayFromZero);

        for (var i = 0; i < snaps.Length; i++)
        {
            if (idx % snaps[i] == 0)
                return i;
        }

        return snaps.Length - 1;
    }
}
