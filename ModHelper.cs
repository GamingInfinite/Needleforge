namespace Needleforge
{
    internal static class ModHelper
    {
        public static void Log(string msg)
        {
            NeedleforgePlugin.logger.LogInfo(msg);
        }

        public static void LogError(string msg)
        {
            NeedleforgePlugin.logger.LogError(msg);
        }

        public static void LogWarning(string msg)
        {
            NeedleforgePlugin.logger.LogWarning(msg);
        }
    }
}
