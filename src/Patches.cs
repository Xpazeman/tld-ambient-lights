using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace AmbientLights
{

    /* Lights Load & Unload */

    [HarmonyPatch(typeof(GameManager), "Awake")]
    internal class GameManager_Awake
    {
        public static void Prefix()
        {
            if (!InterfaceManager.IsMainMenuActive())
            {
                AmbientLightControl.ResetAmbientLights();
            }
        }
    }

    [HarmonyPatch(typeof(MissionServicesManager), "SceneUnloaded")]
    internal class MissionServicesManager_SceneUnloaded
    {
        private static void Postfix(MissionServicesManager __instance)
        {
            AmbientLightControl.RemoveLights();
        }
    }

    [HarmonyPatch(typeof(MissionServicesManager), "SceneLoadCompleted")]
    internal class MissionServicesManager_SceneLoadCompleted
    {
        private static void Postfix(MissionServicesManager __instance)
        {
            AmbientLightControl.RegisterLights();
        }
    }



    /* Time & Weather Control */

    [HarmonyPatch(typeof(TimeOfDay), "Deserialize")]
    internal class TimeOfDay_Deserialize
    {
        public static void Postfix(TimeOfDay __instance)
        {
            AmbientLightUtils.hour_now = GameManager.GetTimeOfDayComponent().GetHour();
            AmbientLightUtils.minute_now = GameManager.GetTimeOfDayComponent().GetMinutes();

            Debug.Log("[ambient-lights] Initialized at: " + AmbientLightUtils.hour_now + ":" + AmbientLightUtils.minute_now);

            AmbientLightControl.scene_time_init = true;
            AmbientLightControl.MaybeUpdateLightsToPeriod(true);
        }
    }

    [HarmonyPatch(typeof(TimeOfDay), "Update")]
    internal class TimeOfDay_Update
    {
        public static void Postfix(TimeOfDay __instance)
        {
            AmbientLightControl.Update();
        }
    }

    [HarmonyPatch(typeof(Weather), "Deserialize")]
    internal class Weather_Deserialize
    {
        public static void Postfix(Weather __instance)
        {
            AmbientLightControl.scene_weather_init = true;
            AmbientLightControl.MaybeUpdateLightsToPeriod(true);
        }
    }

    /* Aurora Lights Patches */

    [HarmonyPatch(typeof(AuroraElectrolizer), "Initialize")]
    internal class AuroraElectrolizer_Initialize
    {
        private static void Postfix(AuroraElectrolizer __instance)
        {
            AuroraLightsControl.LoadAuroraLight(__instance);
        }
    }

    [HarmonyPatch(typeof(AuroraElectrolizer), "UpdateIntensity")]
    internal class AuroraElectrolizer_UpdateIntensity
    {
        private static bool Prefix(AuroraElectrolizer __instance)
        {
            if (AmbientLightsOptions.disable_aurora_flicker)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(AuroraElectrolizer), "UpdateLight", new Type[] { typeof(bool) })]
    internal class AuroraElectrolizer_UpdateLight
    {
        private static void Postfix(AuroraElectrolizer __instance, ref bool allOff)
        {
            if (!allOff)
            {
                AuroraLightsControl.UpdateAuroraLight(__instance);
            }
        }
    }
}