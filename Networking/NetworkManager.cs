using System;
using System.Collections.Generic;
using System.Text;

using Steamworks;

namespace MonMulti.Networking
{
    public static class NetworkManager
    {
        private static bool initialized;


        public static void Initialize()
        {
            if (initialized)
                return;


            initialized = true;

            PacketReceiver.Initialize();

            Core.Logger.Msg("NetworkManager initialized!");
        }


        public static void SendReady()
        {
            PacketSender.SendReady();

            Core.Logger.Msg("Sent READY packet!");
        }
    }
}