using SG.Airlock;
using AirlockAPI.Handlers;
using Fusion;
using SG.Airlock.Network;
using AirlockAPI.Data;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.IO;
using MessHallAPI.Networking;

namespace AirlockClient.Attributes
{
    public class AirlockClientGamemode : CustomGameHandler
    {
        public Dictionary<PlayerState, SubRole> AssignedSubRoles = new Dictionary<PlayerState, SubRole>();
        public Dictionary<PlayerState, Modifier> AssignedModifiers = new Dictionary<PlayerState, Modifier>();
        public static List<PlayerRef> AirlockClientUsers = new List<PlayerRef>();
        SpawnManager spawn;

        public static void RPC_AirlockClientVerification()
        {
            NetworkManager.InvokeRPC("com.airlockclient.youtubey", "AirlockClientVerification");
        }

        [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
        public static void AirlockClientVerification([RPCInfo] MessHallRpcInfo info)
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
