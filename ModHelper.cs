using System.Diagnostics;

namespace Needleforge
{
    internal static class ModHelper
    {
        public static void Log(string msg)
        {
            NeedleforgePlugin.logger.LogInfo(msg);
        }

        public static void LogError(string msg) => LogError(msg, false);

        public static void LogError(string msg, bool stackTrace)
        {
            if (stackTrace)
                msg = $"{msg}\n{new StackTrace(1, true)}";
            NeedleforgePlugin.logger.LogError(msg);
        }

        public static void LogWarning(string msg) => LogWarning(msg, false);

        public static void LogWarning(string msg, bool stackTrace)
        {
            if (stackTrace)
                msg = $"{msg}\n{new StackTrace(1, true)}";
            NeedleforgePlugin.logger.LogWarning(msg);
        }
    }
}
