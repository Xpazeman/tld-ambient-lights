using UnityEngine;
using HarmonyLib;
using Il2Cpp;
using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

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
        public static List<float> gameExtraLightsIntensity = new List<float>();
        public static List<Renderer> gameWindows = new List<Renderer>();

        public static TodAmbientLight gameAmbientLight = null;
        public static float lastAmbientMultiplier = 1f;
        public static Color defaultColorDay;
        public static Color defaultColorNight;

        public static GameObject theSun = null;
        public static Light sunlight = null;
        public static float sunOffset = 0f;

        public static bool hideWindows = false;

        public static InteriorLightingManager mngr = null;
        public static DarkLightingManager darkMngr = null;
        public static bool gameLightsReady = false;

        /****** SETUP ******/

        internal static void AddGameLights()
        {
            if (AmbientLights.config == null || !AmbientLights.config.ready)
            {
                return;
            }

            //if (AmbientLights.debugVer)
            //    Debug.Log("[ambient-lights] InteriorLightingManager initialized.");

            //if (AmbientLights.debugVer)
            //    MelonLoader.MelonLogger.Log("[AL] Game Lights init");

            ALUtils.Log("Game Lights Manager initialized.");

            if (gameLightsList != null)
                gameLightsList.Clear();

            if (gameExtraLightsList != null)
                gameExtraLightsList.Clear();

            if (gameSpotLightsList != null)
                gameSpotLightsList.Clear();

            if (gameExtraLightsColors != null)
                gameExtraLightsColors.Clear();

            if (gameExtraLightsIntensity != null)
                gameExtraLightsIntensity.Clear();

            if (gameWindows != null)
                gameWindows.Clear();

            theSun = null;
            sunlight = null;

            //if (AmbientLights.debugVer)
            //    MelonLoader.MelonLogger.Log("[AL] Add Game Lights");
            ALUtils.Log("Adding game lights.", false);

            gameLights = new GameObject();

            List<InteriorLightingGroup> lightGroups = new List<InteriorLightingGroup>();

            InteriorLightingGroup[] lightGroupsArr = null;

            if (mngr != null)
            {
                lightGroupsArr = mngr.m_LightGroupList.ToArray();

                foreach (InteriorLightingGroup lightGroup in lightGroupsArr)
                {
                    lightGroups.Add(lightGroup);
                }
            }

            int pCount = 0;
            int sCount = 0;
            int wCount = 0;
            int eCount = 0;
            int lCount = 0;

            //Add sun
            if (!AmbientLights.currentScene.ToLower().Contains("cave") && Settings.options.trueSun)
            {
                //if (AmbientLights.debugVer)
                //    MelonLoader.MelonLogger.Log("[AL] Add sun");
                ALUtils.Log("Creating sun", false);

                theSun = new GameObject();
                theSun.name = "AmbientLightsSun";
                sunlight = theSun.AddComponent<Light>();
                sunlight.type = LightType.Directional;
                sunlight.shadows = LightShadows.Soft;
                sunlight.shadowStrength = 1f;
                sunlight.shadowNormalBias = 0;
                sunlight.shadowBias = 0;
                sunlight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;

                sunlight.cullingMask &= ~(1 << 7);


                sunlight.intensity = 1f;

                theSun.transform.position = new Vector3(0, 2f, 0);
                sunlight.transform.parent = theSun.transform;

                Vector3 sunRotation = new Vector3(10f, 10f, 0);
                theSun.transform.localRotation = Quaternion.Euler(sunRotation);

                PrepareSceneShadows();
            }

            //Window Lights
            //if (AmbientLights.debugVer)
            //    MelonLoader.MelonLogger.Log("[AL] Window lights");

            ALUtils.Log("Adding window lights", false);

            foreach (InteriorLightingGroup group in lightGroups)
            {
                List<Light> lights = new List<Light>();
                Light[] groupLights = group.GetLights().ToArray();

                foreach (Light gLight in groupLights)
                {
                    if (gLight != null)
                    {
                        lights.Add(gLight);
                    }
                    else
                    {
                        //if (AmbientLights.debugVer)
                        //    Debug.Log("[ambient-lights] gLight is null.");
                        ALUtils.Log("gLight is null", false);

                    }

                }

                foreach (Light light in lights)
                {
                    GameObject lightMark;

                    //Add lights to list and to debug object
                    if (light.type == LightType.Point)
                    {
                        lightMark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        (lightMark.GetComponent(Il2CppType.Of<SphereCollider>()).Cast<Collider>()).enabled = false;

                        light.gameObject.name += "_XPZ_GameLight";

                        gameLightsList.Add(light);

                        pCount++;
                    }
                    else if (light.type == LightType.Spot)
                    {
                        lightMark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        lightMark.transform.rotation = light.gameObject.transform.rotation;

                        sCount++;

                        light.gameObject.name += "_XPZ_SpotLight";

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
            }

            //Main Ambient Light
            //if (AmbientLights.debugVer)
            //    MelonLoader.MelonLogger.Log("[AL] Main Ambient Light");

            ALUtils.Log("Adding main ambient light", false);

            if (mngr != null)
            {
                gameAmbientLight = mngr.m_AmbientLight;
            }
            else
            {
                TodAmbientLight[] array = UnityEngine.Object.FindObjectsOfType<TodAmbientLight>();
                if (array.Length > 0)
                    gameAmbientLight = array[0];

            }

            if (gameAmbientLight != null)
            {
                defaultColorDay = gameAmbientLight.m_AmbientIndoorsDay;
                defaultColorNight = gameAmbientLight.m_AmbientIndoorsNight;

                gameAmbientLight.name += "_XPZ_AmbientLight";
                //if (AmbientLights.debugVer)
                //    Debug.Log("[ambient-lights] Ambient light found.");
                ALUtils.Log("Main ambient light found", false);
            }
            else
            {
                //if (AmbientLights.debugVer)
                //    Debug.Log("[ambient-lights] Ambient light NOT found.");
                ALUtils.Log("Main ambient light missing", false);
            }

            //Loose Lights
            //With Manager
            //if (AmbientLights.debugVer)
            //    MelonLoader.MelonLogger.Log("[AL] Managed Loose Lights");

            ALUtils.Log("Adding managed loose lights", false);

            List<Light> looseLights = new List<Light>();
            List<Light> looseLightsMidday = new List<Light>();

            Light[] lLightsArr = null;
            if (mngr != null && mngr.m_LooseLightList != null)
                lLightsArr = mngr.m_LooseLightList.ToArray();

            Light[] lLightsMidArr = null;
            if (mngr != null && mngr.m_LooseLightsMiddayList != null)
                lLightsMidArr = mngr.m_LooseLightsMiddayList.ToArray();

            List<Light> extraLights = new List<Light>();
            List<Color> extraLightsColor = new List<Color>();

            if (lLightsArr != null)
            {
                foreach (Light l in lLightsArr)
                {
                    if (!l.gameObject.name.Contains("XPZ_SpotLight") && !l.gameObject.name.Contains("XPZ_GameLight"))
                    {
                        l.gameObject.name += "_XPZ_GameLight";
                        extraLights.Add(l);
                        lCount++;
                    }
                }
            }

            if (lLightsMidArr != null)
            {
                foreach (Light l in lLightsMidArr)
                {
                    if (!l.gameObject.name.Contains("XPZ_SpotLight") && !l.gameObject.name.Contains("XPZ_GameLight"))
                    {
                        l.gameObject.name += "_XPZ_GameLight";
                        extraLights.Add(l);
                        lCount++;
                    }
                }
            }

            foreach (Light light in extraLights)
            {
                extraLightsColor.Add(light.color);

                gameExtraLightsColors.Add(light.color);
                gameExtraLightsIntensity.Add(light.intensity);
                gameExtraLightsList.Add(light);
            }


            //if (AmbientLights.debugVer)
            //    MelonLoader.MelonLogger.Log("[AL] Unmanaged Loose Lights");

            ALUtils.Log("Adding unmanaged loose lights", false);

            //No Manager
            Il2CppReferenceArray<UnityEngine.Object> sclights = UnityEngine.Object.FindObjectsOfType(Il2CppType.Of<Light>());

            foreach (UnityEngine.Object lightItem in sclights)
            {
                Light light = lightItem.Cast<Light>();

                if (!light.gameObject.name.Contains("XPZ_GameLight") && !light.gameObject.name.Contains("XPZ_Light") && light.type == LightType.Point)
                {
                    light.gameObject.name += "_XPZ_GameLight";
                    gameExtraLightsList.Add(light);
                    gameExtraLightsColors.Add(light.color);
                    gameExtraLightsIntensity.Add(light.intensity);
                    eCount++;

                }
                else if (!light.gameObject.name.Contains("XPZ_SpotLight") && !light.gameObject.name.Contains("XPZ_GameLight") && light.type == LightType.Spot)
                {
                    if (light.cookie && light.cookie.ToString().Contains("Window"))
                    {
                        light.gameObject.name += "_XPZ_SpotLight";
                        gameSpotLightsList.Add(light);
                    }
                    else
                    {
                        light.gameObject.name += "_XPZ_GameLight";
                        gameExtraLightsList.Add(light);
                        gameExtraLightsColors.Add(light.color);
                        gameExtraLightsIntensity.Add(light.intensity);
                        eCount++;
                    }
                }
            }

            //Add fill lights to debug object
            //if (AmbientLights.debugVer)
            //    MelonLoader.MelonLogger.Log("[AL] Add to debug");

            ALUtils.Log("Adding fill lights to debug object", false);

            foreach (Light light in gameExtraLightsList)
            {
                GameObject eLightMark;

                eLightMark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //eLightMark.GetComponent(typeof(SphereCollider)).Cast<Collider>().enabled = false;
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

            //MelonLoader.MelonModLogger.Log("[AL] Done Preparing");
            //if (AmbientLights.debugVer)
            //    Debug.Log("[ambient-lights] Gamelights setup done. Window Lights:" + pCount + ". Spotlights:" + sCount +  ". Loose Lights:" + lCount + ". Windows:" + wCount + ". Windows outside lighting groups:" + weCount + ". Extra Lights:" + eCount);

            ALUtils.Log("Gamelights setup complete. Window Lights:" + pCount + ". Spotlights:" + sCount + ". Loose Lights:" + lCount + ". Windows:" + wCount + ". Windows outside lighting groups:" + weCount + ". Extra Lights:" + eCount, false);

            AmbientLights.SetupGameLights();

            gameLightsReady = true;
        }

        internal static int GetWindows()
        {

            GameObject[] rObjs = ALUtils.GetRootObjects().ToArray();

            int wCount = 0;

            foreach (GameObject rootObj in rObjs)
            {
                Renderer childRenderer = rootObj.GetComponent<Renderer>();
                Renderer[] allRenderers = rootObj.GetComponentsInChildren<Renderer>();
                //allRenderers.Add(childRenderer);
                allRenderers.AddItem(childRenderer);

                foreach (Renderer renderer in allRenderers)
                {
                    if (!renderer.gameObject.name.Contains("_XPZ_Window"))
                    {
                        foreach (Material mat in renderer.materials)
                        {
                            if (mat.name.ToLower().Contains("glow") && !renderer.gameObject.name.ToLower().Contains("truck") && !renderer.gameObject.name.ToLower().Contains("car") && !renderer.gameObject.name.ToLower().Contains("lamp"))
                            {
                                gameWindows.Add(renderer);
                                renderer.gameObject.name += "_XPZ_Window";

                                WindowsCastShadow(renderer.gameObject);
                                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                                renderer.receiveShadows = false;

                                wCount++;
                                break;
                            }
                        }
                    }
                }
            }

            return wCount;
        }

        /****** LIGHTS UPDATE ******/
        internal static void UpdateLights(bool isDarkMngr = false)
        {
            if (!AmbientLights.config.ready)
                return;

            UpdateFillLights(isDarkMngr);
            UpdateWindows(isDarkMngr);
            UpdateLightshafts(isDarkMngr);
            UpdateSun();

            //Ambient Light
            if (gameAmbientLight != null && !AmbientLights.enableGameLights)
            {
                gameAmbientLight.SetAmbientLightValue(0, 0);
            }
        }

        internal static void UpdateFillLights(bool isDarkMngr = false)
        {
            if (!AmbientLights.config.ready)
                return;

            //Extra Lights
            int eIndex = 0;
            foreach (Light eLight in gameExtraLightsList)
            {
                if (eLight == null || eLight.gameObject == null)
                {
                    continue;
                }

                if (!AmbientLights.enableGameLights)
                {
                    eLight.intensity = 0;
                }
                else
                {
                    if (!eLight.gameObject.name.Contains("XPZ_SpotLight"))
                    {
                        if (!AmbientLights.lightOverride)
                        {

                            ColorHSV lColor = gameExtraLightsColors[eIndex];

                            lColor.s *= Settings.options.fillColorLevel;
                            lColor.v *= Settings.options.fillLevel;

                            eLight.color = lColor;

                            //eLight.intensity *= Settings.options.fillLevel;

                            if (eLight.shadows != LightShadows.None)
                            {
                                eLight.shadowStrength = AmbientLights.currentLightSet.shadowStr;
                            }
                        }
                        else
                        {
                            eLight.color = gameExtraLightsColors[eIndex];
                        }
                    }
                }

                eIndex++;
            }
        }

        internal static void UpdateWindows(bool isDarkMngr = false)
        {
            if (AmbientLights.lightOverride || !AmbientLights.enableGameLights)
                return;

            //Windows

            Color bColor = AmbientLights.currentLightSet.windowColor;

            foreach (Renderer window in gameWindows)
            {
                if (window == null)
                {
                    continue;
                }
                window.receiveShadows = false;
                window.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                if (window.materials != null)
                {
                    if (window.materials.Length != 0)
                    {
                        for (int l = 0; l < window.materials.Length; l++)
                        {
                            window.materials[l].color = bColor;
                            float curStr = window.materials[l].GetFloat("_EmissiveStrength");
                            window.materials[l].SetFloat("_EmissiveStrength", curStr * AmbientLights.currentLightSet.windowStrMod);
                        }
                    }
                }
            }
        }

        internal static void UpdateLightshafts(bool isDarkMngr = false)
        {
            // Lightshafts
            foreach (Light sLight in gameSpotLightsList)
            {
                if (!AmbientLights.enableGameLights)
                {
                    sLight.intensity = 0f;
                }
                else
                {
                    if (Settings.options.trueSun && !AmbientLights.lightOverride)
                    {
                        sLight.gameObject.SetActive(false);
                    }
                    else if (AmbientLights.lightOverride)
                    {
                        sLight.gameObject.SetActive(true);
                        return;
                    }
                    else
                    {
                        sLight.gameObject.SetActive(true);

                        //sLight.intensity *= AmbientLights.currentLightSet.lightshaftStr;

                        //sLight.color = AmbientLights.currentLightSet.lightshaftColor;
                    }
                }
            }

            /*foreach (Renderer[] shaft in gameShaftsList)
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
            }*/
        }

        internal static void UpdateAmbience(TodAmbientLight TodLightInstance, ref float multiplier)
        {

            if (AmbientLights.lightOverride || Settings.options.alPreset == ALPresets.TLD_Default)
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

            multiplier *= Settings.options.ambienceLevel * AmbientLights.currentLightSet.intMod * AmbientLights.config.data.options.ambient_intensity_multiplier * AmbientLights.globalAmbienceLevel * ALUtils.GetIntensityNightMod();

            TodLightInstance.m_AmbientIndoorsDay = AmbientLights.currentLightSet.ambientDayColor;
            TodLightInstance.m_AmbientIndoorsNight = AmbientLights.currentLightSet.ambientNightColor;
        }

        internal static void UpdateSun()
        {
            if (theSun == null)
            {
                return;
            }

            if (AmbientLights.lightOverride || Settings.options.alPreset == ALPresets.TLD_Default || !Settings.options.trueSun)
            {
                sunlight.intensity = 0;
                theSun.SetActive(false);
            }
            else
            {
                theSun.SetActive(true);
                sunlight.intensity = 5f * AmbientLights.currentLightSet.sunStr;
                sunlight.color = AmbientLights.currentLightSet.ambientDayColor;

            }
        }

        /****** DARK MANAGER ******/
        internal static void UpdateConstantLights()
        {
            if (darkMngr.m_ConstantLightList.Count > 0)
            {
                for (int i = 0; i < darkMngr.m_ConstantLightList.Count; i++)
                {
                    darkMngr.m_ConstantLightList[i].m_Light.intensity *= Settings.options.fillLevel;
                }
            }
        }

        internal static void UpdateColouredLights()
        {
            if (darkMngr.m_ColouredLightList.Count > 0)
            {
                for (int i = 0; i < darkMngr.m_ColouredLightList.Count; i++)
                {
                    darkMngr.m_ColouredLightList[i].m_Light.intensity *= Settings.options.fillLevel;

                    ColorHSV lColor = darkMngr.m_ColouredLightList[i].m_Light.color;
                    lColor.s *= Settings.options.fillColorLevel;
                    darkMngr.m_ColouredLightList[i].m_Light.color = lColor;
                }
            }
        }

        internal static void UpdateNonTodLights()
        {
            if (darkMngr.m_NonTodLightList.Count > 0)
            {
                for (int i = 0; i < darkMngr.m_NonTodLightList.Count; i++)
                {
                    darkMngr.m_NonTodLightList[i].m_Light.intensity *= Settings.options.fillLevel;
                }
            }
        }

        internal static void UpdateDarkLightshafts()
        {
            UpdateSun();

            if (darkMngr.m_LightShaftList.Count > 0)
            {
                for (int i = 0; i < darkMngr.m_LightShaftList.Count; i++)
                {
                    if (Settings.options.trueSun && !AmbientLights.lightOverride)
                    {
                        darkMngr.m_LightShaftList[i].gameObject.SetActive(false);
                        continue;
                    }
                    else
                    {
                        darkMngr.m_LightShaftList[i].gameObject.SetActive(true);
                    }

                    //Light
                    /*if (darkMngr.m_LightShaftList[i].m_Light)
                    {
                        darkMngr.m_LightShaftList[i].m_Light.intensity *= AmbientLights.currentLightSet.lightshaftStr;
                    }

                    //Material
                    if (darkMngr.m_LightShaftList[i].m_LightShaftRenderer != null)
                    {
                        for (int j = 0; j < darkMngr.m_LightShaftList[i].m_LightShaftRenderer.Length; j++)
                        {
                            float gainBase = darkMngr.m_LightShaftList[i].m_LightShaftRenderer[j].material.GetFloat("_Gain");
                            darkMngr.m_LightShaftList[i].m_LightShaftRenderer[j].material.SetFloat("_Gain", gainBase * AmbientLights.currentLightSet.lightshaftStr);
                        }
                    }*/

                    
                }
            }
        }

        /****** SCENE *****/
        internal static void PrepareSceneShadows()
        {
            GameObject[] rObjs = ALUtils.GetRootObjects().ToArray();

            foreach (GameObject rootObj in rObjs)
            {
                MeshRenderer childRenderer = rootObj.GetComponent<MeshRenderer>();
                MeshRenderer[] allRenderers = rootObj.GetComponentsInChildren<MeshRenderer>();
                //allRenderers.Add(childRenderer);
                allRenderers.AddItem(childRenderer);

                foreach (MeshRenderer renderer in allRenderers)
                {

                    //Remove glass from sun layer
                    Material[] mats = renderer.materials;

                    foreach (Material mat in mats)
                    {
                        if (mat.name.ToLower().Contains("glass"))
                        {
                            renderer.gameObject.layer = 7;
                            continue;
                        }
                    }

                    //Ignore
                    if (renderer == null || renderer.gameObject.name.Contains("XPZ_Wall") || renderer.gameObject.name.Contains("XPZ_BaseWindow") ||
                        (renderer.gameObject.name.StartsWith("OBJ_") && !renderer.gameObject.name.ToLower().Contains("door") && !renderer.gameObject.name.ToLower().Contains("curtain") && !renderer.gameObject.name.ToLower().Contains("car")) ||
                        renderer.gameObject.name.StartsWith("GEAR_") ||
                        renderer.gameObject.name.StartsWith("FX_") ||
                        renderer.gameObject.name.StartsWith("CONTAINER_") ||
                        renderer.gameObject.name.StartsWith("INTERACTIVE_")
                    )
                    {
                        continue;
                    }

                    //Hide
                    if (renderer.gameObject.name.ToLower().Contains("shadow_caster")
                        || (renderer.gameObject.name.ToLower().Contains("cylinder") && !renderer.material.name.StartsWith("FX_"))
                        || renderer.gameObject.name.ToLower().Contains("sphere")
                    )
                    {

                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        renderer.gameObject.SetActive(false);

                        continue;
                    }

                    //Glass-Glow
                    if ((renderer.gameObject.name.ToLower().Contains("glass") || renderer.gameObject.name.ToLower().Contains("glow")) && (!renderer.gameObject.name.ToLower().Contains("shadow") && !renderer.gameObject.name.ToLower().Contains("truck") && !renderer.gameObject.name.ToLower().Contains("car") && !renderer.gameObject.name.ToLower().Contains("xpz_window")))
                    {
                        renderer.gameObject.name += "_XPZ_BaseWindow";

                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                        continue;
                    }
                    else if (renderer.gameObject.name.ToLower().Contains("glow") && renderer.gameObject.name.ToLower().Contains("shadow"))
                    {
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        renderer.gameObject.SetActive(false);

                        continue;
                    }

                    if (renderer.gameObject.name.ToLower().Contains("decal"))
                    {
                        renderer.receiveShadows = true;
                        qd_Decal decalInst = renderer.GetComponent<qd_Decal>();

                        vp_Layer.Set(renderer.gameObject, 7, false);

                        continue;
                    }

                    //Rest - Cast shadows if not transparent
                    if (renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off || renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On)
                    {
                        //Material[] mats = renderer.materials;

                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

                        /*foreach (Material mat in mats)
                        {
                            if (mat.name.ToLower().Contains("glass"))
                            {
                                renderer.gameObject.layer = 7;
                                continue;
                            }
                        }*/

                    }
                    else if (renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
                    {
                        FlipMesh(renderer.gameObject);
                    }
                }
            }
        }

        internal static void WindowsCastShadow(GameObject go)
        {
            if (go.transform.parent == null)
            {
                return;
            }

            GameObject windowFrame = go.transform.parent.gameObject;

            if (windowFrame.name.ToLower().Contains("window"))
            {
                windowFrame.layer = vp_Layer.Buildings;
                Renderer renderer = windowFrame.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
                }
            }
        }

        internal static void FlipMesh(GameObject go)
        {
            Renderer rend = go.GetComponent<Renderer>();

            if (rend == null || rend.isPartOfStaticBatch)
            {
                return;
            }

            var mesh = go.GetComponent<MeshFilter>().mesh;
            var vertices = mesh.vertices;
            var uv = mesh.uv;
            var normals = mesh.normals;
            var szV = vertices.Length;
            var newVerts = new Vector3[szV * 2];
            var newUv = new Vector2[szV * 2];
            var newNorms = new Vector3[szV * 2];
            for (var j = 0; j < szV; j++)
            {
                // duplicate vertices and uvs:
                newVerts[j] = newVerts[j + szV] = vertices[j];
                newUv[j] = newUv[j + szV] = uv[j];
                // copy the original normals...
                newNorms[j] = normals[j];
                // and revert the new ones
                newNorms[j + szV] = -normals[j];
            }
            var triangles = mesh.triangles;
            var szT = triangles.Length;
            var newTris = new int[szT * 2]; // double the triangles
            for (var i = 0; i < szT; i += 3)
            {
                // copy the original triangle
                newTris[i] = triangles[i];
                newTris[i + 1] = triangles[i + 1];
                newTris[i + 2] = triangles[i + 2];
                // save the new reversed triangle
                var j = i + szT;
                newTris[j] = triangles[i] + szV;
                newTris[j + 2] = triangles[i + 1] + szV;
                newTris[j + 1] = triangles[i + 2] + szV;
            }
            mesh.vertices = newVerts;
            mesh.uv = newUv;
            mesh.normals = newNorms;
            mesh.triangles = newTris; // assign triangles last!
        }

        /****** UTILS ******/
        internal static void ResetLooseLights()
        {
            if (AmbientLights.enableGameLights && !AmbientLights.lightOverride)
            {
                foreach (Light eLight in gameExtraLightsList)
                {
                    if (!eLight.gameObject.name.StartsWith("XPZ_Light"))
                    {
                        eLight.intensity = 1;
                    }
                }
            }
        }

        internal static void ToggleWindows()
        {
            foreach (Renderer window in gameWindows)
            {
                window.gameObject.SetActive(hideWindows);
            }

            hideWindows = !hideWindows;
        }


    }
}
