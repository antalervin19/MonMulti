using HarmonyLib;
using MelonLoader;
using MonMulti.Networking;
using NWH.VehiclePhysics2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UniStorm;
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

        [HarmonyPatch(typeof(Gameplay))]
        [HarmonyPatch("Death")]
        [HarmonyPatch(MethodType.Enumerator)]
        class DeathPatch
        {
            static MethodInfo MultiplayerActiveGetter = AccessTools.PropertyGetter(typeof(Steam), nameof(Steam.GetState));
            static MethodInfo SkipIfTrue = AccessTools.Method(typeof(DeathPatch), nameof(SkipTimeAdvanceIfMultiplayerDeath));

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = instructions.ToList();
                var addOneSecond = AccessTools.Method(typeof(UniStormSystem), "AddOneSecond");

                int i = 0;
                while (i < codes.Count)
                {
                    var code = codes[i];

                    if (code.Calls(addOneSecond))
                    {
                        yield return new CodeInstruction(OpCodes.Call, SkipIfTrue);
                        i++;
                        continue;
                    }

                    yield return code;
                    i++;
                }
            }

            public static void SkipTimeAdvanceIfMultiplayerDeath(UniStormSystem system)
            {
                if (Steam.GetState())
                    return;

                system.AddOneSecond();
            }
        }
    }
}