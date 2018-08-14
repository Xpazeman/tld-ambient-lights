using UnityEngine;


namespace AmbientLights
{
    public class AmbientLight
    {
        public GameObject light_go;
        public Light light_source;

        public AmbientConfigPeriodSet target_set = null;
        public AmbientConfigPeriodSet start_set = null;

        public string light_set = "";
        public float light_size = 1f;
        public float light_cover = 0f;

        public AmbientLight(Vector3 lightPos, string e_light_set, float e_light_size, float e_light_cover)
        {
            light_go = new GameObject();
            light_set = e_light_set;
            light_size = e_light_size;
            light_cover = e_light_cover;

            Light light = light_go.AddComponent<Light>();
            light_source = light;

            light_go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            light_go.transform.position = lightPos;

            light.intensity = 0f;
            light.range = 0f;
            light.shadows = LightShadows.None;
            light.enabled = false;
        }

        public void SetLightParams(AmbientConfigPeriodSet set, bool instant_apply = false)
        {
            start_set = target_set;

            target_set = new AmbientConfigPeriodSet
            {
                intensity = set.intensity * AmbientLightControl.GetIntensityModifier(),
                range = set.range * AmbientLightControl.GetRangeModifier(),
                color = set.color,
                shadows = set.shadows
            };

            if (start_set == null)
            {
                start_set = target_set;
                instant_apply = true;
            }
            
            if (set.intensity != 0)
            {
                light_source.enabled = true;
            }
            else
            {
                light_source.enabled = false;
            }

            if (AmbientLightControl.config.options.override_shadows != "")
            {
                set.shadows = AmbientLightControl.config.options.override_shadows;
            }

            if (!AmbientLightsOptions.enable_shadows)
            {
                light_source.shadows = LightShadows.None;
            }
            else
            {
                if (set.shadows.ToLower() == "hard")
                {
                    //Hard shadows don't work well
                    //light_source.shadows = LightShadows.Hard;
                    light_source.shadows = LightShadows.Soft;
                }
                else if (set.shadows.ToLower() == "soft")
                {
                    light_source.shadows = LightShadows.Soft;
                }
                else
                {
                    light_source.shadows = LightShadows.None;
                }
            }
            

            if (instant_apply  && !AmbientLightControl.light_override)
            {
                
                SetLightIntensity(target_set.intensity);
                SetLightRange(target_set.range);
                SetLightColor(AmbientLightUtils.ParseColor32(target_set.color));

                DebugLightSet();
            }
        }

        public void UpdateLightTransition(float step)
        {
            float t_intensity = Mathf.Lerp(start_set.intensity, target_set.intensity, step);
            float t_range = Mathf.Lerp(start_set.range, target_set.range, step);
            Color32 t_color = Color32.Lerp(AmbientLightUtils.ParseColor32(start_set.color), AmbientLightUtils.ParseColor32(target_set.color), step);

            if (!AmbientLightControl.light_override)
            {
                SetLightIntensity(t_intensity);
                SetLightRange(t_range);
                SetLightColor(t_color);

                DebugLightSet();
            }
        }

        public void FinishTransition()
        {
            if (!AmbientLightControl.light_override)
            {
                SetLightIntensity(target_set.intensity);
                SetLightRange(target_set.range);
                SetLightColor(AmbientLightUtils.ParseColor32(target_set.color));

                DebugLightSet();
            }
        }

        public void SetLightIntensity(float new_intensity)
        {
            light_source.intensity = new_intensity * (1f - light_cover);
        }

        public void SetLightRange(float new_range)
        {
            light_source.range = new_range * light_size;
        }

        public void SetLightColor(Color32 new_color)
        {
            light_source.color = new_color;
        }

        void DebugLightSet()
        {
            if (AmbientLightsOptions.verbose)
                Debug.Log("Intensity: "+light_source.intensity+", Range: "+light_source.range+", Color: "+light_source.color);
        }
    }
}
