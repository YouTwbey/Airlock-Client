using AirlockAPI.Data;
using AirlockClient.Attributes;
using AirlockClient.Data.Roles.MoreRoles.Neutral;
using AirlockClient.Managers.Debug;
using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Graphics;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections;
using UnityEngine;
using static AirlockClient.Managers.Gamemode.MoreRolesManager;
using static UnityEngine.Object;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.EndGame))]
    public class EndGamePatch
    {
        public static void Prefix(GameStateManager __instance, GameTeam winningTeam)
        {
            if (CurrentMode.IsHosting && CurrentMode.Modded && CurrentMode.Name == "More Roles")
            {
                Logging.Debug_Log($"SubRolesFound: {SubRole.All.ToArray()}");
                Lawyer lawyer = Lawyer.Instance;
                if (lawyer != null)
                {
                    Logging.Debug_Log("Lawyer isnt null checking if they can win.");
                    if (lawyer.LawyerWinsWithClient)
                    {
                        Logging.Debug_Log("lawyer's client wasnt voted out they can win checking if their client won.");
                        if (GetTrueTeam(lawyer.Client) == winningTeam)
                        {
                            Logging.Debug_Log($"Client TrueTeam: {GetTrueTeam(lawyer.Client)}, WinningTeam: {winningTeam}");
                            FindObjectOfType<NetworkedKillBehaviour>().AlterRole(GetTrueRoleMR(lawyer.Client), lawyer.PlayerWithRole.PlayerId, 0);
                            Logging.Debug_Log("Client Won and Wasnt voted out Altering role");
                            Logging.Debug_Log($"Altered Lawyer Role, New Role: {lawyer.PlayerWithRole.KnownGameRole}");
                        }
                        else
                        {
                            Logging.Debug_Log($"Client KnownGameTeam: {GetTrueTeam(lawyer.Client)}, WinningTeam: {winningTeam}");
                            Logging.Debug_Warn("Client didnt win");
                        }
                    }
                    else
                    {
                        Logging.Debug_Warn("Lawyer's client was voted out");
                    }
                }
                else
                {
                    Logging.Debug_Error($"lawyer is null!");
                }
            }
            foreach (SubRole role in SubRole.All)
            {
                role.OnGameEnd(winningTeam);
            }
        }
    }

    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.ReportWin))]
    public class EndGameTestingPatch
    {
        public static void Prefix(GameTeam winningTeam)
        {
            if (CurrentMode.IsHosting && CurrentMode.Modded && CurrentMode.Name == "More Roles")
            {
                Logging.Debug_Log($"SubRolesFound: {SubRole.All.ToArray()}");
                Lawyer lawyer = Lawyer.Instance;
                if (lawyer != null)
                {
                    Logging.Debug_Log("Lawyer isnt null checking if they can win.");
                    if (lawyer.LawyerWinsWithClient)
                    {
                        Logging.Debug_Log("lawyer's client wasnt voted out they can win, checking if their client won.");
                        if (GetTrueTeam(lawyer.Client) == winningTeam)
                        {
                            Logging.Debug_Log($"Client TrueTeam: {GetTrueTeam(lawyer.Client)}, WinningTeam: {winningTeam}");
                            FindObjectOfType<NetworkedKillBehaviour>().AlterRole(GetTrueRoleMR(lawyer.Client), lawyer.PlayerWithRole.PlayerId, 0);
                            Logging.Debug_Log("Client Won and Wasnt voted out Altering role");
                            Logging.Debug_Log($"Altered Lawyer Role, New Role: {lawyer.PlayerWithRole.KnownGameRole}");
                        }
                        else
                        {
                            Logging.Debug_Log($"Client KnownGameTeam: {GetTrueTeam(lawyer.Client)}, WinningTeam: {winningTeam}");
                            Logging.Debug_Warn("Client didnt win");
                        }
                    }
                    else
                    {
                        Logging.Debug_Warn("Lawyer's client was voted out");
                    }
                }
                else
                {
                    Logging.Debug_Error($"lawyer is null!");
                }
            }
        }
    }

    internal static class CosmeticHelper
    {
        public static void RevertCosmeticsToSaved(PlayerState player)
        {
            if (CurrentMode.IsHosting && CurrentMode.Modded && CurrentMode.Name == "More Roles")
            {
                if (PlayerSavedState.TryGet(player.PlayerId, out int color, out int hat))
                {
                    player.ColorId = color;
                    player.HatId = hat;
                }
            }
        }

        public static IEnumerator RevertAllAfterDelay()
        {
            yield return new WaitForSeconds(7f);
            foreach (PlayerState player in Spawn.ActivePlayerStates)
                RevertCosmeticsToSaved(player);
        }
    }

    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.ReportWin))]
    public class EndGamePrefixPatch
    {
        public static void Postfix(GameTeam winningTeam)
        {
            if (CurrentMode.IsHosting && CurrentMode.Modded && CurrentMode.Name == "More Roles")
            {
                MelonCoroutines.Start(CosmeticHelper.RevertAllAfterDelay());
            }
        }
    }

    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.EndGame))]
    public class EndGamePatch2
    {
        public static void Postfix(GameTeam winningTeam)
        {
            if (CurrentMode.IsHosting && CurrentMode.Modded && CurrentMode.Name == "More Roles")
            {
                MelonCoroutines.Start(CosmeticHelper.RevertAllAfterDelay());
            }
        }
    }
}
