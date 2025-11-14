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

        // Get the screen that the main window is currently on BEFORE we hide it.
        var screens = this.mainWindow.Screens;
        var currentScreen = screens.ScreenFromWindow(this.mainWindow);

        this.fullscreenWindow = new Window()
        {
            // Make sure the fullscreen window is focusable so that the content view can receive
            // keyboard input, most notably for the esc key used to return to normal.
            Focusable = true,
            CanMaximize = true,
            Content = view,
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome,
            ShowActivated = true,
            ShowInTaskbar = false,
            SystemDecorations = SystemDecorations.None,
            Topmost = true,
            // We need to set the position before going fullscreen to ensure it appears
            // on the correct screen.
            // LATER => WindowState = WindowState.FullScreen,
        };

        this.mainWindow.ShowInTaskbar = false;
        this.mainWindow.Hide();

        if (currentScreen is not null)
        {
            var screenBounds = currentScreen.WorkingArea;
            this.fullscreenWindow.Position = new PixelPoint(screenBounds.X, screenBounds.Y);
        }

        this.fullscreenWindow.WindowState = WindowState.FullScreen;
        this.fullscreenWindow.Show();
        this.fullscreenWindow.Focus();
        this.fullscreenWindow.ShowInTaskbar = true;
        this.IsFullscreen = true;
    }

    public void ReturnToWindowed()
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
