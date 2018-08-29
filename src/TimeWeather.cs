using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AmbientLights
{
    class TimeWeather
    {
        public static string GetCurrentPeriod()
        {
            string period_name = "default";

            int now_time = AmbientLightUtils.GetCurrentTimeFormatted();

            foreach (KeyValuePair<string, AmbientPeriodItem> prd in AmbientLightControl.periods_data)
            {
                if (now_time >= prd.Value.start_hour && now_time < prd.Value.end_hour)
                {
                    period_name = prd.Key;
                }
            }

            return period_name;
        }

        public static int GetPeriodChangeDuration(string periodName)
        {
            if (AmbientLightControl.periods_data.ContainsKey(periodName))
            {
                return AmbientLightControl.periods_data[periodName].change_duration;
            }
            else
            {
                return 0;
            }
        }

        public static string GetCurrentWeather()
        {
            Weather wth = GameManager.GetWeatherComponent();

            switch (wth.GetWeatherStage())
            {
                case WeatherStage.Clear:
                case WeatherStage.ClearAurora:
                case WeatherStage.PartlyCloudy:
                    return "clear";

                case WeatherStage.LightFog:
                case WeatherStage.DenseFog:
                    return "fog";

                case WeatherStage.LightSnow:
                case WeatherStage.Cloudy:
                    return "cloudy";

                case WeatherStage.HeavySnow:
                case WeatherStage.Blizzard:
                    return "blizzard";

                default:
                    return "default";
            }
        }

        public static AmbientConfigPeriod GetPeriodSet(string periodName)
        {
            AmbientConfigPeriod period = null;

            period = AmbientLightControl.config.periods[periodName];

            return period;
        }

        public static AmbientConfigWeather GetWeatherSet(AmbientConfigPeriod prd, string weather_name)
        {
            AmbientConfigWeather select_weather = null;

            select_weather = TryToFetchWeather(prd, weather_name);

            if (select_weather == null && AmbientLightControl.config.periods.ContainsKey("default"))
            {
                //Debug.Log("[ambient-lights] No WeatherSet found, trying default period for current weather.");
                select_weather = TryToFetchWeather(AmbientLightControl.config.periods["default"], weather_name);

                if (select_weather == null && prd.weathers.ContainsKey("default"))
                {
                    //Debug.Log("[ambient-lights] Default Weather selected.");
                    select_weather = prd.weathers["default"];

                    if (select_weather == null)
                    {
                        //Debug.Log("[ambient-lights] No WeatherSet found at default, trying default period for default weather.");
                        select_weather = TryToFetchWeather(AmbientLightControl.config.periods["default"], "default");

                        if (select_weather == null)
                        {
                            Debug.Log("[ambient-lights] ERROR: No default weather found.");
                        }
                    }
                }
            }

            return select_weather;
        }

        public static AmbientConfigWeather TryToFetchWeather(AmbientConfigPeriod prd, string weather_name)
        {
            AmbientConfigWeather select_weather = null;

            if (prd.weathers != null)
            {
                foreach (KeyValuePair<string, AmbientConfigWeather> weather in prd.weathers)
                {
                    if (weather.Key == weather_name)
                    {
                        select_weather = weather.Value;
                    }
                }
            }

            return select_weather;
        }

        public static AmbientConfigPeriodSet GetLightSet(AmbientConfigWeather weather_set, string light_set)
        {
            AmbientConfigPeriodSet set = null;

            if (weather_set != null)
            {
                if (weather_set.orientations.ContainsKey(light_set))
                {
                    set = weather_set.orientations[light_set];
                }
                else if (weather_set.orientations.ContainsKey("default"))
                {
                    set = weather_set.orientations["default"];
                }
                else
                {
                    weather_set = TryToFetchWeather(AmbientLightControl.config.periods["default"], "default");

                    if (weather_set.orientations.ContainsKey(light_set))
                    {
                        set = weather_set.orientations[light_set];
                    }
                    else
                    {
                        Debug.Log("[ambient-lights] ERROR: No orientation found.");
                    }
                    
                }
            }
            else
            {
                Debug.Log("[ambient-lights] ERROR: No WeatherSet found.");
            }

            return set;
        }
    }
}
