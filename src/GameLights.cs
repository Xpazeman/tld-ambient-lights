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

        /****** SETUP ******/

        internal static void AddGameLights(InteriorLightingManager mngr)
        {
            Debug.Log("[ambient-lights] InteriorLightingManager initialized.");

            gameLights = new GameObject();

            List<InteriorLightingGroup> lightGroups = Traverse.Create(mngr).Field("m_LightGroupList").GetValue<List<InteriorLightingGroup>>();

            //Windows & Window Lights
            foreach (InteriorLightingGroup group in lightGroups)
            {
                List<Light> lights = group.GetLights();

                foreach (Light light in lights)
                {
                    GameObject lightMark;

                    //Add lights to list and to debug object
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
                        rend.material.color = light.color;
                        rend.receiveShadows = false;
                    }
                }

                //Windows
                List<MeshRenderer> windows = Traverse.Create(group).Field("m_GlowObjects").GetValue<List<MeshRenderer>>();

                foreach (MeshRenderer window in windows)
                {
                    window.gameObject.name = "XPZ_Window";
                    gameWindows.Add(window);
                }
            }

            //Main Ambient Light
            gameAmbientLight = Traverse.Create(mngr).Field("m_AmbientLight").GetValue<TodAmbientLight>();
            gameAmbientLight.name = "XPZ_Light";

            //Loose Lights
            //No Manager
            Light[] sclights = UnityEngine.Object.FindObjectsOfType(typeof(Light)) as Light[];
            foreach (Light light in sclights)
            {
                if (light.gameObject.name != "XPZ_Light" && light.type == LightType.Point)
                {
                    gameExtraLightsList.Add(light);
                    gameExtraLightsColors.Add(light.color);
                }
            }

            //With Manager
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

            //Add fill lights to debug object
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
                    rend.material.color = light.color;
                    rend.receiveShadows = false;
                }
            }

            if (!AmbientLights.showGameLights)
            {
                gameLights.SetActive(false);
            }

            AmbientLights.SetupGameLights();
        }

        internal static void GetWindows()
        {
            List<GameObject> rObjs = ALUtils.GetRootObjects();
            List<GameObject> result = new List<GameObject>();

            foreach (GameObject rootObj in rObjs)
            {
                ALUtils.GetChildrenWithName(rootObj, "windowglow", result);

                if (result.Count > 0)
                {
                    foreach (GameObject child in result)
                    {
                        if (child.name != "XPZ_Window")
                        {
                            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                            gameWindows.Add(renderer);
                        }
                    }
                }
            }
        }

        /****** LIGHTS UPDATE ******/
        internal static void UpdateLights()
        {
            if (AmbientLights.lightOverride)
                return;

            UpdateFillLights();
            UpdateWindows();

            //Ambient Light
            if (gameAmbientLight != null && !AmbientLights.enableGameLights)
            {
                gameAmbientLight.SetAmbientLightValue(0, 0);
            }
        }

        internal static void UpdateFillLights()
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
        }

        internal static void UpdateWindows()
        {
            if (AmbientLights.lightOverride)
                return;

            //Windows
            
            Color bColor = AmbientLights.currentLightSet.windowColor;

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
        }

        internal static void UpdateLightshafts()
        {
            if (AmbientLights.lightOverride)
                return;

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

                        sLight.shadowStrength = AmbientLights.currentLightSet.shadowStr;
                        sLight.renderMode = LightRenderMode.ForceVertex;
                    }
                    else
                    {
                        sLight.shadows = LightShadows.None;
                    }

                    sLight.intensity *= AmbientLights.currentLightSet.lightshaftStr;

                    sLight.color = AmbientLights.currentLightSet.lightshaftColor;
                }
            }
        }

        internal static void UpdateAmbience(TodAmbientLight TodLightInstance, ref float multiplier)
        {
            if (AmbientLights.lightOverride)
                return;

            multiplier *= AmbientLights.options.ambienceLevel * AmbientLights.currentLightSet.intMod;

            TodLightInstance.m_AmbientIndoorsDay = AmbientLights.currentLightSet.ambientDayColor;
            TodLightInstance.m_AmbientIndoorsNight = AmbientLights.currentLightSet.ambientNightColor;
        }

        /****** UTILS ******/
        internal static void ResetLooseLights()
        {
            foreach (Light eLight in gameExtraLightsList)
            {
                if (AmbientLights.enableGameLights)
                {
                    eLight.intensity = 1;
                }
            }
        }
    }
}
