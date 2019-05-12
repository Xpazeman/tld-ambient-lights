using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace AmbientLights
{
    class ALUtils
    {
        public static int hourNow;
        public static int minuteNow;

        public static bool debugNext = false;

        public static bool debugMode = true;

        public static Vector3 StringToVector3(string sVector)
        {
            // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")"))
            {
                sVector = sVector.Substring(1, sVector.Length - 2);
            }

            // split the items
            string[] sArray = sVector.Split(',');

            // store as a Vector3
            Vector3 result = new Vector3(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]));

            return result;
        }

        public static int GetCurrentTimeFormatted()
        {
            return (hourNow * 100) + minuteNow;
        }

        public static Color32 ParseColor32(String col)
        {
            string[] strings = col.Split(',');

            Color32 color = new Color32((byte)float.Parse(strings[0]), (byte)float.Parse(strings[1]), (byte)float.Parse(strings[2]), 255);

            return color;
        }

        public static float ParseColorPosition(String col)
        {
            string[] strings = col.Split(',');

            float position = float.Parse(strings[3]);

            return position;
        }

        internal static void RegisterCommands()
        {
            uConsole.RegisterCommand("showgamelights", ShowGameLights);
            uConsole.RegisterCommand("disablelights", DisableGameLights);
            uConsole.RegisterCommand("hidewindows", GameLights.ToggleWindows);
        }

        internal static void HandleHotkeys()
        {
            if (Input.GetKeyUp(KeyCode.L) && Input.GetKey(KeyCode.LeftShift) && AmbientLights.options.enableDebugKey)
            {
                if (AmbientLights.lightOverride)
                {
                    AmbientLights.lightOverride = false;
                    AmbientLights.MaybeUpdateLightsToPeriod(true);

                    HUDMessage.AddMessage("Ambient Lights: On");
                }
                else
                {
                    AmbientLights.lightOverride = true;
                    AmbientLights.SetLightsIntensity(0f);

                    HUDMessage.AddMessage("Ambient Lights: Off");
                }
            }
            else if (Input.GetKeyUp(KeyCode.L) && Input.GetKey(KeyCode.RightShift) && AmbientLights.options.enableDebugKey && AmbientLights.debugVer)
            {
                AmbientLights.Unload();
                AmbientLights.Reset(false);
                AmbientLights.LoadConfigs();
                HUDMessage.AddMessage("Reloading Config");

            }

            if (Input.GetKeyUp(KeyCode.K) && !Input.GetKey(KeyCode.RightControl) && AmbientLights.options.enableDebugKey && AmbientLights.debugVer)
            {
                ShowGameLights();
            }
            else if (Input.GetKeyUp(KeyCode.K) && Input.GetKey(KeyCode.RightControl) && AmbientLights.options.enableDebugKey && AmbientLights.debugVer)
            {
                DisableGameLights();
            }

            if (Input.GetKeyUp(KeyCode.F7) && Input.GetKey(KeyCode.RightControl))
            {
                TimeWeather.GetCurrentPeriodAndWeather();

                HUDMessage.AddMessage(AmbientLights.currentScene + " " + TimeWeather.GetCurrentTimeString() + " - " + TimeWeather.currentWeather + " (" + (Math.Round(TimeWeather.currentWeatherPct, 2) * 100) + " %)" + TimeWeather.currentPeriod + "(" + (Math.Round(TimeWeather.currentPeriodPct, 2) * 100) + "%)");

                ALUtils.debugNext = true;
            }
        }

        internal static void ShowGameLights()
        {
            if (GameLights.gameLights.activeInHierarchy)
            {
                GameLights.gameLights.SetActive(false);

                HUDMessage.AddMessage("Show Game Lights Debug.");
            }
            else
            {
                GameLights.gameLights.SetActive(true);

                HUDMessage.AddMessage("Hide Game Lights Debug.");
            }
        }

        internal static void DisableGameLights()
        {
            AmbientLights.enableGameLights = !AmbientLights.enableGameLights;
            Debug.Log("[ambient-lights] Game lights disabled.");
        }

        internal static float GetIntensityModifier()
        {
            float intMod = AmbientLights.options.intensityMultiplier * AmbientLights.config.data.options.intensity_multiplier * AmbientLights.currentLightSet.intMod * AmbientLights.globalIntMultiplier;

            if (TimeWeather.currentPeriod == "night")
            {
                float nightMod = GetIntensityNightMod();
                intMod *= nightMod;
            }

            return intMod;
        }

        internal static float GetRangeModifier()
        {
            float rngMod = AmbientLights.options.rangeMultiplier * AmbientLights.config.data.options.range_multiplier * AmbientLights.currentLightSet.rngMod * AmbientLights.globalRngMultiplier;

            if (TimeWeather.currentPeriod == "night")
            {
                float nightMod = GetRangeNightMod();
                rngMod *= nightMod;
            }

            return rngMod;
        }

        internal static float GetIntensityNightMod()
        {
            float nightMod = 1f;

            switch (AmbientLights.options.nightBrightness)
            {
                case 0:
                    nightMod = 0f;
                    break;

                case 2:
                    nightMod = 1.5f;
                    break;

                case 3:
                    nightMod = 1.8f;
                    break;
            }

            return nightMod;
        }

        internal static float GetRangeNightMod()
        {
            float nightMod = 1f;

            switch (AmbientLights.options.nightBrightness)
            {
                case 0:
                    nightMod = 0f;
                    break;

                case 2:
                    nightMod = 1.5f;
                    break;

                case 3:
                    nightMod = 2f;
                    break;
            }

            return nightMod;
        }

        internal static float GetShadowStrength(string wth)
        {
            float str = 1;

            switch (wth)
            {
                case "clear":
                    str = 1f;
                    break;

                case "partlycloudy":
                    str = 0.5f;
                    break;

                case "cloudy":
                case "lightfog":
                    str = 0.1f;
                    break;

                case "densefog":
                case "lightsnow":
                case "blizzard":
                    str = 0;
                    break;
            }

            return str;
        }

        internal static Color GetRandomColor()
        {
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

            System.Random random = new System.Random();
            int index = random.Next(0, colors.Length);

            return colors[index];
        }

        internal static List<GameObject> GetRootObjects()
        {
            List<GameObject> rootObj = new List<GameObject>();

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                GameObject[] sceneObj = scene.GetRootGameObjects();

                foreach (GameObject obj in sceneObj)
                {
                    rootObj.Add(obj);
                }
            }

            return rootObj;
        }

        internal static void GetChildrenWithName(GameObject obj, string name, List<GameObject> result)
        {
            if (obj.transform.childCount > 0) {

                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    GameObject child = obj.transform.GetChild(i).gameObject;

                    if (child.name.ToLower().Contains(name))
                    {
                        result.Add(child);
                    }

                    GetChildrenWithName(child, name, result);
                }
            }
        }
    }
}
