using Il2CppFusion;

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
