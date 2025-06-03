using Il2CppFusion;

namespace AirlockClient.AC
{
    public class RpcData
    {
        public int Sender;
        public RpcData(int sender)
        {
            Sender = sender;
        }

        public static RpcData FromSimulationMessage(SimulationMessage msg)
        {
            int sender = msg.Source;

            return new RpcData(sender);
        }
    }
}
