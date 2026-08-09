using Google.Protobuf;
using MonMulti.Networking.Proto;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MonMulti.Networking
{
    public static class PacketSender
    {
        public static void BroadcastTransform(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
            var msg = new PlayerTransform
            {
                SteamId = SteamUser.GetSteamID().m_SteamID,
                PosX = position.x,
                PosY = position.y,
                PosZ = position.z,
                RotX = rotation.x,
                RotY = rotation.y,
                RotZ = rotation.z,
                RotW = rotation.w
            };

            byte[] payload = msg.ToByteArray();

            byte[] data = new byte[payload.Length + 1];
            data[0] = (byte)PacketType.TransformSync;
            Array.Copy(payload, 0, data, 1, payload.Length);

            CSteamID self = SteamUser.GetSteamID();

            foreach (CSteamID member in Steam.GetLobbyMembers())
            {
                if (member == self)
                    continue;

                SteamNetworking.SendP2PPacket(
                    member,
                    data,
                    (uint)data.Length,
                    EP2PSend.k_EP2PSendUnreliableNoDelay
                );
            }
        }

        public static void SendReady()
        {
            byte[] data =
            {
                (byte)PacketType.Ready
            };

            CSteamID target = Steam.GetHostID();

            bool sent = SteamNetworking.SendP2PPacket(
                target,
                data,
                (uint)data.Length,
                EP2PSend.k_EP2PSendReliable
            );

            Core.Logger.Msg($"SendP2PPacket(Ready) -> target={target} valid={target.IsValid()} result={sent}");
        }

        public static void SendSpawnPlayer(CSteamID target, CSteamID player)
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((byte)PacketType.SpawnPlayer);
            writer.Write(player.m_SteamID);


            byte[] data = stream.ToArray();


            bool sent = SteamNetworking.SendP2PPacket(
                target,
                data,
                (uint)data.Length,
                EP2PSend.k_EP2PSendReliable
            );


            Core.Logger.Msg($"Sent SpawnPlayer {player} -> {target} (valid={target.IsValid()}, result={sent})");
        }
    }
}