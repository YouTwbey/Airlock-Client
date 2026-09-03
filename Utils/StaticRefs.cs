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
    public ModerationManager Moderation;
    public RoleManager Role;
    public GameStateManager State;
    public EmergencyButton Button;
    public NetworkedKillBehaviour Kill;
    public AirlockPeer Peer;
    public ChatManager Chat;
    public RoleManager RoleManager;
    public NetworkRunner Runner;
    public static StaticRefs Instance;

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
        RoleManager = FindObjectOfType<RoleManager>();
    }
}