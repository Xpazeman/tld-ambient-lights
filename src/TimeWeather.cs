using Harmony;
using UnityEngine;

namespace AmbientLights
{
    internal class TimeWeather
    {
        public static string currentPeriod = "default";
        public static float currentPeriodPct = 1f;
        public static string currentWeather = "default";
        public static string previousWeather = "default";
        public static float currentWeatherPct = 1f;

        internal static void Reset()
        {
            currentPeriod = "default";
            currentPeriodPct = 1f;
            currentWeather = "default";
            previousWeather = "default";
            currentWeatherPct = 1f;
        }

        internal static void GetCurrentPeriodAndWeather()
        {
            
            UniStormWeatherSystem uniStorm = GameManager.GetUniStorm();
            TODBlendState periodState = uniStorm.GetTODBlendState();
            currentPeriodPct = uniStorm.GetTODBlendPercent(periodState);

            switch (periodState)
            {
                case TODBlendState.NightStartToNightEnd:
                    currentPeriod = "night";
                    break;

                case TODBlendState.NightEndToDawn:
                    currentPeriod = "early_dawn";
                    break;

                case TODBlendState.DawnToMorning:
                    currentPeriod = "dawn";
                    break;

                case TODBlendState.MorningToMidday:
                    currentPeriod = "morning";
                    break;

                case TODBlendState.MiddayToAfternoon:
                    currentPeriod = "afternoon";
                    break;

                case TODBlendState.AfternoonToDusk:
                    currentPeriod = "dusk";
                    break;

                case TODBlendState.DuskToNightStart:
                    currentPeriod = "after_dusk";
                    break;
            }

            float transTime = Traverse.Create(uniStorm).Field("m_WeatherTransitionTime").GetValue<float>();
            float transTimeElapsed = Traverse.Create(uniStorm).Field("m_SecondsSinceLastWeatherChange").GetValue<float>();

            currentWeatherPct = Mathf.Clamp01(transTimeElapsed / transTime);

            Weather wth = GameManager.GetWeatherComponent();

            currentWeather = GetWeatherStageName(wth.GetWeatherStage());
            previousWeather = GetWeatherStageName(uniStorm.m_PreviousWeatherStage);
        }

        internal static string GetWeatherStageName(WeatherStage stage)
        {
            string weatherName = "default";

            switch (stage)
            {
                case WeatherStage.ClearAurora:
                    weatherName = "aurora";
                    break;

                case WeatherStage.Clear:
                    weatherName = "clear";
                    break;

                case WeatherStage.PartlyCloudy:
                    weatherName = "partlycloudy";
                    break;

                case WeatherStage.Cloudy:
                    weatherName = "cloudy";
                    break;

                case WeatherStage.LightFog:
                    weatherName = "lightfog";
                    break;

                case WeatherStage.DenseFog:
                    weatherName = "densefog";
                    break;

                case WeatherStage.LightSnow:
                    weatherName = "lightsnow";
                    break;

                case WeatherStage.HeavySnow:
                case WeatherStage.Blizzard:
                    weatherName = "blizzard";
                    break;

                default:
                    weatherName = "default";
                    break;
            }

            return weatherName;
        }
    }
}
