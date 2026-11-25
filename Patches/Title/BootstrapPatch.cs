using AirlockClient.Core;
using Fusion.Photon.Realtime;
using HarmonyLib;
using SG.Airlock;

namespace AirlockClient.Patches.Title
{
    [HarmonyPatch(typeof(AirlockBootstrap), nameof(AirlockBootstrap.Main))]
    public class BootstrapPatch
    {
        public static void Prefix(AirlockBootstrap __instance)
        {
            Base.OnInit();
            PhotonConfig.ApplyAppIds();
        }
    }
    
     public static class PhotonConfig
    {
        public static void ApplyAppIds()
        {
            PhotonAppSettings.Instance.AppSettings.AppIdFusion = "ae0fa525-91ff-4d86-ad3f-fe517abd5874";
            PhotonAppSettings.Instance.AppSettings.AppIdVoice = "be27f699-8f34-4520-92e6-1e97552720ca";
        }
    }
}
