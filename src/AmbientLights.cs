using System.Reflection;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AmbientLights
{
    internal class AmbientLights
    {
        public static string modsFolder;
        public static string modDataFolder;
        public static string settingsFile = "config.json";

        public static AmbLitOptions options;

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

        public static bool verbose = true;
        public static bool debugVer = false;
        public static bool showGameLights = false;
        public static bool enableGameLights = true;

        public static LightSet currentLightSet;

        public static void OnLoad()
        {
            Debug.Log("[ambient-lights] Version " + Assembly.GetExecutingAssembly().GetName().Version);

            modsFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            modDataFolder = Path.Combine(modsFolder, "ambient-lights");

            AmbLitSettings ambLitSettings = new AmbLitSettings();
            ambLitSettings.AddToModSettings("Ambient Lights Settings");
            options = ambLitSettings.setOptions;

            ALUtils.RegisterCommands();
        }

        public static void Reset(bool firstPass = true)
        {
            Debug.Log("[ambient-lights] Scene reset.");
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
            }
        }

        public static void Unload()
        {
            foreach (AmbLight light in lightList)
            {
                UnityEngine.Object.Destroy(light.go);
            }

            GameLights.gameLightsList.Clear();
            GameLights.gameExtraLightsList.Clear();
            GameLights.gameSpotLightsList.Clear();
            GameLights.gameExtraLightsColors.Clear();
            GameLights.gameShaftsList.Clear();
            GameLights.gameWindows.Clear();

            UnityEngine.Object.Destroy(GameLights.gameLights);
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
                    }
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
