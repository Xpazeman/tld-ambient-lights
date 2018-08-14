using System;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace AmbientLights
{
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

    [HarmonyPatch(typeof(TimeOfDay), "Update")]
    internal class TimeOfDay_Update
    {
        public static void Postfix(TimeOfDay __instance)
        {
            UniStormWeatherSystem gTime = __instance.m_WeatherSystem;

            if (AmbientLightUtils.minute_now != __instance.GetMinutes())
            {
                AmbientLightUtils.hour_now = __instance.GetHour();
                AmbientLightUtils.minute_now = __instance.GetMinutes();

                AmbientLightControl.MaybeUpdateLightsToPeriod();
            }

            if (Input.GetKeyUp(KeyCode.F4))
            {
                AmbientLightUtils.GetPoint();
            }

            if (Input.GetKeyUp(KeyCode.F7))
            {
                if (AmbientLightControl.light_override)
                {
                    AmbientLightControl.light_override = false;
                    AmbientLightControl.MaybeUpdateLightsToPeriod(true);
                }
                else
                {
                    AmbientLightControl.light_override = true;
                    AmbientLightControl.SetLightsIntensity(0f);
                }
            }
        }
    }
}