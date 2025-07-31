using AirlockClient.Attributes;
using AirlockClient.Handlers;
using AirlockClient.Managers.Gamemode;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.XR;
using MelonLoader;

namespace AirlockClient.Data.Roles.HideNSeek.Crewmate
{
    public class Hider : SubRole
    {
        public static SubRoleData Data = new SubRoleData
        {
            Name = "Runner",
            Description = "Run from imp",
            Team = GameTeam.Crewmember,
            Amount = 9
        };
        public static int NumOfVents = 3;
        public static int ForceHiderColorId = -1;
        int DefaultColorId = -1;

        public int VentsLeft;
        public bool AllowedMoreVents = true;

        void Start()
        {
            VentsLeft = NumOfVents;

            DefaultColorId = PlayerWithRole.ColorId;
            MelonCoroutines.Start(HideNSeekManager.DisplayRoleInfo(PlayerWithRole, this));
        }

        public override void OnPlayerDied(PlayerState killer)
        {
            PlayerWithRole.LocomotionPlayer.TaskPlayer.AssignedTasks.Clear();
            PlayerWithRole.ActivePowerUps = PowerUps.None;

            if (PlayerWithRole == FindObjectOfType<XRRig>().PState)
            {
                DangerMeterHandler.Instance.HasDied = true;
            }
        }

        public override void OnRoleRemoved()
        {
            PlayerWithRole.ActivePowerUps = PowerUps.None;
        }

        void Update()
        {
            if (ForceHiderColorId != -1)
            {
                PlayerWithRole.ColorId = ForceHiderColorId;
            }
            else
            {
                if (PlayerWithRole.ColorId != DefaultColorId)
                {
                    PlayerWithRole.ColorId = DefaultColorId;
                }
            }

            if (VentsLeft < 0)
            {
                AllowedMoreVents = false;
            }

            if (PlayerWithRole.IsAlive)
            {
                if (AllowedMoreVents)
                {
                    if (PlayerWithRole.ActivePowerUps != PowerUps.CanVent)
                    {
                        PlayerWithRole.ActivePowerUps = PowerUps.CanVent;
                        VentsLeft -= 1;
                    }
                }
                else
                {
                    PlayerWithRole.ActivePowerUps = PowerUps.None;
                }
            }
            else
            {
                if (PlayerWithRole.LocomotionPlayer.TaskPlayer.AssignedTasks.Count != 0)
                {
                    PlayerWithRole.LocomotionPlayer.TaskPlayer.AssignedTasks.Clear();
                }
                if (PlayerWithRole.ActivePowerUps != PowerUps.None)
                {
                    PlayerWithRole.ActivePowerUps = PowerUps.None;
                }
            }
        }
    }
}
