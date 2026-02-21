using System;
using Avalonia;

namespace Lyt.Jigsaw.Desktop;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any SynchronizationContext-reliant
    // code before AppMain is called: things aren't initialized yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) 
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<Lyt.Jigsaw.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .With(new SkiaOptions() { MaxGpuResourceSizeBytes = 2L * 1024L * 1024L * 1024L }) // 2 GB 
            .LogToTrace()
            .WithDeveloperTools();
}
