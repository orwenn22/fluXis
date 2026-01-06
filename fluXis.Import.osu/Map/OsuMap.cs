using System.Collections.Generic;
using System.Linq;
using fluXis.Import.osu.Map.Components;
using fluXis.Import.osu.AutoImport;
using fluXis.Map;
using fluXis.Map.Structures;
using JetBrains.Annotations;

namespace fluXis.Import.osu.Map;

public class OsuMap
{
    // [General]
    public string AudioFilename { get; set; }
    public int PreviewTime { get; set; }
    public int Mode { get; set; }

    // [Editor]
    // uuhhhh

    // [Metadata]
    public string Title { get; set; }
    public string TitleRoman { get; set; }
    public string Artist { get; set; }
    public string ArtistRoman { get; set; }
    public string Creator { get; set; }
    public string Version { get; set; }
    public string Source { get; set; }
    public string Tags { get; set; }

    // [Difficulty]
    public float HealthDrainRate { get; set; }
    public float CircleSize { get; set; }
    public float OverallDifficulty { get; set; }

    // [Events]
    public List<OsuEvent> Events { get; init; } = new();

    // [TimingPoints]
    public List<OsuTimingPoint> TimingPoints { get; init; } = new();

    // [Colours]
    // dont need this

    // [HitObjects]
    public List<OsuHitObject> HitObjects { get; init; } = new();

    public List<string> MapFiles { get; set; } = new();

    public string GetBackground()
    {
        foreach (var osuEvent in Events)
        {
            string param = osuEvent.Parameter.Trim().Replace("\"", "");

            switch (osuEvent.EventType)
            {
                case "0":
                    return param;
            }
        }

        return "";
    }

    [CanBeNull]
    public OsuMapInfo ToMapInfo()
    {
        if (Mode != 3)
            return null;

        var mapInfo = new OsuMapInfo
        {
            Metadata = new MapMetadata
            {
                Title = Title?.Trim() ?? "",
                TitleRomanized = TitleRoman?.Trim() ?? Title?.Trim() ?? "",
                Artist = Artist?.Trim() ?? "",
                ArtistRomanized = ArtistRoman?.Trim() ?? Artist?.Trim() ?? "",
                Mapper = Creator?.Trim() ?? "",
                Difficulty = Version?.Trim() ?? "",
                AudioSource = Source?.Trim() ?? "",
                Tags = Tags?.Trim() ?? "",
                PreviewTime = PreviewTime
            },
            AudioFile = AudioFilename?.Trim(),
            BackgroundFile = "",
            HitObjects = new List<HitObject>(),
            TimingPoints = new List<TimingPoint>(),
            ScrollVelocities = new List<ScrollVelocity>(),
            InitialKeyCount = (int)CircleSize,
            AccuracyDifficulty = OverallDifficulty,
            HealthDifficulty = HealthDrainRate
        };

        float dominantBpm = getDominantBPM();
        float currentBpm = dominantBpm;

        for (int i = 0; i < TimingPoints.Count; ++i)
        {
            OsuTimingPoint currentPoint = TimingPoints[i];

            //check if we have both a bpm point and effect point at the same time
            if (i != TimingPoints.Count - 1 && TimingPoints[i + 1].IsScrollVelocity != currentPoint.IsScrollVelocity && TimingPoints[i + 1].Time == currentPoint.Time)
            {
                OsuTimingPoint secondaryPoint = TimingPoints[i + 1];

                //identify which point is sv and which point is bpm
                OsuTimingPoint svPoint;
                OsuTimingPoint bpmPoint;

                if (currentPoint.IsScrollVelocity)
                {
                    svPoint = currentPoint;
                    bpmPoint = secondaryPoint;
                }
                else
                {
                    svPoint = secondaryPoint;
                    bpmPoint = currentPoint;
                }

                mapInfo.TimingPoints.Add(bpmPoint.ToTimingPointInfo());
                currentBpm = bpmPoint.BPM;
                mapInfo.ScrollVelocities.Add(svPoint.ToScrollVelocityInfo(dominantBpm, currentBpm));

                i++; //skip next point because we already handled it
            }
            //only one point at the current time
            else
            {
                if (currentPoint.IsScrollVelocity)
                {
                    mapInfo.ScrollVelocities.Add(currentPoint.ToScrollVelocityInfo(dominantBpm, currentBpm));
                }
                else
                {
                    mapInfo.TimingPoints.Add(currentPoint.ToTimingPointInfo());
                    currentBpm = currentPoint.BPM;

                    //in osu mania, a bpm timing point alone (i.e. no simultaneous sv/effect point) will change the scroll speed
                    //(this matches osu!stable, osu!lazer behaves a bit differently, but most maps expect stable's behavior)
                    double multiplier = currentBpm / dominantBpm;

                    //some gimmick maps might add A LOT of timing points really close to each others to add lots of barlines.
                    //in cases these timing points have the same bpm, we don't want to add the same scroll velocities lots of times
                    //(right now this is commented because this makes the barlines look different on some maps and i don't know why)
                    //if (mapInfo.ScrollVelocities.Count >= 1 && mapInfo.ScrollVelocities.Last().Multiplier == multiplier) continue;

                    mapInfo.ScrollVelocities.Add(new ScrollVelocity
                    {
                        Time = currentPoint.Time,
                        Multiplier = multiplier
                    });
                }
            }
        }

        mapInfo.HitObjects.AddRange(HitObjects.Select(h => h.ToHitObjectInfo(this)));

        foreach (var osuEvent in Events)
        {
            string param = osuEvent.Parameter.Trim().Replace("\"", "");

            switch (osuEvent.EventType)
            {
                case "0":
                    mapInfo.BackgroundFile = param;
                    break;

                case "Video":
                    mapInfo.VideoFile = param;
                    break;
            }
        }

        return mapInfo;
    }

    private float getDuration()
    {
        float duration = 0;

        foreach (var osuHitObject in HitObjects)
        {
            float objectEnd = float.IsNaN(osuHitObject.StartTime) ? osuHitObject.EndTime : //p note (this should fall in the "long note" category, but do it just in case)
                osuHitObject.Type == OsuHitObjectType.Hold ? osuHitObject.EndTime : //long note
                osuHitObject.StartTime; //simple note

            if (objectEnd > duration) duration = objectEnd;
        }

        return duration;
    }

    private float getDominantBPM()
    {
        float mapDuration = getDuration();

        Dictionary<float, float> bpmDurations = new();
        var bpmPoints = TimingPoints.FindAll(p => !p.IsScrollVelocity);

        for (int i = 0; i < bpmPoints.Count; ++i)
        {
            OsuTimingPoint bpmPoint = bpmPoints[i];
            float bpm = bpmPoint.BPM;
            float duration = (i == bpmPoints.Count - 1 || bpmPoints[i + 1].Time > mapDuration) ? mapDuration - bpmPoint.Time : bpmPoints[i + 1].Time - bpmPoint.Time;

            if (!bpmDurations.ContainsKey(bpm)) bpmDurations[bpm] = duration;
            else bpmDurations[bpm] += duration;
        }

        float dominantBPM = 1;
        float highestDuration = 0;

        foreach (var bpmDuration in bpmDurations)
        {
            if (bpmDuration.Value > highestDuration)
            {
                dominantBPM = bpmDuration.Key;
                highestDuration = bpmDuration.Value;
            }
        }

        return dominantBPM;
    }
}
