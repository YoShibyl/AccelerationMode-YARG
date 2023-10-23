# Please don't install this.
It doesn't work on the newer builds of YARG (like `a6`), and even on the `a5` build, it was very unoptimized.

# ~~Acceleration Mode~~ for [YARG](https://yarg.in/)'s new engine (v0.12.0-a5)
A [BepInEx 6](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.1) game mod for YARG (Yet Another Rhythm Game) that makes songs speed up with every note hit.

### ***PLEASE do not report bugs to the YARG team while this mod is installed!***
This plugin is not affiliated with or endorsed by the game or its devs.  If you experience issues, especially if you know a good way to fix them, feel free let me know.

## Features
- Every note hit increases the song's speed by 0.25% (can be changed in the config at `BepInEx/config/AccelerationMode.cfg` while the game isn't running)
  - Missing notes decreases speed by 1% by default
- Toggleable by clicking the version text in the upper-right corner
- Configurable speedup/slowdown properties

This mod is still early in development, so expect issues.

### Configuration
See the config file located at `BepInEx/config/AccelerationMode.cfg` in the game folder for more uptions (as of v0.2.0 of the mod)

## Installation
1) Install [BepInEx 6.0.0](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.1) to the root folder of YARG's new engine build
2) Download and extract the current release of the mod from [Releases](https://github.com/YoShibyl/AccelerationMode-YARG/releases)
3) Start YARG as usual.  **Make sure you pick the "new engine" build!**

Once you're in-game, make sure to click the text in the upper-right corner of the game window to toggle Acceleration Mode on.  Then, pick a song to play, and have fun!  (Good luck.)

## Known issues
- The highway jitters when hitting notes, probably due to how the game handles song speed changes.
- I'm not great at optimizing code 😂

## Building
*soon™*

## Credits, special thanks, and stuff
- YARG was made by [EliteAsian123](https://github.com/EliteAsian123) : [Download YARG here](https://yarg.in/)
- BepInEx : https://github.com/BepInEx/BepInEx

Special thanks to the Clone Hero and YARG communities, especially [JasonParadise](https://twitch.tv/JasonParadise), [Acai](https://twitch.tv/Acai), and [Frif](https://twitch.tv/Frif).
