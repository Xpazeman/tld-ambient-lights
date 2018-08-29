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

            if (Input.GetKeyUp(KeyCode.F6))
            {
                if (AmbientLightUtils.debug_mode)
                {
                    AmbientLightUtils.debug_mode = false;
                    AmbientLightUtils.game_grid.SetActive(false);
                    AmbientLightUtils.game_lights.SetActive(false);
                }
                else
                {
                    AmbientLightUtils.debug_mode = true;
                    AmbientLightUtils.game_grid.SetActive(true);
                    AmbientLightUtils.game_lights.SetActive(true);
                }
            }

            if (Input.GetKeyUp(KeyCode.F7) && !Input.GetKey(KeyCode.LeftControl))
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
            else if (Input.GetKeyUp(KeyCode.F7) && Input.GetKey(KeyCode.LeftControl))
            {
                AmbientLightControl.RemoveLights();
                AmbientLightControl.ResetAmbientLights();
                AmbientLightControl.RegisterLights();
            }
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

            AmbientLightUtils.game_lights = new GameObject();

            List<InteriorLightingGroup> light_groups = (List < InteriorLightingGroup >)AmbientLightUtils.GetPrivateFieldObject(__instance, "m_LightGroupList");

            foreach(InteriorLightingGroup group in light_groups)
            {
                Color new_color = colors[color_index];

                logger.Add("GROUP: " + group.gameObject.name + " - " + new_color.ToString());

                List<Light> lights = group.GetLights();

                foreach(Light light in lights)
                {
                    logger.Add("{" + "|" + string.Format("\"description\": \"{0}\",", light.gameObject.name) + "|" + string.Format("\"position\": \"{0}\",", light.gameObject.transform.position.ToString()) + "|" + "\"orientation\":\"\"," + "|" + "\"size\": 1," + "|" + "\"cover\": 0" + "|" + "},");

                    GameObject light_mark;

                    if (light.type == LightType.Point)
                    {
                        light_mark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    }
                    else if (light.type == LightType.Spot)
                    {
                        light_mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        light_mark.transform.rotation = light.gameObject.transform.rotation;
                    }
                    else
                    {
                        light_mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    }

                    light_mark.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    light_mark.transform.position = light.gameObject.transform.position;

                    light_mark.transform.parent = AmbientLightUtils.game_lights.transform;

                    foreach (Renderer rend in light_mark.GetComponentsInChildren<Renderer>())
                    {
                        rend.material.color = new_color;
                        rend.receiveShadows = false;
                    }
                }

                color_index++;
                if (color_index >= colors.Length)
                {
                    color_index = 0;
                }
            }

            if (!AmbientLightUtils.debug_mode)
            {
                AmbientLightUtils.game_lights.SetActive(false);
            }

            Debug.Log(Utils.SerializeObject(logger));

            AmbientLightUtils.BuildGrid();
        }
    }
}