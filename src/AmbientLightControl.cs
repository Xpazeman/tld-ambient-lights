using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace AmbientLights
{
    class AmbientLightControl
    {
        public static List<AmbientLight> light_list = new List<AmbientLight>();

        public static string current_scene;
        public static string current_period;

        public static int current_period_start;
        public static int current_period_transition_duration;
        public static int current_period_transition_end;
        public static bool current_period_transition_complete = true;

        public static string current_weather;

        public static bool first_tick;

        public static AmbientLocationConfig config = null;
        public static Dictionary<string, AmbientConfigPeriod> global_periods_config = null;
        public static Dictionary<string, AmbientPeriodItem> periods_data = null;

        public static bool light_override = false;

        static AmbientLightControl()
        {
            
            AmbientLightUtils.RegisterCommands();
        }

        public static void ResetAmbientLights()
        {
            Debug.Log("[ambient-lights] Light data cleared.");

            light_list.Clear();

            config = null;
            global_periods_config = null;
            periods_data = null;

            current_scene = null;
            current_period = null;
            current_weather = null;

            light_override = false;
        }

        public static void RemoveLights()
        {
            foreach(AmbientLight light in light_list)
            {
                UnityEngine.Object.Destroy(light.light_go);
            }
        }

        public static void RegisterLights()
        {
            current_scene = GameManager.m_ActiveScene;

            Debug.Log("[ambient-lights] Loaded Scene: "+current_scene);

            if (current_scene != "MainMenu")
            {
                GetGlobalConfig();

                if (File.Exists(@"mods/ambient-lights/scene_" + current_scene + ".json"))
                {
                    config = Utils.DeserializeObject<AmbientLocationConfig>(File.ReadAllText(@"mods/ambient-lights/scene_" + current_scene + ".json"));

                    MergeConfigs();
                }
                else
                {
                    Debug.Log("[ambient-lights] No lighting data for scene " + current_scene + " found. Using game default.");
                }
            }
        }

        public static void GetGlobalConfig()
        {
            if (current_scene != "MainMenu")
            {
                if (File.Exists(@"mods/ambient-lights/global_periods.json"))
                {
                    periods_data = Utils.DeserializeObject<Dictionary<string, AmbientPeriodItem>>(File.ReadAllText(@"mods/ambient-lights/global_periods.json"));
                }
                else
                {
                    Debug.Log("[ambient-lights] ERROR: No global periods data found");
                }

                if (File.Exists(@"mods/ambient-lights/global_sets.json"))
                {
                    global_periods_config = Utils.DeserializeObject<Dictionary<string, AmbientConfigPeriod>>(File.ReadAllText(@"mods/ambient-lights/global_sets.json"));
                }
                else
                {
                    Debug.Log("[ambient-lights] ERROR: No global sets data found");
                }
            }
        }

        

        public static void MergeConfigs()
        {
            foreach (KeyValuePair<string, AmbientConfigPeriod> prd in global_periods_config)
            {
                if (config.periods.ContainsKey(prd.Key))
                {
                    foreach (KeyValuePair<string, AmbientConfigWeather> wth in global_periods_config[prd.Key].weathers)
                    {
                        if (config.periods[prd.Key].weathers.ContainsKey(wth.Key))
                        {
                            foreach (KeyValuePair<string, AmbientConfigPeriodSet> set in global_periods_config[prd.Key].weathers[wth.Key].orientations)
                            {
                                if (!config.periods[prd.Key].weathers[wth.Key].orientations.ContainsKey(set.Key))
                                {
                                    config.periods[prd.Key].weathers[wth.Key].orientations.Add(set.Key, set.Value);
                                }
                            }
                        }
                        else
                        {
                            config.periods[prd.Key].weathers.Add(wth.Key, wth.Value);
                        }
                    }
                }
                else
                {
                    config.periods.Add(prd.Key, prd.Value);
                }
            }

            //Debug.Log("[ambient-lights] Config:");
            //Debug.Log(Utils.SerializeObject(_config));

            SetupLights();
        }

        public static void SetupLights()
        {
            Debug.Log("[ambient-lights] Setting up "+ config.emitters.Count+" light sources for scene.");

            foreach (AmbientConfigEmitter emitter in config.emitters)
            {
                Vector3 new_pos = AmbientLightUtils.StringToVector3(emitter.position);

                AmbientLight new_light = new AmbientLight(new_pos, emitter.orientation, emitter.size, emitter.cover);

                light_list.Add(new_light);
            }
        }

        public static void MaybeUpdateLightsToPeriod(bool force_update = false)
        {
            if (config != null || first_tick)
            {
                int now_time = AmbientLightUtils.GetCurrentTimeFormatted();

                string period_name = TimeWeather.GetCurrentPeriod();
                string weather_name = TimeWeather.GetCurrentWeather();

                if (period_name != current_period || weather_name != current_weather || force_update)
                {
                    AmbientConfigPeriod period = TimeWeather.GetPeriodSet(period_name);
                    AmbientConfigWeather weatherSet = TimeWeather.GetWeatherSet(period, weather_name);

                    current_period_start = now_time;
                    

                    if (period_name == current_period && weather_name != current_weather)
                    {
                        current_period_transition_duration = 15;
                        current_period_transition_end = now_time + 15;
                    }
                    else
                    {
                        current_period_transition_duration = TimeWeather.GetPeriodChangeDuration(period_name);
                        current_period_transition_end = now_time + TimeWeather.GetPeriodChangeDuration(period_name);
                    }
                    
                    current_period_transition_complete = force_update;

                    current_weather = weather_name;
                    current_period = period_name;

                    if (AmbientLightsOptions.verbose)
                    {
                        Debug.Log("[ambient-lights] * Light period change:");
                        Debug.Log("[ambient-lights] Period: " + current_period);
                        Debug.Log("[ambient-lights] Weather: " + current_weather);
                    }

                    foreach (var light in light_list)
                    {
                        AmbientConfigPeriodSet set = TimeWeather.GetLightSet(weatherSet, light.light_set);

                        if (set != null)
                        {
                            light.SetLightParams(set, force_update);
                        }
                    }
                }
                else if (now_time > current_period_start && now_time <= current_period_transition_end && !current_period_transition_complete)
                {
                    
                    float time_passed = now_time - current_period_start;
                    float transition_pos = time_passed / current_period_transition_duration;

                    //Debug.Log("[ambient-lights] Transition position: " + transition_pos.ToString());

                    foreach (var light in light_list)
                    {
                        light.UpdateLightTransition(transition_pos);
                    }
                }
                else if (now_time > current_period_transition_end && !current_period_transition_complete)
                {
                    //Debug.Log("[ambient-lights] End of transition.");
                    current_period_transition_complete = true;
                }
            }
            else
            {
                first_tick = false;
            }
        }

        public static float GetIntensityModifier()
        {
            if (current_period != "night" && current_period != "early_night")
            {
                return AmbientLightsOptions.intensity_multiplier;
            }
            else
            {
                float night_mod = GetIntensityNightMod();

                return AmbientLightsOptions.intensity_multiplier * config.options.intensity_multiplier * night_mod;
            }
            
        }

        public static float GetRangeModifier()
        {
            if (current_period != "night" && current_period != "early_night")
            {
                return AmbientLightsOptions.range_multiplier;
            }
            else
            {
                float night_mod = GetRangeNightMod();

                return AmbientLightsOptions.range_multiplier * config.options.range_multiplier * night_mod;
            }
        }

        public static float GetIntensityNightMod()
        {
            float night_mod = 1f;

            switch (AmbientLightsOptions.night_brightness)
            {
                case 0:
                    night_mod = -5f;
                    break;

                case 1:
                    night_mod = 0f;
                    break;

                case 3:
                    night_mod = 1.3f;
                    break;

                case 4:
                    night_mod = 1.7f;
                    break;

                default:
                    night_mod = 1f;
                    break;
            }

            return night_mod;
        }

        public static float GetRangeNightMod()
        {
            float night_mod = 1f;

            switch (AmbientLightsOptions.night_brightness)
            {
                case 0:
                    night_mod = 10f;
                    break;

                case 1:
                    night_mod = 0f;
                    break;

                case 3:
                    night_mod = 1.5f;
                    break;

                case 4:
                    night_mod = 2f;
                    break;

                default:
                    night_mod = 1f;
                    break;
            }

            return night_mod;
        }

        public static void SetLightsIntensity(float intensity = -1f, string set = "")
        {
            if (intensity >= 0f)
            {
                foreach (var light in light_list)
                {
                    if (set == "" || set == light.light_set)
                        light.light_source.intensity = intensity;
                }
            }
        }

        public static void SetLightsRange(float range = -1f, string set = "")
        {
            if (range >= 0f)
            {
                foreach (var light in light_list)
                {
                    if (set == "" || set == light.light_set)
                        light.light_source.range = range;
                }
            }
        }

        public static void SetLightsColor(Color32 color, string set = "")
        {
            foreach (var light in light_list)
            {
                if (set == "" || set == light.light_set)
                    light.light_source.color = color;
            
            }
        }

        public static void SetLightsShadow(string type, string set = "")
        {
            var shadow = LightShadows.None;

            if (type == "soft")
            {
                shadow = LightShadows.Soft;
            }
            else if (type == "hard")
            {
                shadow = LightShadows.Hard;
            }

            foreach (var light in light_list)
            {
                if (set == "" || set == light.light_set)
                    light.light_source.shadows = shadow;

            }
        }
    }
}
