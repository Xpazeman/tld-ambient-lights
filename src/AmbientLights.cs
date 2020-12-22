using System.Reflection;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MelonLoader;

namespace AmbientLights
{
    internal class AmbientLights : MelonMod
    {
        public static readonly string MODS_FOLDER_PATH = Path.GetFullPath(typeof(MelonMod).Assembly.Location + @"\..\..\Mods\ambient-lights");

        public static string currentScene;

        public static LightConfig config = null;
        public static float globalIntMultiplier = 0.9f;
        public static float globalRngMultiplier = 1f;
        public static float globalAmbienceLevel = 0.5f;

        public static List<AmbLight> lightList = new List<AmbLight>();

        public static bool lightsInit = false;
        public static bool timeInit = false;
        public static bool weatherInit = false;

        public static bool lightOverride = false;

        public static bool verbose = false;
        public static bool debugVer = true;
        public static bool showGameLights = false;
        public static bool enableGameLights = true;

        public static LightSet currentLightSet;

        public override void OnApplicationStart()
        {
            Settings.OnLoad();

            Debug.Log("[ambient-lights] Version " + Assembly.GetExecutingAssembly().GetName().Version);
        }

        public static void Reset(bool firstPass = true)
        {
            if (AmbientLights.debugVer) Debug.Log("[ambient-lights] Scene reset.");

            lightList.Clear();
                        
            config = null;

            lightsInit = false;

            lightOverride = false;
            showGameLights = false;
            enableGameLights = true;

            if (firstPass)
            {
                TimeWeather.Reset();
                timeInit = false;
                weatherInit = false;
            }
        }

        public static void Unload(bool reload = false)
        {
            foreach (AmbLight light in lightList)
            {
                UnityEngine.Object.Destroy(light.go);
            }

            if (!reload)
            {
                GameLights.gameLightsList.Clear();
                GameLights.gameExtraLightsList.Clear();
                GameLights.gameSpotLightsList.Clear();
                GameLights.gameExtraLightsColors.Clear();
                GameLights.gameExtraLightsIntensity.Clear();
                GameLights.gameShaftsList.Clear();
                GameLights.gameWindows.Clear();

                GameLights.gameLightsReady = false;

                UnityEngine.Object.Destroy(GameLights.gameLights);
            }
        }

        public static void LoadConfigs()
        {
            currentScene = GameManager.m_ActiveScene;

            Debug.Log("[ambient-lights] Loaded Scene: " + currentScene);

            if (currentScene != "MainMenu")
            {
                config = new LightConfig();
                config.Load();

                if (config.ready)
                    currentLightSet = config.GetCurrentLightSet();

                SetupLights();
            }
        }

        public static void SetupLights()
        {
            if (config.ready)
            {
                Debug.Log("[ambient-lights] Setting up " + config.data.emitters.Count + " light sources for scene.");

                foreach (AmbEmitter emitter in config.data.emitters)
                {
                    //Debug.Log("Emitter pos: " + emitter.position);

                    Vector3 newPos = ALUtils.StringToVector3(emitter.position);

                    AmbLight newLight = new AmbLight(newPos, emitter.orientation, emitter.size, emitter.cover);

                    lightList.Add(newLight);
                }

                lightsInit = true;
                MaybeUpdateLightsToPeriod(true);

                if (!GameLights.gameLightsReady)
                {
                    GameLights.AddGameLights();
                }
            }
            else
            {
                Debug.Log("[ambient-lights] ERROR: Config isn't ready or not present.");
            }
        }

        public static void MaybeUpdateLightsToPeriod(bool forceUpdate = false)
        {
            if (lightsInit && timeInit && weatherInit && config.ready)
            {
                //Generate lightsets for emitters based on time, weather and transition progress

                LightSet lightSet = config.GetCurrentLightSet();
                
                if (lightSet != null)
                {
                    currentLightSet = lightSet;

                    foreach (var light in lightList)
                    {
                        LightOrientation set = lightSet.orientations[light.orientation];

                        if (set != null)
                        {
                            light.SetLightParams(set, forceUpdate);
                        }
                        else
                        {
                            MelonLoader.MelonLogger.Log("[AL] No set matches orientation.");
                        }
                    }
                }
                else
                {
                    MelonLoader.MelonLogger.Log("[AL] No lightset defined");
                }
            }
        }

        public static void Update()
        {
            MaybeUpdateLightsToPeriod();

            ALUtils.HandleHotkeys();
        }

        public static void SetupGameLights()
        {
            foreach (AmbLight aLight in lightList)
            {
                aLight.AssignGameLights();
            }
        }

        public static void UpdateGameLights(bool isDarkMngr = false)
        {
            foreach (AmbLight aLight in lightList)
            {
                aLight.UpdateGameLights();
            }

            if (!isDarkMngr)
            {
                GameLights.UpdateLights();
            }
            
            
        }

        public static void SetLightsIntensity(float intensity = -1f, string set = "")
        {
            if (intensity >= 0f)
            {
                foreach (var light in lightList)
                {
                    if (set == "" || set == light.orientation)
                        light.light.intensity = intensity;
                }
            }
        }

        public static void SetLightsRange(float range = -1f, string set = "")
        {
            if (range >= 0f)
            {
                foreach (var light in lightList)
                {
                    if (set == "" || set == light.orientation)
                        light.light.range = range;
                }
            }
        }

        public static void SetLightsColor(Color32 color, string set = "")
        {
            foreach (var light in lightList)
            {
                if (set == "" || set == light.orientation)
                    light.light.color = color;

            }
        }
    }
}
