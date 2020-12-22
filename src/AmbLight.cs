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

        internal GameObject eLightMark;

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
            go.transform.position = new Vector3(lightPos.x, lightPos.y, lightPos.z);

            eLightMark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eLightMark.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            eLightMark.transform.position = go.transform.position;

            eLightMark.transform.parent = go.transform;

            foreach (Renderer rend in eLightMark.GetComponentsInChildren<Renderer>())
            {
                if (orientation == "west")
                {
                    rend.material.color = Color.red;
                }else if (orientation == "east")
                {
                    rend.material.color = Color.yellow;
                }else if (orientation == "north")
                {
                    rend.material.color = Color.green;
                }else if (orientation == "south")
                {
                    rend.material.color = Color.magenta;
                }
                else
                {
                    rend.material.color = Color.green;
                }
                rend.receiveShadows = false;
            }

            eLightMark.SetActive(false);

            go.name = "XPZ_Light";

            light.intensity = 0f;
            light.range = 0f;
            
            light.enabled = false;
        }

        internal void AssignGameLights()
        {
            float range = 5f;
            
            //Search in light list and select closer ones
            foreach (Light gLight in GameLights.gameLightsList)
            {
                if (Vector3.Distance(gLight.gameObject.transform.position, go.transform.position) < (range * lightSize))
                { 
                    gLight.gameObject.name = "XPZ_Light";
                    gameLights.Add(gLight);
                }
            }
        }

        internal void UpdateGameLights()
        {
            //Window Lights
            if (!AmbientLights.lightOverride)
            {
                foreach (Light gLight in gameLights)
                {
                    if (!AmbientLights.enableGameLights)
                    {
                        gLight.intensity = 0f;
                    }
                    else
                    {
                        if (currentSet != null)
                        {
                            ColorHSV lColor = (Color)currentSet.color;
                            lColor.s *= Mathf.Min(AmbientLights.config.ApplyWeatherSaturationMod() - 0.2f, 0.7f);
                            gLight.color = lColor;

                            if (gLight.shadows != LightShadows.None)
                            {
                                gLight.shadowStrength = AmbientLights.currentLightSet.shadowStr;
                            }
                        }
                    }
                }
            }

            if (AmbientLights.showGameLights && !eLightMark.active)
            {
                eLightMark.SetActive(true);
            }
            else if (!AmbientLights.showGameLights && eLightMark.active)
            {
                eLightMark.SetActive(false);
            }
        }

        internal void SetLightParams(LightOrientation set, bool instantApply = false)
        {
            set.intensity *= ALUtils.GetIntensityModifier();
            set.range *= ALUtils.GetRangeModifier();

            currentSet = set;

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
            else
            {
                light.shadows = LightShadows.None;
            }
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
            if (AmbientLights.debugVer)
                Debug.Log("Intensity: " + light.intensity + ", Range: " + light.range + ", Color: " + light.color);
        }

        
    }
}
