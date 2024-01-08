# YATM
Yet Another Terminal Mod for Lethal Company

## Stats
You can show the current stats of the savefile that are usually shown after being fired.<br />
The stats include:
- Days survived
- Deaths
- Scrap collected (value)
- Steps taken
You can either type "show stats" or just "stats".

## Teleport
You can teleport a player without leaving the terminal.<br />
Both "tp" and "teleport" work for this.<br />
Sending "tp [playername]" or "teleport [playername]" will switch the camera view and teleport the specific player.

## Known Issues
Due to a race condition with switching camera targets and starting the teleport it will try to teleport before the camera view can switch.