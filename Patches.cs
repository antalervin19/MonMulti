using HarmonyLib;
using MelonLoader;
using MonMulti.Networking;
using NWH.VehiclePhysics2;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonMulti
{
    internal class Patches
    {
        public static void Initialize()
        {
            var harmony = new HarmonyLib.Harmony("antalervin19.MonMulti");

            harmony.PatchAll();

            Core.Logger.Msg("Harmony patches loaded!");
        }


        [HarmonyPatch(typeof(Gameplay))]
        [HarmonyPatch("PauseGame")]
        class PauseGamePatch
        {
            static bool Prefix() => SceneManager.GetActiveScene().name != "Master";
        }

        [HarmonyPatch(typeof(Cash_Manager))]
        [HarmonyPatch("Event_Spent")]
        class EventSpentPatch
        {
            static void Postfix(float value, bool sound)
            {
                if (!Steam.GetState())
                    return;

                if (MoneySync.ApplyingRemoteChange)
                    return;

                MoneySync.BroadcastDelta(-value);
            }
        }

        [HarmonyPatch(typeof(Cash_Manager))]
        [HarmonyPatch("Event_Received")]
        class EventReceivedPatch
        {
            static void Postfix(float value)
            {
                if (!Steam.GetState())
                    return;

                if (MoneySync.ApplyingRemoteChange)
                    return;

                MoneySync.BroadcastDelta(value);
            }
        }

        [HarmonyPatch(typeof(VehicleController))]
        [HarmonyPatch("SetInputToPlayer")]
        class VehicleSetInputPatch
        {
            static void Postfix(VehicleController __instance, bool _active)
            {
                if (!Steam.GetState())
                    return;

                if (VehicleSync.SuppressInputEvents)
                    return;

                if (_active)
                    VehicleSync.OnLocalPlayerEnteredVehicle(__instance);
                else
                    VehicleSync.OnLocalPlayerExitedVehicle(__instance);
            }
        }
    }
}