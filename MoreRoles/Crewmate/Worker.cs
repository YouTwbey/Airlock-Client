using AirlockClient.Attributes;
using AirlockClient.Managers;
using AirlockClient.Managers.Debug;
using AirlockClient.Managers.Gamemode;
using Il2CppFusion;
using Il2CppFusion.Photon.Realtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Minigames;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using Il2CppSystem.Runtime.InteropServices;
using MelonLoader;
using System;
using System.CodeDom;
using System.Threading;
using Unity;
using static UnityEngine.Object;



namespace AirlockClient.Data.Roles.MoreRoles.Crewmate
{
    /// <summary>
    /// Worker 
    /// Crewmate
    /// Complete X amount of tasks to get more 
    /// </summary>
    public class Worker : SubRole
    {
        public bool Tasks = true;
        public int InstaComplete = 3;
        public MinigameManager Task;
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Worker",
            RoleType = "Crewmate",
            Description = "Infinite tasks",
            AC_Description = "You have an infinite amount of tasks. Everytime you complete all tasks point to get more",
            Team = GameTeam.Crewmember,
            Amount = 0
        };

        void Start()
        {
            Task = PlayerWithRole.LocomotionPlayer.TaskPlayer._minigameManager;
            MoreRolesManager.QueueRoleDisplay(PlayerWithRole, this, Data);
        }

        public override void OnPlayerInput(XRRigInput input)
        {
            if ((PlayerWithRole.LocomotionPlayer._prevLeftHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._prevRightHandPose == HandPoses.Point || PlayerWithRole.LocomotionPlayer._previousBool == "Gesture_Point") && PlayerWithRole.IsAlive && InstaComplete != 0)
            {
                if (Task == null)
                {
                    Logging.Warn("If you're seeing this and an error please make a bug report in the Airlock Client server");
                    return;
                }
                else if (!FindObjectOfType<GameStateManager>().InTaskState())
                {
                    Logging.Debug_Log("Not in a Task State");
                    return;
                }
                else if (Task.TasksComplete < PlayerWithRole.LocomotionPlayer.TaskPlayer.AssignedTasks.Count)
                {
                    Logging.Debug_Log(PlayerWithRole.LocomotionPlayer.TaskPlayer._minigameManager.TasksComplete.ToString());
                    Logging.Debug_Log(PlayerWithRole.LocomotionPlayer.TaskPlayer.AssignedTasks.Count.ToString());
                    return;
                }
                else if (Task.TasksComplete >= PlayerWithRole.LocomotionPlayer.TaskPlayer.AssignedTasks.Count)
                {
                    foreach (var minigameConsole in FindObjectsOfType<MinigameConsole>(true))
                    {
                        try
                        {
                            PlayerWithRole.LocomotionPlayer.TaskPlayer.ClearAssignedTasks();
                            Logging.Debug_Log("test");
                            FindObjectOfType<MinigameManager>().AssignTaskSlots(MoreRolesManager.TasksAssignedCount, PlayerWithRole.LocomotionPlayer.LocalMinigamePlayer);
                            Logging.Debug_Log($"Assigned New Tasks to the worker");
                            return;
                        }
                        catch (Exception ex)
                        {
                            Logging.Debug_Log($"Task Not Assigned: {ex}");
                        }
                    }
                }
            }
        }
    }
}
