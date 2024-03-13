# SpeedrunUtils

## Features
- Built in FPS limiter
- A new autosplitter that's more accurate, more stable and easier to maintain
- Leaderboard legal automash for unskippable dialogue

## How to use
` to open the GUI.

By default, the O key will uncap your framerate and the P key will limit your framerate to 30FPS. If you want to change this. Read "How to configure".

If LiveSplit is open, SpeedrunUtils will automatically connect to it. However, if you've opened LiveSplit after you opened your game, you can attempt to connect to it by opening the GUI and clicking "Connect to LiveSplit".

Enter your framerate into the input field and click "Set FPS" to set your framerate cap.

## How to configure
SpeedrunUtils by default is set up to automatically work with the current fastest Any% route so if you're doing that then you don't need to do any configuration at all. However, if you want some extra splits, want to change frame limiting keys or are deviating from the current route (Doing the Flesh Prince challenges) then you need to do some configuration.

Splits can be configured with the in-game "Splits" menu.

Keybinds can be configured by editing the Keys.txt file in the SpeedrunUtils config folder. All keys are parsed as KeyCode so use [this reference](https://docs.unity3d.com/ScriptReference/KeyCode.html) to properly input your keybinds.
(Controller binds *are* supported. Read the linked reference page above.)


## How to install
#### Thunderstore
SpeedrunUtils is available on [Thunderstore](https://thunderstore.io/c/bomb-rush-cyberfunk/p/Loomeh/SpeedrunUtils/) and can be installed via the Thunderstore mod client or r2modman.

#### Manual
You can install SpeedrunUtils with a batch script. You can download the batch file [here](https://raw.githubusercontent.com/Loomeh/SpeedrunUtilsInstaller/main/InstallSpeedrunUtils.bat) or just copy and paste this command into command prompt.
```
curl https://raw.githubusercontent.com/Loomeh/SpeedrunUtilsInstaller/main/InstallSpeedrunUtils.bat -o %temp%\sruinstall.bat && %temp%\sruinstall.bat
```
Then add the LiveSplit Server component to your LiveSplit layout by going to Edit Layout -> Add -> Control -> LiveSplit Server. 

Activate the server by right click LiveSplit -> Control -> Start Server.
