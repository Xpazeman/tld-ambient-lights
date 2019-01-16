using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Harmony;

namespace AmbientLights
{
    internal class AmbientLights
    {
        public static string modsFolder;
        public static string modDataFolder;
        public static string settingsFile = "config.json";

        public static AmbLitOptions options;

        public static string currentScene;

        public static LightConfig config;
        public static float globalIntMultiplier = 0.9f;
        public static float globalRngMultiplier = 1f;

        public static List<AmbLight> lightList = new List<AmbLight>();

        public static bool lightsInit = false;
        public static bool timeInit = false;
        public static bool weatherInit = false;

        public static bool lightOverride = false;

        public static bool verbose = true;
        public static bool showGameLights = false;
        public static bool enableGameLights = true;

        public static void OnLoad()
        {
            Debug.Log("[ambient-lights] Version " + Assembly.GetExecutingAssembly().GetName().Version);

            modsFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            modDataFolder = Path.Combine(modsFolder, "ambient-lights");

            AmbLitSettings ambLitSettings = new AmbLitSettings();
            ambLitSettings.AddToModSettings("Ambient Lighting Settings");
            options = ambLitSettings.setOptions;

            //AmbientLightUtils.RegisterCommands();
        }

        public static void Reset(bool firstPass = true)
        {
            lightList.Clear();
                        
            TimeWeather.Reset();

            config = null;

            lightsInit = false;

            lightOverride = false;
            showGameLights = false;
            enableGameLights = true;

            if (firstPass)
            {
                timeInit = false;
                weatherInit = false;

                GameLights.gameLightsList.Clear();
                GameLights.gameExtraLightsList.Clear();
                GameLights.gameSpotLightsList.Clear();

                UnityEngine.Object.Destroy(GameLights.gameLights);

                //Init Aurora Lights
            }
        }

        public static void Unload()
        {
            foreach (AmbLight light in lightList)
            {
                UnityEngine.Object.Destroy(light.go);
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
                    Vector3 newPos = ALUtils.StringToVector3(emitter.position);

                    AmbLight newLight = new AmbLight(newPos, emitter.orientation, emitter.size, emitter.cover);

                    lightList.Add(newLight);
                }

                lightsInit = true;
                MaybeUpdateLightsToPeriod(true);
            }
            else
            {
                Debug.Log("[ambient-lights] ERROR: Config isn't ready.");
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
                    foreach (var light in lightList)
                    {
                        LightOrientation set = lightSet.orientations[light.orientation];

                        if (set != null)
                        {
                            light.SetLightParams(set, forceUpdate);
                        }
                    }
                }

                //AuroraLightsControl.UpdateAuroraLightsRanges();
            }
        }

        public static void Update()
        {
            MaybeUpdateLightsToPeriod();

            if (Input.GetKeyUp(KeyCode.L) && !Input.GetKey(KeyCode.RightControl) && options.enableDebugKey)
            {
                if (lightOverride)
                {
                    lightOverride = false;
                    MaybeUpdateLightsToPeriod(true);
                }
                else
                {
                    lightOverride = true;
                    SetLightsIntensity(0f);
                }
            }
            else if (Input.GetKeyUp(KeyCode.L) && Input.GetKey(KeyCode.RightControl) && options.enableDebugKey)
            {
                Unload();
                Reset(false);
                LoadConfigs();
                HUDMessage.AddMessage("Reloading Config");

            }

            if (Input.GetKeyUp(KeyCode.K) && !Input.GetKey(KeyCode.RightControl) && options.enableDebugKey)
            {
                if (GameLights.gameLights.activeInHierarchy)
                {
                    GameLights.gameLights.SetActive(false);
                }
                else
                {
                    GameLights.gameLights.SetActive(true);
                }
            }
            else if (Input.GetKeyUp(KeyCode.K) && Input.GetKey(KeyCode.RightControl) && options.enableDebugKey)
            {
                enableGameLights = !enableGameLights;
                HUDMessage.AddMessage("Game Lights: " + enableGameLights);
            }

            if (Input.GetKeyUp(KeyCode.F9))
            {
                TimeWeather.GetCurrentPeriodAndWeather();

                HUDMessage.AddMessage(TimeWeather.GetCurrentTimeString() + " - " + TimeWeather.currentWeather + " " + TimeWeather.currentPeriod + "(" + (Math.Round(TimeWeather.currentPeriodPct, 2) * 100) + "%)");
            }
        }

        public static void SetupGameLights()
        {
            foreach (AmbLight aLight in lightList)
            {
                aLight.AssignGameLights();
            }
        }

        public static void UpdateGameLights()
        {
            foreach (AmbLight aLight in lightList)
            {
                aLight.UpdateGameLights();
            }

            GameLights.UpdateLights();
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
