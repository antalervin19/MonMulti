using System;
using System.Collections.Generic;
using System.Text;

namespace MonMulti.Networking
{
    public class SyncedVehicleConfig
    {
        public byte Id;
        public string ScenePath;
    }

    public static class VehicleRegistry
    {
        public static readonly List<SyncedVehicleConfig> Vehicles = new()
        {
            new SyncedVehicleConfig { Id = 0, ScenePath = "Konig" },
            new SyncedVehicleConfig { Id = 1, ScenePath = "SmollATV" },
            new SyncedVehicleConfig { Id = 2, ScenePath = "OlTruck" },
            new SyncedVehicleConfig { Id = 3, ScenePath = "Buggy" },
            new SyncedVehicleConfig { Id = 4, ScenePath = "Prinz" },
            new SyncedVehicleConfig { Id = 4, ScenePath = "Boat_Fishing" },
            new SyncedVehicleConfig { Id = 5, ScenePath = "AI/MartinCar/NWH_MartinCar" },
            //new SyncedVehicleConfig { Id = 69, ScenePath = "NWH_SchoolBus" }
        };
    }
}
