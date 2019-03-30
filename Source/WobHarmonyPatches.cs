using System;
using Verse;
using RimWorld;
using Harmony;

namespace WobCapacityLimits {
    // Class to contain values from settings that are visible to harmony patch code
    public static class SettingsContainer {
        // Static values used by the harmony patches
        public static float pawnMassCapacityValue = 35f;
        public static float podFuelPerTileValue = 2.25f;
    }

    [HarmonyPatch( typeof( MassUtility ) )]
    [HarmonyPatch( "Capacity" )]
    public static class Patch_MassUtility_Capacity {
        [HarmonyPostfix]
        public static void Post_Capacity( ref float __result ) {
            // Base game function: float num = p.BodySize * 35f;
            __result *= SettingsContainer.pawnMassCapacityValue / 35f;
        }
    }


    [HarmonyPatch( typeof( CompLaunchable ), "MaxLaunchDistanceAtFuelLevel", new Type[] { typeof( float ) } )]
    public static class Patch_CompLaunchable_MaxLaunchDistanceAtFuelLevel {
        [HarmonyPrefix]
        public static bool Pre_MaxLaunchDistanceAtFuelLevel( ref float fuelLevel ) {
            fuelLevel *= 2.25f / SettingsContainer.podFuelPerTileValue;
            // Base game function: return Mathf.FloorToInt(fuelLevel / 2.25f);
            return true;
        }
    }

    [HarmonyPatch( typeof( CompLaunchable ), "FuelNeededToLaunchAtDist", new Type[] { typeof( float ) } )]
    public static class Patch_CompLaunchable_FuelNeededToLaunchAtDist {
        [HarmonyPrefix]
        public static bool Pre_FuelNeededToLaunchAtDist( ref float dist ) {
            dist *= SettingsContainer.podFuelPerTileValue / 2.25f;
            // Base game function: return 2.25f * dist;
            return true;
        }
    }
}
