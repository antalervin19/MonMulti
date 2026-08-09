using System.IO;
using Steamworks;
using UniStorm;

namespace MonMulti.Networking
{
    public static class TimeSync
    {
        private static bool hostHookInitialized;

        public static void InitializeHost()
        {
            if (hostHookInitialized)
                return;

            UniStormSystem system = UniStormSystem.Instance;

            if (system == null)
            {
                Core.Logger.Error("UniStormSystem.Instance not found");
                return;
            }

            system.OnHourChangeEvent.AddListener(BroadcastTime);
            hostHookInitialized = true;
        }

        private static byte[] BuildTimePacket()
        {
            UniStormSystem system = UniStormSystem.Instance;

            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((byte)PacketType.TimeSync);
            writer.Write(system.Day);
            writer.Write(system.Month);
            writer.Write(system.Year);
            writer.Write(system.Hour);
            writer.Write(system.Minute);

            return stream.ToArray();
        }

        public static void BroadcastTime()
        {
            if (!Steam.IsHost())
                return;

            if (UniStormSystem.Instance == null)
                return;

            byte[] data = BuildTimePacket();
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

            UniStormSystem s = UniStormSystem.Instance;
        }

        public static void SendTimeTo(CSteamID target)
        {
            if (!Steam.IsHost())
                return;

            if (UniStormSystem.Instance == null)
                return;

            byte[] data = BuildTimePacket();

            SteamNetworking.SendP2PPacket(
                target,
                data,
                (uint)data.Length,
                EP2PSend.k_EP2PSendReliable
            );

        }

        public static void ApplyReceivedTime(byte[] data)
        {
            UniStormSystem system = UniStormSystem.Instance;

            if (system == null)
            {
                return;
            }

            using MemoryStream stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream);

            reader.ReadByte();

            int day = reader.ReadInt32();
            int month = reader.ReadInt32();
            int year = reader.ReadInt32();
            int hour = reader.ReadInt32();
            int minute = reader.ReadInt32();

            system.Day = day;
            system.Month = month;
            system.Year = year;

            system.m_TimeFloat = (float)hour / 24f + (float)minute / 1440f;

        }
    }
}