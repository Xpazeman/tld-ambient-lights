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

    [HarmonyPatch(typeof(AuroraElectrolizer), "UpdateLight", new Type[] { typeof(bool) })]
    internal class AuroraElectrolizer_UpdateLight
    {
        private static void Postfix(AuroraElectrolizer __instance, ref bool allOff)
        {
            for (int i = 0; i < __instance.m_LocalLights.Length; i++)
            {
                float cur_intensity = __instance.m_LocalLights[i].intensity;
                cur_intensity *= AmbientLightsOptions.aurora_intensity;
                __instance.m_LocalLights[i].intensity = cur_intensity;
            }
        }
    }
}