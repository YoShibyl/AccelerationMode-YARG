# Acceleration Mode for [YARG](https://yarg.in/)'s new engine (v0.12.0 alpha builds)
A [BepInEx 6](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.1) game mod for YARG (Yet Another Rhythm Game) that makes songs speed up with every note hit.

### ***PLEASE do not report bugs to the YARG team while this mod is installed!***
This plugin is not affiliated with or endorsed by the game or its devs.  If you experience issues, especially if you know a good way to fix them, feel free let me know.

## Features
- Every note hit increases the song's speed by 0.25% (can be changed in the config at `BepInEx/config/AccelerationMode.cfg` while the game isn't running)
  - Missing notes decreases speed by 1% by default
- Toggleable by clicking the version text in the upper-right corner
- Configurable speedup/slowdown properties in the config

This mod is still early in development, so expect issues.

### Configuration
See the config file located at `BepInEx/config/AccelerationMode.cfg` in the game folder for more uptions (as of v0.2.0 of the mod)

## Installation
1) Install [BepInEx 6.0.0](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.1) to the root folder of YARG's new engine build
2) Download and extract the current release of the mod from [Releases](https://github.com/YoShibyl/AccelerationMode-YARG/releases) (v0.3.0+ adds compatibility for updated builds of the game, hopefully)
3) Start YARG as usual.  **Make sure you pick the "new engine" build!**

Once you're in-game, make sure to click the text in the upper-right corner of the game window to toggle Acceleration Mode on or off.  Then, pick a song to play, and have fun!  (Good luck.)

## Updating YARG
As of the v0.3.0 pre-releases, it *should* be possible to load this mod on YARG's new engine regardless of build number.

***In the event that a new YARG build is released:***
1) **Back up the BepInEx folder in the game folder** by copying it to a different location.  This is because the YARC launcher deletes the old build's game folder.
2) Update the game as usual from the YARC launcher.
3) Reinstall BepInEx 6 into the new build's folder.
4) Merge the BepInEx folder with that of the new build's folder.

If all goes well, your config should be the same as before the update.

## Known issues
- The highway jitters when hitting notes, probably due to how the game handles song speed changes.
- The game may be slower with the mod installed
- I'm not great at optimizing code 😂

## Building
*soon™*

## Credits, special thanks, and stuff
- YARG was made by [EliteAsian123](https://github.com/EliteAsian123) : [Download YARG here](https://yarg.in/)
- BepInEx : https://github.com/BepInEx/BepInEx

Special thanks to the Clone Hero and YARG communities, especially [JasonParadise](https://twitch.tv/JasonParadise), [Acai](https://twitch.tv/Acai), and [Frif](https://twitch.tv/Frif).
