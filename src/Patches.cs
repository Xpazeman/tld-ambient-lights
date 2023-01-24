using System;
using System.Collections.Generic;
//using Il2CppSystem.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Il2Cpp;

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
                if (!InterfaceManager.IsMainMenuEnabled())
                {
                    GameLights.gameLightsList.Clear();
                    GameLights.gameExtraLightsList.Clear();
                    GameLights.gameSpotLightsList.Clear();
                    GameLights.gameExtraLightsColors.Clear();
                    GameLights.gameExtraLightsIntensity.Clear();
                    GameLights.gameShaftsList.Clear();
                    GameLights.gameWindows.Clear();

                    AmbientLights.Reset(true);
                }
            }
        }

        /****** Load & Unload scene ******/



        [HarmonyPatch(typeof(SaveGameSystem), "LoadSceneData", new Type[] { typeof(string), typeof(string) })]
        internal class SaveGameSystem_LoadSceneData
        {
            public static void Postfix(SaveGameSystem __instance, string name, string sceneSaveName)
            {

                //MelonLoader.MelonLogger.Log("[AL] Unload and load");
                AmbientLights.Unload();
                AmbientLights.LoadConfigs();

            }
        }

        [HarmonyPatch(typeof(MissionServicesManager), "SceneUnloaded")]
        internal class MissionServicesManager_SceneUnloaded
        {
            private static void Postfix(MissionServicesManager __instance)
            {
                //MelonLoader.MelonLogger.Log("[AL] SceneUnloaded");
                AmbientLights.Unload();
            }
        }

        /****** Initial Time & Weather setup ******/

        [HarmonyPatch(typeof(TimeOfDay), "Deserialize")]
        //[HarmonyPatch(typeof(TimeOfDay), "Start")]
        internal class TimeOfDay_Start
        {
            public static void Postfix(TimeOfDay __instance)
            {
                ALUtils.hourNow = GameManager.GetTimeOfDayComponent().GetHour();
                ALUtils.minuteNow = GameManager.GetTimeOfDayComponent().GetMinutes();

                //Debug.Log("[ambient-lights] Initialized at: " + ALUtils.hourNow + ":" + ALUtils.minuteNow);
                ALUtils.Log("Initialized at: " + ALUtils.hourNow + ":" + ALUtils.minuteNow, false, true);

                AmbientLights.timeInit = true;
                AmbientLights.MaybeUpdateLightsToPeriod(true);
            }
        }

        //[HarmonyPatch(typeof(UniStormWeatherSystem), "InitializeForScene")]
        //[HarmonyPatch(typeof(UniStormWeatherSystem), "InitializeAfterSceneLoad")]
        [HarmonyPatch(typeof(TimeOfDay), "InstantiateUniStorm")]
        internal class TimeOfDay_InstantiateUniStorm
        {
            public static void Postfix(UniStormWeatherSystem __instance)
            {
                AmbientLights.weatherInit = true;
                AmbientLights.MaybeUpdateLightsToPeriod(true);
            }
        }

        /****** Main Update ******/

        [HarmonyPatch(typeof(GameManager), "Update")]
        internal class GameManager_Update
        {
            public static void Postfix(GameManager __instance)
            {
                //AmbientLights.Update();
                //ALUtils.Log("GameManager Update 1.", false, true);
                if (!InterfaceManager.GetPanel<Panel_PauseMenu>().IsEnabled())
                {
                    //ALUtils.Log("GameManager Update 1.", false, true);
                    AmbientLights.Update();
                }
            }
        }

        /****** Game Lights ******/

        [HarmonyPatch(typeof(InteriorLightingManager), "Initialize")]
        internal class InteriorLightingManager_Initialize
        {
            private static void Postfix(InteriorLightingManager __instance)
            {
                if (GameLights.mngr != null)
                {
                    GameLights.mngr = __instance;
                    GameLights.AddGameLights();
                }
            }
        }

        [HarmonyPatch(typeof(InteriorLightingManager), "Update")]
        internal class InteriorLightingManager_Update
        {

            private static void Postfix(InteriorLightingManager __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                {
                    //AmbientLights.UpdateGameLights();
                    GameLights.UpdateLights();
                }

            }
        }

        [HarmonyPatch(typeof(DarkLightingManager), "Start")]
        internal class DarkLightingManager_Initialize
        {
            private static void Postfix(DarkLightingManager __instance)
            {
                GameLights.darkMngr = __instance;
                GameLights.AddGameLights();
            }
        }

        [HarmonyPatch(typeof(DarkLightingManager), "UpdateConstantLights")]
        internal class DarkLightingManager_UpdateConstantLights
        {
            private static void Postfix(DarkLightingManager __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                    GameLights.UpdateConstantLights();

            }
        }

        [HarmonyPatch(typeof(DarkLightingManager), "UpdateNonTodLights")]
        internal class DarkLightingManager_UpdateNonTodLights
        {
            private static void Postfix(DarkLightingManager __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                    GameLights.UpdateNonTodLights();

            }
        }

        [HarmonyPatch(typeof(DarkLightingManager), "UpdateColouredLights")]
        internal class DarkLightingManager_UpdateColouredLights
        {
            private static void Postfix(DarkLightingManager __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                    GameLights.UpdateColouredLights();

            }
        }

        [HarmonyPatch(typeof(DarkLightingManager), "UpdateGimbles")]
        internal class DarkLightingManager_UpdateGimbles
        {
            private static void Postfix(DarkLightingManager __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                    GameLights.UpdateDarkLightshafts();

            }
        }

        [HarmonyPatch(typeof(DarkLightingManager), "UpdateGlowObjects")]
        internal class DarkLightingManager_UpdateGlowObjects
        {
            private static void Postfix(DarkLightingManager __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                    GameLights.UpdateWindows();

            }
        }

        [HarmonyPatch(typeof(InteriorLightingManager), "UpdateLightShaft", new Type[] { typeof(float), typeof(bool) })]
        internal class InteriorLightingManager_UpdateLightShaft
        {
            private static void Prefix(InteriorLightingManager __instance, ref float timeOfDayIntensity, bool followTod)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                {
                    timeOfDayIntensity *= AmbientLights.currentLightSet.lightshaftStr;
                }

            }
        }

        [HarmonyPatch(typeof(LightShaftGimble), "UpdateLight", new Type[] { typeof(float) })]
        internal class LightShaftGimble_GetCombinedIntensity
        {
            private static void Postfix(LightShaftGimble __instance, float tod)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                {
                    if (__instance.m_Light != null)
                        __instance.m_Light.intensity *= AmbientLights.currentLightSet.lightshaftStr;
                }
            }
        }

        [HarmonyPatch(typeof(LightShaftTod), "Update")]
        internal class LightShaftTod_Update
        {
            private static void Postfix(LightShaftGimble __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                {
                    if (__instance.m_Light != null)
                        __instance.m_Light.intensity *= AmbientLights.currentLightSet.lightshaftStr;
                }
            }
        }

        [HarmonyPatch(typeof(LightTOD), "UpdateLights")]
        internal class LightTOD_UpdateLights
        {
            private static void Postfix(LightTOD __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                {
                    if (__instance.m_DayLights != null)
                    {
                        for (int i = 0; i < __instance.m_DayLights.Count; i++)
                        {
                            if (!(__instance.m_DayLights[i] == null))
                            {
                                __instance.m_DayLights[i].intensity = __instance.m_DayLightOriginal[i] * __instance.m_TODMultiplier * AmbientLights.currentLightSet.lightshaftStr;
                            }
                        }
                    }
                    if (__instance.m_NightLights != null)
                    {
                        for (int j = 0; j < __instance.m_NightLights.Count; j++)
                        {
                            if (!(__instance.m_NightLights[j] == null))
                            {
                                __instance.m_NightLights[j].intensity = __instance.m_NightLightOriginal[j] * __instance.m_TONMultiplier * AmbientLights.currentLightSet.lightshaftStr;
                            }
                        }
                    }
                }

            }
        }

        /**/

        [HarmonyPatch(typeof(TodAmbientLight), "SetAmbientLightValue", new Type[] { typeof(float), typeof(float) })]
        internal class TodAmbientLight_SetAmbientLightValue
        {
            private static bool Prefix(TodAmbientLight __instance, ref float multiplier)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
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
                GameLights.gameShaftsList.Add(__instance.gameObject.GetComponentsInChildren<Renderer>());
            }
        }

        [HarmonyPatch(typeof(LightShaftTod), "Start")]
        internal class LightShaftTod_Start
        {
            private static void Postfix(LightShaftTod __instance)
            {
                GameLights.gameShaftsList.Add(__instance.gameObject.GetComponentsInChildren<Renderer>());
            }
        }

        [HarmonyPatch(typeof(LightShaftGimble), "UpdateLight")]
        internal class LightShaftGimble_UpdateLight
        {
            private static void Postfix(LightShaftGimble __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                {
                    GameLights.UpdateLightshafts();

                }
            }
        }

        [HarmonyPatch(typeof(LightShaftTod), "UpdateLight")]
        internal class LightShaftTod_UpdateLight
        {
            private static void Postfix(LightShaftTod __instance)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null && AmbientLights.config.ready && GameLights.gameLightsReady)
                    GameLights.UpdateLightshafts();
            }
        }

        [HarmonyPatch(typeof(InteriorLightingManager), "FindLightGroups")]
        internal class InteriorLightingManager_FindLightGroups
        {
            private static void Postfix(InteriorLightingManager __instance)
            {
                List<string> logger = new List<string>();
                Color[] colors = new Color[20];
                colors[0] = new Color(1f, 0, 0);
                colors[1] = new Color(0, 1f, 0);
                colors[2] = new Color(0, 0, 1f);
                colors[3] = new Color(1f, 1f, 0);
                colors[4] = new Color(1f, 0, 1f);
                colors[5] = new Color(0, 1f, 1f);
                colors[6] = new Color(1f, 1f, 1f);
                colors[7] = new Color(0, 0, 0);
                colors[8] = new Color(0.5f, 0, 0);
                colors[9] = new Color(0, 0.5f, 0);
                colors[10] = new Color(0, 0, 0.5f);
                colors[11] = new Color(0.5f, 0.5f, 0);
                colors[12] = new Color(0.5f, 0, 0.5f);
                colors[13] = new Color(0, 0.5f, 0.5f);
                colors[14] = new Color(0.5f, 0.5f, 0.5f);
                colors[15] = new Color(1f, 0.5f, 0);
                colors[16] = new Color(1f, 0, 0.5f);
                colors[17] = new Color(0, 1f, 0.5f);
                colors[18] = new Color(0.5f, 1f, 0);
                colors[19] = new Color(1f, 0.5f, 0.5f);
                int color_index = 0;

                Il2CppSystem.Collections.Generic.List<InteriorLightingGroup> lightGroups = __instance.m_LightGroupList;

                foreach (InteriorLightingGroup group in lightGroups)
                {
                    Color new_color = colors[color_index];

                    logger.Add("GROUP: " + group.gameObject.name + " - " + new_color.ToString());

                    Il2CppSystem.Collections.Generic.List<Light> lights = group.GetLights();

                    foreach (Light light in lights)
                    {
                        logger.Add("{" + "|" + string.Format("\"description\": \"{0}\",", light.gameObject.name) + "|" + string.Format("\"position\": \"{0}\",", light.gameObject.transform.position.ToString()) + "|" + "\"orientation\":\"\"," + "|" + "\"size\": 1," + "|" + "\"cover\": 0" + "|" + "},");
                    }

                    color_index++;
                    if (color_index >= colors.Length)
                    {
                        color_index = 0;
                    }
                }

                //Debug.Log(Utils.SerializeObject(logger));
            }
        }

        [HarmonyPatch(typeof(UniStormWeatherSystem), "UpdateSunTransform")]
        internal class UniStormWeatherSystem_UpdateSunTransform
        {
            public static void Postfix(UniStormWeatherSystem __instance)
            {
                if (__instance.m_SunLight == null || __instance.m_SunLight.transform == null || GameLights.theSun == null || !Settings.options.trueSun)
                {
                    return;
                }

                GameLights.theSun.transform.rotation = Quaternion.AngleAxis(GameLights.sunOffset - 90f, Vector3.up);
                GameLights.theSun.transform.rotation *= Quaternion.AngleAxis(__instance.m_NormalizedTime * 360f - 90f - __instance.m_MasterTimeKeyOffset * 15f, Vector3.right + Vector3.up * __instance.m_SunAngle * 0.1f);
            }
        }
    }
}
