using System;
using System.Globalization;
using Il2CppSystem.Collections.Generic;
using System.Text.RegularExpressions;
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

        public static bool windowShadow = false;

        public static void Log(string text, bool showHUD = true, bool forceLog = false, bool forceHUD = false)
        {
            if (AmbientLights.debugVer || forceLog)
            {
                Debug.Log("[Ambient Lights] "+text);
                MelonLoader.MelonLogger.Msg(text);

                if (showHUD)
                {
                    HUDMessage.AddMessage(text, false, false);
                }
            }
            else if (forceHUD)
            {
                HUDMessage.AddMessage(text, false, false);
            }

        }

        public static void LogHUD(string text, bool isDebug = false)
        {
            if (isDebug && !AmbientLights.debugVer)
            {
                return;
            }
            else
            {
                HUDMessage.AddMessage(text, false, false);
            }
        }

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
                Convert.ToSingle(sArray[0], CultureInfo.InvariantCulture),
                Convert.ToSingle(sArray[1], CultureInfo.InvariantCulture),
                Convert.ToSingle(sArray[2], CultureInfo.InvariantCulture)
            );

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
            /*uConsole.RegisterCommand("showgamelights", ShowGameLights);
            uConsole.RegisterCommand("disablelights", DisableGameLights);
            uConsole.RegisterCommand("hidewindows", GameLights.ToggleWindows);
            uConsole.RegisterCommand("sunoffset", ChangeSunOffset);*/
        }

        internal static void HandleHotkeys()
        {
            
            if ((InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.L) && !InputManager.GetSprintDown(InputManager.m_CurrentContext) && AmbientLights.debugVer) || (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.L) && InputManager.GetSprintDown(InputManager.m_CurrentContext) && Settings.options.enableDebugKey && !AmbientLights.debugVer))
            {
                if (AmbientLights.lightOverride)
                {
                    AmbientLights.lightOverride = false;
                    AmbientLights.MaybeUpdateLightsToPeriod(true);

                    Log("Ambient Lights: On", true, false, true);

                    /*HUDMessage.AddMessage("Ambient Lights: On");
                    if (AmbientLights.debugVer)
                        Debug.Log("Ambient Lights: On");*/
                }
                else
                {
                    AmbientLights.lightOverride = true;
                    AmbientLights.SetLightsIntensity(0f);

                    Log("Ambient Lights: Off", true, false, true);

                    /*HUDMessage.AddMessage("Ambient Lights: Off");
                    if (AmbientLights.debugVer)
                        Debug.Log("Ambient Lights: Off");*/
                }
            }

            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.L) && InputManager.GetSprintDown(InputManager.m_CurrentContext) && AmbientLights.debugVer)
            {
                AmbientLights.Unload(true);
                AmbientLights.Reset(false);
                AmbientLights.LoadConfigs();
                //HUDMessage.AddMessage("Reloading Config");
                Log("Reloading Scene Config", true, false, true);
            }

            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Z) && !InputManager.GetSprintDown(InputManager.m_CurrentContext) && AmbientLights.debugVer)
            {
                GetPoint();
            }

            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.Z) && InputManager.GetSprintDown(InputManager.m_CurrentContext) && AmbientLights.debugVer)
            {
                
                ShowGameLights();
            }

            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.F6) && AmbientLights.debugVer)
            {
                TimeWeather.GetCurrentPeriodAndWeather();

                //HUDMessage.AddMessage(AmbientLights.currentScene + " " + TimeWeather.GetCurrentTimeString() + " - " + TimeWeather.currentWeather + " (" + (Math.Round(TimeWeather.currentWeatherPct, 2) * 100) + " %)" + TimeWeather.currentPeriod + "(" + (Math.Round(TimeWeather.currentPeriodPct, 2) * 100) + "%)");

                Log("Environment data: " + AmbientLights.currentScene + " " + TimeWeather.GetCurrentTimeString() + " - " + TimeWeather.currentWeather + " (" + (Math.Round(TimeWeather.currentWeatherPct, 2) * 100) + " %)" + TimeWeather.currentPeriod + "(" + (Math.Round(TimeWeather.currentPeriodPct, 2) * 100) + "%)", true);

                ALUtils.debugNext = true;
            }
        }

        public static void GetPoint()
        {
            vp_FPSCamera cam = GameManager.GetVpFPSPlayer().FPSCamera;
            RaycastHit raycastHit = DoRayCast(cam.transform.position, cam.transform.forward);

            Log("Point: " + raycastHit.point.x + "," + raycastHit.point.y + "," + raycastHit.point.z, true, false, true);

            /*Debug.Log(raycastHit.point.x+","+ raycastHit.point.y+","+ raycastHit.point.z);
            HUDMessage.AddMessage(raycastHit.point.x + "," + raycastHit.point.y + "," + raycastHit.point.z);
            MelonLoader.MelonLogger.Log("Point: "+ raycastHit.point.x + "," + raycastHit.point.y + "," + raycastHit.point.z);*/
        }

        public static RaycastHit DoRayCast(Vector3 start, Vector3 direction)
        {
            RaycastHit result;
            Physics.Raycast(start, direction, out result, float.PositiveInfinity);
            return result;
        }

        internal static void ShowGameLights()
        {
            if (GameLights.gameLights.active == true)
            {
                //GameLights.gameLights.SetActive(false);
                AmbientLights.showGameLights = false;

                LogHUD("Game Lights Debug OFF.", true);
                //HUDMessage.AddMessage("Game Lights Debug OFF.");
            }
            else
            {
                //GameLights.gameLights.SetActive(true);
                AmbientLights.showGameLights = true;

                LogHUD("Game Lights Debug ON.", true);
                //HUDMessage.AddMessage("Game Lights Debug ON.");
            }
        }

        internal static void DisableGameLights()
        {
            AmbientLights.enableGameLights = !AmbientLights.enableGameLights;

            Log("Show game lights:" + AmbientLights.enableGameLights, false);
            //Debug.Log("Show game lights:"+ AmbientLights.enableGameLights);
        }

        internal static void ChangeSunOffset()
        {

            if (uConsole.GetNumParameters() == 0)
            {
                Log("Current offset: " + GameLights.sunOffset, false);
                //Debug.Log("[ambient-lights] Current offset: " + GameLights.sunOffset);
            }
            else if (uConsole.GetNumParameters() == 1)
            {
                GameLights.sunOffset = uConsole.GetFloat();

                Log("New offset: " + GameLights.sunOffset, false);
                //Debug.Log("[ambient-lights] New offset: " + GameLights.sunOffset);
            }
        }

        internal static float GetIntensityModifier()
        {
            float intMod = Settings.options.intensityMultiplier * AmbientLights.config.data.options.intensity_multiplier * AmbientLights.currentLightSet.intMod * AmbientLights.globalIntMultiplier;

            if (TimeWeather.currentPeriod == "night")
            {
                float nightMod = GetIntensityNightMod();
                intMod *= nightMod;
            }

            return intMod;
        }

        internal static float GetRangeModifier()
        {
            float rngMod = Settings.options.rangeMultiplier * AmbientLights.config.data.options.range_multiplier * AmbientLights.currentLightSet.rngMod * AmbientLights.globalRngMultiplier;

            if (TimeWeather.currentPeriod == "night")
            {
                float nightMod = GetRangeNightMod();
                rngMod *= nightMod;
            }

            return rngMod;
        }

        internal static float GetIntensityNightMod()
        {
            float baseMod = 1f;
            float nightMod = 1f;

            switch (Settings.options.nightBrightness)
            {
                case 0:
                    nightMod = 0f;
                    break;

                case 1:
                    nightMod = 1f;
                    break;

                case 2:
                    nightMod = 1.5f;
                    break;

                case 3:
                    nightMod = 1.8f;
                    break;
            }

            TODBlendState bState = GameManager.GetUniStorm().GetTODBlendState();

            if (bState == TODBlendState.DuskToNightStart)
            {
                baseMod = Mathf.Lerp(1f, nightMod, GameManager.GetUniStorm().GetTODBlendPercent(bState));
            }
            else if (bState == TODBlendState.NightEndToDawn)
            {
                baseMod = Mathf.Lerp(nightMod, 1f, GameManager.GetUniStorm().GetTODBlendPercent(bState));
            }

            return baseMod;
        }

        internal static float GetRangeNightMod()
        {
            float nightMod = 1f;

            switch (Settings.options.nightBrightness)
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
                    str = 0.6f;
                    break;

                case "cloudy":
                case "lightfog":
                    str = 0.1f;
                    break;

                case "densefog":
                case "lightsnow":
                case "heavysnow":
                case "blizzard":
                    str = 0.0f;
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

        internal static double GetRandomNumber(double minimum, double maximum)
        {
            System.Random random = new System.Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
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

        internal static void GetChildrenWithName(GameObject obj, string name, List<GameObject> result, bool isRegex = false, string[] aliases = null)
        {
            Regex rgx = null;

            if (isRegex)
            {
                rgx = new Regex(name);
            }
            

            if (obj.transform.childCount > 0) {

                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    GameObject child = obj.transform.GetChild(i).gameObject;

                    if (!isRegex)
                    {

                        if (child.name.ToLower().Contains(name))
                        {
                            result.Add(child);
                            continue;
                        }

                        if (aliases != null)
                        {
                            for (int j = 0; j < aliases.Length; j++)
                            {
                                if (child.name.ToLower().Contains(aliases[j]))
                                {
                                    result.Add(child);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (rgx.IsMatch(child.name))
                        {
                            result.Add(child);
                            continue;
                        }

                        if (aliases != null)
                        {
                            for (int j = 0; j < aliases.Length; j++)
                            {
                                Regex rgxa = new Regex(aliases[j]);

                                if (rgxa.IsMatch(child.name))
                                {
                                    result.Add(child);
                                    break;
                                }
                            }
                        }
                    }

                    GetChildrenWithName(child, name, result, isRegex, aliases);
                }
            }
        }
    }
}
