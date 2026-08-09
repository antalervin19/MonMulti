using System;
using System.Collections.Generic;
using System.Text;

namespace MonMulti.Networking
{
    public enum PacketType : byte
    {
        Ready = 1,
        SpawnPlayer = 2,
        TransformSync = 3,
        TimeSync = 4,
        MoneyDelta = 5,
        MoneyFullSync = 6,
        VehicleClaimRequest = 7,
        VehicleClaimGranted = 8,
        VehicleRelease = 9,
        VehicleTransformSync = 10,
        ObjectStateSync = 11
    }
}