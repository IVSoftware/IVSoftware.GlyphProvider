using System.Diagnostics;
using System.Text;

namespace QuickStart.Maui.Demo
{
    internal static class StartupDiagnostics
    {
        static readonly object _lock = new();
        static int _initialized;

        public static void RegisterGlobalExceptionHooks()
        {
            if (Interlocked.Exchange(ref _initialized, 1) == 1)
            {
                return;
            }

            Log("RegisterGlobalExceptionHooks");

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    LogException("AppDomain.CurrentDomain.UnhandledException", ex);
                }
                else
                {
                    Log($"AppDomain.CurrentDomain.UnhandledException | Non-Exception payload: {e.ExceptionObject}");
                }
            };

            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                LogException("TaskScheduler.UnobservedTaskException", e.Exception);
            };
        }

        public static void Log(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss.fff} | {message}";
            Debug.WriteLine(line);

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(GetLogPath(), line + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // Best-effort only.
                }
            }
        }

        public static void LogException(string context, Exception ex)
        {
            Log($"{context} | {ex.GetType().Name}: {ex.Message}");
            Log(ex.StackTrace ?? "[no stack trace]");

            if (ex.InnerException is not null)
            {
                LogException(context + " | InnerException", ex.InnerException);
            }
        }

        static string GetLogPath()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "IVSoftware",
                "QuickStart.Maui.Demo");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "startup.log");
        }
    }
}
