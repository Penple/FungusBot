# FungusBot

FungusBot is a [Discord](https://discord.com/) bot designed to streamline [Among Us](http://www.innersloth.com/gameAmongUs.php) games by completely automatically muting/deafening Discord users based on their status in game.

## Installation

1. Download the latest release of FungusBot from GitHub.
2. Download and place the [config.json](config.json) in the same directory as the FungusBot.exe binary.
3. Modify config.json with your discord bot token and the ID of your guild and voice channel that you would like controlled.
4. Start Among Us and start FungusBot.exe.

## Usage

FungusBot is able to automatically mute and deafen users in the configured voice channel automatically as the game progresses.  
In-game players will be deafened to allow for the dead players to speak to each other, and will not be undeafened/unmuted until the next meeting is called (to prevent players from using others voice status to cheat).

Discord users must have the exact same discord username or nickname as their name within the game.

FungusBot can not yet detect when games end.  
`!end` may be used during the win screen to undeafen everyone in the channel.

`!start` can also be used to re-deafen everyone if necessary.

Users that do not have a username that is the same as one in game will not be controlled by the bot.

## How does it work?

FungusBot reads the memory of Among Us to detect several components of the game state. This is more reliable than other methods of reading game state such as OCR. Unfortunately it also means that new memory addresses will have to be found after every game update.