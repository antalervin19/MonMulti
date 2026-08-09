using System.Collections.Generic;
using System.IO;
using Steamworks;
using UnityEngine;

namespace MonMulti.Networking
{
    public enum SyncCategory : byte
    {
        Door = 0,
        DetachablePart = 1,
        HouseholdItem = 2,
    }

    public class SyncedInteractable
    {
        public byte Id;
        public SyncCategory Category;
        public string ScenePath;
    }

    public static class InteractableRegistry
    {
        public static readonly List<SyncedInteractable> Interactables = new()
        {
            // House
            new SyncedInteractable { Id = 0, Category = SyncCategory.Door, ScenePath = "Home/Home/FrontDoor" },
            new SyncedInteractable { Id = 1, Category = SyncCategory.Door, ScenePath = "Home/Home/Refrigerator_Door_Top" },
            new SyncedInteractable { Id = 2, Category = SyncCategory.Door, ScenePath = "Home/Home/Refrigerator_Door_Bottom" },
            new SyncedInteractable { Id = 3, Category = SyncCategory.Door, ScenePath = "Home/Home/BathroomDoor_Broken" },
            new SyncedInteractable { Id = 4, Category = SyncCategory.Door, ScenePath = "Home/Home/BedroomDoor" },
            new SyncedInteractable { Id = 5, Category = SyncCategory.Door, ScenePath = "Home/Home/BedChestLid" },
            new SyncedInteractable { Id = 6, Category = SyncCategory.Door, ScenePath = "Home/Home/BodyOrigin/Bathtub" },
            new SyncedInteractable { Id = 7, Category = SyncCategory.Door, ScenePath = "Home/Shed/Shed/ShedDoor" },
            new SyncedInteractable { Id = 8, Category = SyncCategory.Door, ScenePath = "Home/SugarShack/SugarShack/Door" },
            new SyncedInteractable { Id = 9, Category = SyncCategory.Door, ScenePath = "Home/SugarShack/SugarShack/EvaporatorDoor" },

            // Garage Doors
            new SyncedInteractable { Id = 10, Category = SyncCategory.Door, ScenePath = "Buildings/EntreposagePrinz/DoorParent1/Door1" },
            new SyncedInteractable { Id = 11, Category = SyncCategory.Door, ScenePath = "Home/EntreposageHome/DoorParent1/Door1" },
            new SyncedInteractable { Id = 12, Category = SyncCategory.Door, ScenePath = "Home/EntreposageHome/DoorParent2/Door2" },

            new SyncedInteractable { Id = 13, Category = SyncCategory.Door, ScenePath = "Buildings/EntreposageGarage_SixBayGarage/DoorParent1/Door1" },
            new SyncedInteractable { Id = 14, Category = SyncCategory.Door, ScenePath = "Buildings/EntreposageGarage_SixBayGarage/DoorParent2/Door2" },
            new SyncedInteractable { Id = 15, Category = SyncCategory.Door, ScenePath = "Buildings/EntreposageGarage_SixBayGarage/DoorParent3/Door3" },
            new SyncedInteractable { Id = 16, Category = SyncCategory.Door, ScenePath = "Buildings/EntreposageGarage_SixBayGarage/DoorParent4/Door4" },
            new SyncedInteractable { Id = 17, Category = SyncCategory.Door, ScenePath = "Buildings/EntreposageGarage_SixBayGarage/DoorParent5/Door5" },
            new SyncedInteractable { Id = 18, Category = SyncCategory.Door, ScenePath = "Buildings/EntreposageGarage_SixBayGarage/DoorParent6/Door6" },
            new SyncedInteractable { Id = 19, Category = SyncCategory.Door, ScenePath = "Buildings/Jims-U-Pull/Jims-U-Pull/DoorParent1/Door1" },
            new SyncedInteractable { Id = 20, Category = SyncCategory.Door, ScenePath = "Buildings/Jims-U-Pull/Jims-U-Pull/DoorParent2/Door2" },
            new SyncedInteractable { Id = 21, Category = SyncCategory.Door, ScenePath = "Buildings/Jims-U-Pull/Jims-U-Pull/DoorParent3/Door3" },

            // WindTurbines
            new SyncedInteractable { Id = 22, Category = SyncCategory.Door, ScenePath = "ENVIRONEMENT/WindTurbines/WindTurbine (2)/TopDoor" },
            new SyncedInteractable { Id = 23, Category = SyncCategory.Door, ScenePath = "ENVIRONEMENT/WindTurbines/WindTurbine (2)/Door" },
            new SyncedInteractable { Id = 24, Category = SyncCategory.Door, ScenePath = "ENVIRONEMENT/WindTurbines/WindTurbine (2)/TechToolBox/TechToolBoxDoor" },
            new SyncedInteractable { Id = 25, Category = SyncCategory.Door, ScenePath = "ENVIRONEMENT/WindTurbines/WindTurbine (1)/TopDoor" },
            new SyncedInteractable { Id = 26, Category = SyncCategory.Door, ScenePath = "ENVIRONEMENT/WindTurbines/WindTurbine (1)/Door" },
            new SyncedInteractable { Id = 27, Category = SyncCategory.Door, ScenePath = "ENVIRONEMENT/WindTurbines/WindTurbine (1)/TechToolBox/TechToolBoxDoor" },
            new SyncedInteractable { Id = 28, Category = SyncCategory.Door, ScenePath = "ENVIRONEMENT/WindTurbines/WindTurbine/TopDoor" },
            new SyncedInteractable { Id = 29, Category = SyncCategory.Door, ScenePath = "ENVIRONEMENT/WindTurbines/WindTurbine/Door" },
            new SyncedInteractable { Id = 30, Category = SyncCategory.Door, ScenePath = "ENVIRONEMENT/WindTurbines/WindTurbine/TechToolBox/TechToolBoxDoor" },

            // Cottage
            new SyncedInteractable { Id = 31, Category = SyncCategory.Door, ScenePath = "Cottage/CottageGate/FenceDoor" },
            new SyncedInteractable { Id = 32, Category = SyncCategory.Door, ScenePath = "Cottage/CottageGate/Door" },
            new SyncedInteractable { Id = 33, Category = SyncCategory.Door, ScenePath = "Cottage/CottageGate/InteriorDoor" },
            new SyncedInteractable { Id = 34, Category = SyncCategory.Door, ScenePath = "Cottage/CottageGate/TrapDoor" },
            new SyncedInteractable { Id = 35, Category = SyncCategory.Door, ScenePath = "Cottage/CottageGate/FurnaceDoor" },
            new SyncedInteractable { Id = 36, Category = SyncCategory.Door, ScenePath = "Cottage/Cottage/CompostMachine/CompostLid" },

            // Junkyard
            new SyncedInteractable { Id = 37, Category = SyncCategory.Door, ScenePath = "Buildings/Jims-U-Pull/Jims-U-Pull/DoorFenceRight" },
            new SyncedInteractable { Id = 38, Category = SyncCategory.Door, ScenePath = "Buildings/Jims-U-Pull/Jims-U-Pull/DoorFenceRight/DoorHandle" },
            new SyncedInteractable { Id = 39, Category = SyncCategory.Door, ScenePath = "Buildings/Jims-U-Pull/Jims-U-Pull/DoorFenceLeft" },

            // Speedway
            new SyncedInteractable { Id = 40, Category = SyncCategory.Door, ScenePath = "RaceTrackIsland/Speedway/Speedway/Appartment/GarageDoor" },
            new SyncedInteractable { Id = 41, Category = SyncCategory.Door, ScenePath = "RaceTrackIsland/Speedway/Speedway/Appartment/Appartment_Door" },
            new SyncedInteractable { Id = 42, Category = SyncCategory.Door, ScenePath = "RaceTrackIsland/Speedway/Speedway/EntreposageGarage_DoorParent1/Door1" },
            new SyncedInteractable { Id = 43, Category = SyncCategory.Door, ScenePath = "RaceTrackIsland/Speedway/Speedway/EntreposageGarage_DoorParent2/Door2" },

            // Dealership
            new SyncedInteractable { Id = 44, Category = SyncCategory.Door, ScenePath = "Mainland/Mainland_Dealer/Mainland_Dealer/CarEntrance/CarDoors" },
            new SyncedInteractable { Id = 45, Category = SyncCategory.Door, ScenePath = "Mainland/Mainland_Dealer/Mainland_Dealer/DoorStore" },
            new SyncedInteractable { Id = 46, Category = SyncCategory.Door, ScenePath = "Mainland/Mainland_Dealer/Mainland_Dealer/Door" },

            // OlTruck doors
            new SyncedInteractable { Id = 47, Category = SyncCategory.Door, ScenePath = "OlTruck/OlTruck/Door_Driver" },
            new SyncedInteractable { Id = 48, Category = SyncCategory.Door, ScenePath = "OlTruck/OlTruck/Door_Passenger" },
            new SyncedInteractable { Id = 49, Category = SyncCategory.Door, ScenePath = "OlTruck/OlTruck/Door_Tailgate" },

            // Konig doors
            new SyncedInteractable { Id = 50, Category = SyncCategory.Door, ScenePath = "Konig/Konig/Door_Trunk" },
            new SyncedInteractable { Id = 51, Category = SyncCategory.Door, ScenePath = "Konig/Konig/Door_Hood" },
            new SyncedInteractable { Id = 52, Category = SyncCategory.Door, ScenePath = "Konig/Konig/DoorLeft" },
            new SyncedInteractable { Id = 53, Category = SyncCategory.Door, ScenePath = "Konig/Konig/DoorRight" },

            // Bus doors
            new SyncedInteractable { Id = 54, Category = SyncCategory.Door, ScenePath = "AI/SchoolBus/NWH_SchoolBus/SchoolBus/sidedoor_right"},
            new SyncedInteractable { Id = 55, Category = SyncCategory.Door, ScenePath = "AI/SchoolBus/NWH_SchoolBus/SchoolBus/sidedoor_left"},

            //Kali-Gaz
            new SyncedInteractable { Id = 56, Category = SyncCategory.Door, ScenePath = "Buildings/Kali-Gaz/Kali-Gaz/FrontDoor" },
            new SyncedInteractable { Id = 57, Category = SyncCategory.Door, ScenePath = "Buildings/Kali-Gaz/Kali-Gaz/RefrigeratorDoorRight" },
            new SyncedInteractable { Id = 58, Category = SyncCategory.Door, ScenePath = "Buildings/Kali-Gaz/Kali-Gaz/RefrigeratorDoorLeft" },

            // Restaurant
            new SyncedInteractable { Id = 59, Category = SyncCategory.Door, ScenePath = "Buildings/SnackPizza/SnackPizza/DoorLeft" },
            new SyncedInteractable { Id = 60, Category = SyncCategory.Door, ScenePath = "Buildings/SnackPizza/SnackPizza/DoorRight" },
            new SyncedInteractable { Id = 61, Category = SyncCategory.Door, ScenePath = "Buildings/SnackPizza/SnackPizza/DoorFront" },

            // Post-Office
            new SyncedInteractable { Id = 62, Category = SyncCategory.Door, ScenePath = "Buildings/PostOffice/PostOffice/Door" },
            new SyncedInteractable { Id = 63, Category = SyncCategory.Door, ScenePath = "Buildings/PostOffice/PostOffice/PlayerMailbox" },

            // Bar
            new SyncedInteractable { Id = 64, Category = SyncCategory.Door, ScenePath = "Buildings/Bar/Bar/Door" },

            // Hardware-Store
            new SyncedInteractable { Id = 65, Category = SyncCategory.Door, ScenePath = "Buildings/HardwareStore/HardwareStore/Door_VIPSalon" },
            new SyncedInteractable { Id = 66, Category = SyncCategory.Door, ScenePath = "Buildings/HardwareStore/HardwareStore/Door" },

            // Federation-Shop
            new SyncedInteractable { Id = 67, Category = SyncCategory.Door, ScenePath = "Buildings/FederationSirop/FederationSyrup/Door" },

            // Abandoned Grocery Store
            new SyncedInteractable { Id = 68, Category = SyncCategory.Door, ScenePath = "Buildings/AbandonedGroceryStore/AbandonedGroceryStore/Door" }
        };
    }

    public static class ObjectSync
    {
        private class TrackedInteractable
        {
            public SyncedInteractable Config;
            public GameObject GameObject;
            public Quaternion LastKnownLocalRotation;
            public Vector3 LastKnownLocalPosition;
        }

        private static readonly Dictionary<(SyncCategory, byte), TrackedInteractable> tracked = new();

        private static float pollTimer;
        private const float PollInterval = 0.1f;
        private const float RotationChangeThreshold = 0.5f;
        private const float PositionChangeThreshold = 0.01f;

        public static void Initialize()
        {
            tracked.Clear();

            foreach (var config in InteractableRegistry.Interactables)
            {
                GameObject go = GameObject.Find(config.ScenePath);

                if (go == null)
                {
                    Core.Logger.Error($"ObjectSync: couldn't find '{config.ScenePath}'");
                    continue;
                }

                var key = (config.Category, config.Id);

                if (tracked.ContainsKey(key))
                {
                    Core.Logger.Error($"ObjectSync: duplicate key ({config.Category}, {config.Id}) for '{config.ScenePath}' - skipping");
                    continue;
                }

                tracked[key] = new TrackedInteractable
                {
                    Config = config,
                    GameObject = go,
                    LastKnownLocalRotation = go.transform.localRotation,
                    LastKnownLocalPosition = go.transform.localPosition
                };
            }

            Core.Logger.Msg($"ObjectSync: tracking {tracked.Count} interactable(s)");
        }

        public static void Update()
        {
            if (!Steam.GetState() || tracked.Count == 0)
                return;

            pollTimer += Time.deltaTime;

            if (pollTimer < PollInterval)
                return;

            pollTimer = 0f;

            foreach (var entry in tracked.Values)
            {
                Transform t = entry.GameObject.transform;
                Quaternion currentRotation = t.localRotation;
                Vector3 currentPosition = t.localPosition;

                bool rotationChanged = Quaternion.Angle(currentRotation, entry.LastKnownLocalRotation) >= RotationChangeThreshold;
                bool positionChanged = Vector3.Distance(currentPosition, entry.LastKnownLocalPosition) >= PositionChangeThreshold;

                if (!rotationChanged && !positionChanged)
                    continue;

                entry.LastKnownLocalRotation = currentRotation;
                entry.LastKnownLocalPosition = currentPosition;
                BroadcastState(entry.Config.Category, entry.Config.Id, currentPosition, currentRotation);
            }
        }

        private static void BroadcastState(SyncCategory category, byte id, Vector3 position, Quaternion rotation)
        {
            byte[] data = BuildStatePacket(category, id, position, rotation);
            CSteamID self = SteamUser.GetSteamID();

            foreach (CSteamID member in Steam.GetLobbyMembers())
            {
                if (member == self)
                    continue;

                SteamNetworking.SendP2PPacket(member, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);
            }
        }

        private static byte[] BuildStatePacket(SyncCategory category, byte id, Vector3 position, Quaternion rotation)
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write((byte)PacketType.ObjectStateSync);
            writer.Write((byte)category);
            writer.Write(id);
            writer.Write(position.x);
            writer.Write(position.y);
            writer.Write(position.z);
            writer.Write(rotation.x);
            writer.Write(rotation.y);
            writer.Write(rotation.z);
            writer.Write(rotation.w);

            return stream.ToArray();
        }

        public static void SendFullSyncTo(CSteamID target)
        {
            if (!Steam.IsHost())
                return;

            foreach (var entry in tracked.Values)
            {
                Transform t = entry.GameObject.transform;
                byte[] data = BuildStatePacket(entry.Config.Category, entry.Config.Id, t.localPosition, t.localRotation);

                SteamNetworking.SendP2PPacket(target, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);
            }

            Core.Logger.Msg($"ObjectSync: sent full sync ({tracked.Count} object(s)) to {target}");
        }

        public static void HandleStatePacket(byte[] data)
        {
            using MemoryStream stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream);

            reader.ReadByte();
            SyncCategory category = (SyncCategory)reader.ReadByte();
            byte id = reader.ReadByte();

            Vector3 position = new Vector3(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            Quaternion rotation = new Quaternion(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            if (!tracked.TryGetValue((category, id), out TrackedInteractable entry))
                return;

            entry.GameObject.transform.localPosition = position;
            entry.GameObject.transform.localRotation = rotation;

            entry.LastKnownLocalPosition = position;
            entry.LastKnownLocalRotation = rotation;
        }
    }
}