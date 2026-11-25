using AirlockAPI.Data;
using MessHallAPI.Networking;
using SG.Airlock.Customization;

namespace AirlockClient.Managers.Network;

public class RPCManager
{
    public static void RPC_UpdateSettings()
    {
        
    }

    public static void RPC_RequestCoHost(string id)
    {
        NetworkManager.InvokeRPC("com.airlockclient.youtubey", "RequestCoHost", id);
    }

    public static void RequestCoHost(string id)
    {
        
    }

    public static void RPC_SendCoHost(int playerId)
    {
        if (CurrentMode.IsHosting)
            NetworkManager.InvokeRPC("com.airlockclient.youtubey", "SendCoHost", playerId);
    }

    public static void SendCoHost([RPCTarget] int playerId)
    {
        
    }

    public static void RPC_SendID()
    {
        
    }
}