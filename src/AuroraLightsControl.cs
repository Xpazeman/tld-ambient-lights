using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AmbientLights
{
    public class ElectrolizerConfig
    {
        public AuroraElectrolizer electrolizer = null;
        public float[] ranges = null;
    }
    class AuroraLightsControl
    {
        public static List<ElectrolizerConfig> electrolizer_objs = new List<ElectrolizerConfig>();

        public static void InitAuroraLights()
        {
            electrolizer_objs.Clear();
        }

        public static void LoadAuroraLight(AuroraElectrolizer electrolizer)
        {
            ElectrolizerConfig new_elect = new ElectrolizerConfig();

            new_elect.electrolizer = electrolizer;
            new_elect.ranges = new float[electrolizer.m_LocalLights.Length];

            for (int i = 0; i < electrolizer.m_LocalLights.Length; i++)
            {
                float cur_range = electrolizer.m_LocalLights[i].range;
                new_elect.ranges[i] = cur_range;
            }

            electrolizer_objs.Add(new_elect);
        }

        public static void UpdateAuroraLightsRanges()
        {
            for (int e=0; e < electrolizer_objs.Count; e++)
            {
                for (int i = 0; i < electrolizer_objs[e].electrolizer.m_LocalLights.Length; i++)
                {
                    float cur_range = electrolizer_objs[e].ranges[i];

                    float range_multiplier = AmbientLightControl.config.options.aurora_range_multiplier;

                    cur_range *= range_multiplier;
                    cur_range = Math.Min(cur_range, 20f);

                    electrolizer_objs[e].electrolizer.m_LocalLights[i].range = cur_range;
                }
            }
        }

        public static void UpdateAuroraLight(AuroraElectrolizer electrolizer)
        {
            float aurora_int = AmbientLightUtils.GetPrivateFieldFloat(electrolizer, "m_AuroraLightFade");
            float static_int = AmbientLightUtils.GetPrivateFieldFloat(electrolizer, "m_StaticIntensity");

            for (int i = 0; i < electrolizer.m_LocalLights.Length; i++)
            {

                float cur_intensity = electrolizer.m_LocalLights[i].intensity;

                if (AmbientLightsOptions.disable_aurora_flicker)
                {
                    cur_intensity = aurora_int * static_int;
                }

                if (AmbientLightControl.config != null)
                {
                    float int_multiplier = AmbientLightsOptions.aurora_intensity * AmbientLightControl.config.options.aurora_intensity_multiplier;

                    int_multiplier = Math.Max(int_multiplier, 1f);

                    cur_intensity *= int_multiplier;
                }

                electrolizer.m_LocalLights[i].intensity = cur_intensity;

                AmbientLightUtils.SetPrivateFieldFloat(electrolizer, "m_CurIntensity", cur_intensity);
            }
        }
    }
}
