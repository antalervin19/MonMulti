using NWH;
using NWH.VehiclePhysics2;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MonMulti.Networking
{
    public static class VehicleSync
    {
        private class TrackedVehicle
        {
            public SyncedVehicleConfig Config;
            public GameObject GameObject;
            public VehicleController Controller;
            public CSteamID? Driver;

            public bool HasRemoteTarget;
            public Vector3 TargetPosition;
            public Quaternion TargetRotation;
            public Vector3 TargetVelocity;

            public bool IsLocalPlayerSeated;

            public CSteamID? ParentedDriver;
        }

        public static bool SuppressInputEvents { get; private set; }

        private static bool applyingOwnership;

        private static readonly Dictionary<byte, TrackedVehicle> tracked = new();
        private static float syncTimer;
        private const float SyncInterval = 0.05f;

        private const float PositionLerpRate = 15f;
        private const float RotationLerpRate = 15f;

        private const float SnapDistance = 8f;

        private const float PositionExtrapolation = 1f;

        public static void Initialize()
        {
            tracked.Clear();

            foreach (var config in VehicleRegistry.Vehicles)
            {
                GameObject go = GameObject.Find(config.ScenePath);

                if (go == null)
                {
                    Core.Logger.Error($"VehicleSync: couldn't find '{config.ScenePath}'");
                    continue;
                }

                var controller = go.GetComponent<VehicleController>();

                if (controller == null)
                {
                    Core.Logger.Error($"VehicleSync: '{config.ScenePath}' has no VehicleController");
                    continue;
                }

                var entry = new TrackedVehicle
                {
                    Config = config,
                    GameObject = go,
                    Controller = controller,
                    Driver = null
                };

                tracked[config.Id] = entry;

                bool weAreAuthority = Steam.IsHost();
                SetAuthority(entry, weAreAuthority);

                Core.Logger.Msg($"VehicleSync: tracking '{config.ScenePath}' (id {config.Id})");
            }
        }

        private static void SetAuthority(TrackedVehicle entry, bool weAreAuthority)
        {
            entry.Controller.SetMultiplayerInstanceType(
                weAreAuthority ? Vehicle.MultiplayerInstanceType.Local : Vehicle.MultiplayerInstanceType.Remote
            );

            entry.Controller.input.autoSetInput = weAreAuthority;

            if (weAreAuthority)
                entry.Controller.enabled = true;

            Rigidbody rb = entry.Controller.vehicleRigidbody;

            if (rb == null)
                return;

            bool wasKinematic = rb.isKinematic;
            rb.isKinematic = !weAreAuthority;

            if (weAreAuthority && wasKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                foreach (var wheel in entry.GameObject.GetComponentsInChildren<WheelCollider>())
                {
                    wheel.enabled = false;
                    wheel.enabled = true;
                }

                Core.Logger.Msg($"VehicleSync: refreshed wheel colliders for '{entry.Config.ScenePath}' after regaining authority");
            }

            rb.interpolation = weAreAuthority ? RigidbodyInterpolation.None : RigidbodyInterpolation.Interpolate;
        }

        public static void Update()
        {
            if (!Steam.GetState() || tracked.Count == 0)
                return;

            InterpolateRemoteVehicles();

            syncTimer += Time.deltaTime;

            if (syncTimer < SyncInterval)
                return;

            syncTimer = 0f;

            CSteamID self = SteamUser.GetSteamID();

            foreach (var entry in tracked.Values)
            {
                bool weAreAuthority = entry.Driver.HasValue
                    ? entry.Driver.Value == self
                    : Steam.IsHost();

                if (weAreAuthority)
                    BroadcastTransform(entry);
            }
        }

        private static void InterpolateRemoteVehicles()
        {
            CSteamID self = SteamUser.GetSteamID();

            foreach (var entry in tracked.Values)
            {
                if (!entry.HasRemoteTarget)
                    continue;

                bool weAreAuthority = entry.Driver.HasValue
                    ? entry.Driver.Value == self
                    : Steam.IsHost();

                if (weAreAuthority)
                    continue;

                Transform t = entry.GameObject.transform;
                Rigidbody rb = entry.Controller.vehicleRigidbody;

                Vector3 currentPos = rb != null ? rb.position : t.position;
                Quaternion currentRot = rb != null ? rb.rotation : t.rotation;

                Vector3 extrapolatedTarget = entry.TargetPosition + entry.TargetVelocity * SyncInterval * PositionExtrapolation;

                float posT = 1f - Mathf.Exp(-PositionLerpRate * Time.deltaTime);
                float rotT = 1f - Mathf.Exp(-RotationLerpRate * Time.deltaTime);

                Vector3 newPos;
                Quaternion newRot;

                if (Vector3.Distance(currentPos, extrapolatedTarget) > SnapDistance)
                {
                    newPos = entry.TargetPosition;
                    newRot = entry.TargetRotation;
                    posT = 1f;
                }
                else
                {
                    newPos = Vector3.Lerp(currentPos, extrapolatedTarget, posT);
                    newRot = Quaternion.Slerp(currentRot, entry.TargetRotation, rotT);
                }

                if (rb != null)
                {
                    rb.MovePosition(newPos);
                    rb.MoveRotation(newRot);
                }
                else
                {
                    t.SetPositionAndRotation(newPos, newRot);
                }
            }
        }


        public static void OnLocalPlayerEnteredVehicle(VehicleController controller)
        {
            var entry = Find(controller);

            if (entry == null)
                return;

            if (entry.IsLocalPlayerSeated)
                return;

            entry.IsLocalPlayerSeated = true;

            if (Steam.IsHost())
            {
                GrantClaim(entry, SteamUser.GetSteamID());
                return;
            }

            SendClaimRequest(entry.Config.Id);
        }

        public static void OnLocalPlayerExitedVehicle(VehicleController controller)
        {
            var entry = Find(controller);

            if (entry == null)
                return;

            if (!entry.IsLocalPlayerSeated)
                return;

            entry.IsLocalPlayerSeated = false;

            if (entry.Driver != SteamUser.GetSteamID())
                return;

            if (Steam.IsHost())
            {
                ReleaseClaim(entry);
                return;
            }

            SendRelease(entry.Config.Id);
        }

        private static TrackedVehicle Find(VehicleController controller)
        {
            foreach (var entry in tracked.Values)
            {
                if (entry.Controller == controller)
                    return entry;
            }

            return null;
        }


        private static void SendClaimRequest(byte vehicleId)
        {
            byte[] data = { (byte)PacketType.VehicleClaimRequest, vehicleId };

            SteamNetworking.SendP2PPacket(
                Steam.GetHostID(),
                data,
                (uint)data.Length,
                EP2PSend.k_EP2PSendReliable
            );
        }

        public static void HandleClaimRequest(byte[] data, CSteamID sender)
        {
            if (!Steam.IsHost())
                return;

            byte vehicleId = data[1];

            if (!tracked.TryGetValue(vehicleId, out var entry))
                return;

            if (entry.Driver.HasValue)
            {
                Core.Logger.Msg($"VehicleSync: claim denied, '{entry.Config.ScenePath}' already driven");
                return;
            }

            GrantClaim(entry, sender);
        }

        private static void GrantClaim(TrackedVehicle entry, CSteamID driver)
        {
            entry.Driver = driver;
            ApplyOwnership(entry);

            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((byte)PacketType.VehicleClaimGranted);
            writer.Write(entry.Config.Id);
            writer.Write(driver.m_SteamID);

            byte[] data = stream.ToArray();
            CSteamID self = SteamUser.GetSteamID();

            foreach (CSteamID member in Steam.GetLobbyMembers())
            {
                if (member == self)
                    continue;

                SteamNetworking.SendP2PPacket(member, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);
            }

            Core.Logger.Msg($"VehicleSync: granted '{entry.Config.ScenePath}' to {driver}");
        }

        public static void HandleClaimGranted(byte[] data)
        {
            using MemoryStream stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream);

            reader.ReadByte();
            byte vehicleId = reader.ReadByte();
            ulong driverId = reader.ReadUInt64();

            if (!tracked.TryGetValue(vehicleId, out var entry))
                return;

            entry.Driver = new CSteamID(driverId);
            ApplyOwnership(entry);
        }


        private static void SendRelease(byte vehicleId)
        {
            byte[] data = { (byte)PacketType.VehicleRelease, vehicleId };

            SteamNetworking.SendP2PPacket(
                Steam.GetHostID(),
                data,
                (uint)data.Length,
                EP2PSend.k_EP2PSendReliable
            );
        }

        public static void HandleReleaseRequest(byte[] data)
        {
            if (!Steam.IsHost())
                return;

            byte vehicleId = data[1];

            if (tracked.TryGetValue(vehicleId, out var entry))
                ReleaseClaim(entry);
        }

        private static void ReleaseClaim(TrackedVehicle entry)
        {
            entry.Driver = null;
            ApplyOwnership(entry);

            byte[] data = { (byte)PacketType.VehicleRelease, entry.Config.Id };
            CSteamID self = SteamUser.GetSteamID();

            foreach (CSteamID member in Steam.GetLobbyMembers())
            {
                if (member == self)
                    continue;

                SteamNetworking.SendP2PPacket(member, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);
            }

            Core.Logger.Msg($"VehicleSync: released '{entry.Config.ScenePath}', authority back to host");
        }

        public static void HandleReleaseBroadcast(byte[] data)
        {
            byte vehicleId = data[1];

            if (tracked.TryGetValue(vehicleId, out var entry))
            {
                entry.Driver = null;
                ApplyOwnership(entry);
            }
        }

        private static void ApplyOwnership(TrackedVehicle entry)
        {
            if (applyingOwnership)
                return;

            CSteamID self = SteamUser.GetSteamID();

            bool weAreAuthority = entry.Driver.HasValue
                ? entry.Driver.Value == self
                : Steam.IsHost();

            SetAuthority(entry, weAreAuthority);

            applyingOwnership = true;

            try
            {
                if (weAreAuthority && entry.IsLocalPlayerSeated)
                {
                    SuppressInputEvents = true;

                    try
                    {
                        entry.Controller.SetInputToPlayer(true);
                    }
                    finally
                    {
                        SuppressInputEvents = false;
                    }
                }

                if (entry.ParentedDriver.HasValue && entry.ParentedDriver != entry.Driver)
                {
                    PlayerManager.ClearPlayerVehicleParent(entry.ParentedDriver.Value);
                    entry.ParentedDriver = null;
                }

                if (entry.Driver.HasValue && entry.Driver.Value != self && entry.ParentedDriver != entry.Driver)
                {
                    if (PlayerManager.GetRemotePlayerObject(entry.Driver.Value) != null)
                    {
                        PlayerManager.SetPlayerVehicleParent(entry.Driver.Value, entry.GameObject.transform);
                        entry.ParentedDriver = entry.Driver;
                    }
                }
            }
            finally
            {
                applyingOwnership = false;
            }
        }

        private static void BroadcastTransform(TrackedVehicle entry)
        {
            Transform t = entry.GameObject.transform;
            Rigidbody rb = entry.Controller.vehicleRigidbody;

            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((byte)PacketType.VehicleTransformSync);
            writer.Write(entry.Config.Id);

            writer.Write(t.position.x);
            writer.Write(t.position.y);
            writer.Write(t.position.z);

            writer.Write(t.rotation.x);
            writer.Write(t.rotation.y);
            writer.Write(t.rotation.z);
            writer.Write(t.rotation.w);

            Vector3 velocity = rb != null ? rb.velocity : Vector3.zero;
            writer.Write(velocity.x);
            writer.Write(velocity.y);
            writer.Write(velocity.z);

            byte[] data = stream.ToArray();
            CSteamID self = SteamUser.GetSteamID();

            foreach (CSteamID member in Steam.GetLobbyMembers())
            {
                if (member == self)
                    continue;

                SteamNetworking.SendP2PPacket(member, data, (uint)data.Length, EP2PSend.k_EP2PSendUnreliableNoDelay);
            }
        }

        public static void ApplyTransform(byte[] data)
        {
            using MemoryStream stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream);

            reader.ReadByte();
            byte vehicleId = reader.ReadByte();

            Vector3 pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Quaternion rot = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 vel = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            if (!tracked.TryGetValue(vehicleId, out var entry))
                return;

            entry.TargetPosition = pos;
            entry.TargetRotation = rot;
            entry.TargetVelocity = vel;
            entry.HasRemoteTarget = true;
        }
    }
}