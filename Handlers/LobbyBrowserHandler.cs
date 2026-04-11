using AirlockClient.Managers.Debug;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Object;

namespace AirlockClient.Handlers
{
    public static class LobbyBrowserHandler
    {
        public static bool browseAdded;
        public static bool waitingForBrowseChange;
        public static bool inBrowseMenu;
        public static bool firstTimeInBrowseMenu;
        public static GameObject quickMatchHud;
        public static AirlockPeer peer;
        public static List<SessionInfo> sessions = new List<SessionInfo>();
        public static List<SessionInfo> filteredSessions = new List<SessionInfo>();
        public static Vector2 scrollPos;
        public static float WindowWidth = 600f;
        public static float WindowHeight = 400f;
        public static float RowHeight = 40f;
        public static string regionText = "0 Lobbies Open | 0 Players Online";

        public enum VoiceChatFilter
        {
            VC_And_QC,
            VC_Only,
            QC_Only
        }

        public static VoiceChatFilter voiceChatFilter = VoiceChatFilter.VC_And_QC;

        public enum MapFilter
        {
            All,
            Skeld_II,
            Polus_Point
        }

        public static MapFilter mapFilter = MapFilter.All;

        public enum ModdedFilter
        {
            Both,
            Vanilla,
            Modded
        }

        public static ModdedFilter moddedFilter = ModdedFilter.Both;

        public static void OnEnteredBrowseLobbies()
        {
            Logging.Log("OnEnteredBrowseLobbies");
            waitingForBrowseChange = true;

            if (!firstTimeInBrowseMenu)
            {
                peer = Instantiate(Resources.FindObjectsOfTypeAll<AirlockPeer>()[0].gameObject).GetComponent<AirlockPeer>();
                DontDestroyOnLoad(peer.gameObject);
                peer.name = "BrowserPeer";
                peer.JoinLobbyAuthorized();
                firstTimeInBrowseMenu = true;
            }
        }

        public static void RefreshSessions()
        {
            sessions.Clear();
            filteredSessions.Clear();

            foreach (SessionInfo session in peer.Sessions)
            {
                if (IsPublic(session) && session.PlayerCount != session.MaxPlayers)
                {
                    sessions.Add(session);

                    bool shouldBeAdded = true;

                    if (mapFilter != MapFilter.All)
                    {
                        if (GetMap(session) != mapFilter.ToString().Replace("_", " "))
                        {
                            shouldBeAdded = false;
                        }
                    }
                    if (voiceChatFilter != VoiceChatFilter.VC_And_QC)
                    {
                        if (IsVoiceChatEnabled(session) && voiceChatFilter == VoiceChatFilter.QC_Only)
                        {
                            shouldBeAdded = false;
                        }
                        if (!IsVoiceChatEnabled(session) && voiceChatFilter == VoiceChatFilter.VC_Only)
                        {
                            shouldBeAdded = false;
                        }
                    }
                    if (moddedFilter != ModdedFilter.Both)
                    {
                        if (IsModded(session) && moddedFilter == ModdedFilter.Vanilla)
                        {
                            shouldBeAdded = false;
                        }
                        if (!IsModded(session) && moddedFilter == ModdedFilter.Modded)
                        {
                            shouldBeAdded = false;
                        }
                    }

                    if (shouldBeAdded)
                    {
                        filteredSessions.Add(session);
                    }
                }
            }
        }
        
        public static void OnGUI()
        {
            if (inBrowseMenu)
            {
                float centerX = (Screen.width - WindowWidth) * 0.5f;
                float centerY = (Screen.height - WindowHeight) * 0.5f;

                RefreshSessions();
                regionText = $"{sessions.Count} Lobbies Open | {GetPlayersOnline()} Players Online";
                GUI.Label(
                    new Rect(centerX, centerY - 30f, WindowWidth, 25f),
                    regionText,
                    CenteredLabelStyle()
                );

                GUI.Box(
                    new Rect(centerX, centerY, WindowWidth, WindowHeight),
                    "Lobbies"
                );

                Rect viewRect = new Rect(0, 0, WindowWidth - 20f, filteredSessions.Count * RowHeight);

                scrollPos = GUI.BeginScrollView(
                    new Rect(centerX + 10f, centerY + 30f, WindowWidth - 20f, WindowHeight - 40f),
                    scrollPos,
                    viewRect
                );

                for (int i = 0; i < filteredSessions.Count; i++)
                {
                    DrawLobbyRow(filteredSessions[i], i);
                }

                GUI.EndScrollView();

                // ---- Filters layout constants ----
                float filtersTop = centerY + WindowHeight + 15f;
                float filtersPadding = 10f;
                float rowHeight = 22f;
                float rowSpacing = 6f;
                float labelWidth = 110f;

                float toolbarX = centerX + filtersPadding + labelWidth + 10f;
                float toolbarWidth = WindowWidth - (filtersPadding * 2) - labelWidth - 10f;

                // Total height: title + padding + 3 rows
                float filtersHeight =
                    20f +                        // title space
                    filtersPadding * 2 +
                    (rowHeight * 3) +
                    (rowSpacing * 2);

                // ---- Filters box ----
                GUI.Box(
                    new Rect(centerX, filtersTop, WindowWidth, filtersHeight),
                    "Filters"
                );

                float rowY = filtersTop + 30f; // make all filter rows relative to filtersTop

                // ---- Voice Chat filter ----
                GUI.Label(
                    new Rect(centerX + filtersPadding, rowY, labelWidth, rowHeight),
                    "Voice Chat"
                );

                string[] vcOptions =
                {
                    "VC and QC",
                    "Voice Chat Only",
                    "Quick Chat Only"
                };

                voiceChatFilter = (VoiceChatFilter)GUI.Toolbar(
                    new Rect(toolbarX, rowY, toolbarWidth, rowHeight),
                    (int)voiceChatFilter,
                    vcOptions
                );

                rowY += rowHeight + rowSpacing;

                // ---- Map filter ----
                GUI.Label(
                    new Rect(centerX + filtersPadding, rowY, labelWidth, rowHeight),
                    "Map"
                );

                string[] mapOptions =
                {
                    "All",
                    "Skeld II",
                    "Polus Point"
                };

                mapFilter = (MapFilter)GUI.Toolbar(
                    new Rect(toolbarX, rowY, toolbarWidth, rowHeight),
                    (int)mapFilter,
                    mapOptions
                );

                rowY += rowHeight + rowSpacing;

                // ---- Modded filter ----
                GUI.Label(
                    new Rect(centerX + filtersPadding, rowY, labelWidth, rowHeight),
                    "Lobby Type"
                );

                string[] moddedOptions =
                {
                    "All",
                    "Vanilla",
                    "Modded"
                };

                moddedFilter = (ModdedFilter)GUI.Toolbar(
                    new Rect(toolbarX, rowY, toolbarWidth, rowHeight),
                    (int)moddedFilter,
                    moddedOptions
                );
            }
        }

        static void DrawLobbyRow(SessionInfo lobby, int index)
        {
            Color defaultColor = GUI.color;

            if (IsModded(lobby)) GUI.color = Color.yellow;

            float y = index * RowHeight;

            GUI.Box(new Rect(0, y, WindowWidth - 20f, RowHeight - 5f), GUIContent.none);

            GUI.Label(
                new Rect(10f, y + 5f, 220f, RowHeight),
                lobby.Name
            );

            GUI.Label(
                new Rect(240f, y + 5f, 180f, RowHeight),
                GetLobbyDetails(lobby)
            );

            if (GUI.Button(
                new Rect(WindowWidth - 120f, y + 5f, 90f, RowHeight - 10f),
                "Join"
            ))
            {
                JoinLobby(lobby);
            }

            if (IsModded(lobby)) GUI.color = defaultColor;
        }

        static int GetPlayersOnline()
        {
            int result = 0;

            foreach (SessionInfo lobby in sessions)
            {
                result += lobby.PlayerCount;
            }

            return result;
        }
        static string GetLobbyDetails(SessionInfo lobby)
        {
            return $"{lobby.PlayerCount}/{lobby.MaxPlayers} | {GetMap(lobby)} | {GetLobbyState(lobby)} | {GetVCState(lobby)} | {GetGamemodeFormatted(lobby)}";
        }

        static void JoinLobby(SessionInfo lobby)
        {
            Debug.Log("Joining lobby: " + lobby.Name);
            inBrowseMenu = false;
            peer._screenFadePlayerLoop.FadeOut(1);

            peer.Runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = lobby.Name,
                SceneManager = peer.Runner.gameObject.AddComponent<NetworkSceneManagerDefault>().Cast<INetworkSceneManager>()
            });
        }

        static GUIStyle CenteredLabelStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        static bool IsModded(SessionInfo lobby)
        {
            return lobby.Properties.ContainsKey("modded");
        }

        static bool IsPublic(SessionInfo lobby)
        {
            if (IsModded(lobby))
            {
                if (lobby.Properties.ContainsKey("modded_private"))
                {
                    return lobby.Properties["modded_private"].PropertyValue.Unbox<int>() == 0;
                }
            }

            return lobby.Properties["private"].PropertyValue.Unbox<int>() == 0;
        }

        static bool IsInProgress(SessionInfo lobby)
        {
            return lobby.Properties["game_in_progress"].PropertyValue.Unbox<int>() == 1;
        }

        static string GetLobbyState(SessionInfo lobby)
        {
            if (lobby.Properties["game_in_progress"].PropertyValue.Unbox<int>() == 1)
            {
                return "In-Game";
            }

            return "In-Lobby";
        }

        static bool IsVoiceChatEnabled(SessionInfo lobby)
        {
            return lobby.Properties["voice_chat"].PropertyValue.Unbox<int>() == 1;
        }

        static string GetVCState(SessionInfo lobby)
        {
            if (lobby.Properties["voice_chat"].PropertyValue.Unbox<int>() == 1)
            {
                return "Voice Chat";
            }

            return "Quick Chat";
        }

        static string GetMap(SessionInfo lobby)
        {
            return lobby.Properties["mapname"].PropertyValue.ToString();
        }

        static string GetGamemodeFormatted(SessionInfo lobby)
        {
            GameModes mode = (GameModes)lobby.Properties["gamemode"].PropertyValue.Unbox<int>();
            string result = mode.ToString();

            switch (mode)
            {
                case GameModes.Gameplay:
                    result = "Classic";
                    break;
                case GameModes.BuffGhosts:
                    result = "Wraith";
                    break;
            }

            return result;
        }

        static GameModes GetGamemode(SessionInfo lobby)
        {
            return (GameModes)lobby.Properties["gamemode"].PropertyValue.Unbox<int>();
        }
    }
}
