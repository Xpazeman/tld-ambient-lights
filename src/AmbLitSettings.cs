using System.IO;
using System.Reflection;
using ModSettings;
using System;

namespace AmbientLights
{
    internal class AmbLitOptions
    {
        public float intensityMultiplier = 1f;
        public float rangeMultiplier = 1f;
        public int nightBrightness = 1;

        public float auroraIntensity = 2f;
        public bool disableAuroraFlicker = false;

        public bool enableDebugKey = false;
    }

    internal class AmbLitSettings : ModSettingsBase
    {
        internal readonly AmbLitOptions setOptions = new AmbLitOptions();

        [Section("Ambient Lights Settings")]

        [Name("General Ambient Intensity Multiplier")]
        [Description("Values above 1 make the lights brighter, under 1 they become dimmer than default. 0 turns the ambient lights off and makes it game default.")]
        [Slider(0f, 2f, 1)]
        public float intensityMultiplier = 1f;

        [Name("General Ambient Range Multiplier")]
        [Description("Values above 1 make the ambient lights cast light further. 2 will make them reach double the distance than default, 0 turns the lights off.")]
        [Slider(0f, 2f, 1)]
        public float rangeMultiplier = 1f;

        [Name("Night Brightness")]
        [Description("How bright it gets at night.")]
        [Choice("Game Default", "Mod Default", "Brighter Nights", "Endless day")]
        public int nightBrightness = 1;

        [Section("Aurora Powered Lights Settings")]

        [Name("Aurora powered lights intensity")]
        [Description("Makes the aurora powered lights brighter or dimmer. 1 is game default, recommended value is 2 - 2.5 if light flicker is on, and around 1.7 if it's off.")]
        [Slider(1f, 2.5f, 1)]
        public float auroraIntensity = 2f;

        [Name("Turn off aurora light flicker")]
        [Description("If set to yes, aurora powered lights won't flicker and will stay on.")]
        public bool disableAuroraFlicker = false;

        [Section("Debug Settings")]

        [Name("Enable debug keys")]
        [Description("If enabled, with L you can toggle between modded/unmodded lights, and Ctrl+L reloads the lighting data.")]
        public bool enableDebugKey = false;

        internal AmbLitSettings()
        {
            //Load settings
            if (File.Exists(Path.Combine(AmbientLights.modDataFolder, AmbientLights.settingsFile)))
            {
                string opts = File.ReadAllText(Path.Combine(AmbientLights.modDataFolder, AmbientLights.settingsFile));
                setOptions = FastJson.Deserialize<AmbLitOptions>(opts);

                intensityMultiplier = setOptions.intensityMultiplier;
                rangeMultiplier = setOptions.rangeMultiplier;
                nightBrightness = setOptions.nightBrightness;

                auroraIntensity = setOptions.auroraIntensity;
                disableAuroraFlicker = setOptions.disableAuroraFlicker;

                enableDebugKey = setOptions.enableDebugKey;
            }
        }

        protected override void OnConfirm()
        {
            //Save settings
            setOptions.intensityMultiplier = intensityMultiplier;
            setOptions.rangeMultiplier = rangeMultiplier;
            setOptions.nightBrightness = nightBrightness;

            setOptions.auroraIntensity = auroraIntensity;
            setOptions.disableAuroraFlicker = disableAuroraFlicker;

            setOptions.enableDebugKey = enableDebugKey;

            string jsonOpts = FastJson.Serialize(setOptions);

            File.WriteAllText(Path.Combine(AmbientLights.modDataFolder, AmbientLights.settingsFile), jsonOpts);
        }

        protected override void OnChange(FieldInfo field, object oldVal, object newVal)
        {
            //Change evt
        }

        internal void RefreshFields()
        {
            //Refresh fields
        }
    }
}
