using System.Collections.Generic;
using System.Reflection;
using ModSettings;
using UnityEngine;

namespace AmbientLights
{
    public class AmbientLightsOptions
    {
        public static bool enable_shadows = false;
        public static float intensity_multiplier = 1f;
        public static float range_multiplier = 1f;
        public static int night_brightness = 2;
    }

    public class AmbientPeriodItem
    {
        public int start_hour = -1;
        public int end_hour = -1;
        public int change_duration = 10;
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

        [Name("Show Shadows")]
        [Description("Enable/Disable ambient light shadows")]
        public bool enable_shadows = false;

        [Name("General Ambient Intensity Multiplier")]
        [Description("Values above 1 make the lights brighter, under 1 they become dimmer. 0 turns the lights off. Under 0 makes scenes darker than default.")]
        [Slider(0f, 2f)]
        public float intensity_multiplier = 1f;

        [Name("General Ambient Range Multiplier")]
        [Description("Values above 1 make the lights cast light further. 0 turns the light off.")]
        [Slider(0f, 2f)]
        public float range_multiplier = 1f;

        [Name("Night Brightness")]
        [Description("How bright or dark it gets at night.")]
        [Choice("Dark Hole", "Game Default", "Mod Default", "Brighter Nights", "Endless day")]
        public int night_brightness = 2;

        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            RequiresConfirmation();
            
        }

        protected override void OnConfirm()
        {
            AmbientLightsOptions.enable_shadows = enable_shadows;
            AmbientLightsOptions.intensity_multiplier = intensity_multiplier;
            AmbientLightsOptions.range_multiplier = range_multiplier;
            AmbientLightsOptions.night_brightness = night_brightness;

            AmbientLightControl.light_override = false;
            AmbientLightControl.MaybeUpdateLightsToPeriod(true);
        }
    }

    internal static class AmbienLightSettingsLoad
    {

        private static readonly AmbientLightsSettings custom_settings = new AmbientLightsSettings();

        public static void OnLoad()
        {
            custom_settings.AddToCustomModeMenu(Position.AboveAll);
            custom_settings.AddToModSettings("Ambient Lighting Settings");
        }

        internal static void SetFieldsVisible(int visibleFields)
        {
            FieldInfo[] fields = custom_settings.GetType().GetFields();
            for (int i = 0; i < fields.Length; ++i)
            {
                bool shouldBeVisible = i < visibleFields;
                custom_settings.SetFieldVisible(fields[i], shouldBeVisible);
            }
        }
    }
}
