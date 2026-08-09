using System.IO;
using Steamworks;
using Google.Protobuf;
using MonMulti.Networking.Proto;
using UnityEngine;

namespace MonMulti.Networking
{
    public static class PacketReceiver
    {
        private static bool initialized;

        public static void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
        }

        public static void Update()
        {
            while (SteamNetworking.IsP2PPacketAvailable(out uint size))
            {
                byte[] buffer = new byte[size];

                CSteamID sender;


                if (SteamNetworking.ReadP2PPacket(buffer, size, out uint bytesRead, out sender))
                {
                    HandlePacket(buffer, sender);
                }
            }
        }

        private static void HandlePacket(byte[] data, CSteamID sender)
        {
            PacketType type = (PacketType)data[0];


            switch (type)
            {
                case PacketType.Ready:

                    Core.Logger.Msg($"Player {sender} is READY!");

                    if (Steam.IsHost())
                    {
                        CSteamID hostId = SteamUser.GetSteamID();

                        PacketSender.SendSpawnPlayer(sender, hostId);
                        PlayerManager.SpawnPlayer(sender);

                        foreach (CSteamID member in Steam.GetLobbyMembers())
                        {
                            if (member == hostId || member == sender)
                                continue;

                            PacketSender.SendSpawnPlayer(sender, member);
                            PacketSender.SendSpawnPlayer(member, sender);
                        }

                        TimeSync.SendTimeTo(sender);
                        MoneySync.SendFullSyncTo(sender);
                        ObjectSync.SendFullSyncTo(sender);
                    }

                    break;


                case PacketType.SpawnPlayer:

                    using (MemoryStream stream = new MemoryStream(data))
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        reader.ReadByte();

                        ulong steamId = reader.ReadUInt64();

                        PlayerManager.SpawnPlayer(
                            new CSteamID(steamId)
                        );
                    }

                    break;


                case PacketType.TransformSync:

                    ByteString payload = ByteString.CopyFrom(data, 1, data.Length - 1);
                    PlayerTransform msg = PlayerTransform.Parser.ParseFrom(payload);

                    PlayerManager.UpdateRemoteTransform(
                        new CSteamID(msg.SteamId),
                        new Vector3(msg.PosX, msg.PosY, msg.PosZ),
                        new Quaternion(msg.RotX, msg.RotY, msg.RotZ, msg.RotW)
                    );

                    break;


                case PacketType.TimeSync:

                    TimeSync.ApplyReceivedTime(data);

                    break;

                case PacketType.MoneyDelta:
                    MoneySync.ApplyReceivedDelta(data);
                    break;

                case PacketType.MoneyFullSync:
                    MoneySync.ApplyFullSync(data);
                    break;

                case PacketType.VehicleClaimRequest:
                    VehicleSync.HandleClaimRequest(data, sender);
                    break;

                case PacketType.VehicleClaimGranted:
                    VehicleSync.HandleClaimGranted(data);
                    break;

                case PacketType.VehicleRelease:
                    if (Steam.IsHost())
                        VehicleSync.HandleReleaseRequest(data);
                    else
                        VehicleSync.HandleReleaseBroadcast(data);
                    break;

                case PacketType.VehicleTransformSync:
                    VehicleSync.ApplyTransform(data);
                    break;

                case PacketType.ObjectStateSync:
                    ObjectSync.HandleStatePacket(data);
                    break;
            }
        }
    }
}