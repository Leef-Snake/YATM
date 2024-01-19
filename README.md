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
### EXPERIMENTAL
Sending "tp [playername]" or "teleport [playername]" will switch the camera view and teleport the specific player.<br />
This is disabled by default, but can be turned on in the configs.

## ITP
Using the inverse teleporter via the command "itp" is possible.
It works the same as the regular inverse teleporter; it literally just presses the button.

## Configuration
Every command can be toggled off client-side via the config file.<br />
Plans for the future include syncing the config to clients when you're the host but this is currently NYI.

## Known Issues
Being the host and trying to teleport a player via name (teleport [name]) causes a desync and won't teleport anyone.