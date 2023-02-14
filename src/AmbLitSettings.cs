using System.IO;
using System.Reflection;
using ModSettings;
using System;

namespace AmbientLights
{
    internal enum ALPresets
    {
        Default, TLD_Default, Realistic, Darker_Interiors, Brighter_Interiors, Endless_Day, Dark_World, Custom
    }

    internal class AmbLitSettings : JsonModSettings
    {
        [Section("Lighting Preset")]
        [Name("Choose Base Preset")]
        [Description("Choose a default preset to load options.\n *** WARNING *** Changing this will overwrite your current settings.")]
        public ALPresets alPreset = ALPresets.Default;

        [Section("Ambient Lights Settings")]

        [Name("General Intensity Multiplier")]
        [Description("Values above 1 make the lights brighter, under 1 they become dimmer than default. 0 turns the ambient lights off and makes it game default.")]
        [Slider(0f, 2f, 1, NumberFormat = "{0:F2}")]
        public float intensityMultiplier = 1f;

        [Name("General Range Multiplier")]
        [Description("Values above 1 make the ambient lights cast light further. 2 will make them reach double the distance than default, 0 turns the lights off.")]
        [Slider(0f, 2f, 1, NumberFormat = "{0:F2}")]
        public float rangeMultiplier = 1f;

        [Name("Night Brightness")]
        [Description("How bright it gets at night.")]
        [Choice("Game Default", "Mod Default", "Brighter Nights", "Endless day")]
        public int nightBrightness = 1;

        [Name("True Sun Lightshafts (Experimental)")]
        [Description("Experimental Feature. This makes lightshafts come from the real sun position. Might impact performance. To reduce impact, set this option before loading a game.")]
        public bool trueSun = true;

        [Section("Game Lights Settings")]

        [Name("Default Ambience Level")]
        [Description("How bright is the default game ambience light (1 is game default, 0 would make interiors darker)")]
        [Slider(0f, 2f, 1, NumberFormat = "{0:F2}")]
        public float ambienceLevel = 0.3f;

        [Name("Default Fill Lights Level")]
        [Description("How bright are the default game fill lights (1 is game default, 0 would make interiors darker)")]
        [Slider(0f, 2f, 1, NumberFormat = "{0:F2}")]
        public float fillLevel = 0.4f;

        [Name("Colored Fill Lights")]
        [Description("Here you can set how saturated will be the fill lights that are colored (These are the green, cyan, red, etc. lights on some interiors.)")]
        [Slider(0f, 2f, 1, NumberFormat = "{0:F2}")]
        public float fillColorLevel = 0.5f;

        [Section("Weather Settings")]

        [Name("Base Aurora Intensity")]
        [Description("How bright is the aurora light that come through windows")]
        [Slider(0f, 2f, 1, NumberFormat = "{0:F2}")]
        public float auroraIntensity = 0.8f;

        [Name("Base Aurora Saturation")]
        [Description("How colorful is the aurora light that come through windows")]
        [Slider(0f, 2f, 1, NumberFormat = "{0:F2}")]
        public float auroraSaturation = 1.4f;

        [Section("Misc Settings")]

        [Name("Enable debug keys")]
        [Description("If enabled, with Shift+L you can disable/enable the mod.")]
        public bool enableDebugKey = false;

        [Name("Verbose Log")]
        [Description("With this enabled, the log will have much more information. Useful for troobleshooting.")]
        public bool verbose = false;

        protected override void OnChange(FieldInfo field, object oldVal, object newVal)
        {
            //Change evt
            if (field.Name == nameof(alPreset))
            {
                ChangeALPreset((ALPresets) newVal);
            }
            else
            {
                alPreset = ALPresets.Custom;
            }

            RefreshGUI();
        }

        internal void ChangeALPreset(ALPresets preset)
        {
            //Refresh fields
            switch (preset)
            {
                case ALPresets.Default:
                    intensityMultiplier = 1.0f;
                    rangeMultiplier = 1.0f;
                    nightBrightness = 1;
                    ambienceLevel = 0.3f;
                    fillLevel = 0.4f;
                    fillColorLevel = 0.5f;
                    auroraIntensity = 0.8f;
                    auroraSaturation = 1.4f;

                    break;

                case ALPresets.TLD_Default:
                    intensityMultiplier = 0f;
                    rangeMultiplier = 0f;
                    nightBrightness = 0;
                    ambienceLevel = 1f;
                    fillLevel = 1f;
                    fillColorLevel = 1f;
                    auroraIntensity = 1f;
                    auroraSaturation = 1f;

                    break;

                case ALPresets.Realistic:
                    intensityMultiplier = 1f;
                    rangeMultiplier = 1.1f;
                    nightBrightness = 0;
                    ambienceLevel = 0f;
                    fillLevel = 0.1f;
                    fillColorLevel = 0f;
                    auroraIntensity = 0.3f;
                    auroraSaturation = 1.4f;

                    break;

                case ALPresets.Darker_Interiors:
                    intensityMultiplier = 0.7f;
                    rangeMultiplier = 0.7f;
                    nightBrightness = 1;
                    ambienceLevel = 0.2f;
                    fillLevel = 0.1f;
                    fillColorLevel = 0f;
                    auroraIntensity = 0.1f;
                    auroraSaturation = 0.8f;

                    break;

                case ALPresets.Brighter_Interiors:
                    intensityMultiplier = 1.3f;
                    rangeMultiplier = 1.3f;
                    nightBrightness = 2;
                    ambienceLevel = 1.4f;
                    fillLevel = 1.2f;
                    fillColorLevel = 1f;
                    auroraIntensity = 1.2f;
                    auroraSaturation = 1.6f;

                    break;

                case ALPresets.Endless_Day:
                    intensityMultiplier = 1.5f;
                    rangeMultiplier = 1.5f;
                    nightBrightness = 3;
                    ambienceLevel = 1.7f;
                    fillLevel = 1.5f;
                    fillColorLevel = 2f;
                    auroraIntensity = 2f;
                    auroraSaturation = 2f;

                    break;

                case ALPresets.Dark_World:
                    intensityMultiplier = 0f;
                    rangeMultiplier = 0f;
                    nightBrightness = 0;
                    ambienceLevel = 0f;
                    fillLevel = 0f;
                    fillColorLevel = 0f;
                    auroraIntensity = 0f;
                    auroraSaturation = 0f;

                    break;
            }
        }
    }

    internal static class Settings
    {
        public static AmbLitSettings options;

        public static void OnLoad()
        {
            options = new AmbLitSettings();
            options.AddToModSettings("Ambient Lights Settings");
        }
    }
}
