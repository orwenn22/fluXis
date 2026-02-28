using System;
using System.Linq;
using fluXis.Graphics.Sprites.Icons;
using fluXis.Graphics.Sprites.Text;
using fluXis.Graphics.UserInterface.Buttons;
using fluXis.Graphics.UserInterface.Panel;
using fluXis.Graphics.UserInterface.Text;
using fluXis.Map.Structures.Bases;
using fluXis.Overlay.Auth.UI;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace fluXis.Screens.Edit.UI.Variable;

public partial class EditorGroupInfo : EditorVariableBase
{
    [Resolved]
    private PanelContainer panels { get; set; }

    private EditorMap map { get; }
    private IHasGroups groups { get; }

    public EditorGroupInfo(EditorMap map, IHasGroups groups)
    {
        this.map = map;
        this.groups = groups;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        updateChild();
    }

    private void updateChild()
    {
        InternalChild = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(6, 0),

            ChildrenEnumerable = new Drawable[]
            {
                new FluXisSpriteText
                {
                    Text = "Groups",
                    WebFontSize = 16
                }
            }.Concat(groups.Groups.Where(x => x != "" && x[0] != '$').Select(x => new GroupEntry(x)
            {
                OnRemove = removeGroup
            })).Concat(new Drawable[]
            {
                new FluXisButton
                {
                    Text = "Add group",
                    RelativeSizeAxes = Axes.X,
                    Height = 30,
                    Action = () =>
                    {
                        panels.Content = new AddGroupPanel { OnAddGroup = addGroup };
                    }
                }
            })
        };
    }

    private void addGroup(string newGroup)
    {
        if (newGroup.Length == 0 || groups.Groups.Contains(newGroup)) return;

        groups.Groups.Add(newGroup);

        if (groups is ITimedObject obj) map.Update(obj);

        updateChild();
    }

    private void removeGroup(string group)
    {
        if (groups.Groups.Contains(group)) groups.Groups.Remove(group);

        if (groups is ITimedObject obj) map.Update(obj);

        updateChild();
    }

    private partial class GroupEntry : Container
    {
        private string group { get; }

        public Action<string> OnRemove { get; set; }

        public GroupEntry(string group)
        {
            this.group = group;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Spacing = new Vector2(4, 0),
                    Child = new FluXisSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = group,
                        WebFontSize = 16
                    }
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Child = new IconButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Icon = FontAwesome6.Solid.Trash,
                        ButtonSize = 26,
                        IconSize = 16,
                        Action = () => OnRemove?.Invoke(group)
                    }
                }
            };
        }
    }

    private partial class AddGroupPanel : Panel
    {
        private FluXisTextBox textBox;

        public Action<string> OnAddGroup { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Width = 420;
            AutoSizeAxes = Axes.Y;
            Content.RelativeSizeAxes = Axes.X;
            Content.AutoSizeAxes = Axes.Y;

            Content.Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(20),
                Spacing = new Vector2(10),
                Children = new Drawable[]
                {
                    new FluXisSpriteText
                    {
                        Text = "Add group",
                        WebFontSize = 32,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre
                    },
                    textBox = new AuthOverlayTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        PlaceholderText = "Group name"
                    },
                    new AuthOverlayButton("Add") { Action = create },
                    new AuthOverlayButton("Cancel") { Action = Hide }
                }
            };
        }

        private void create()
        {
            if (textBox.Text.Length == 0) return;

            OnAddGroup?.Invoke(textBox.Text);
            Hide();
        }
    }
}
