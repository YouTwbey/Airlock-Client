using AirlockClient.Managers.Lobby;
using Fusion;
using SG.Airlock;
using SG.Airlock.Network;
using SG.Airlock.Roles;
using UnityEngine;

namespace AirlockClient.Utils;
/// <summary>
/// handles refs for the extensions cuz anticheat isnt always available
/// </summary>
public class StaticRefs : MonoBehaviour
{
    public static ModerationManager Moderation;
    public static RoleManager Role;
    public static GameStateManager State;
    public static EmergencyButton Button;
    public static NetworkedKillBehaviour Kill;
    public static AirlockPeer Peer;
    public static ChatManager Chat;
//  public RoleManager RoleManager; ah yes two role managers 10/10 code
    public static NetworkRunner Runner;
    public static StaticRefs Instance;
    public static SpawnManager Spawn;

    private void Start()
    {
        Instance = this;
        Chat = FindObjectOfType<ChatManager>();
        Role = FindObjectOfType<RoleManager>();
        State = FindObjectOfType<GameStateManager>();
        Moderation = FindObjectOfType<ModerationManager>();
        Peer = FindObjectOfType<AirlockPeer>();
        Chat = FindObjectOfType<ChatManager>();
        Button = FindObjectOfType<EmergencyButton>();
        Kill =  FindObjectOfType<NetworkedKillBehaviour>();
        Runner = FindObjectOfType<NetworkRunner>();
        //RoleManager = FindObjectOfType<RoleManager>(); 
        Spawn = FindObjectOfType<SpawnManager>();
    }

    void Update()
    {
       SettingsMenuManager.OnUpdate();
    }
}