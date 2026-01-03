using AirlockAPI.Attributes;
using AirlockAPI.Data;
using AirlockAPI.Handlers;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSystem.IO;
using System.Collections.Generic;
using System.Linq;
using static AirlockAPI.Managers.NetworkManager;

namespace AirlockClient.Attributes
{
    public class AirlockClientGamemode : CustomGameHandler
    {
        public Dictionary<PlayerState, SubRole> AssignedSubRoles = new Dictionary<PlayerState, SubRole>();
        public static List<PlayerRef> AirlockClientUsers = new List<PlayerRef>();
        SpawnManager spawn;

        public static void RPC_AirlockClientVerification()
        {
            SendRpc("AirlockClientVerification");
        }

        [AirlockRpc("AirlockClientVerification", RpcTarget.Host, RpcCaller.All)]
        public static void AirlockClientVerification(AirlockRpcInfo info)
        {
            AirlockClientUsers.Add(info.Sender);
        }

        void LateUpdate()
        {
            if (spawn == null)
            {
                spawn = FindObjectOfType<SpawnManager>();
                return;
            }

            List<PlayerRef> usersToRemove = new List<PlayerRef>();
            List<PlayerRef> activePlayers = spawn.Runner.ActivePlayers.ToArray().ToList();

            foreach (PlayerRef plr in AirlockClientUsers)
            {
                if (!activePlayers.Contains(plr))
                {
                    usersToRemove.Add(plr);
                }
            }

            foreach (PlayerRef plr in usersToRemove)
            {
                AirlockClientUsers.Remove(plr);
            }
        }

        public static AirlockClientGamemode Get()
        {
            return (AirlockClientGamemode)Current;
        }
    }
}
