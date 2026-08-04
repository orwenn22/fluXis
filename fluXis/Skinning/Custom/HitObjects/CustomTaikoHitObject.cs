using fluXis.Graphics.UserInterface.Color;
using fluXis.Skinning.Bases;
using fluXis.Skinning.Json;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;

namespace fluXis.Skinning.Custom.HitObjects;

public partial class CustomTaikoHitObject : ColorableSkinDrawable
{
    private Drawable sprite { get; }

    public CustomTaikoHitObject(SkinJson skinJson, MapColor index, Texture texture, [CanBeNull] Texture tintless)
        : base(skinJson, index)
    {
        AutoSizeAxes = Axes.Y;

        InternalChild = sprite = new SkinnableSprite
        {
            RelativeSizeAxes = Axes.X,
            Texture = texture,
            Width = 1
        };

        if (tintless != null)
        {
            AddInternal(new SkinnableSprite
            {
                RelativeSizeAxes = Axes.X,
                Texture = tintless,
                Width = 1
            });
        }
    }

    public override void SetColor(Colour4 color)
    {
        sprite.Colour = color;
    }

    public void ApplySnapColor(int start, int end)
    {
        UseCustomColor = true;
        SetColor(SkinJson.SnapColors.GetColor(start));
    }
}
