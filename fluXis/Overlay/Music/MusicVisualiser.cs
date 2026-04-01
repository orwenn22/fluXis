using System;
using fluXis.Audio;
using fluXis.Audio.FFT;
using fluXis.Audio.FFT.Structures.Processor;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;

namespace fluXis.Overlay.Music;

public partial class MusicVisualiser : Container
{
    [Resolved]
    private GlobalClock clock { get; set; }

    private const int bar_count = 128;
    private const int trim_bars = 12; // trims the very high ends of the bars where it is always near zero
    private const int active_bars = bar_count - trim_bars;
    private const float bar_width = 1f / active_bars;

    private readonly FFTProcessor processor;
    private readonly float[] visualHeights;
    private readonly float[] downwardVelocities;

    private const float noise_floor = 0.01f;
    private const float gravity = 7.5f;
    private const float popup_speed = 35f;

    public bool Visible;

    public MusicVisualiser()
    {
        visualHeights = new float[bar_count];
        downwardVelocities = new float[bar_count];

        processor = new FFTProcessor(FFTParameters.Reactive with
        {
            SpatialWindowSize = 1,
            Gamma = 1.1f,
            ReleaseHigh = 0.2f,
        });
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        Blending = BlendingParameters.Additive;
        Height = 400;
        Alpha = 0.1f;
        Anchor = Anchor.BottomLeft;
        Origin = Anchor.BottomLeft;

        for (int i = 0; i < bar_count; i++)
        {
            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Both,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                X = i * bar_width,
                Width = bar_width
            });
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!Visible) return;

        var rawAmplitudes = clock.Amplitudes;
        if (rawAmplitudes == null || rawAmplitudes.Length == 0) return;

        var amplitudes = processor.Process(rawAmplitudes);

        float delta = (float)Time.Elapsed / 1000f;
        int binsPerBar = amplitudes.Length / bar_count;

        for (var i = 0; i < active_bars; i++)
        {
            float sum = 0;
            float max = 0;
            int startBin = i * binsPerBar;

            for (int j = 0; j < binsPerBar && (startBin + j) < amplitudes.Length; j++)
            {
                float val = amplitudes[startBin + j];
                sum += val;
                if (val > max) max = val;
            }

            float targetHeight = (sum / Math.Max(1, binsPerBar)) * 0.5f + max * 0.5f;

            if (targetHeight < noise_floor) targetHeight = 0;

            if (targetHeight > visualHeights[i])
            {
                visualHeights[i] = (float)Interpolation.Lerp(visualHeights[i], targetHeight, Math.Clamp(delta * popup_speed, 0, 1));
                downwardVelocities[i] = 0;
            }
            else
            {
                downwardVelocities[i] += gravity * delta;
                visualHeights[i] -= downwardVelocities[i] * delta;

                if (visualHeights[i] < targetHeight)
                {
                    visualHeights[i] = targetHeight;
                    downwardVelocities[i] = 0;
                }
            }

            visualHeights[i] = Math.Max(0, visualHeights[i]);

            Children[i].Height = visualHeights[i];
        }
    }
}
