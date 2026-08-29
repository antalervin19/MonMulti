using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MonMulti.Networking
{
    public static class PlayerManager
    {
        private static Dictionary<ulong, GameObject> players = new();
        private static readonly HashSet<ulong> seatedPlayers = new();

        private static GameObject localPlayerCache;
        private static Transform localPlayerCameraControlsCache;

        private const int RemotePlayerLayer = 0;

        private static readonly Quaternion RotationOffset = Quaternion.identity;

        public static GameObject GetLocalPlayerObject()
        {
            if (localPlayerCache == null)
                localPlayerCache = GameObject.Find("FirstPersonWalker_Audio");

            return localPlayerCache;
        }

        public static Transform GetLocalPlayerCameraControls()
        {
            if (localPlayerCameraControlsCache == null)
            {
                GameObject local = GetLocalPlayerObject();

                if (local != null)
                    localPlayerCameraControlsCache = local.transform.Find("CameraRoot/CameraControls");
            }

            return localPlayerCameraControlsCache;
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;

            foreach (Transform child in go.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public static void UpdateRemoteTransform(CSteamID id, Vector3 position, Quaternion rotation)
        {
            if (seatedPlayers.Contains(id.m_SteamID))
                return;

            if (!players.TryGetValue(id.m_SteamID, out GameObject remote) || remote == null)
                return;

            if (remote.transform.parent != null)
                return;

            remote.transform.SetPositionAndRotation(position, rotation * RotationOffset);
        }

        public static GameObject GetRemotePlayerObject(CSteamID id)
        {
            players.TryGetValue(id.m_SteamID, out GameObject remote);
            return remote;
        }

        private static readonly string[] DriverSeatChildNames =
        {
            "DriverSeat", "Driver Seat", "SeatDriver", "Seat_Driver"
        };

        public static void SetPlayerVehicleParent(CSteamID id, Transform vehicleTransform)
        {
            if (vehicleTransform == null)
                return;

            if (!players.TryGetValue(id.m_SteamID, out GameObject remote) || remote == null)
                return;

            remote.transform.SetParent(vehicleTransform, false);

            Transform seat = null;

            foreach (string name in DriverSeatChildNames)
            {
                seat = vehicleTransform.Find(name);

                if (seat != null)
                    break;
            }

            if (seat != null)
            {
                remote.transform.localPosition = vehicleTransform.InverseTransformPoint(seat.position);
                remote.transform.localRotation = Quaternion.Inverse(vehicleTransform.rotation) * seat.rotation;
            }
            else
            {
                remote.transform.localPosition = Vector3.zero;
                remote.transform.localRotation = Quaternion.identity;
            }

            seatedPlayers.Add(id.m_SteamID);
        }

        public static void ClearPlayerVehicleParent(CSteamID id)
        {
            seatedPlayers.Remove(id.m_SteamID);

            if (!players.TryGetValue(id.m_SteamID, out GameObject remote) || remote == null)
                return;

            if (remote.transform.parent != null)
            {
                remote.transform.SetParent(null, true);
            }
        }

        public static void SpawnPlayer(CSteamID id)
        {
            if (players.ContainsKey(id.m_SteamID))
                return;

            GameObject template = GameObject.Find("AI/MrBonjour/Actor_MrBonjour");

            if (template == null)
            {
                Core.Logger.Error("MrBonjour template not found!");
                return;
            }

            GameObject remote = UnityEngine.Object.Instantiate(template);
            remote.name = $"RemotePlayer_{id.m_SteamID}";

            var aiScript = remote.GetComponent<Actor_MrBonjour>();
            if (aiScript != null)
                UnityEngine.Object.DestroyImmediate(aiScript);

            var collider = remote.GetComponent<BoxCollider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);

            SetLayerRecursively(remote, RemotePlayerLayer);

            remote.SetActive(true);

            players.Add(id.m_SteamID, remote);

            Core.Logger.Msg($"Spawned remote player {id}");
        }
    }
}