# Star Fox 64 Score Tracker
The Star Fox 64 Score Tracker is a tool I created for the Star Fox 64 score running Discord community [HIT + 64](https://discord.gg/k2JnzC4). The program is heavily inspired by the speedrunning tool LiveSplit and is intended to not only be used for personal tracking but to be incorporated into streaming layouts to provide viewers with an indication of how well a run is going.

### Download: [SF64ScoreTracker.exe](https://drazil100.bitbucket.io/SF64ScoreTracker.exe) 

# Whats New?

### Last Updated: 4/18/2022

- New "Sum of Worst" comparison! Starting this build the Score Tracker will keep a record of all of your past PBs and will be able to generate a Sum of Worst based on the worst scores of your most recent PBs. You can configure in the options window how many runs back it looks with the default being your past 3 PBs.
- You can now have more (or less) than 7 splits. The number of splits displayed on screen is currently non-configurable so if you add a lot of splits you will need to expand the window size to accommodate.
- Aliases for the Horizontal layout are no longer hard coded in and can be configured in your config.ini file (GUI option coming). Upgrading will automatically generate a full list of Aliases for Star Fox 64.

# How to update?
The tracker will always be backwards compatible with previous config and score tracking files. Simply replace the executable of the file you are currently using with the new one linked above and it will function with all of the same files generated by the old copy of the program. 

# Running on Linux
This program is written in C# which is a Microsoft programming language and doesn't really port super well to other operating systems. This program is however tested and compatible with mono for linux. The package you need on Debian / Ubuntu based systems is mono-complete and the command to run the program assuming you cant just opening from your file browsing program is mono SF64ScoreTracker.exe

[License](LICENSE.md)


