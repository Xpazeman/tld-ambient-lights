using UnityEngine;
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
        public static List<Renderer[]> gameShaftsList = new List<Renderer[]>();
        public static List<Light> gameExtraLightsList = new List<Light>();
        public static List<Color> gameExtraLightsColors = new List<Color>();
        public static List<MeshRenderer> gameWindows = new List<MeshRenderer>();

        public static TodAmbientLight gameAmbientLight = null;
        public static float lastAmbientMultiplier = 1f;
        public static Color defaultColorDay;
        public static Color defaultColorNight;

        public static bool hideWindows = false;

        public static bool gameLightsReady = false;

        /****** SETUP ******/

        internal static void AddGameLights(InteriorLightingManager mngr)
        {
            Debug.Log("[ambient-lights] InteriorLightingManager initialized.");

            gameLightsList.Clear();
            gameExtraLightsList.Clear();
            gameSpotLightsList.Clear();
            gameExtraLightsColors.Clear();
            gameWindows.Clear();

            gameLights = new GameObject();

            List<InteriorLightingGroup> lightGroups = Traverse.Create(mngr).Field("m_LightGroupList").GetValue<List<InteriorLightingGroup>>();

            int pCount = 0;
            int sCount = 0;
            int wCount = 0;
            int eCount = 0;
            int lCount = 0;

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

                        pCount++;
                    }
                    else if (light.type == LightType.Spot)
                    {
                        lightMark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        lightMark.transform.rotation = light.gameObject.transform.rotation;

                        sCount++;

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
                    wCount++;
                }
            }

            //Main Ambient Light
            gameAmbientLight = Traverse.Create(mngr).Field("m_AmbientLight").GetValue<TodAmbientLight>();
            if (gameAmbientLight != null)
            {
                gameAmbientLight.name = "XPZ_Light";
                Debug.Log("[ambient-lights] Ambient light found.");
            }

            //Loose Lights
            //No Manager
            Light[] sclights = UnityEngine.Object.FindObjectsOfType(typeof(Light)) as Light[];
            foreach (Light light in sclights)
            {
                if (light.gameObject.name != "XPZ_Light" && light.type == LightType.Point)
                {
                    gameExtraLightsList.Add(light);
                    gameExtraLightsColors.Add(light.color);

                    eCount++;
                }else if (light.gameObject.name != "XPZ_Light" && light.type == LightType.Spot)
                {
                    if (light.cookie.ToString().Contains("Window"))
                    {
                        light.gameObject.name = "XPZ_Light";
                        gameSpotLightsList.Add(light);
                    }
                }
            }

            //With Manager
            List<Light> looseLights = Traverse.Create(mngr).Field("m_LooseLightList").GetValue<List<Light>>();
            List<Light> looseLightsMidday = Traverse.Create(mngr).Field("m_LooseLightsMiddayList").GetValue<List<Light>>();

            List<Light> extraLights = new List<Light>();
            List<Color> extraLightsColor = new List<Color>();

            if (looseLights != null)
            {
                looseLights.ForEach(l => extraLights.Add(l));
                lCount += looseLights.Count;
            }


            if (looseLightsMidday != null)
            {
                looseLightsMidday.ForEach(l => extraLights.Add(l));
                lCount += looseLightsMidday.Count;
            }
            
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

            int weCount = GetWindows();

            if (!AmbientLights.showGameLights)
            {
                gameLights.SetActive(false);
            }

            Debug.Log("[ambient-lights] Gamelights setup done. Window Lights:" + pCount + ". Spotlights:" + sCount +  ". Loose Lights:" + lCount + ". Windows:" + wCount + ". Windows outside lighting groups:" + weCount + ". Extra Lights:" + eCount);
            gameLightsReady = true;

            AmbientLights.SetupGameLights();
        }

        internal static int GetWindows()
        {
            List<GameObject> rObjs = ALUtils.GetRootObjects();
            List<GameObject> result = new List<GameObject>();

            int wCount = 0;

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
                            if (renderer != null)
                            {
                                gameWindows.Add(renderer);
                                wCount++;
                            }
                        }
                    }
                }
            }

            return wCount;
        }

        /****** LIGHTS UPDATE ******/
        internal static void UpdateLights()
        {
            if (AmbientLights.lightOverride || !gameLightsReady)
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
                    if (eLight.gameObject.name != "XPZ_Light")
                    {
                        ColorHSV lColor = gameExtraLightsColors[eIndex];

                        lColor.s *= AmbientLights.options.fillColorLevel;

                        eLight.color = lColor;

                        eLight.intensity *= AmbientLights.options.fillLevel;
                    }
                }

                eIndex++;
            }
        }

        internal static void UpdateWindows()
        {
            if (AmbientLights.lightOverride || !AmbientLights.enableGameLights)
                return;

            //Windows
            
            Color bColor = AmbientLights.currentLightSet.windowColor;

            foreach (MeshRenderer window in gameWindows)
            {
                try
                {
                    if (AmbientLights.options.transparentWindows && window.gameObject.activeInHierarchy)
                    {
                        window.gameObject.SetActive(false);
                    }
                    else if (!AmbientLights.options.transparentWindows && !window.gameObject.activeInHierarchy)
                    {
                        window.gameObject.SetActive(true);
                    }
                }
                catch(Exception e)
                {
                    Debug.Log(e.Message);
                    Debug.Log(Utils.SerializeObject(window));
                }

                if (window.materials != null)
                {
                    if (window.materials.Length != 0)
                    {
                        for (int l = 0; l < window.materials.Length; l++)
                        {
                            if (window.materials[l].shader.name == "Shader Forge/TLD_StandardComplexProp")
                            {
                                window.materials[l].color = bColor;
                                //window.materials[l].color = Color.red;
                                float curStr = window.materials[l].GetFloat("_EmissiveStrength");
                                window.materials[l].SetFloat("_EmissiveStrength", curStr * AmbientLights.currentLightSet.windowStrMod);
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

            foreach(Renderer[] shaft in gameShaftsList)
            {
                if (shaft != null)
                {
                    foreach (Renderer mat in shaft)
                    {
                        if (mat.material != null)
                        {
                            float gain = mat.material.GetFloat("_Gain");
                            mat.material.SetFloat("_Gain", gain * AmbientLights.currentLightSet.lightshaftStr);
                        }
                    }
                }
            }
        }

        internal static void UpdateAmbience(TodAmbientLight TodLightInstance, ref float multiplier)
        {
            if (AmbientLights.lightOverride || AmbientLights.options.alPreset == ALPresets.TLD_Default)
            {
                TodLightInstance.m_AmbientIndoorsDay = defaultColorDay;
                TodLightInstance.m_AmbientIndoorsNight = defaultColorNight;

                return;
            }

            if (defaultColorDay == null)
            {
                defaultColorDay = TodLightInstance.m_AmbientIndoorsDay;
            }

            if (defaultColorNight == null)
            {
                defaultColorNight = TodLightInstance.m_AmbientIndoorsNight;
            }

            multiplier *= AmbientLights.options.ambienceLevel * AmbientLights.currentLightSet.intMod * AmbientLights.config.data.options.ambient_intensity_multiplier * AmbientLights.globalAmbienceLevel;

            TodLightInstance.m_AmbientIndoorsDay = AmbientLights.currentLightSet.ambientDayColor;
            TodLightInstance.m_AmbientIndoorsNight = AmbientLights.currentLightSet.ambientNightColor;
        }

        /****** UTILS ******/
        internal static void ResetLooseLights()
        {
            foreach (Light eLight in gameExtraLightsList)
            {
                if (AmbientLights.enableGameLights && !AmbientLights.lightOverride && eLight.gameObject.name != "XPZ_Light")
                {
                    eLight.intensity = 1;
                }
            }
        }

        internal static void ToggleWindows()
        {
            foreach (MeshRenderer window in gameWindows)
            {
                window.gameObject.SetActive(hideWindows);
            }

            hideWindows = !hideWindows;
        }
    }
}
