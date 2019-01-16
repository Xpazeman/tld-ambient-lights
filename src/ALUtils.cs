using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AmbientLights
{
    class ALUtils
    {
        public static int hourNow;
        public static int minuteNow;

       

        public static bool debugMode = true;

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
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]));

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

        internal static float GetIntensityModifier()
        {
            float intMod = AmbientLights.options.intensityMultiplier * AmbientLights.config.data.options.intensity_multiplier * AmbientLights.globalIntMultiplier;

            if (TimeWeather.currentPeriod == "night")
            {
                float nightMod = GetIntensityNightMod();
                intMod *= nightMod;
            }

            return intMod;
        }

        internal static float GetRangeModifier()
        {
            float rngMod = AmbientLights.options.rangeMultiplier * AmbientLights.config.data.options.range_multiplier * AmbientLights.globalRngMultiplier;

            if (TimeWeather.currentPeriod == "night")
            {
                float nightMod = GetRangeNightMod();
                rngMod *= nightMod;
            }

            return rngMod;
        }

        internal static float GetIntensityNightMod()
        {
            float nightMod = 1f;

            switch (AmbientLights.options.nightBrightness)
            {
                case 0:
                    nightMod = 0f;
                    break;

                case 2:
                    nightMod = 1.3f;
                    break;

                case 3:
                    nightMod = 1.7f;
                    break;
            }

            return nightMod;
        }

        internal static float GetRangeNightMod()
        {
            float nightMod = 1f;

            switch (AmbientLights.options.nightBrightness)
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
    }
}
