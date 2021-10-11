# Ambient lights mod
This mod aims to modify the default lightning for interiors with windows, making them brighter so you don't need a light source during midday to move around, and also more reflective of what's happening outside (time of day, weather, and sun position).
These lights are just ambiental and not "real" lights, so to craft or do things at night you will still need a lightsource just like in the base game.

## Installation
* If you haven't done so already, install MelonLoader by downloading and running [MelonLoader.Installer.exe](https://github.com/HerpDerpinstine/MelonLoader/releases/latest/download/MelonLoader.Installer.exe).
* You'll need to download and install [ModSettings](https://github.com/zeobviouslyfakeacc/ModSettings/releases/latest/download/ModSettings.dll) by Zeo. **MOD WON'T WORK WITHOUT IT**
* Download the latest version of AmbientLigths.zip from the [releases page](https://github.com/Xpazeman/tld-ambient-lights/releases/latest).
* Extract the zip file into your **Mods** folder, you should have AmbientLights.dll and ambient-lights folder.
* **IMPORTANT: DO NOT leave the .zip folder in the Mods folder as this will make Melon Loader error.**
* Make sure your game brightness it's set to default, otherwise it might be too bright.
* **UPDATING**: Simply unzip the file in your mods folder and overwrite the old files. Remember to remove the .zip file from the folder.

![Your Mods folder should look like this](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/folder.jpg "Your Mods folder should look like this")

## Updates

### v2.5
* Updated to work with TLD 1.95 - Fury, then silence.

### v2.4
* Mapped Ash Canyon interiors
* Fixed some glass illumination issues whil under sunlight
* Added extra decimal to mod settings
* Added verbose option to settings for troubleshooting

### v2.3
* Fixed decals and glass being too bright in low light environments

### v2.2
* Fixed for TLD 1.93

### v2.1
* Major rewrite to make it work with newer versions
* NEW: Added True Sun Lightshafts, this makes the sun come through the windows, making lightshafts more realistic. Since this manipulates the scene heavily, it can lead to performance hits. It's toggeable in the settings menu. If you enter a scene and it looks burned out, it's because the scene manipulation has failed for some reason (usually due to performance during scene load), this should be fixed by exiting the interior and going in again.
* NEW: Added light flickering to some of the weather types, to represent better what's going on outside.
* FIX: Fixed "aurora ambience" coming up too early, now it should come progressively as the aurora progresses.
* FIX: Tuned values all over the mod.

### v1.0.2
* FIX: Fixed issue that was throwing errors on some locations.

### v1.0.1
* FIX: Fixed issue with some lightshafts not reacting properly to cloudy weathers (they stayed lit like in the base game).

### v1.0.0
Major overhaul:
* Changed time system to be based on game states instead of clock, makes the mod compatible with any other mod that changes day duration like Solstice.
* NEW: Added windows, ambient light, fill lights and lightshafts to the Ambient Lights Manager, so all light sources in the scene are affected by weather and sun position.
* NEW: Added a preset system for quick tweaking. It overwrites the settings, so it's better to select one as base and tweak from there.
* NEW: Added controls to general ambient light level. Setting this to 0 makes all ambient light disapear, so only light sources illuminate the scene (i.e. makes the Lower Dam pitch black except for the offices)
* NEW: Added controls for fill lights. These are those spots of lights, usually coloured, that are used in scenes to highlight parts of it. With the new sliders you can control how much saturated (as in how colorful the light is), and the intensity level of it.
* NEW: Added an experimental feature: making window lights cast shadows. It's still basic, and might make the game take a good performance hit, but with a 970 gtx, I get around 60 FPS with everyhting maxed out and this toggled on.
* NEW: Made the light shafts disappear on every weather type except for clear and partly cloudy, to reflect the outside environment better. There are some scenes where this doesn't work properly, but I'll try to fix in next releases.
* REM: Removed the Aurora Lights tweaking, as I felt they were a bit out of context inside Ambient Lights.

### v0.9.7
* FIX: Updated to work with TLD 1.42
* FIX: Reduced daylight brightness to compensate for interior lighting changes.

### v0.9.5
* FIX: Fixed new issue with incorrect lights loading on some locations.
* NEW: Added multipliers to aurora lights intensity and range manually, on a scene by scene basis.

### v0.9
* FIX: Fixed issue where lighting took an in-game minute to render on some locations.
* NEW: Added options to control aurora powered lights

## Main Features
* Lighting intensity (how bright the light is), range (how far the light reaches) and color changes depending of what's happening outside.
* Clear and light cloudy weathers will have a warmer color.
* Foggy weathers are greenish, blizzards are more dark blue, and overcast weathers are more gray-ish.
* Makes most interiors brighter by default.
* Mod options to change default intensity and range even mid-game.
* Option to make nights as dark as the base game, or even brighter than the mod's default.
* Window orientations. For example, with clear weather during dawn, the east side of a place will be brighter and will have a warmer color than the west side.
* Window panels, ambient light, fill lights and lightshafts are controlled by the Ambient Lights Manager, so all light sources in the scene are affected by weather and sun position.
* Light shafts now fade out on every weather type except for clear and partly cloudy instead of shining all the time, to reflect the outside environment better.



## Debug Controls
* Debug keys can be enabled in the Mod setting menu option for Ambient Lights.
* Shift+L toggles the ambient lighting on/off

## Known Issues
* Natural interiors such as caves aren't mapped yet.
* Some features don't work on some interiors as they use a different (static & baked) lighting system.
* Some interiors might have a brighter than normal default ambient light.
* Some larger spaces might be too dark/too bright, you can tweak this by going to the scene config in the ambient-lights folder and tweaking the intensity_multiplier and range_multiplier at the bottom of the file.
* Transition to night time with increased night brightness is instant and not progressive.

## Editing the mod's lighting
Since AmbientLights is based on JSON files, it's not complicated to edit the mod's default lighting values.
Inside the ambient-lights folder there are three types of files: scene_*, global_sets, global_periods, and weather_sets.

* global_periods is used to define on what period we are so global_sets can use that data. New periods can be added, but they need to be added to global_sets as well.
* global_sets is used to define the lighting for each period and orientation. Each orientation has the data to render the light: color, intensity and range. There's also a shadow type paramenter, but it's not used yet.
* weather_sets is used to define how each weather affects color and intensity of the lights. rMod, bMod and gMod modify the tone of the light, sMod modifies the saturation of the color, and vMod the brightness/intensity.
* scene_* are the files where the windows are defined (position, orientation data from the period/weather it will use, a cover modifier that reduces intensity for that light and a size modifier that changes the maximum range at which the light is cast), as well as the multipliers for range and intensity for that particular scene (the options object). In this file, the default sets can also be overriden. Inside the "periods" object we can use the same structure as in global_sets.json

## Uninstalling
* Go into your mods folder (<path_to_TheLongDark_installation_folder>/mods) and delete **AmbientLights.dll** and the **ambient-lights** folder.

## Screenshots

[Ambient Lights on imgur](https://imgur.com/a/r1GOzKt)

![Clear dawn at Camp Office](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/example-1.jpg "Clear dawn at Camp Office")

![Weather comparison](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/lighthouse_weathers.jpg "Weather comparison")

![Clear dawn at Trappers](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/example-2.jpg "Clear dawn at Trappers")

![Early dawn at PV Farm](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/example-3.jpg "Early dawn at PV Farm")

![Tweaked aurora powered lights](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/aurora_1.jpg "Tweaked aurora powered lights")
