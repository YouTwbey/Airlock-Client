using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SG.Platform;
using SG.Airlock.Network;
using Fusion.Photon.Realtime;
using AirlockClient.Managers.Debug;
using Fusion;
using SG.Platform.Oculus;
using SG.Platform.Steam;
using UnityEngine;
using AuthenticationValues = Photon.Realtime.AuthenticationValues;

namespace AirlockClient.Patches.Network.Photon
{
    internal static class PhotonAuthTargets
    {

        [HarmonyPatch(typeof(SteamPhotonAuthentication), nameof(SteamPhotonAuthentication.GetFusionAuthenticationValues))]
        public static class SteamFusionAuthPatch
        {
            public static bool Prefix(SteamPhotonAuthentication __instance, ref Il2CppSystem.Threading.Tasks.Task< PhotonAuthentication.FusionAuthenticationResponse<Fusion.Photon.Realtime.AuthenticationValues>> __result)
            {
                PhotonAuthentication.FusionAuthenticationResponse<Fusion.Photon.Realtime.AuthenticationValues> result =
                    new PhotonAuthentication.FusionAuthenticationResponse<Fusion.Photon.Realtime.AuthenticationValues>();
                result.AuthenticationValues = new Fusion.Photon.Realtime.AuthenticationValues(SystemInfo.deviceUniqueIdentifier);
                __result = Il2CppSystem.Threading.Tasks.Task.FromResult(result);
                return false;
            }
        }

        [HarmonyPatch(typeof(SteamPhotonAuthentication),
            nameof(SteamPhotonAuthentication.GetVoiceAuthenticationValues))]
        public static class SteamPhotonAuthPatch
        {
            public static bool Prefix(SteamPhotonAuthentication __instance,
                ref Il2CppSystem.Threading.Tasks.Task<
                    PhotonAuthentication.FusionAuthenticationResponse<AuthenticationValues>> __result)
            {
                PhotonAuthentication.FusionAuthenticationResponse<AuthenticationValues> result =
                    new PhotonAuthentication.FusionAuthenticationResponse<AuthenticationValues>();
                result.AuthenticationValues = new AuthenticationValues(SystemInfo.deviceUniqueIdentifier);
                __result = Il2CppSystem.Threading.Tasks.Task.FromResult(result);
                return false;
            }
        }

        [HarmonyPatch(typeof(AirlockPeer), nameof(AirlockPeer.PhotonAuth))]
        public static class PhotonAuthPeerPatch
        {
            public static void Prefix(AirlockPeer __instance, ref StartGameArgs args, ref bool requireAuthentication,
                ref PhotonAuthentication photonAuth)
            {
                requireAuthentication = false;
            }
        }

        [HarmonyPatch(typeof(OculusPhotonAuthentication), nameof(OculusPhotonAuthentication.GetFusionAuthenticationValues))]
        public static class FusionAuthPatch
        {
            public static bool Prefix(OculusPhotonAuthentication __instance, ref Il2CppSystem.Threading.Tasks.Task<PhotonAuthentication.FusionAuthenticationResponse<Fusion.Photon.Realtime.AuthenticationValues>> __result)
            {
                PhotonAuthentication.FusionAuthenticationResponse<Fusion.Photon.Realtime.AuthenticationValues> result = new PhotonAuthentication.FusionAuthenticationResponse<Fusion.Photon.Realtime.AuthenticationValues>();
                result.AuthenticationValues = new Fusion.Photon.Realtime.AuthenticationValues(SystemInfo.deviceUniqueIdentifier);
                __result = Il2CppSystem.Threading.Tasks.Task.FromResult(result);
                return false;
            }
        }

        [HarmonyPatch(typeof(OculusPhotonAuthentication), nameof(OculusPhotonAuthentication.GetVoiceAuthenticationValues))]
        public static class PhotonAuthPatch
        {
            public static bool Prefix(OculusPhotonAuthentication __instance, ref Il2CppSystem.Threading.Tasks.Task<PhotonAuthentication.FusionAuthenticationResponse<AuthenticationValues>> __result)
            {
                PhotonAuthentication.FusionAuthenticationResponse<AuthenticationValues> result = new PhotonAuthentication.FusionAuthenticationResponse<AuthenticationValues>();
                result.AuthenticationValues = new AuthenticationValues(SystemInfo.deviceUniqueIdentifier);
                __result = Il2CppSystem.Threading.Tasks.Task.FromResult(result);
                return false;
            }
        }
    }
}