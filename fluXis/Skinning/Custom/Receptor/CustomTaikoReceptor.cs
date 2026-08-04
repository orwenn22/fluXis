using fluXis.Graphics.UserInterface.Color;
using fluXis.Skinning.Bases;
using fluXis.Skinning.Json;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;

namespace fluXis.Skinning.Custom.Receptor;

public partial class CustomTaikoReceptor : ColorableSkinDrawable
{
    private Drawable sprite { get; }

    public CustomTaikoReceptor(SkinJson skinJson, Texture texture, [CanBeNull] Texture tintless, MapColor index = MapColor.Accent)
        : base(skinJson, index)
    {
        RelativeSizeAxes = Axes.Both;

        InternalChild = sprite = new SkinnableSprite
        {
            RelativeSizeAxes = Axes.X,
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            Texture = texture,
            Width = 1
        };

        if (tintless != null)
        {
            AddInternal(new SkinnableSprite
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Texture = tintless,
                Width = 1
            });
        }
    }

    public override void SetColor(Colour4 color)
    {
        sprite.Colour = color;
    }
}
