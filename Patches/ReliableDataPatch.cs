using AirlockClient.Core;
using AirlockClient.Managers;
using HarmonyLib;
using Il2CppFusion;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Linq;
using System.Text;

namespace AirlockClient.Patches
{
    [HarmonyPatch(typeof(NetworkRunner), nameof(NetworkRunner.Fusion_Simulation_ICallbacks_OnReliableData))]
    public class ReliableDataPatch
    {
        public static void Prefix(NetworkRunner __instance, PlayerRef player, Il2CppStructArray<byte> dataArray)
        {
            if (dataArray != null)
            {
                string message = DataToString(dataArray);

                if (Listener.Instance == null)
                {
                    Base.SceneStorage.AddComponent<Listener>();
                }

                Listener.OnMessageRecieved(message, player);
            }
        }

        public static Il2CppStructArray<byte> StringToData(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            Il2CppStructArray<byte> dataArray = new Il2CppStructArray<byte>(bytes);
            return dataArray;
        }

        public static string DataToString(Il2CppStructArray<byte> data)
        {
            byte[] managedBytes = data.ToArray();
            string message = Encoding.UTF8.GetString(managedBytes);
            return message;
        }
    }
}
