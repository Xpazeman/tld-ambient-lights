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

                GameLights.GetWindows();
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
                if(!GameManager.m_IsPaused)
                    AmbientLights.Update();
            }
        }

        /****** Game Lights ******/
        
        [HarmonyPatch(typeof(InteriorLightingManager), "Initialize")]
        internal class InteriorLightingManager_Initialize
        {
            private static void Postfix(InteriorLightingManager __instance)
            {
                GameLights.AddGameLights(__instance);
            }
        }

        [HarmonyPatch(typeof(InteriorLightingManager), "Update")]
        internal class InteriorLightingManager_Update
        {
            private static void Prefix(InteriorLightingManager __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null)
                    GameLights.ResetLooseLights();

            }

            private static void Postfix(InteriorLightingManager __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null)
                    AmbientLights.UpdateGameLights();
                
            }
        }

        [HarmonyPatch(typeof(TodAmbientLight), "SetAmbientLightValue", new Type[] { typeof(float), typeof(float) })]
        internal class TodAmbientLight_SetAmbientLightValue
        {
            private static bool Prefix(TodAmbientLight __instance, ref float multiplier)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null)
                    GameLights.UpdateAmbience(__instance, ref multiplier);

                return true;
            }
        }

        /****** LightShafts ******/
        [HarmonyPatch(typeof(LightShaftGimble), "Start")]
        internal class LightShaftGimble_Start
        {
            private static void Postfix(LightShaftGimble __instance)
            {
                if (__instance.m_Light)
                {
                    GameLights.gameSpotLightsList.Add(__instance.m_Light);
                }

                GameLights.gameShaftsList.Add(__instance.gameObject.GetComponentsInChildren<Renderer>());
            }
        }

        [HarmonyPatch(typeof(LightShaftTod), "Start")]
        internal class LightShaftTod_Start
        {
            private static void Postfix(LightShaftTod __instance)
            {
                if (__instance.m_Light)
                {
                    GameLights.gameSpotLightsList.Add(__instance.m_Light);
                }
            }
        }

        [HarmonyPatch(typeof(LightShaftGimble), "UpdateLight")]
        internal class LightShaftGimble_UpdateLight
        {
            private static void Postfix(LightShaftGimble __instance)
            {
                GameLights.UpdateLightshafts();
            }
        }

        [HarmonyPatch(typeof(LightShaftTod), "UpdateLight")]
        internal class LightShaftTod_UpdateLight
        {
            private static void Postfix(LightShaftTod __instance)
            {
                GameLights.UpdateLightshafts();
            }
        }
    }
}
