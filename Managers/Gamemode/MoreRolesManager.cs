using AirlockClient.Attributes;
using AirlockClient.Data;
using AirlockClient.Data.Roles.MoreRoles.Crewmate;
using AirlockClient.Data.Roles.MoreRoles.Imposter;
using AirlockClient.Data.Roles.MoreRoles.Neutral;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AirlockClient.Managers.Gamemode
{
    public class MoreRolesManager : ModdedGamemode
    {
        public override void OnAssignRoles()
        {
            foreach (SubRole role in SubRole.All)
            {
                Destroy(role);
            }

            List<PlayerState> normalCrewmates = new List<PlayerState>();
            List<PlayerState> imposters = new List<PlayerState>();

            foreach (PlayerState player in FindObjectsOfType<PlayerState>())
            {
                if (GetTrueRole(player) == GameRole.Crewmember)
                {
                    normalCrewmates.Add(player);
                }
                if (GetTrueRole(player) == GameRole.Imposter)
                {
                    imposters.Add(player);
                }
            }
            System.Random rng1 = new System.Random();
            normalCrewmates = normalCrewmates.OrderBy(_ => rng1.Next()).ToList();
            System.Random rng2 = new System.Random();
            imposters = imposters.OrderBy(_ => rng2.Next()).ToList();

            List<System.Action> roleAssignments = new List<System.Action>();

            if (Bait.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Bait>(normalCrewmates, Bait.Data.Amount));
            if (Magician.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Magician>(normalCrewmates, Magician.Data.Amount));
            if (Mayor.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Mayor>(normalCrewmates, Mayor.Data.Amount));
            if (Silencer.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Silencer>(normalCrewmates, Silencer.Data.Amount));
            if (Yapper.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Yapper>(normalCrewmates, Yapper.Data.Amount));
            if (GuardianAngel.Data.Amount > 0) roleAssignments.Add(() => AssignRole<GuardianAngel>(normalCrewmates, GuardianAngel.Data.Amount));
            if (Sheriff.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Sheriff>(normalCrewmates, Sheriff.Data.Amount));

            if (Bomber.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Bomber>(imposters, Bomber.Data.Amount));
            if (Janitor.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Janitor>(imposters, Janitor.Data.Amount));
            if (Vampire.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Vampire>(imposters, Vampire.Data.Amount));
            if (Witch.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Witch>(imposters, Witch.Data.Amount));

            if (Executioner.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Executioner>(normalCrewmates, Executioner.Data.Amount));
            if (Jester.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Jester>(normalCrewmates, Jester.Data.Amount));
            if (Lover.Data.Amount > 0) roleAssignments.Add(() => AssignRole<Lover>(normalCrewmates, Lover.Data.Amount));

            System.Random rng3 = new System.Random();
            roleAssignments = roleAssignments.OrderBy(_ => rng3.Next()).ToList();

            foreach (var assignment in roleAssignments)
            {
                assignment.Invoke();
            }
        }

        public void AssignRole<T>(List<PlayerState> candidates, int amount) where T : Component
        {
            for (int i = 0; i < amount && candidates.Count > 0; i++)
            {
                candidates[0].gameObject.AddComponent<T>();
                candidates.RemoveAt(0);
            }
        }

        public static System.Collections.IEnumerator DisplayRoleInfo(PlayerState Player, SubRole Role, SubRoleData Data, string additional = "", GameRole roleToChange = GameRole.NotSet)
        {
            if (Player != null && Role != null && Data != null && CurrentMode.Name != "Sandbox")
            {
                Role.IsDisplayingRole = true;
                Listener.Send("MoreRoles_RecievedRole_" + Data.Name, Player.PlayerId);
                string ogName = Player.NetworkName.Value;
                yield return new WaitForSeconds(1);
                Player.NetworkName = "WMWMWMWMWMWMWMWM";
                yield return new WaitForSeconds(3);
                Player.NetworkName = Data.Name;
                if (roleToChange != GameRole.NotSet)
                {
                    Current.Role.AlterPlayerRole(roleToChange, Player.PlayerId);
                }
                yield return new WaitForSeconds(2);
                if (string.IsNullOrEmpty(additional))
                {
                    Player.NetworkName = Data.Description;
                    yield return new WaitForSeconds(3);
                }
                else
                {
                    Player.NetworkName = Data.Description;
                    yield return new WaitForSeconds(1.5f);
                    Player.NetworkName = additional;
                    yield return new WaitForSeconds(1.5f);
                }
                Player.NetworkName = ogName;
                Role.IsDisplayingRole = false;
            }
        }

        void OnGUI()
        {
            if (!State.InLobbyState()) return;

            if (GUI.Button(new Rect(0, 0, 200, 50), "Bait: " + Bait.Data.Amount))
            {
                Bait.Data.Amount++;
                if (Bait.Data.Amount == 11)
                {
                    Bait.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 50, 200, 50), "G_Angel: " + GuardianAngel.Data.Amount))
            {
                GuardianAngel.Data.Amount++;
                if (GuardianAngel.Data.Amount == 11)
                {
                    GuardianAngel.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 100, 200, 50), "Magician: " + Magician.Data.Amount))
            {
                Magician.Data.Amount++;
                if (Magician.Data.Amount == 11)
                {
                    Magician.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 150, 200, 50), "Mayor: " + Mayor.Data.Amount))
            {
                Mayor.Data.Amount++;
                if (Mayor.Data.Amount == 11)
                {
                    Mayor.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 200, 200, 50), "Silencer: " + Silencer.Data.Amount))
            {
                Silencer.Data.Amount++;
                if (Silencer.Data.Amount == 11)
                {
                    Silencer.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 250, 200, 50), "Yapper: " + Yapper.Data.Amount))
            {
                Yapper.Data.Amount++;
                if (Yapper.Data.Amount == 11)
                {
                    Yapper.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 300, 200, 50), "Bomber: " + Bomber.Data.Amount))
            {
                Bomber.Data.Amount++;
                if (Bomber.Data.Amount == 11)
                {
                    Bomber.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 350, 200, 50), "Janitor: " + Janitor.Data.Amount))
            {
                Janitor.Data.Amount++;
                if (Janitor.Data.Amount == 11)
                {
                    Janitor.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 400, 200, 50), "Vampire: " + Vampire.Data.Amount))
            {
                Vampire.Data.Amount++;
                if (Vampire.Data.Amount == 11)
                {
                    Vampire.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 450, 200, 50), "Witch: " + Witch.Data.Amount))
            {
                Witch.Data.Amount++;
                if (Witch.Data.Amount == 11)
                {
                    Witch.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 500, 200, 50), "Executioner: " + Executioner.Data.Amount))
            {
                Executioner.Data.Amount++;
                if (Executioner.Data.Amount == 11)
                {
                    Executioner.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 550, 200, 50), "Jester: " + Jester.Data.Amount))
            {
                Jester.Data.Amount++;
                if (Jester.Data.Amount == 11)
                {
                    Jester.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 600, 200, 50), "Lover: " + Lover.Data.Amount))
            {
                Lover.Data.Amount++;
                if (Lover.Data.Amount == 11)
                {
                    Lover.Data.Amount = 0;
                }
            }
            if (GUI.Button(new Rect(0, 650, 200, 50), "Sheriff: " + Sheriff.Data.Amount))
            {
                Sheriff.Data.Amount++;
                if (Sheriff.Data.Amount == 11)
                {
                    Sheriff.Data.Amount = 0;
                }
            }
        }
    }
}
