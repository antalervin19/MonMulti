using MelonLoader;
using MonMulti.Networking;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace MonMulti
{
    public static class Steam
    {
        private static CallResult<LobbyCreated_t> m_LobbyCreated;

        private static Callback<GameLobbyJoinRequested_t> m_LobbyJoinRequested;
        private static Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
        private static Callback<LobbyEnter_t> m_LobbyEntered;
        private static Callback<P2PSessionRequest_t> m_P2PSessionRequest;

        private static bool isMultiplayer = false;
        private static bool isHost = false;

        private static bool pendingNewGame = false;

        private static CSteamID currentLobby;

        public static bool GetState()
        {
            return isMultiplayer;
        }

        public static bool IsHost()
        {
            return isHost;
        }
        public static CSteamID GetHostID()
        {
            return SteamMatchmaking.GetLobbyOwner(currentLobby);
        }

        public static CSteamID GetCurrentLobbyID()
        {
            return currentLobby;
        }

        public static void Initialize()
        {
            try
            {
                if (!SteamAPI.IsSteamRunning())
                {
                    Core.Logger.Error("Steam is not running!");
                    return;
                }


                if (SteamAPI.Init())
                {
                    Core.Logger.Msg("Steam initialized!");

                    m_LobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
                    m_LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
                    m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
                    m_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
                }
            }
            catch (System.Exception ex)
            {
                Core.Logger.Error($"Steam initialization exception: {ex.Message}");
            }
        }

        public static List<CSteamID> GetLobbyMembers()
        {
            List<CSteamID> members = new();

            int count = SteamMatchmaking.GetNumLobbyMembers(currentLobby);

            for (int i = 0; i < count; i++)
            {
                members.Add(
                    SteamMatchmaking.GetLobbyMemberByIndex(currentLobby, i)
                );
            }

            return members;
        }

        public static void StartMultiplayerSession(bool newGame)
        {
            if (isMultiplayer)
                return;


            isMultiplayer = true;
            isHost = true;

            pendingNewGame = newGame;


            try
            {
                if (!SteamAPI.IsSteamRunning())
                {
                    isMultiplayer = false;
                    return;
                }


                m_LobbyCreated = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);


                SteamAPICall_t call = SteamMatchmaking.CreateLobby(
                    ELobbyType.k_ELobbyTypePublic,
                    4
                );


                m_LobbyCreated.Set(call);

                Core.Logger.Msg("Creating Steam lobby...");
            }
            catch (System.Exception ex)
            {
                Core.Logger.Error($"Failed to create lobby: {ex.Message}");
                isMultiplayer = false;
            }
        }

        private static void OnLobbyCreated(LobbyCreated_t callback, bool ioFailure)
        {
            if (ioFailure || callback.m_eResult != EResult.k_EResultOK)
            {
                Core.Logger.Error($"Lobby creation failed: {callback.m_eResult}");

                isMultiplayer = false;
                return;
            }

            currentLobby = new CSteamID(callback.m_ulSteamIDLobby);

            Core.Logger.Msg($"Lobby created! ID: {currentLobby}");

            SteamMatchmaking.SetLobbyData(currentLobby, "host", SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(currentLobby, "state", "loading");
            SteamMatchmaking.SetLobbyData(currentLobby, "gamemode", pendingNewGame ? "new" : "continue");

            StartHostGame(pendingNewGame);
        }

        private static void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Core.Logger.Msg($"Lobby invite received: {callback.m_steamIDLobby}");

            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        private static void OnLobbyEntered(LobbyEnter_t callback)
        {
            currentLobby = new CSteamID(callback.m_ulSteamIDLobby);

            isMultiplayer = true;

            Core.Logger.Msg($"Entered lobby: {currentLobby}");

            NetworkManager.Initialize();

            int members = SteamMatchmaking.GetNumLobbyMembers(currentLobby);

            Core.Logger.Msg($"Lobby members: {members}");

            CSteamID owner = SteamMatchmaking.GetLobbyOwner(currentLobby);

            if (owner == SteamUser.GetSteamID())
            {
                isHost = true;
                Core.Logger.Msg("I am the lobby host! MU HAHAHA");
            }
            else
            {
                isHost = false;

                Core.Logger.Msg($"Connected to host: {owner}");

                JoinHostGame();
            }
        }

        private static void OnP2PSessionRequest(P2PSessionRequest_t callback)
        {
            CSteamID requester = callback.m_steamIDRemote;

            Core.Logger.Msg($"P2P session requested by {requester}, accepting.");

            SteamNetworking.AcceptP2PSessionWithUser(requester);
        }

        private static void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            if (callback.m_ulSteamIDLobby != currentLobby.m_SteamID)
                return;

            CSteamID changedPlayer = new CSteamID(callback.m_ulSteamIDUserChanged);

            Core.Logger.Msg($"Lobby player changed: {changedPlayer}");

            int members = SteamMatchmaking.GetNumLobbyMembers(currentLobby);

            Core.Logger.Msg($"New Lobby member count: {members}");

            for (int i = 0; i < members; i++)
            {
                Core.Logger.Msg(
                    $"Member {i}: {SteamMatchmaking.GetLobbyMemberByIndex(currentLobby, i)}"
                );
            }
        }

        private static void StartHostGame(bool newGame)
        {
            MainMenu_Manager menu = UnityEngine.Object.FindObjectOfType<MainMenu_Manager>();

            if (menu == null)
            {
                Core.Logger.Error("MainMenu_Manager not found!");
                return;
            }


            if (newGame)
            {
                PlayerPrefs.SetInt("SaveTimestamp", -1);
                PlayerPrefs.SetString("SaveName", "MultiplayerWorld");
                PlayerPrefs.SetInt("HardMode", 0);
                PlayerPrefs.SetInt("Permadeath", 0);

                menu.StartGame(true);
            }
            else
            {
                menu.Button_Continue();
            }
        }

        private static void JoinHostGame()
        {
            Core.Logger.Msg("Starting multiplayer client...");

            PlayerPrefs.SetInt("SaveTimestamp", -1);
            PlayerPrefs.SetString("SaveName", $"MonMulti_{SteamUser.GetSteamID()}");
            PlayerPrefs.SetInt("HardMode", 0);
            PlayerPrefs.SetInt("Permadeath", 0);

            MainMenu_Manager menu = UnityEngine.Object.FindObjectOfType<MainMenu_Manager>();

            if (menu == null)
            {
                Core.Logger.Error("MainMenu_Manager not found!");
                return;
            }

            menu.StartGame(true);
        }
    }
}