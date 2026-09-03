
using AirlockClient.Core;
using UnityEngine;

namespace AirlockClient.Managers.Debug
{
    public class Logging
    {
        public static void Log(string message)
        {
            Base.Instance.Log.LogInfo(message);
        }

        public static void Warn(string message)
        {
            Base.Instance.Log.LogWarning(message);
        }

        public static void Error(string message, bool crash = false)
        {
            Base.Instance.Log.LogError(message);

            if (crash)
            {
                Application.Quit();
            }
        }

        public static void Debug_Log(string message)
        {
#if DEBUG
            Log("[DEBUG] " + message);
#endif
        }

        public static void Debug_Warn(string message)
        {
#if DEBUG
            Warn("[DEBUG] " + message);
#endif
        }

        public static void Debug_Error(string message)
        {
#if DEBUG
            Error("[DEBUG] " + message);
#endif
        }
    }
}
