using System;
using System.Collections.Generic;
using System.Text;

namespace Needleforge
{
    internal class ModHelper
    {
        public static void Log(string msg)
        {
            NeedleforgePlugin.logger.LogInfo(msg);
        }

        public static void LogError(string msg)
        {
            NeedleforgePlugin.logger.LogError(msg);
        }
    }
}
