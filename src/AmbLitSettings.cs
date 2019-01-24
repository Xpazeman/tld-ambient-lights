using System.IO;
using System.Reflection;
using ModSettings;
using System;

namespace AmbientLights
{
    internal class AmbLitOptions
    {
        public ALPresets alPreset = ALPresets.Default;

        public float intensityMultiplier = 1f;
        public float rangeMultiplier = 1f;
        public int nightBrightness = 1;

        public bool castShadows = false;

        public bool disableAmbience = false;
        public float ambienceLevel = 1f;
        public bool disableFill = false;
        public float fillLevel = 1f;
        public float fillColorLevel = 1f;

        public bool transparentWindows = false;

        //public float auroraIntensity = 2f;
        //public bool disableAuroraFlicker = false;

        public bool enableDebugKey = false;
    }

    internal enum ALPresets
    {
        Default, TLD_Default, Realistic, Darker_Interiors, Brighter_Interiors, Endless_Day, Dark_World, Mushrooms, Custom
    }

    internal class AmbLitSettings : ModSettingsBase
    {
        internal readonly AmbLitOptions setOptions = new AmbLitOptions();

        [Section("Lighting Preset")]
        [Name("Choose Base Preset")]
        [Description("Choose a default preset to load options.\n *** WARNING *** Changing this will overwrite your current settings.")]
        public ALPresets alPreset = ALPresets.Default;

        [Section("Ambient Lights Settings")]

        [Name("General Intensity Multiplier")]
        [Description("Values above 1 make the lights brighter, under 1 they become dimmer than default. 0 turns the ambient lights off and makes it game default.")]
        [Slider(0f, 2f, 1)]
        public float intensityMultiplier = 1f;

        [Name("General Range Multiplier")]
        [Description("Values above 1 make the ambient lights cast light further. 2 will make them reach double the distance than default, 0 turns the lights off.")]
        [Slider(0f, 2f, 1)]
        public float rangeMultiplier = 1f;

        [Name("Night Brightness")]
        [Description("How bright it gets at night.")]
        [Choice("Game Default", "Mod Default", "Brighter Nights", "Endless day")]
        public int nightBrightness = 1;

        [Name("Cast Shadows (Performance hit)")]
        [Description("Enable this to make light through the windows cast shadows. Needs a beefy GPU to work properly, and some scenes will take a big performance hit. Will also make everything considerably darker, so it is recommended to set the Default Ambience and/or Fill Lights Level to at least 0.1")]
        public bool castShadows = false;

        [Section("Game Lights Settings")]

        [Name("Default Ambience Level")]
        [Description("How bright is the default game ambience light (1 is game default, 0 would make interiors darker)")]
        [Slider(0f, 2f, 1)]
        public float ambienceLevel = 1f;

        [Name("Default Fill Lights Level")]
        [Description("How bright are the default game fill lights (1 is game default, 0 would make interiors darker)")]
        [Slider(0f, 2f, 1)]
        public float fillLevel = 1f;

        [Name("Colored Fill Lights")]
        [Description("Here you can set how saturated will be the fill lights that are colored (These are the green, cyan, red, etc. lights on some interiors.)")]
        [Slider(0f, 2f, 1)]
        public float fillColorLevel = 1f;

        [Name("See-Through windows")]
        [Description("Removes the window panes so it adds a bit of depth to windows. Kinda glitchy.")]
        public bool transparentWindows = false;

        /*[Section("Aurora Powered Lights Settings")]

        [Name("Aurora powered lights intensity")]
        [Description("Makes the aurora powered lights brighter or dimmer. 1 is game default, recommended value is 2 - 2.5 if light flicker is on, and around 1.7 if it's off.")]
        [Slider(1f, 2.5f, 1)]
        public float auroraIntensity = 2f;

        [Name("Turn off aurora light flicker")]
        [Description("If set to yes, aurora powered lights won't flicker and will stay on.")]
        public bool disableAuroraFlicker = false;*/

        [Section("Misc Settings")]

        /*[Name("Lights Optimization")]
        [Description("Setting this to Balanced or Performance will reduce the number of lights to try to increase performance. You might need to adjust other settings like Range Multiplier or Ambience Level to compensate for the lack of lighting.")]
        [Choice("Quality", "Balanced", "Performance")]*/

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

                alPreset = setOptions.alPreset;

                intensityMultiplier = setOptions.intensityMultiplier;
                rangeMultiplier = setOptions.rangeMultiplier;
                nightBrightness = setOptions.nightBrightness;

                castShadows = setOptions.castShadows;

                ambienceLevel = setOptions.ambienceLevel;
                fillLevel = setOptions.fillLevel;
                fillColorLevel = setOptions.fillColorLevel;

                transparentWindows = setOptions.transparentWindows;

                //auroraIntensity = setOptions.auroraIntensity;
                //disableAuroraFlicker = setOptions.disableAuroraFlicker;

                enableDebugKey = setOptions.enableDebugKey;
            }
        }

        protected override void OnConfirm()
        {
            //Save settings
            setOptions.alPreset = alPreset;

            setOptions.intensityMultiplier = (float)Math.Round(intensityMultiplier, 1);
            setOptions.rangeMultiplier = (float)Math.Round(rangeMultiplier, 1);
            setOptions.nightBrightness = nightBrightness;

            setOptions.castShadows = castShadows;

            setOptions.ambienceLevel = ambienceLevel;
            setOptions.fillLevel = fillLevel;
            setOptions.fillColorLevel = fillColorLevel;

            setOptions.transparentWindows = transparentWindows;

            //setOptions.auroraIntensity = (float)Math.Round(auroraIntensity, 1);
            //setOptions.disableAuroraFlicker = disableAuroraFlicker;

            setOptions.enableDebugKey = enableDebugKey;

            string jsonOpts = FastJson.Serialize(setOptions);

            File.WriteAllText(Path.Combine(AmbientLights.modDataFolder, AmbientLights.settingsFile), jsonOpts);
        }

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
                case ALPresets.Mushrooms:
                    intensityMultiplier = 1.1f;
                    rangeMultiplier = 1.0f;
                    nightBrightness = 1;
                    ambienceLevel = 0.6f;
                    fillLevel = 0.7f;
                    fillColorLevel = 0.5f;

                    break;

                case ALPresets.TLD_Default:
                    intensityMultiplier = 0f;
                    rangeMultiplier = 0f;
                    nightBrightness = 0;
                    ambienceLevel = 1f;
                    fillLevel = 1f;
                    fillColorLevel = 1f;

                    break;

                case ALPresets.Realistic:
                    intensityMultiplier = 1.2f;
                    rangeMultiplier = 1.2f;
                    nightBrightness = 0;
                    ambienceLevel = 0.1f;
                    fillLevel = 0f;
                    fillColorLevel = 0.1f;

                    break;

                case ALPresets.Darker_Interiors:
                    intensityMultiplier = 0.7f;
                    rangeMultiplier = 0.7f;
                    nightBrightness = 1;
                    ambienceLevel = 0.1f;
                    fillLevel = 0.1f;
                    fillColorLevel = 0f;

                    break;

                case ALPresets.Brighter_Interiors:
                    intensityMultiplier = 1.5f;
                    rangeMultiplier = 1.5f;
                    nightBrightness = 2;
                    ambienceLevel = 1.2f;
                    fillLevel = 1.2f;
                    fillColorLevel = 1f;

                    break;

                case ALPresets.Endless_Day:
                    intensityMultiplier = 1.5f;
                    rangeMultiplier = 1.5f;
                    nightBrightness = 3;
                    ambienceLevel = 1.7f;
                    fillLevel = 1.5f;
                    fillColorLevel = 2f;

                    break;

                case ALPresets.Dark_World:
                    intensityMultiplier = 0f;
                    rangeMultiplier = 0f;
                    nightBrightness = 0;
                    ambienceLevel = 0f;
                    fillLevel = 0f;
                    fillColorLevel = 0f;

                    break;
            }
        }
    }
}
