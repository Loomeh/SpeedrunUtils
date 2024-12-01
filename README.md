# SpeedrunUtils

## Features
- Built in FPS limiter
- A new autosplitter that's more accurate, more stable and easier to maintain
- Leaderboard legal automash for unskippable dialogue

## How to use
Insert to open the GUI (is configurable).

By default, the O key will uncap your framerate and the P key will limit your framerate to 30FPS. If you want to change this, read "How to configure".

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

##### Manual
SpeedrunUtils can be manually installed by downloading `SpeedrunUtils.dll` from the Releases section and placing it inside your BepInEx plugins folder.


Make sure to start LiveSplit's TCP Server by right clicking on LiveSplit -> Control -> Start TCP Server. SpeedrunUtils will not work with LiveSplit if you don't do this.


## Credits
Judah Caruso - Making SpeedUtils, which SpeedrunUtils was based upon. \
realJomoko - Research and code contributions. \
NinjaCookie - Research and code contributions. \
Erisrine - Italian translation \
BRC Speedrunning Discord - Testing and bug reports
