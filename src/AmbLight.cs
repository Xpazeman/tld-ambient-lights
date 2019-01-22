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
        internal List<Material> gameWindows = new List<Material>();

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

            go.name = "XPZ_Light";

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
                if (Vector3.Distance(gLight.gameObject.transform.position, go.transform.position) < range)
                {
                    Debug.Log("Close light found");
                    gameLights.Add(gLight);
                }
            }

            /*foreach (MeshRenderer window in GameLights.gameWindows)
            {
                if (Vector3.Distance(window.gameObject.transform.position, go.transform.position) < range)
                {
                    Debug.Log("Window found");
                    MeshRenderer component = window.GetComponent<MeshRenderer>();
                    if (!(component == null))
                    {
                        int num = window.materials.Length;
                        if (num != 0)
                        {
                            for (int l = 0; l < num; l++)
                            {
                                if (window.materials[l].shader.name == "Shader Forge/TLD_StandardComplexProp")
                                {
                                    gameWindows.Add(window.materials[l]);
                                }
                            }
                        }
                    }
                }
            }*/
        }

        internal void UpdateGameLights()
        {
            //Window Lights
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

            //Windows
            /*foreach (Material wMat in gameWindows)
            {
                if (AmbientLights.enableGameLights)
                {
                    ColorHSV lColor = (Color)currentSet.color;

                    AmbPeriod prd = AmbientLights.config.GetPeriodSet();

                    float sMod = 0.6f;

                    if (prd.orientations.ContainsKey(orientation))
                    {
                        if (prd.orientations[orientation].sat != null)
                        {
                            sMod *= Mathf.Lerp(prd.orientations[orientation].sat[0], prd.orientations[orientation].sat[1], TimeWeather.currentPeriodPct);
                        }
                    }

                    lColor.s *= sMod;
                    lColor.v = Mathf.Lerp(prd.intensity[0], prd.intensity[1], TimeWeather.currentPeriodPct);
                    wMat.color = lColor;

                    wMat.color = Color.red;
                }
            }*/
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

                float str = ALUtils.GetShadowStrength(TimeWeather.currentWeather);

                if (TimeWeather.currentWeatherPct < 1f)
                {
                    float prevStr = ALUtils.GetShadowStrength(TimeWeather.previousWeather);

                    str = Mathf.Lerp(prevStr, str, TimeWeather.currentWeatherPct);
                }

                if (TimeWeather.currentPeriod == "night")
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

        
    }
}
