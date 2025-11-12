namespace Lyt.Jigsaw.Utilities;

public sealed class Fullscreen(Window mainWindow)
{
    private readonly Window mainWindow = mainWindow;

    private Window? fullscreenWindow;
    private View? fullscreenView;
    private Panel? parentPanel;

    public bool IsFullscreen { get; private set; }

    public void GoFullscreen(Panel parentPanel, View view)
    {
        if (!parentPanel.Children.Remove(view))
        {
            throw new InvalidOperationException("Failed to remove view");
        }

        this.parentPanel = parentPanel;
        this.fullscreenView = view;
        this.fullscreenWindow = new Window()
        {
            Focusable = true,
            CanMaximize = true,
            Content = view,
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome,
            ShowActivated = true,
            ShowInTaskbar = false,
            SystemDecorations = SystemDecorations.None,
            Topmost = true,
            WindowState = WindowState.FullScreen,
        };

        this.mainWindow.ShowInTaskbar = false;
        this.mainWindow.Hide();

        this.fullscreenWindow.Show();
        this.fullscreenWindow.Focus();
        this.fullscreenWindow.ShowInTaskbar = true;
        this.IsFullscreen = true;
    }

    public void Return()
    {
        if (!this.IsFullscreen)
        {
            return;
        }

        if (this.fullscreenWindow is null || this.fullscreenView is null || this.parentPanel is null)
        {
            throw new InvalidOperationException("No fullscreen data");
        }

        this.fullscreenWindow.Content = null;
        this.fullscreenWindow.Close();
        this.fullscreenWindow = null;

        this.parentPanel.Children.Add(this.fullscreenView);
        this.mainWindow.ShowInTaskbar = true;
        this.mainWindow.Show();
        this.IsFullscreen = false;

        this.fullscreenView = null;
        this.parentPanel = null;
    }
}
