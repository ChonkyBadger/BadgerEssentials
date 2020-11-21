# BadgerEssentials
A C# FiveM script. It is inspired by Badssentials by Badger, a lua script.  

This script basically add lots of useful stuff, such as /revive and /respawn,  
or like the name of the script name, essentials.

## Commands
`/Die` Kills the player who executes this command.

`/Revive [Target Player ID]` Revives the player who executes this command or another player, specified by their ID.

`/Respawn` Respawns the player who executes this command.

`/Peacetime` Toggles peacetime on and off.

`/SetAOP <aop>` Sets the aop to whatever arguments are given.

`/Postal <postal>` Sets a waypoint to the specified postal.

`/Announce <Announcement Message>` Displays a message to all players on the server. Requires permission node BadgerEssentials.Command.Announce  

## Configuration
### "displayElements"
In the displayElements section of the config file, you are able to change properties of text which is displayed on screen.  
For example, the x and y position of the aop.  
**textAllignment:** 0 = Center | 1 = Left | 2 = Right.

### "commands"
Here, you can edit properties of commands.  
**Cooldown values:** A number of seconds, must be an integer (whole number).

## Permissions
`BadgerEssentials.Command.Announce` Gives access to the /Announce command.

## Links
- [Postal Map used by this script](https://github.com/ocrp/postal_map/)
- [My Discord Server](https://discord.gg/TFCQE8d)

