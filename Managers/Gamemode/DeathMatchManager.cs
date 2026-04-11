using AirlockClient.AC;
using AirlockClient.Attributes;
using AirlockClient.Managers.Debug;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Minigames;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.Sabotage;
using System.Collections;
using Unity;
using UnityEngine;
using UnityEngine.Playables;

namespace AirlockClient.Managers.Gamemode
{
    public class DeathMatchManager : AirlockClientGamemode
    {
        public SabotageManager sabotage;
        public DoorsSabotage doors;
        public MinigameManager tasks;

        void Start()
        {
            sabotage = FindObjectOfType<SabotageManager>();
            doors = FindObjectOfType<DoorsSabotage>();
            tasks = FindObjectOfType<MinigameManager>();
        }

        public override bool OnGameStart()
        {

            if (ModdedGameStateManager.Instance == null)
            {
                Logging.Warn("ModdedGameStateManager Is null please report this error");
                return true;
            }
            else
            {
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.SabotageCooldown, 0);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleBoolSettings.AllowDoorSabotage, false);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchBoolSettings.ReportBodies, false);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.LongTasks, 0);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.NumEmergencyMeetings, 0);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.NumImposters, 5);
                ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.ShortTasks, 0);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.MaxVigilantes, 5);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.VigilanteNumOfKills, 50);
                ModdedGameStateManager.Instance.SetRoleSetting(Data.Enums.RoleIntSettings.KillCooldown, 5);
                return true;
            }
        }

        public override bool OnTargetedAction(ref PlayerState killer, ref PlayerState victim, ref int action)
        {
            if (killer.KnownGameRole.Equals(GameRole.Vigilante) && victim.KnownGameRole.Equals(GameRole.Vigilante))
            {
                return false;
            }
            else if (killer.KnownGameTeam == GameTeam.Crewmember && victim.KnownGameTeam == GameTeam.Impostor || killer.KnownGameTeam == GameTeam.Impostor && victim.KnownGameTeam == GameTeam.Crewmember)
            {
                return true;
            }
            else return true;
        }

        public void OnPlayerDied(PlayerState killer, PlayerState victim)
        {
            DelayReviveDeadPlayers(30, victim);
        }

        public IEnumerator DelayReviveDeadPlayers(int delay, PlayerState dead)
        {
            yield return new WaitForSeconds(delay);

            dead.IsAlive = true;
        }
    }
}