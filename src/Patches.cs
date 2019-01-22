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
                    GameLights.UpdateLooseLights();

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

        /****** Windows ******/
        /*[HarmonyPatch(typeof(TodMaterial), "UpdateMaterial")]
        internal class TodMaterial_UpdateMaterial
        {
            private static void Postfix(TodMaterial __instance)
            {
                Renderer win = Traverse.Create(__instance).Field("m_Renderer").GetValue<Renderer>();
                int matIndex = Traverse.Create(__instance).Field("m_MaterialIndex").GetValue<int>();

                win.materials[matIndex].color = Color.green;
            }
        }

        [HarmonyPatch(typeof(DarkLightingManager), "UpdateGlowObjects")]
        internal class DarkLightingManager_UpdateGlowObjects
        {
            private static void Postfix(DarkLightingManager __instance)
            {
                List<DarkLightingManager.GlowObjectDef> glowObjs = Traverse.Create(__instance).Field("m_GlowObjectsList").GetValue<List<DarkLightingManager.GlowObjectDef>>();

                if (glowObjs.Count > 0)
                {
                    for (int i = 0; i < glowObjs.Count; i++)
                    {
                        glowObjs[i].m_Material.color = Color.blue;
                    }
                }
            }
        }*/

        /****** Lightshafts ******/
        /*[HarmonyPatch(typeof(InteriorLightingManager), "UpdateLightShaft", new Type[] { typeof(float), typeof(bool) })]
        internal class InteriorLightingManager_UpdateLightShaft
        {
            private static bool Prefix(InteriorLightingManager __instance, ref float timeOfDayIntensity)
            {
                if (!GameManager.m_IsPaused && AmbientLights.config != null)
                {
                    float str = ALUtils.GetShadowStrength(TimeWeather.currentWeather);

                    if (TimeWeather.currentWeatherPct < 1f)
                    {
                        float prevStr = ALUtils.GetShadowStrength(TimeWeather.previousWeather);

                        str = Mathf.Lerp(prevStr, str, TimeWeather.currentWeatherPct);
                    }

                    if (TimeWeather.currentPeriod == "night")
                    {
                        str = 0;
                    }

                    //timeOfDayIntensity *= str;
                }

                return true;
            }
        }*/



        /*[HarmonyPatch(typeof(LightShaftGimble), "UpdateLight")]
        internal class LightShaftGimble_UpdateLight
        {
            private static void Postfix(LightShaftGimble __instance)
            {
                GameLights.UpdateLightshafts();

                if (__instance.m_Light)
                {
                    float str = ALUtils.GetShadowStrength(TimeWeather.currentWeather);

                    if (TimeWeather.currentWeatherPct < 1f)
                    {
                        float prevStr = ALUtils.GetShadowStrength(TimeWeather.previousWeather);

                        str = Mathf.Lerp(prevStr, str, TimeWeather.currentWeatherPct);
                    }

                    if (TimeWeather.currentPeriod == "night")
                    {
                        str = 0;
                    }

                    __instance.m_Light.intensity *= str;

                    //ALUtils.DebugGameObject(__instance.gameObject);

                    __instance.m_Light.color = Color.red;

                }
            }
        }*/



        /*[HarmonyPatch(typeof(LightShaftGimble), "UpdateMaterial", new Type[] { typeof(float) })]
        internal class LightShaftGimble_UpdateMaterial
        {
            private static void Postfix(LightShaftGimble __instance)
            {
                //Renderer[] renderers = Traverse  m_LightShaftRenderer
                Renderer[] renderers = Traverse.Create(__instance).Field("m_LightShaftRenderer").GetValue<Renderer[]>();

                if (renderers != null)
                {
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        //Debug.Log(renderers[i].material.GetFloat("_Gain"));
                        //renderers[i].material.SetFloat("_Gain", 0);

                        /*float num;
                        if (this.m_Light)
                        {
                            num = this.m_Light.intensity / this.m_LightStartIntensity;
                        }
                        else
                        {
                            num = 1f;
                        }
                        if (this.m_Light && this.m_UseLightColour)
                        {
                            float value = this.CalculateDot() * this.m_MaximumRayIntensity.curve.Evaluate(tod) * this.m_ExternalIntensity * this.m_TodIntensity * num;
                            this.m_LightShaftRenderer[i].material.SetFloat("_Gain", value);
                            this.m_LightShaftRenderer[i].material.SetColor("_Color", this.m_Light.color);
                        }
                        else
                        {
                            float value2 = this.CalculateDot() * this.m_MaximumRayIntensity.curve.Evaluate(tod) * this.m_ExternalIntensity * this.m_TodIntensity;
                            this.m_LightShaftRenderer[i].material.SetFloat("_Gain", value2);
                        }
                    }
                }
            }
        }*/
    }
}
