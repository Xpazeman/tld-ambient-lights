using System;
using System.Collections.Generic;
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
            AmbientLightControl.Update();
        }
    }

    [HarmonyPatch(typeof(UniStormWeatherSystem), "Update")]
    internal class UniStormWeatherSystem_Update
    {
        public static void Postfix(UniStormWeatherSystem __instance)
        {
            if (AmbientLightControl.scene_time_init)
                return;

            AmbientLightControl.scene_time_init = true;
            AmbientLightControl.MaybeUpdateLightsToPeriod(true);
            //Debug.Log("[ambient-lights] Update at: "+GameManager.GetTimeOfDayComponent().GetHour()+":"+GameManager.GetTimeOfDayComponent().GetMinutes());

            //AmbientLightControl.MaybeUpdateLightsToPeriod();
        }
    }

    [HarmonyPatch(typeof(AuroraElectrolizer), "Initialize")]
    internal class AuroraElectrolizer_Initialize
    {
        private static void Postfix(AuroraElectrolizer __instance)
        {
            for (int i = 0; i < __instance.m_LocalLights.Length; i++)
            {
                float cur_range = __instance.m_LocalLights[i].range;
                cur_range = Math.Max(cur_range, 5f);

                cur_range *= AmbientLightsOptions.aurora_range;
                cur_range = Math.Min(cur_range, 20f);

                __instance.m_LocalLights[i].range = cur_range;

                Debug.Log("[ambient-lights] Range: " + cur_range);
            }
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
            float fade_int = AmbientLightUtils.GetPrivateFieldFloat(__instance, "m_FadeIntensity");
            float aurora_int = AmbientLightUtils.GetPrivateFieldFloat(__instance, "m_AuroraLightFade");
            float static_int = AmbientLightUtils.GetPrivateFieldFloat(__instance, "m_StaticIntensity");

            for (int i = 0; i < __instance.m_LocalLights.Length; i++)
            {

                float cur_intensity = __instance.m_LocalLights[i].intensity;

                if (AmbientLightsOptions.disable_aurora_flicker)
                {
                    cur_intensity = aurora_int * static_int;
                }

                cur_intensity *= AmbientLightsOptions.aurora_intensity;

                __instance.m_LocalLights[i].intensity = cur_intensity;

                AmbientLightUtils.SetPrivateFieldFloat(__instance, "m_CurIntensity", cur_intensity);
            }
        }
    }
}