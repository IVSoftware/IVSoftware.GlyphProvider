using Microsoft.Extensions.DependencyInjection;

namespace QuickStart.Maui.Demo
{
    public partial class App : Application
    {
        public App()
        {
            StartupDiagnostics.Log("App ctor: begin");
            InitializeComponent();
            StartupDiagnostics.Log("App ctor: end");
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            StartupDiagnostics.Log("CreateWindow: begin");
            var window = new Window(new AppShell());

#if WINDOWS
            window.Created += async (_, _) =>
            {
                StartupDiagnostics.Log("Window.Created: begin");
                if (window.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow)
                {
                    nativeWindow.Activate();
                    StartupDiagnostics.Log("Window.Created: nativeWindow.Activate");

                    IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                    var winId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

                    var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(
                        winId,
                        Microsoft.UI.Windowing.DisplayAreaFallback.Primary
                    );

                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(winId);
                    appWindow.Resize(new(540, 960));
                    appWindow.Move(new(
                        displayArea.WorkArea.X + (displayArea.WorkArea.Width - 540) / 2,
                        displayArea.WorkArea.Y + (displayArea.WorkArea.Height - 960) / 2
                    ));
                    StartupDiagnostics.Log($"Window.Created: positioned hwnd={hWnd}");
                }
                StartupDiagnostics.Log("Window.Created: end");
            };
#endif

            StartupDiagnostics.Log("CreateWindow: end");
            return window;
        }
    }
}
