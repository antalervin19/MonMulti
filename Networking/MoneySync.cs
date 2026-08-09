using System.IO;
using Steamworks;
using UnityEngine;

namespace MonMulti.Networking
{
    public static class MoneySync
    {
        public static bool ApplyingRemoteChange { get; private set; }

        private static float lastBroadcastDelta;
        private static int lastBroadcastFrame = -1;

        private static byte[] BuildDeltaPacket(float delta)
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((byte)PacketType.MoneyDelta);
            writer.Write(delta);

            return stream.ToArray();
        }

        public static void BroadcastDelta(float delta)
        {
            if (!Steam.GetState())
                return;

            int frame = Time.frameCount;

            if (Mathf.Approximately(delta, lastBroadcastDelta) && frame == lastBroadcastFrame)
            {
                Core.Logger.Msg($"Skipped duplicate money delta broadcast: {delta}");
                return;
            }

            lastBroadcastDelta = delta;
            lastBroadcastFrame = frame;

            byte[] data = BuildDeltaPacket(delta);
            CSteamID self = SteamUser.GetSteamID();

            foreach (CSteamID member in Steam.GetLobbyMembers())
            {
                if (member == self)
                    continue;

                SteamNetworking.SendP2PPacket(
                    member,
                    data,
                    (uint)data.Length,
                    EP2PSend.k_EP2PSendReliable
                );
            }

            Core.Logger.Msg($"Broadcast money delta: {delta}");
        }

        public static void ApplyReceivedDelta(byte[] data)
        {
            using MemoryStream stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream);

            reader.ReadByte();

            float delta = reader.ReadSingle();

            ApplyingRemoteChange = true;

            try
            {
                if (delta >= 0f)
                    Singleton<Cash_Manager>.i.Event_Received(delta);
                else
                    Singleton<Cash_Manager>.i.Event_Spent(-delta, false);
            }
            finally
            {
                ApplyingRemoteChange = false;
            }

            Core.Logger.Msg($"Applied money delta: {delta}");
        }


        private static byte[] BuildFullSyncPacket(int cashCents)
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((byte)PacketType.MoneyFullSync);
            writer.Write(cashCents);

            return stream.ToArray();
        }

        public static void SendFullSyncTo(CSteamID target)
        {
            if (!Steam.IsHost())
                return;

            int cashCents = Singleton<Cash_Manager>.i.GetCash;

            byte[] data = BuildFullSyncPacket(cashCents);

            SteamNetworking.SendP2PPacket(
                target,
                data,
                (uint)data.Length,
                EP2PSend.k_EP2PSendReliable
            );

            Core.Logger.Msg($"Sent full money sync ({cashCents}) to {target}");
        }

        public static void ApplyFullSync(byte[] data)
        {
            using MemoryStream stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream);

            reader.ReadByte();

            int cashCents = reader.ReadInt32();

            ApplyingRemoteChange = true;

            try
            {
                float[] weekCash = Singleton<Cash_Manager>.i.GetWeekCash;
                Singleton<Cash_Manager>.i.LoadSaveSystem(cashCents, weekCash);
            }
            finally
            {
                ApplyingRemoteChange = false;
            }

            Core.Logger.Msg($"Applied full money sync: {cashCents}");
        }
    }
}