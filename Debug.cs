#nullable disable
using MelonLoader;
using MelonLoader.Utils;
using System.IO;

namespace CatchColdMod
{
    internal static class DebugHelper
    {
        private static bool _debugEnabled = false;
        private static readonly string DebugFile = Path.Combine(MelonEnvironment.UserDataDirectory, "catchcold.debug");

        internal static void Init()
        {
            _debugEnabled = File.Exists(DebugFile);

            if (_debugEnabled)
                MelonLogger.Msg("[CatchColdMod] Debug ENABLED");
        }

        internal static void Log(string msg)
        {
            if (_debugEnabled)
                MelonLogger.Msg("[CatchColdMod DEBUG] " + msg);
        }
    }
}