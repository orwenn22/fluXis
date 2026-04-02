using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using fluXis.Graphics.Sprites.Icons;
using fluXis.Graphics.UserInterface.Panel;
using fluXis.Graphics.UserInterface.Panel.Presets;
using osu.Framework.Allocation;

namespace fluXis.Tests.Panels;

public partial class TestFormPanel : FluXisTestScene
{
    [BackgroundDependencyLoader]
    private void load()
    {
        CreateClock();

        var panelContainer = new PanelContainer();
        Add(panelContainer);

        AddStep("Add Panel", () =>
        {
            var panel = new FormPanel<FormData>(FontAwesome6.Solid.Pencil, "Really, really long title that will hopefully cut off to show truncating", new FormData(), (_, _) => true);
            panelContainer.Content = panel;
        });

        AddStep("Remove Panel", () => panelContainer.Content = null);
    }

    private class FormData
    {
        [MaxLength(24)]
        [Description("String")]
        public string SimpleString { get; set; } = "value";

        [ReadOnly(true)]
        [Description("Read-Only")]
        public string ReadOnlyString { get; set; } = "can't be edited";

        [PasswordPropertyText(true)]
        [Description("Password")]
        public string PasswordString { get; set; } = "secure password";

        public int SimpleInt { get; set; }
        public float SimpleFloat { get; set; }
    }
}
