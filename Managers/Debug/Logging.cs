﻿using MelonLoader;
using UnityEngine;

namespace AirlockClient.Managers.Debug
{
    public class Logging
    {
        public static void Log(string message)
        {
            MelonLogger.Msg(message);
        }

        public static void Warn(string message)
        {
            MelonLogger.Warning(message);
        }

        public static void Error(string message, bool crash = false)
        {
            MelonLogger.Error(message);

            if (crash)
            {
                Application.Quit();
            }
        }

        public static void Debug_Log(string message)
        {
#if DEBUG
            MelonLogger.Msg("[DEBUG] " + message);
#endif
        }

        public static void Debug_Warn(string message)
        {
#if DEBUG
            MelonLogger.Warning("[DEBUG] " + message);
#endif
        }

        public static void Debug_Error(string message)
        {
#if DEBUG
            MelonLogger.Error("[DEBUG] " + message);
#endif
        }
    }
}
