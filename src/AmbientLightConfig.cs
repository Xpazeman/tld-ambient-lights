using System.Collections.Generic;
using System.Reflection;
using System.IO;
using ModSettings;
using UnityEngine;

namespace AmbientLights
{
    public class AmbientLightsOptions
    {
        public static bool enable_shadows = false;
        public static float intensity_multiplier = 1f;
        public static float range_multiplier = 1f;
        public static int night_brightness = 1;
        public static float aurora_intensity = 1f;
        public static bool enable_debug_key = false;
        public static bool verbose = false;
    }

    public class AmbientPeriodItem
    {
        public int start_hour = -1;
        public int end_hour = -1;
        public int change_duration = 10;
    }

    public class AmbientPeriodTransition
    {
        public int start;
        public int duration;
        public int end;
        public bool complete = true;
    }

    public class AmbientLocationConfig
    {
        public List<AmbientConfigEmitter> emitters = null;
        public Dictionary<string, AmbientConfigPeriod> periods = new Dictionary<string, AmbientConfigPeriod>();
        public AmbientLocationOptions options = new AmbientLocationOptions();
    }

    public class AmbientLocationOptions
    {
        public string override_shadows = "";
        public float intensity_multiplier = 1f;
        public float range_multiplier = 1f;
    }

    public class AmbientConfigEmitter
    {
        public string description = "";
        public string position = "";
        public string orientation = "";
        public float size = 1f;
        public float cover = 0f;
    }

    public class AmbientConfigPeriod
    {
        public Dictionary<string, AmbientConfigWeather> weathers = new Dictionary<string, AmbientConfigWeather>();
    }

    public class AmbientConfigWeather
    {
        public Dictionary<string, AmbientConfigPeriodSet> orientations = new Dictionary<string, AmbientConfigPeriodSet>();
    }

    public class AmbientConfigPeriodSet
    {
        public string color = "";
        public float intensity = 0;
        public float range = 0;
        public string shadows = "none";
    }

    internal class AmbientLightsSettings : ModSettingsBase
    {
        [Section("General Settings")]

        [Name("General Ambient Intensity Multiplier")]
        [Description("Values above 1 make the lights brighter, under 1 they become dimmer than default. 0 turns the ambient lights off and makes it game default.")]
        [Slider(0f, 2f, 1)]
        public float intensity_multiplier = 1f;

        [Name("General Ambient Range Multiplier")]
        [Description("Values above 1 make the ambient lights cast light further. 2 will make them reach double the distance than default, 0 turns the lights off.")]
        [Slider(0f, 2f, 1)]
        public float range_multiplier = 1f;

        [Name("Aurora lights intensity")]
        [Description("Makes the aurora powered lights brighter or dimmer. 1 is game default, 0 turns off these lights (visual effects remain).")]
        [Slider(0f, 3f, 1)]
        public float aurora_intensity = 1f;

        [Name("Night Brightness")]
        [Description("How bright it gets at night.")]
        [Choice("Game Default", "Mod Default", "Brighter Nights", "Endless day")]
        public int night_brightness = 1;

        [Name("Enable debug keys")]
        [Description("If enabled, with L you can toggle between modded/unmodded, and Ctrl+L reloads the lighting data.")]
        public bool enable_debug_key = false;

        protected override void OnConfirm()
        {
            AmbientLightsOptions.enable_shadows = false;
            AmbientLightsOptions.intensity_multiplier = intensity_multiplier;
            AmbientLightsOptions.range_multiplier = range_multiplier;
            AmbientLightsOptions.aurora_intensity = aurora_intensity;
            AmbientLightsOptions.night_brightness = night_brightness;
            AmbientLightsOptions.enable_debug_key = enable_debug_key;

            AmbientLightControl.light_override = false;
            AmbientLightControl.MaybeUpdateLightsToPeriod(true);

            string json_opts = FastJson.Serialize(this);

            File.WriteAllText(Path.Combine(AmbientLightControl.mod_data_folder, "config.json"), json_opts);
        }
    }

    internal static class AmbienLightSettingsLoad
    {

        private static AmbientLightsSettings custom_settings = new AmbientLightsSettings();

        public static void OnLoad()
        {
            if (File.Exists(@"mods/ambient-lights/config.json"))
            {
                string opts = File.ReadAllText(Path.Combine(AmbientLightControl.mod_data_folder, "config.json"));
                custom_settings = FastJson.Deserialize<AmbientLightsSettings>(opts);

                AmbientLightsOptions.intensity_multiplier = custom_settings.intensity_multiplier;
                AmbientLightsOptions.range_multiplier = custom_settings.range_multiplier;
                AmbientLightsOptions.aurora_intensity = custom_settings.aurora_intensity;
                AmbientLightsOptions.night_brightness = custom_settings.night_brightness;
                AmbientLightsOptions.enable_debug_key = custom_settings.enable_debug_key;
            }

            custom_settings.AddToModSettings("Ambient Lighting Settings");
        }
    }
}
