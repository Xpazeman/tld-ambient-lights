# Ambient lights mod
This mod aims to modify the default lightning for interiors with windows, making them brighter so you don't need a light source during midday to move around, and also more reflective of what's happening outside (time of day, weather, and sun position).
These lights are just ambiental and not "real" lights, so to craft or do things at night you will still need a lightsource just like in the base game.

## Important Notice
Before you can use this, or any other mod, make sure of the following:
* You have The Long Dark **version 1.49** installed.
* You have downloaded the latest version of **ModLoader** (https://github.com/zeobviouslyfakeacc/ModLoaderInstaller/releases/tag/v1.5) and patched your TLD game.

Also, in particular to this mod, you'll need **ModSettings 1.5** installed in order to have the Mod Options menu. You can get it in two ways:
* Using Wulfmarius' Mod Installer
* Installed manually from https://github.com/zeobviouslyfakeacc/ModSettings/releases/tag/v1.5

## Updates
### v1.0.0
Major overhaul:
* Changed time system to be based on game states instead of clock, makes the mod compatible with any other mod that changes day duration like Solstice.
* Added windows, ambient light, fill lights and lightshafts to the Ambient Lights Manager, so all light sources in the scene are affected by weather and sun position.
* Added a preset system for quick tweaking. It overwrites the settings, so it's better to select one as base and tweak from there.
* Added controls to general ambient light level. Setting this to 0 makes all ambient light disapear, so only light sources illuminate the scene (i.e. makes the Lower Dam pitch black except for the offices)
* Added controls for fill lights. These are those spots of lights, usually coloured, that are used in scenes to highlight parts of it. With the new sliders you can control how much saturated (as in how colorful the light is), and the intensity level of it.
* Added an experimental feature: making window lights cast shadows. It's still basic, and might make the game take a good performance hit, but with a 970 gtx, I get around 60 FPS with everyhting maxed out and this toggled on.
* Made the light shafts disappear on every weather type except for clear and partly cloudy, to reflect the outside environment better. There are some scenes where this doesn't work properly, but I'll try to fix in next releases.
* Removed the Aurora Lights tweaking, as I felt they were a bit out of context inside Ambient Lights.

### v0.9.7
* Updated to work with TLD 1.42
* Reduced daylight brightness to compensate for interior lighting changes.

### v0.9.5
* Fixed new issue with incorrect lights loading on some locations.
* Added multipliers to aurora lights intensity and range manually, on a scene by scene basis.

### v0.9
* Fixed issue where lighting took an in-game minute to render on some locations.
* Added options to control aurora powered lights

## Main Features
* Lighting intensity (how bright the light is), range (how far the light reaches) and color changes depending of what's happening outside.
* Clear and light cloudy weathers will have a warmer color.
* Foggy weathers are greenish, blizzards are more dark blue, and overcast weathers are more gray-ish.
* Makes most interiors brighter by default.
* Mod options to change default intensity and range even mid-game.
* Option to make nights as dark as the base game, or even brighter than the mod's default.
* Window orientations. For example, with clear weather during dawn, the east side of a place will be brighter and will have a warmer color than the west side.
* Window panels, ambient light, fill lights and lightshafts are controlled by the Ambient Lights Manager, so all light sources in the scene are affected by weather and sun position.
* Experimental feature: making window lights cast shadows. It's still basic, and might make the game take a good performance hit, but with a 970 gtx, I get around 60 FPS with everyhting maxed out and this toggled on.
* Light shafts disappear on every weather type except for clear and partly cloudy, to reflect the outside environment better. Not all scenes have this implemented.
* Optional debugging keys to check if everything's working and to make easier the process of changing the lighting settings (disabled by default).

## Installation with Mod Installer
* Download and install [Mod Installer](https://github.com/WulfMarius/Mod-Installer/releases) by **WulfMarius** if you don't have it already.
* After opening it, click on refresh sources at the top.
* Install ModSettings from the list if you don't have it or have an earlier version.
* Install AmbienLigths from the list.

## Manual Installation
* If you haven't already done so, install the [Mod Loader](https://github.com/zeobviouslyfakeacc/ModLoaderInstaller) by **zeobviouslyfakeacc** and patch your game.
* You should also have [ModSettings](https://github.com/zeobviouslyfakeacc/ModSettings/releases/tag/v1.5) by **zeo** installed in order to be able to change the mod's default options. Download the .dll and put it in your mods folder.
* Alternatively, download Ambient_Lights_vX.X_with_dependencies.zip, that already contains ModSettings.
* Head over to the [Releases](https://github.com/Xpazeman/tld-ambient-lights/releases/) page and download Ambient_Lights_vX.X.zip
* Unzip this file and move AmbientLights.dll and the ambient-lights folder into your mods directory (it's in <path_to_TheLongDark_installation_folder>/mods ), it should look like this (of course you might have more mods installed):
* Make sure your game brightness it's set to default, otherwise it might be too bright.
* **UPDATING**: Simply unzip the file in your mods folder and overwrite the old files.

![Your mod folder](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/folder.jpg "Your mod folder")

## Debug Controls
* Debug keys can be enabled in the Mod setting menu option for Ambient Lights.
* Shift+L toggles the ambient lighting on/off

## Known Issues
* Natural interiors such as caves aren't mapped yet.
* Tweaking of lightshafts don't work on every interior.
* Some interiors might have a brighter than normal default ambient light.
* Transition to night time with increased night brightness is instant and not progressive.

## Editing the mod's lighting
Since AmbientLights is based on JSON files, it's not complicated to edit the mod's default lighting values.
Inside the ambient-lights folder there are three types of files: scene_*, global_sets, and global_periods.

* global_periods is used to define on what period we are so global_sets can use that data. New periods can be added, but they need to be added to global_sets as well.
* global_sets is used to define the lighting for each period, weather and orientation. Each orientation has the data to render the light: color, intensity and range. There's also a shadow type paramenter, but it's not used yet.
* scene_* are the files where the windows are defined (position, orientation data from the period/weather it will use, a cover modifier that reduces intensity for that light and a size modifier that changes the maximum range at which the light is cast), as well as the multipliers for range and intensity for that particular scene. In this file, the default sets can also be overriden. Inside the "periods" object we can use the same structure as in global_sets.json

## Uninstalling
* Go into your mods folder (<path_to_TheLongDark_installation_folder>/mods) and delete **AmbientLights.dll** and the **ambient-lights** folder.

## Screenshots

[Ambient Lights on imgur](https://imgur.com/a/r1GOzKt)

![Clear dawn at Camp Office](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/example-1.jpg "Clear dawn at Camp Office")

![Weather comparison](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/lighthouse_weathers.jpg "Weather comparison")

![Clear dawn at Trappers](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/example-2.jpg "Clear dawn at Trappers")

![Early dawn at PV Farm](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/example-3.jpg "Early dawn at PV Farm")

![Tweaked aurora powered lights](https://raw.githubusercontent.com/Xpazeman/tld-ambient-lights/master/screenshots/aurora_1.jpg "Tweaked aurora powered lights")
