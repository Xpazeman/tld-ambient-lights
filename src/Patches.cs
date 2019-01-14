using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace AmbientLights
{
    class Patches
    {

        /****** AmbientLights Init ******/

        [HarmonyPatch(typeof(GameManager), "Awake")]
        internal class GameManager_Awake
        {
            public static void Prefix()
            {
                if (!InterfaceManager.IsMainMenuActive())
                {
                    AmbientLights.Reset();
                }
            }
        }

        /****** Load & Unload scene ******/

        [HarmonyPatch(typeof(MissionServicesManager), "SceneLoadCompleted")]
        internal class MissionServicesManager_SceneLoadCompleted
        {
            private static void Postfix(MissionServicesManager __instance)
            {
                AmbientLights.LoadConfigs();
            }
        }

        [HarmonyPatch(typeof(MissionServicesManager), "SceneUnloaded")]
        internal class MissionServicesManager_SceneUnloaded
        {
            private static void Postfix(MissionServicesManager __instance)
            {
                AmbientLights.Unload();
            }
        }

        /****** Initial Time & Weather setup ******/

        [HarmonyPatch(typeof(TimeOfDay), "Deserialize")]
        internal class TimeOfDay_Deserialize
        {
            public static void Postfix(TimeOfDay __instance)
            {
                ALUtils.hourNow = GameManager.GetTimeOfDayComponent().GetHour();
                ALUtils.minuteNow = GameManager.GetTimeOfDayComponent().GetMinutes();

                Debug.Log("[ambient-lights] Initialized at: " + ALUtils.hourNow + ":" + ALUtils.minuteNow);

                AmbientLights.timeInit = true;
                AmbientLights.MaybeUpdateLightsToPeriod(true);
            }
        }

        [HarmonyPatch(typeof(Weather), "Deserialize")]
        internal class Weather_Deserialize
        {
            public static void Postfix(Weather __instance)
            {
                AmbientLights.weatherInit = true;
                AmbientLights.MaybeUpdateLightsToPeriod(true);
            }
        }

        /****** Main Update ******/

        [HarmonyPatch(typeof(TimeOfDay), "Update")]
        internal class TimeOfDay_Update
        {
            public static void Postfix(TimeOfDay __instance)
            {
                AmbientLights.Update();
            }
        }
    }
}
