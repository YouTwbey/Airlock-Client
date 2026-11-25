using System;
using System.IO;
using System.Linq;
using AirlockClient.Data;
using AirlockClient.Managers.Debug;
using AirlockClient.Utils;
using MessHallAPI.Networking;
using SG.Airlock;
using SG.Airlock.Settings;
using UnityEngine;

namespace AirlockClient.Managers.Network;

public class CoHostManager : MonoBehaviour
{
    public static void AddPlayerAsCoHost(string id, string name)
    {
        if (!id.Contains("Steam_")) return;

        try
        {
            if (!File.Exists(BepInEx.Paths.PluginPath + "/CoHosts.txt"))
            {
                File.Create(BepInEx.Paths.PluginPath + "/CoHosts.txt").Close();
            }

            var cohosts = $"name: {name} ||  id: {id}";
            File.AppendAllText(BepInEx.Paths.PluginPath + "/CoHosts.txt", cohosts + Environment.NewLine);
        }
        catch (Exception e)
        {
            Logging.Error(e.ToString());
            return;
        }
    }

    public static void RPC_RequestSettingUpdate(string setting)
    {
        NetworkManager.InvokeRPC("com.airlockclient.youtubey", "UpdateSetting", setting);
    }

    [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
    public static void UpdateSetting([MessHallAPI.Networking.RPCInfo] MessHallRpcInfo info,  string setting)
    {
        var playerpotato = GameObject.Find("PlayerState (" + info.Sender + ")").GetComponent<PlayerState>();
        var potato = GameObject.Find("PlayerState (" + info.Sender + ")").GetComponent<PlayerState>().GetActualModId();
        var foundID = File.ReadAllLines(BepInEx.Paths.PluginPath + "/CoHosts.txt").Contains(potato);
        if (!foundID) StaticRefs.Runner.Disconnect(info.Sender);
    }

    public static int getNewValue(Enums.MatchIntSettings setting, int newVal)
    {
        var value = ModdedGameStateManager.Instance.GetMatchSetting(setting).GetValue();
        
        return value + newVal;
    }

    public static int MatchSettingValue()
    {
        return -1;
    }
    
    [MessHallRPC(RPCTarget.Host, RPCCaller.Anyone)]
    public static void RPC_RequestSettingChange([RPCInfo] MessHallRpcInfo info, string setting)
    {
        var change = Enums.GetSettingFromString(setting);
        //var value = args.GetType();

        switch (change)
        {
            
            case Enums.MatchBoolSettings:
                //UpdateMatchBool(change, value);
                return;
            case Enums.MatchIntSettings:
                return;
            case Enums.MatchFloatSettings:
                return;
            case Enums.RoleBoolSettings:
                return;
            case Enums.RoleIntSettings:
                return;
            case Enums.RoleFloatSettings:
                return;
        }
    }

    public static void UpdateMatchBool(Enum setting, bool value)
    {
        var set2 = (Enums.MatchBoolSettings)setting;
    }
}
