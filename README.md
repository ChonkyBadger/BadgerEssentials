# BadgerEssentials
A C# FiveM script. It is inspired by Badssentials, a great lua script by Badger.

NOTE: This script is no longer maintained, there is now a newer version available: [BadgerEssentials v2](https://github.com/ChonkyBadger/BadgerEssentialsv2)

This script basically add lots of useful stuff, such as /revive and /respawn,  
or like the name of the script name, essentials.

Support is available in my discord, linked at the bottom of this page.

## Latest Version  
V1.11

## Features
**Ragdoll:** Players can toggle ragdoll by pressing "U" by default, editable in config.

## Commands
`/toggle-hud` Toggles the hud.

`/die` Kills the player who executes this command.

`/revive [Target Player ID]` Revives the player who executes this command or another player, specified by their ID.

`/respawn` Respawns the player who executes this command.

`/pt` Toggles peacetime on and off.

`/pc <duration>` Turns on priority cooldown for a set time in minutes.  
`/pc-inprogress` Turns on "Priority in Progress".  
`/pc-onhold` Turns on "Priorities on hold".  
`/pc-reset` Resets priority cooldown status to none.  

`/setAOP <aop>` Sets the aop to whatever arguments are given. Requires permission node BadgerEssentials.Command.SetAOP

`/postal <postal>` Sets a waypoint to the specified postal.

`/announce <Announcement Message>` Displays a message to all players on the server. Requires permission node BadgerEssentials.Command.Announce  

## configuration
### "displays.json"
This is where everythig displayed on the HUD is stored. You can add  
your own custom displays here by following the format of other ones.

**textAllignment:** 0 = Center | 1 = Left | 2 = Right.  
**colours:** This chooses the primary and secondary colour of the display elements using fiveM colour codes.  
This is to make a quick and easy way of changing colours without manually doing it fot every display.

### config.json
#### "commands"  
Here, you can edit properties of commands.  
**Cooldown values:** A number of seconds, must be an integer (whole number).

#### "ragdoll"  
**Key:** This decides what key will cause the player to ragdoll, by default, "U"  
https://docs.fivem.net/docs/game-references/controls/

## Permissions
`BadgerEssentials.Command.Announce` Gives access to the /Announce command.  

`BadgerEssentials.Command.PriorityCooldown` Gives access to all priority cooldown commands.  
`BadgerEssentials.Command.PC` Gives access to /pc  
`BadgerEssentials.Command.PCInProgress` Gives access to /pc-inprogress  
`BadgerEssentials.Command.PCOnHold` Gives access to /pc-onhold  
`BadgerEssentials.Command.PCReset` Gives access to /pc-reset  


`BadgerEssentials.Command.Peacetime` Gives access to /pt  
`BadgerEssentials.Command.SetAOP` Gives access to /SetAOP.  
`BadgerEssentials.Bypass.ReviveTimer` Bypasses the timer before you can revive.    

## Installation
Under "releases", download the latest version and extract the files to a folder. You should have one folder with the script's  
files in it. You should see a config folder, fxmanifest.lua, README.md, License, and 3 dlls. IF you see a .sln, it means you downloaded  
the source code instead of the compiled files. In your server.cfg, you will want to add:  
"start BadgerEssentials". If you did not name the folder, "BadgerEssentials", just replace it with whatever you did name it.  

## Credit
- [Badger](https://forum.cfx.re/u/OfficialBadger)

## License
Full license is viewable in the LICENSE file in this repository.  
This license is subject to be changed at any given time.

## Links
- [Postal Map used by this script](https://github.com/ocrp/postal_map/)
- [My Discord Server](https://discord.gg/TFCQE8d)

