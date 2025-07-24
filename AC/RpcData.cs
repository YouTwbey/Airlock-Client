using Il2CppFusion;
using Il2CppSG.Airlock.Network;
using Il2CppSG.CoreLite;
using MelonLoader;

namespace AirlockClient.AC
{
    public class RpcData
    {
        public static RpcInfo info;

        public static unsafe void FromMessage(NetworkRunner runner, SimulationMessage* message)
        {
            info = RpcInfo.FromMessage(runner, message, RpcHostMode.SourceIsHostPlayer);
        }
    }
}
