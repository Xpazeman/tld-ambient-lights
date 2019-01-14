using UnityEngine;

namespace AmbientLights
{
    internal class AmbLight
    {
        internal GameObject go;
        internal Light light;

        internal string orientation = "";
        internal float lightSize = 1f;
        internal float lightCover = 0f;

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
            light.shadows = LightShadows.None;
            light.enabled = false;
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
            light.color = newColor;
        }

        internal void DebugLightSet()
        {
            if (AmbientLights.verbose)
                Debug.Log("Intensity: " + light.intensity + ", Range: " + light.range + ", Color: " + light.color);
        }

        
    }
}
