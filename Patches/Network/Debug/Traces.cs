using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SG.Platform;
using AirlockClient.Managers.Debug;

namespace AirlockClient.Patches.Network.Debug;

[HarmonyPatch]
internal static class PhotonAuthenticationTrace
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        return new MethodBase[]
        {
            AccessTools.Method(typeof(PhotonAuthentication),
                "GetFusionAuthenticationValues"),

            AccessTools.Method(typeof(PhotonAuthentication),
                "GetVoiceAuthenticationValues"),

            AccessTools.Method(typeof(PhotonAuthentication),
                "GetFusionBaseAuthValues"),

            AccessTools.Method(typeof(PhotonAuthentication),
                "GetVoiceBaseAuthValues")
        };
    }

    private static void Prefix(
        object __instance,
        MethodBase __originalMethod)
    {
        Logging.Log(
            "[AUTH TRACE] ENTER " +
            __originalMethod.Name +
            " Instance=" +
            (__instance?.GetType().FullName ?? "NULL"));
    }

    private static void Postfix(
        object __instance,
        MethodBase __originalMethod,
        object __result)
    {
        Logging.Log(
            "[AUTH TRACE] EXIT " +
            __originalMethod.Name +
            " Result=" +
            (__result == null ? "NULL" : __result.GetType().FullName));

        if (__result == null)
            return;

        if (__result is Il2CppSystem.Threading.Tasks.Task task)
        {
            Logging.Log(
                "[AUTH TRACE] TASK " +
                __originalMethod.Name +
                " Status=" + task.Status +
                " Completed=" + task.IsCompleted +
                " Faulted=" + task.IsFaulted +
                " Canceled=" + task.IsCanceled);
        }
        else
        {
            try
            {
                var type = __result.GetType();

                foreach (var property in type.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!property.CanRead ||
                        property.GetIndexParameters().Length != 0)
                        continue;

                    try
                    {
                        Logging.Log(
                            "[AUTH TRACE] " +
                            __originalMethod.Name + "." +
                            property.Name + "=" +
                            (property.GetValue(__result) ?? "NULL"));
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                Logging.Log("[AUTH TRACE] Inspection error: " + e);
            }
        }
    }

    private static Exception Finalizer(
        Exception __exception,
        MethodBase __originalMethod)
    {
        if (__exception != null)
        {
            Logging.Log(
                "[AUTH TRACE] EXCEPTION " +
                __originalMethod.Name + ": " + __exception);
        }

        return __exception;
    }
}
