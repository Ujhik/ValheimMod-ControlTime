<h1 align="center">ControlTime</h1>
 <p align="center">
   <img src="https://github.com/Ujhik/ValheimMod-ControlTime/blob/main/design/valheimControlTimeIcon256x256.png?raw=true">
 </p>

This valheim mod allows you to force the time of day, disable rain, disable nights by skipping them by adjusting the night skip interval and setting the minute duration of day/night cycle to make days longer or shorter.


## Features
It syncs with all the people that have the mod installed and the values can only be changed by server admins. To sync to other players the configuration manager has to be closed, until then the changes are only local to the player.

- **Force Time of Day**: 
  - Enable `isForceTimeOfDay` and adjust the `forcedTimeOfDayFraction` slider to set a fixed time of day. The sun/moon will move in real-time as you adjust the slider.
  
- **Disable Rain**: 
  - Use `isRemoveRain` to stop rain from interrupting your adventures.

- **Skip Night**: 
  - Activate `isSkipNight` to skip the night without the usual animations, allowing you to adjust the `skipTimeStep` for customizable speed.


Warning: This mod can mess up the number shown in number of days passed, becase the game calculates it based on day duration, so changing the game duration makes it fluctuate. I don't think that has any real repercussion on gameplay, but can affect other mods that use that number of days. 


## Installation (Manual)
1. Install [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) and [Jötunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/).
2. Extract the contents of `ControlTime` into the `BepInEx/plugins` folder

For best results, install this mod on both the server and all clients to avoid time desynchronization, ensuring everyone experiences the same conditions.

## Links
- [Thunderstore](https://valheim.thunderstore.io/package/Ujhik/ControlTime/)
- [Github](https://github.com/Ujhik/ValheimMod-ControlTime)
- Discord: You can ping me in the Jotunn discord: [Jötunn discord](https://discord.gg/DdUt6g7gyA)

## Similar mods
- [Time scale](https://www.nexusmods.com/valheim/mods/995?tab=posts)
