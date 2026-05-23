using fluXis.Screens.Edit.Tabs.Design;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Screens.Edit.Windows;

public partial class FunnyWindow : Window
{
    // private WindowContainer windowContainer;

    public FunnyWindow()
    {
        Title = "Funny";
    }

    [BackgroundDependencyLoader]
    private void Load()
    {
        Content = new Container()
        {
            Width = 1920f / 2f,
            Height = 1080f / 2f,
            Children = new Drawable[]
            {
                /*new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Theme.Yellow
                },
                windowContainer = new WindowContainer()
                {
                    RelativeSizeAxes = Axes.Both,
                }*/
                new DesignContainer()
            }
        };

        // windowContainer.Add(new TestWindow());
    }
}
