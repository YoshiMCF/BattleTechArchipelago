# BATTLETECH Archipelago Client

This mod for [BATTLETECH](https://store.steampowered.com/app/637090/BATTLETECH/) by Harebrained Schemes integrates with [Archipelago](https://archipelago.gg) to give a randomized and optionally co-op campaign experience. This mod is currently under development. Almost nothing has been implemented yet.

For questions and discussion, please find the BATTLETECH thread in the [Archipelago discord](https://discord.gg/archipelago), in the channel named `future-game-design`. It is too early in development to accept feedback or bug reports. In the future, the Archipelago discord will be the appropriate place for feedback, and opening an issue on GitHub will be the best place for bug reports.

## Installation
Compiled releases have not yet been set up. The only way to try this mod is to build it from source.

## Building
* Install [ModTek](https://github.com/BattletechModders/ModTek) v4.5.0+
* Open BattleTechARchipelago.sln in Visual Studio 2026
* Copy Directory.Build.props.CHANGEME to Directory.Build.props
* Modify Directory.Build.props so that BattleTechGameDir points at your installation of BattleTech (e.g. `C:\Steam\steamapps\common\BATTLETECH\`)
* Install dependencies
* Create an empty directory at $(BattleTechGameDir)\Mods\BattleTechArchipelago
* Compile the mod. Check that it copied mod.json, BattleTechArchipelago.dll, and other files to $(BattleTechGameDir)\Mods\BattleTechArchipelago.
* Run BATTLETECH. Ensure the mod is enabled in the mods menu.

## Developing
* Install [dnSpyEx](https://github.com/dnSpyEx/dnSpy/releases)
    * Follow [ModTek's directions](https://github.com/BattletechModders/ModTek/blob/master/doc/DEVELOPMENT_GUIDE.md) on using dnSpyEx to decompile and set breakpoints. Ignore dnSpyEx's own instructions.
    * Export all of Assembly-CSharp.dll. dnSpyEx's search function only finds definitions, not call sites.
* Install [BTDebug](https://github.com/CWolfs/BTDebug)
* Enable BATTLETECH's built-in debugging tools. In BATTLETECH/BattleTech_Data/StreamingAssets/data/debug/settings.json
    * add "testToolsEnable": "true"
    * set "disableSplashScreens": true
    * set "disableIntroMove": true
    * More info about that [here](https://forumcontent.paradoxplaza.com/public/455608/Battletech%20Cheat-Debug%20Guide%20-%20Google%20Docs.pdf)