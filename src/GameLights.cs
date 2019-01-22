using UnityEngine;
using DumpData;
using Harmony;
using System;
using System.Collections.Generic;

namespace AmbientLights
{
    class GameLights
    {
        public static GameObject gameLights = new GameObject();

        public static List<Light> gameLightsList = new List<Light>();
        public static List<Light> gameSpotLightsList = new List<Light>();
        public static List<Light> gameExtraLightsList = new List<Light>();
        public static List<Color> gameExtraLightsColors = new List<Color>();
        public static List<MeshRenderer> gameWindows = new List<MeshRenderer>();

        public static TodAmbientLight gameAmbientLight = null;

        public static void GetWindows()
        {
            List<GameObject> rObjs = ALUtils.GetRootObjects();
            List<GameObject> result = new List<GameObject>();

            foreach (GameObject rootObj in rObjs)
            {
                ALUtils.GetChildrenWithName(rootObj, "windowglow", result);

                if (result.Count > 0)
                {
                    foreach(GameObject child in result)
                    {
                        MeshRenderer renderer = child.GetComponent<MeshRenderer>();

                        gameWindows.Add(renderer);
                    }
                }
            }
        }

        public static void AddGameLights(InteriorLightingManager mngr)
        {
            Debug.Log("[ambient-lights] InteriorLightingManager initialized.");

            GameLights.gameLights = new GameObject();

            

            //Window Lights
            List<InteriorLightingGroup> lightGroups = Traverse.Create(mngr).Field("m_LightGroupList").GetValue<List<InteriorLightingGroup>>();

            foreach (InteriorLightingGroup group in lightGroups)
            {
                List<Light> lights = group.GetLights();

                foreach (Light light in lights)
                {
                    GameObject lightMark;

                    if (light.type == LightType.Point)
                    {
                        lightMark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        (lightMark.GetComponent(typeof(SphereCollider)) as Collider).enabled = false;

                        gameLightsList.Add(light);
                    }
                    else if (light.type == LightType.Spot)
                    {
                        lightMark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        lightMark.transform.rotation = light.gameObject.transform.rotation;

                        light.gameObject.name = "XPZ_Light";

                        if (AmbientLights.options.castShadows)
                        {
                            light.shadows = LightShadows.Soft;
                        }
                        else
                        {
                            light.shadows = LightShadows.None;
                        }
                        gameSpotLightsList.Add(light);
                    }
                    else
                    {
                        lightMark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    }

                    lightMark.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    lightMark.transform.position = light.gameObject.transform.position;

                    lightMark.transform.parent = gameLights.transform;

                    foreach (Renderer rend in lightMark.GetComponentsInChildren<Renderer>())
                    {
                        rend.material.color = new Color(1f, 0, 0);
                        rend.receiveShadows = false;
                    }
                }

                //Windows
                List<MeshRenderer> windows = Traverse.Create(group).Field("m_GlowObjects").GetValue<List<MeshRenderer>>();

                foreach (MeshRenderer window in windows)
                {
                    gameWindows.Add(window);
                }
            }

            //Loose Lights
            Light[] sclights = UnityEngine.Object.FindObjectsOfType(typeof(Light)) as Light[];
            foreach (Light light in sclights)
            {
                if (light.gameObject.name != "XPZ_Light" && light.type == LightType.Point)
                {
                    gameExtraLightsList.Add(light);
                    gameExtraLightsColors.Add(light.color);
                }
            }

            //Fill Lights
            List<Light> looseLights = Traverse.Create(mngr).Field("m_LooseLightList").GetValue<List<Light>>();
            List<Light> looseLightsMidday = Traverse.Create(mngr).Field("m_LooseLightsMiddayList").GetValue<List<Light>>();

            List<Light> extraLights = new List<Light>();
            List<Color> extraLightsColor = new List<Color>();

            if (looseLights != null)
                looseLights.ForEach(l => extraLights.Add(l));

            if (looseLightsMidday != null)
                looseLightsMidday.ForEach(l => extraLights.Add(l));

            foreach (Light light in extraLights)
            {
                extraLightsColor.Add(light.color);
            }

            gameExtraLightsList.AddRange(extraLights);
            gameExtraLightsColors.AddRange(extraLightsColor);

            foreach (Light light in gameExtraLightsList)
            {
                GameObject eLightMark;

                eLightMark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                (eLightMark.GetComponent(typeof(SphereCollider)) as Collider).enabled = false;
                eLightMark.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                eLightMark.transform.position = light.gameObject.transform.position;

                eLightMark.transform.parent = gameLights.transform;

                foreach (Renderer rend in eLightMark.GetComponentsInChildren<Renderer>())
                {
                    Color rendColor = light.color;
                    rendColor.a = light.intensity;
                    rend.material.color = light.color;
                    rend.receiveShadows = false;
                }
            }

            Debug.Log(Utils.SerializeObject(gameExtraLightsList));
            Debug.Log(Utils.SerializeObject(gameExtraLightsColors));

            if (!AmbientLights.showGameLights)
            {
                gameLights.SetActive(false);
            }

            //Main Ambient Light
            gameAmbientLight = Traverse.Create(mngr).Field("m_AmbientLight").GetValue<TodAmbientLight>();

            AmbientLights.SetupGameLights();

            
        }

        public static void UpdateLights()
        {
            if (AmbientLights.lightOverride)
                return;

            //Extra Lights
            int eIndex = 0;
            foreach (Light eLight in gameExtraLightsList)
            {
                if (!AmbientLights.enableGameLights)
                {
                    eLight.intensity = 0;
                }
                else
                {

                    ColorHSV lColor = gameExtraLightsColors[eIndex];

                    lColor.s *= AmbientLights.options.fillColorLevel;

                    //Debug.Log("From:" + (ColorHSV)gameExtraLightsColors[eIndex] + " To:" + lColor);
                    eLight.color = lColor;

                    eLight.intensity *= AmbientLights.options.fillLevel;
                }

                eIndex++;
            }

            //Windows
            UniStormWeatherSystem uniStorm = GameManager.GetUniStorm();
            TODStateConfig state = uniStorm.GetActiveTODState();

            ColorHSV bColor = state.m_FogColor;

            bColor = AmbientLights.config.ApplyWeatherMod(bColor);
            bColor.s *= 0.8f;

            foreach (MeshRenderer window in gameWindows)
            {
                MeshRenderer component = window.GetComponent<MeshRenderer>();
                if (!(component == null))
                {
                    if (window.materials.Length != 0)
                    {
                        for (int l = 0; l < window.materials.Length; l++)
                        {
                            if (window.materials[l].shader.name == "Shader Forge/TLD_StandardComplexProp")
                            {
                                window.materials[l].color = bColor;
                            }
                        }
                    }
                }
            }

            //Ambient Light
            if (gameAmbientLight != null && !AmbientLights.enableGameLights)
            {
                gameAmbientLight.SetAmbientLightValue(0, 0);
            }
        }

        internal static void UpdateLightshafts()
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

            // Lightshafts
            foreach (Light sLight in gameSpotLightsList)
            {
                if (!AmbientLights.enableGameLights)
                {
                    sLight.intensity = 0f;
                }
                else
                {
                    if (AmbientLights.options.castShadows)
                    {
                        sLight.shadows = LightShadows.Soft;

                        float shdwStr = ALUtils.GetShadowStrength(TimeWeather.currentWeather);

                        if (TimeWeather.currentWeatherPct < 1f)
                        {
                            float prevStr = ALUtils.GetShadowStrength(TimeWeather.previousWeather);

                            shdwStr = Mathf.Lerp(prevStr, shdwStr, TimeWeather.currentWeatherPct);
                        }

                        if (TimeWeather.currentPeriod == "night")
                        {
                            shdwStr *= .5f;
                        }

                        sLight.shadowStrength = shdwStr;
                        sLight.renderMode = LightRenderMode.ForceVertex;
                    }
                    else
                    {
                        sLight.shadows = LightShadows.None;
                    }

                    sLight.intensity *= str;

                    sLight.color = Color.red;
                }
            }
        }

        internal static void UpdateLooseLights()
        {
            foreach (Light eLight in gameExtraLightsList)
            {
                if (AmbientLights.enableGameLights)
                {
                    eLight.intensity = 1;
                }
            }
        }

        public static void UpdateAmbience(TodAmbientLight TodLightInstance, ref float multiplier)
        {
            if (AmbientLights.lightOverride)
                return;

            multiplier *= AmbientLights.options.ambienceLevel;

            UniStormWeatherSystem uniStorm = GameManager.GetUniStorm();
            TODStateConfig state = uniStorm.GetActiveTODState();

            Color bColor = state.m_FogColor;

            bColor = AmbientLights.config.ApplyWeatherMod(bColor);

            ColorHSV fColor = bColor;
            fColor.s *= 0.5f;
            fColor.v = 0.1f;

            ColorHSV nColor = fColor;
            nColor.v = 0.01f;

            TodLightInstance.m_AmbientIndoorsDay = fColor;
            TodLightInstance.m_AmbientIndoorsNight = nColor;

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

            
        }
    }
}
