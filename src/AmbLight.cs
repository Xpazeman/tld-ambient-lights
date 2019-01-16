using UnityEngine;
using System;
using System.Collections.Generic;

namespace AmbientLights
{
    internal class AmbLight
    {
        internal GameObject go;
        internal Light light;

        internal List<Light> gameLights = new List<Light>();

        internal string orientation = "";
        internal float lightSize = 1f;
        internal float lightCover = 0f;

        internal LightOrientation currentSet = null;

        internal AmbLight(Vector3 lightPos, string orientation, float lightSize, float lightCover)
        {
            go = new GameObject();

            this.orientation = orientation;
            this.lightSize = lightSize;
            this.lightCover = lightCover;

            Light newLight = go.AddComponent<Light>();
            light = newLight;

            go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            go.transform.position = lightPos;

            light.intensity = 0f;
            light.range = 0f;
            if (AmbientLights.options.castShadows)
            {
                light.shadows = LightShadows.Soft;
            }
            else
            {
                light.shadows = LightShadows.None;
            }
            
            light.enabled = false;

            //Debug.Log(Utils.SerializeObject(ALUtils.gameLights));
        }

        internal void AssignGameLights()
        {
            float range = 3f;
            
            //Search in light list and select closer ones
            foreach (Light gLight in GameLights.gameLightsList)
            {
                if (Vector3.Distance(gLight.gameObject.transform.position, this.go.transform.position) < range)
                {
                    Debug.Log("Close light found");
                    this.gameLights.Add(gLight);
                }
            }
        }

        internal void UpdateGameLights()
        {
            foreach (Light gLight in gameLights)
            {
                if (!AmbientLights.enableGameLights)
                {
                    gLight.intensity = 0f;
                }
                else
                {
                    ColorHSV lColor = (Color)currentSet.color;
                    lColor.s *= 0.6f;
                    gLight.color = lColor;
                }
            }
        }

        internal void SetLightParams(LightOrientation set, bool instantApply = false)
        {
            set.intensity *= ALUtils.GetIntensityModifier();
            set.range *= ALUtils.GetRangeModifier();

            if (set.intensity > 0)
            {
                light.enabled = true;
            }
            else
            {
                light.enabled = false;
            }

            if (!AmbientLights.lightOverride)
            {
                SetLightIntensity(set.intensity);
                SetLightRange(set.range);
                SetLightColor(set.color);
            }

            currentSet = set;

            if (AmbientLights.options.castShadows)
            {
                light.shadows = LightShadows.Soft;

                 
                    float str = GetShadowStrength(TimeWeather.currentWeather);

                    if (TimeWeather.currentWeatherPct < 1f)
                    {
                        float prevStr = GetShadowStrength(TimeWeather.previousWeather);

                        str = Mathf.Lerp(prevStr, str, TimeWeather.currentWeatherPct);
                    }

                if (TimeWeather.currentPeriod != "night")
                {
                    str *= .5f;
                }

                light.shadowStrength = str;
                light.renderMode = LightRenderMode.ForceVertex;
            }
            else
            {
                light.shadows = LightShadows.None;
            }

            //DebugLightSet();
        }

        internal void SetLightIntensity(float newIntensity)
        {
            light.intensity = newIntensity * (1f - lightCover);
        }

        internal void SetLightRange(float newRange)
        {
            light.range = newRange * lightSize;
        }

        internal void SetLightColor(Color32 newColor)
        {
            ColorHSV lColor = (Color)newColor;
            lColor.s = Math.Min(lColor.s, 0.8f);
            light.color = lColor;
        }

        internal void DebugLightSet()
        {
            if (AmbientLights.verbose)
                Debug.Log("Intensity: " + light.intensity + ", Range: " + light.range + ", Color: " + light.color);
        }

        internal float GetShadowStrength(string wth)
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
                    str = 0.2f;
                    break;

                case "densefog":
                case "lightsnow":
                case "blizzard":
                    str = 0;
                    break;
            }

            return str;
        }
    }
}
