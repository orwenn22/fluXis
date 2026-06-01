using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Map.Structures;
using fluXis.Map.Structures.Bases;
using fluXis.Screens.Edit;

namespace fluXis.Utils;

// these are mostly unused for now, but keeping them here just in case i end up doing something with them
// they are mostly untested for now
public class SvUtils
{
    // caller is responsible for making sure that the SVs list is sorted
    public static double AverageVelocityInRange(List<ScrollVelocity> svs, double rangeStart, double rangeEnd)
    {
        double weightedSum = 0;
        double totalWeight = 0;

        for (int i = 0; i < svs.Count; i++)
        {
            double stepStart = svs[i].Time;
            double stepEnd = i + 1 < svs.Count ? svs[i + 1].Time : rangeEnd;

            double clampedStart = Math.Max(stepStart, rangeStart);
            double clampedEnd = Math.Min(stepEnd, rangeEnd);

            if (clampedEnd > clampedStart)
            {
                double weight = clampedEnd - clampedStart;
                weightedSum += svs[i].Multiplier * weight;
                totalWeight += weight;
            }
        }

        return totalWeight > 0 ? weightedSum / totalWeight : 1.0;
    }

    // caller is responsible for making sure that the SVs list is sorted
    public static List<ITimedObject> GenerateCorrection(EditorMap map, List<ScrollVelocity> svs, double startTime, double endTime, double targetVelocity, string group)
    {
        var corrective = new List<ITimedObject>();

        var notes = map.MapInfo.HitObjects
                       .Where(h => h.Time > startTime && h.Time < endTime && string.Equals(group, h.Group))
                       .OrderBy(h => h.Time)
                       .ToList();

        foreach (var note in notes)
        {
            double noteTime = note.Time;

            // ok so, basically, for every note, we want to add an additive AV spike to make it so the average velocity
            // between the start of the effect and the note is the target velocity
            double windowMs = noteTime - startTime; // total ms we care about
            const double spike_ms = 0.05; // width of the corrective spike (one side)

            // Average of base effect over [StartTime, noteTime - spikeMs]
            double avgBefore = AverageVelocityInRange(svs, startTime, noteTime - spike_ms);

            // Solve: targetVelocity * windowMs = avgBefore * (windowMs - spikeMs) + correctiveValue * spikeMs
            double correctiveValue = (targetVelocity * windowMs - avgBefore * (windowMs - spike_ms)) / spike_ms;

            corrective.Add(new AdditiveVelocity
            {
                Time = noteTime - spike_ms,
                EffectName = "_corr",
                Groups = new List<string> { group },
                VelocityOffset = correctiveValue,
            });
            corrective.Add(new AdditiveVelocity
            {
                Time = noteTime,
                EffectName = "_corr",
                Groups = new List<string> { group },
                VelocityOffset = -correctiveValue, // cancel the spike
            });
            corrective.Add(new AdditiveVelocity
            {
                Time = noteTime + spike_ms,
                EffectName = "_corr",
                Groups = new List<string> { group },
                VelocityOffset = 0, // end of correction
            });
        }

        return corrective;
    }

    public static List<ITimedObject> GenerateCorrection(EditorMap map, double startTime, double endTime, double targetVelocity, string group)
    {
        var svs = map.MapInfo.ScrollVelocities
                     .Where(sv => sv.Time > startTime && sv.Time < endTime && sv.Groups.Contains(group))
                     .OrderBy(sv => sv.Time)
                     .ToList();

        return GenerateCorrection(map, svs, startTime, endTime, targetVelocity, group);
    }

    // these two below are highly unoptimized, but they should work
    public static List<ITimedObject> GenerateCorrection(EditorMap map, double startTime, double endTime, double targetVelocity, List<string> groups)
    {
        List<ITimedObject> result = new List<ITimedObject>();

        foreach (var group in groups)
            result.AddRange(GenerateCorrection(map, startTime, endTime, targetVelocity, group));

        return result;
    }

    // caller is responsible for making sure that the SVs list is sorted
    public static List<ITimedObject> GenerateCorrection(EditorMap map, List<ScrollVelocity> svs, double startTime, double endTime, double targetVelocity, List<string> groups)
    {
        List<ITimedObject> result = new List<ITimedObject>();

        foreach (var group in groups)
            result.AddRange(GenerateCorrection(map, svs, startTime, endTime, targetVelocity, group));

        return result;
    }
}
