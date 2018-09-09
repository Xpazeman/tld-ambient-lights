using System.Reflection;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AmbientLights
{
    class AmbientLightControl
    {
        public static string mods_folder;
        public static string mod_data_folder;

        public static List<AmbientLight> light_list = new List<AmbientLight>();

        public static string current_scene;
        public static string current_period;

        public static AmbientPeriodTransition period_transition = new AmbientPeriodTransition();

        public static string current_weather;

        public static bool first_tick;

        public static AmbientLocationConfig config = null;
        public static Dictionary<string, AmbientConfigPeriod> global_periods_config = null;
        public static Dictionary<string, AmbientPeriodItem> periods_data = null;

        public static bool light_override = false;

        static AmbientLightControl()
        {
            mods_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            mod_data_folder = Path.Combine(mods_folder, "ambient-lights");

            AmbientLightUtils.RegisterCommands();
        }

        public static void ResetAmbientLights()
        {
            Debug.Log("[ambient-lights] Light data cleared.");

            light_list.Clear();

            config = null;
            global_periods_config = null;
            periods_data = null;

            period_transition = new AmbientPeriodTransition();

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

                string scene_file = "scene_" + current_scene + ".json";

                if (File.Exists(Path.Combine(AmbientLightControl.mod_data_folder, scene_file)))
                {
                    config = Utils.DeserializeObject<AmbientLocationConfig>(File.ReadAllText(Path.Combine(AmbientLightControl.mod_data_folder, scene_file)));

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
                if (File.Exists(Path.Combine(AmbientLightControl.mod_data_folder, "global_periods.json")))
                {
                    periods_data = Utils.DeserializeObject<Dictionary<string, AmbientPeriodItem>>(File.ReadAllText(Path.Combine(AmbientLightControl.mod_data_folder, "global_periods.json")));
                }
                else
                {
                    Debug.Log("[ambient-lights] ERROR: No global periods data found");
                }

                if (File.Exists(Path.Combine(AmbientLightControl.mod_data_folder, "global_sets.json")))
                {
                    global_periods_config = Utils.DeserializeObject<Dictionary<string, AmbientConfigPeriod>>(File.ReadAllText(Path.Combine(AmbientLightControl.mod_data_folder, "global_sets.json")));
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

                    period_transition.start = now_time;
                    

                    if (period_name == current_period && weather_name != current_weather)
                    {
                        period_transition.duration = 15;
                        period_transition.end = now_time + 15;
                    }
                    else
                    {
                        period_transition.duration = TimeWeather.GetPeriodChangeDuration(period_name);
                        period_transition.end = now_time + TimeWeather.GetPeriodChangeDuration(period_name);
                    }

                    period_transition.complete = force_update;

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
                else if (now_time > period_transition.start && now_time <= period_transition.end && !period_transition.complete)
                {
                    
                    float time_passed = now_time - period_transition.start;
                    float transition_pos = time_passed / period_transition.duration;

                    //Debug.Log("[ambient-lights] Transition position: " + transition_pos.ToString());

                    foreach (var light in light_list)
                    {
                        light.UpdateLightTransition(transition_pos);
                    }
                }
                else if (now_time > period_transition.end && !period_transition.complete)
                {
                    //Debug.Log("[ambient-lights] End of transition.");
                    period_transition.complete = true;
                }
            }
            else
            {
                first_tick = false;
            }
        }

        public static void Update()
        {
            TimeOfDay tod = GameManager.GetTimeOfDayComponent();

            if (AmbientLightUtils.minute_now != tod.GetMinutes())
            {
                AmbientLightUtils.hour_now = tod.GetHour();
                AmbientLightUtils.minute_now = tod.GetMinutes();

                AmbientLightControl.MaybeUpdateLightsToPeriod();
            }

            if (Input.GetKeyUp(KeyCode.L) && !Input.GetKey(KeyCode.LeftControl) && AmbientLightsOptions.enable_debug_key)
            {
                if (AmbientLightControl.light_override)
                {
                    AmbientLightControl.light_override = false;
                    AmbientLightControl.MaybeUpdateLightsToPeriod(true);
                }
                else
                {
                    AmbientLightControl.light_override = true;
                    AmbientLightControl.SetLightsIntensity(0f);
                }
            }
            else if (Input.GetKeyUp(KeyCode.L) && Input.GetKey(KeyCode.LeftControl) && AmbientLightsOptions.enable_debug_key)
            {
                AmbientLightControl.RemoveLights();
                AmbientLightControl.ResetAmbientLights();
                AmbientLightControl.RegisterLights();
            }
        }

        public static float GetIntensityModifier()
        {
            if (current_period != "night" && current_period != "early_night")
            {
                return AmbientLightsOptions.intensity_multiplier * config.options.intensity_multiplier;
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
                return AmbientLightsOptions.range_multiplier * config.options.range_multiplier;
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
                    night_mod = 0f;
                    break;

                case 2:
                    night_mod = 1.3f;
                    break;

                case 3:
                    night_mod = 1.7f;
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
                    night_mod = 0f;
                    break;

                case 2:
                    night_mod = 1.5f;
                    break;

                case 3:
                    night_mod = 2f;
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
