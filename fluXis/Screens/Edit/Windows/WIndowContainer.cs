using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;

namespace fluXis.Screens.Edit.Windows;

public partial class WindowContainer : Container<Window>
{
    private InputManager inputManager;

    private Window draggedWindow;
    private Vector2 dragOffset;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
    }

    protected override void LoadComplete()
    {
        inputManager = GetContainingInputManager();
        base.LoadComplete();
    }

    protected override void Update()
    {
        if (!inputManager.CurrentState.Mouse.IsPressed(MouseButton.Left)) draggedWindow = null;

        foreach (var childWindow in Children.ToList())
        {
            if (childWindow != draggedWindow)
            {
                if (childWindow.X + childWindow.DrawWidth - 60 < 0) childWindow.MoveToX(-childWindow.DrawWidth + 65, 300, Easing.Out);
                if (childWindow.Y < 0) childWindow.MoveToY(5, 300, Easing.Out);
                if (childWindow.X > DrawWidth - 60) childWindow.MoveToX(DrawWidth - 65, 300, Easing.Out);
                if (childWindow.Y > DrawHeight - 30) childWindow.MoveToY(DrawHeight - 35, 300, Easing.Out);
            }
        }

        base.Update();
    }

    public void BringToFront(Window window)
    {
        foreach (var childWindow in Children.ToList())
        {
            ChangeInternalChildDepth(childWindow, childWindow.Depth + 1);
        }

        ChangeInternalChildDepth(window, 0);
    }

    public void Close(Window window)
    {
        if (draggedWindow == window) draggedWindow = null;
        window.Origin = Anchor.Centre;
        window.X += window.DrawWidth / 2;
        window.Y += window.DrawHeight / 2;
        window.FadeTo(0, 300, Easing.Out).ScaleTo(0, 300, Easing.Out).OnComplete(window1 =>
        {
            Remove(window1, false); // true?
        });
    }

    public void SetDraggedWindow(Window window, Vector2 dragOffset)
    {
        draggedWindow = window;
        draggedWindow.ClearTransforms(); // necessary to be able to drag windows while they are being moved to remain on screen
        this.dragOffset = dragOffset;
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        draggedWindow = null;
        dragOffset = Vector2.Zero;
        base.OnMouseUp(e);
    }

    protected override bool OnMouseMove(MouseMoveEvent e)
    {
        if (draggedWindow == null) return false;

        draggedWindow.Position = e.MousePosition - dragOffset;
        return true;
    }
}
