# SpeedrunUtils

## Features
- Built in FPS limiter that can be used as a replacement for RivaTuner
- A new autosplitter that's more accurate, more stable and easier to maintain

## How to use
` to open the GUI.

By default, the O key will uncap your framerate and the P key will limit your framerate to 30FPS. If you want to change this. Read "How to configure".

If LiveSplit is open, SpeedrunUtils will automatically connect to it. However, if you've opened LiveSplit after you opened your game, you can attempt to connect to it by opening the GUI and clicking "Connect to LiveSplit".

Enter your framerate into the input field and click "Set FPS" to set your framerate cap.

## How to configure
SpeedrunUtils by default is set up to automatically work with the current fastest Any% route so if you're doing that then you don't need to do any configuration at all. However, if you want some extra splits, want to change frame limiting keys or are deviating from the current route (Doing the Flesh Prince challenges) then you need to do some configuration.

First, open the game so that it can download the files it needs. Then go to your game directory -> BepInEx -> config -> SpeedrunUtils.

If you want to change your frame limiting keys then open Keys.txt.
If you want to change your splits then open splits.txt.


## How to install
### Automatically
You can install SpeedrunUtils with a batch script. You can download the batch file [here](https://raw.githubusercontent.com/Loomeh/SpeedrunUtilsInstaller/main/InstallSpeedrunUtils.bat) or just copy and paste this command into command prompt.
```
curl https://raw.githubusercontent.com/Loomeh/SpeedrunUtilsInstaller/main/InstallSpeedrunUtils.bat -o %temp%\sruinstall.bat && %temp%\sruinstall.bat
```
Then add the LiveSplit Server component to your LiveSplit layout by going to Edit Layout -> Add -> Control -> LiveSplit Server. Activate the server by right click LiveSplit -> Control -> Start Server.

### Manually
If you've installed SpeedUtils or NinjaUtils:
- Download the [SpeedrunUtils DLL](https://github.com/Loomeh/SpeedrunUtils/releases/latest) and place it in your BepInEx Plugins folder
- Download the [LiveSplit Server Component](https://github.com/LiveSplit/LiveSplit.Server/releases/tag/1.8.19)
- Extract its contents into your LiveSplit Components folder
- Then add the LiveSplit Server component to your LiveSplit layout by going to Edit Layout -> Add -> Control -> LiveSplit Server.
- Activate the server by right click LiveSplit -> Control -> Start Server.


If you haven't then follow these instructions:
- Download [BepInEx 5.4.21](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21) and extract it to your BRCF install directory.
- Open Bomb Rush Cyberfunk so BepInEx will run its setup, then close the game once you're at the main menu.
- Download `SpeedrunUtils.dll` from [GitHub](https://github.com/Loomeh/SpeedrunUtils/releases/latest) and place it into `[BRCF Install Dir]\BepInEx\plugins`
- Then add the LiveSplit Server component to your LiveSplit layout by going to Edit Layout -> Add -> Control -> LiveSplit Server.
- Activate the server by right click LiveSplit -> Control -> Start Server.
