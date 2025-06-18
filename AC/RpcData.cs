using Il2CppFusion;
using Il2CppSG.Airlock.Network;
using Il2CppSG.CoreLite;

namespace AirlockClient.AC
{
    public class RpcData
    {
        public static RpcInfo Info;

        public static unsafe void FromSimulationMessage(NetworkRunner runner, SimulationMessage* msg)
        {
            Info = RpcInfo.FromMessage(runner, msg, RpcHostMode.SourceIsHostPlayer);
        }
    }
}
