using AirlockClient.Attributes;
using Il2CppSG.Airlock.Sabotage;
using UnityEngine;

namespace AirlockClient.Managers.Gamemode
{
    public class ContainmentManager : AirlockClientGamemode
    {
        public SabotageManager sabotage;
        public DoorsSabotage doors;

        void Start()
        {
            sabotage = FindObjectOfType<SabotageManager>();
            doors = FindObjectOfType<DoorsSabotage>();
        }

        public override bool OnGameStart()
        {
            ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchIntSettings.SabotageCooldown, 0);
            ModdedGameStateManager.Instance.SetMatchSetting(Data.Enums.MatchBoolSettings.AllowDoorSabotage, false);
            return true;
        }

        public void OnRepairedSabotage()
        {
            eventID = -1;
        }

        public void OnGameEnd()
        {
            eventID = -1;
        }

        int eventID = 0;
        void Update()
        {
            if (State.InTaskState())
            {
                if (State._gamemodeTimerCurrent >= 0 && eventID != 4)
                {
                    eventID += 1;
                    if (eventID == 0)
                    {
                        State._gamemodeTimerCurrent = 30;
                        State._gamemodeTimerRunning = true;
                    }
                    else if (eventID == 1)
                    {
                        LockAllDoors();
                        State._gamemodeTimerCurrent = 15;
                        State._gamemodeTimerRunning = true;
                    }
                    if (eventID == 2)
                    {
                        State._gamemodeTimerCurrent = 30;
                        State._gamemodeTimerRunning = true;
                    }
                    else if (eventID == 3)
                    {
                        LockAllDoors();
                        State._gamemodeTimerCurrent = 15;
                        State._gamemodeTimerRunning = true;
                    }
                    else if (eventID == 4)
                    {
                        ToggleRandomSabotage();
                    }
                }
            }
            else
            {
                eventID = -1;
                State._gamemodeTimerCurrent = 0;
                State._gamemodeTimerRunning = false;
            }
        }

        void ToggleRandomSabotage()
        {
            int rand = Random.Range(0, 2);

            if (rand == 0)
            {
                FindObjectOfType<OxygenSabotage>().Begin();
            }
            else
            {
                FindObjectOfType<ReactorSabotage>().Begin();
            }
        }

        void LockAllDoors()
        {
            doors.CloseAllActiveDoors();
        }
    }
}
