using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Map.Structures.Bases;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;

namespace fluXis.Screens.Edit.Tabs.Charting.Playfield.Tags;

public partial class EditorTagContainer : Container<EditorTag>
{
    [Resolved]
    protected EditorSettings Settings { get; private set; }

    [Resolved]
    protected EditorMap Map { get; private set; }

    [Resolved]
    protected EditorClock EditorClock { get; private set; }

    // protected List<EditorTag> Tags { get; } = new();
    protected List<EditorTag> PastTags { get; } = new();
    protected List<EditorTag> FutureTags { get; } = new();

    protected virtual bool RightSide => false;

    private List<EditorTag> sortedChildrenCache = new();

    private static readonly IComparer<EditorTag> timeComparer =
        Comparer<EditorTag>.Create((a, b) => a.TimedObject.Time.CompareTo(b.TimedObject.Time));

    public delegate bool HighlightFileter(EditorTag tag);

    private HighlightFileter highlightFilter;
    private bool useHighlightFilter = false; //TODO: this sucks, find a better way to achieve this

    public EditorTagContainer()
    {
        highlightFilter = _ => true;
        useHighlightFilter = false;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AutoSizeAxes = Axes.X;
        RelativeSizeAxes = Axes.Y;
        Anchor = RightSide ? Anchor.CentreRight : Anchor.CentreLeft;
        Origin = RightSide ? Anchor.CentreLeft : Anchor.CentreRight;
        X = RightSide ? 20 : -20;
    }

    protected void AddTag(EditorTag tag)
    {
        tag.RightSide = RightSide;

        if (tag.TimedObject.Time < EditorClock.CurrentTime - 1000)
        {
            int index = PastTags.BinarySearch(tag, timeComparer);
            if (index < 0) index = ~index;

            PastTags.Insert(index, tag);
        }
        else
        {
            int index = FutureTags.BinarySearch(tag, timeComparer);
            if (index < 0) index = ~index;

            FutureTags.Insert(index, tag);
        }

        // TODO: the tag might be visible when inserted
        //       currently, adding a tag will cause the list to be out of order

        setTagAlpha(tag);
    }

    protected void RemoveTag(ITimedObject obj)
    {
        var tag = PastTags.FirstOrDefault(t => t.TimedObject == obj);
        tag ??= FutureTags.FirstOrDefault(t => t.TimedObject == obj);
        tag ??= Children.FirstOrDefault(t => t.TimedObject == obj);

        if (tag == null) return;

        // TODO: only remove from correct one?
        FutureTags.Remove(tag);
        PastTags.Remove(tag);
        sortedChildrenCache.Remove(tag);
        Remove(tag, true);
    }

    protected void ClearTags<T>()
    {
        var tags = Children.Where(t => t.TimedObject is T).ToList();
        foreach (var tag in tags)
            Remove(tag, true);

        tags = PastTags.Where(t => t.TimedObject is T).ToList();
        foreach (var tag in tags)
            PastTags.Remove(tag);

        tags = FutureTags.Where(t => t.TimedObject is T).ToList();
        foreach (var tag in tags)
            FutureTags.Remove(tag);
    }

    protected void UpdateTag(ITimedObject obj)
    {
        var tag = PastTags.FirstOrDefault(t => t.TimedObject == obj);
        tag ??= FutureTags.FirstOrDefault(t => t.TimedObject == obj);
        tag ??= Children.FirstOrDefault(t => t.TimedObject == obj);

        if (tag == null)
            return; // throw?

        // TODO: only remove from correct one?
        FutureTags.Remove(tag);
        PastTags.Remove(tag);
        Remove(tag, false);
        AddTag(tag);
    }

    protected override void Update()
    {
        base.Update();

        var tagsToHide = Children.Where(t => t.Y < -20 || t.Y > DrawHeight + 20).ToList();

        foreach (var tag in tagsToHide)
        {
            if (tag.TimedObject.Time < EditorClock.CurrentTime - 1000)
            {
                int index = PastTags.BinarySearch(tag, timeComparer);
                if (index < 0) index = ~index;

                PastTags.Insert(index, tag);
            }
            else
            {
                int index = FutureTags.BinarySearch(tag, timeComparer);
                if (index < 0) index = ~index;

                FutureTags.Insert(index, tag);
            }

            Remove(tag, false);
            sortedChildrenCache.Remove(tag);
        }

        // handling past tags
        while (PastTags.Count > 0 && PastTags[^1].TimedObject.Time > EditorClock.CurrentTime - 1000)
        {
            var tag = PastTags[^1];
            PastTags.RemoveAt(PastTags.Count - 1);

            if (tag.TimedObject.Time > EditorClock.CurrentTime + 1000)
            {
                FutureTags.Insert(0, tag);
            }
            else
            {
                Add(tag);
                sortedChildrenCache.Insert(0, tag);
            }
        }

        // handling future tags
        while (FutureTags.Count > 0 && FutureTags[0].TimedObject.Time < EditorClock.CurrentTime + 1000)
        {
            var tag = FutureTags[0];
            FutureTags.RemoveAt(0);

            if (tag.TimedObject.Time < EditorClock.CurrentTime - 1000)
            {
                PastTags.Add(tag);
            }
            else
            {
                Add(tag);
                sortedChildrenCache.Add(tag);
            }
        }

        var tagsAtTime = new Dictionary<int, int>();
        var timeOffsets = new Dictionary<int, float>();

        double timeThreshold = 10 / (Settings.ZoomBindable.Value - 0.99);
        timeThreshold = Math.Clamp(timeThreshold, 5, 25);

        foreach (var tag in sortedChildrenCache)
        {
            var time = (int)tag.TimedObject.Time;
            int closestTime = -1;

            foreach (var existingTime in tagsAtTime.Keys)
            {
                if (Precision.AlmostEquals(time, existingTime, timeThreshold))
                {
                    closestTime = existingTime;
                    break;
                }
            }

            if (closestTime != -1)
            {
                tag.X = timeOffsets[closestTime] * (RightSide ? 1 : -1);
                timeOffsets[closestTime] += tag.DrawWidth + 10;
                tagsAtTime[closestTime]++;
            }
            else
            {
                tag.X = 0;
                timeOffsets[time] = tag.DrawWidth + 10;
                tagsAtTime[time] = 1;
            }
        }
    }

    public void SetHighlightFilter(HighlightFileter highlightFilter)
    {
        this.highlightFilter = highlightFilter ?? (_ => true);
        useHighlightFilter = true;

        foreach (var tag in Children)
            setTagAlpha(tag);

        foreach (var tag in PastTags)
            setTagAlpha(tag);

        foreach (var tag in FutureTags)
            setTagAlpha(tag);

        if (highlightFilter == null)
            useHighlightFilter = false;
    }

    private void setTagAlpha(EditorTag tag)
    {
        if (useHighlightFilter)
            tag.Alpha = highlightFilter.Invoke(tag) ? 1.0f : 0.4f;
    }
}
