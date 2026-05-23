using fluXis.Graphics.UserInterface.Color;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;

namespace fluXis.Screens.Edit.Windows;

public partial class TestWindow : Window
{
    public TestWindow()
    {
        Title = "Test";
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Content = new Box
        {
            Width = 400,
            Height = 400,
            Colour = Theme.Green
        };
    }
}
