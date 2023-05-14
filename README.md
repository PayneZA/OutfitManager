# Outfit Manager
`https://raw.githubusercontent.com/PayneZA/OutfitManager/main/OutfitManager/latest/OutfitManager.json`

# User Guide

## 1. Introduction
The Outfit Manager is a Dalamud plugin that helps you manage and equip different outfits for your character. It has several features like persisting outfits, locking outfits, setting collections, previewing outfits, and more.

## 2. Installation
To install the plugin, follow these steps:

- Open the Dalamud plugin installer
- Add `https://raw.githubusercontent.com/PayneZA/OutfitManager/main/OutfitManager/latest/OutfitManager.json` to your sources.
- Search for "Outfit Manager"
- Install the plugin

## 3. Commands
To access the features of the plugin, use the following commands:

- `/omg`: Opens the Outfit Manager UI, showing the list of outfits if you have any, otherwise opens the configuration window
- `/omg config`: Opens the configuration window
- `/omg wear OUTFITNAME`: Equips a saved outfit by name
- `/omg random TAGNAME`: Equips a random outfit with a specified tag
- `/omg other`: Opens the remote outfit control window
- `/omg persist`: Toggles outfit persistence. If enabled, the plugin will reapply your outfit whenever it is needed (e.g. zone changes, login)
- `/omg lockoutfit SECONDS`: Locks your last worn outfit, including gearset, for a specified amount of seconds (optional)
- `/omg reset`: Clears your last equipped outfit and sets your primary Penumbra collection, if specified
- `/omg setcollectiontype COLLECTIONTYPE`: Sets your Penumbra collection type away from the default 'Your Character' to a custom type or 'reset' to go to the default option
- `/omg snapshot`: Will have a temporary penumbra / glamourer combination that will re-apply if re-apply is enabled and be lost on wear outfit or restart.
- `/omg clearsnapshot`: Will manually clear the snapshot.

## 4. Configuration
The configuration window allows you to set up the plugin's settings, such as:

- Enabling or disabling chat control
- Setting up Penumbra collections
- Setting up outfit previews

## 5. Windows
The plugin has several UI windows to manage different aspects of your outfits:

- Config Window: Configuration settings
- Main Window: General outfit management
- Allowed Character Window: Manage allowed characters for outfit control
- Outfit List Window: A list of all saved outfits as well as adding new ones.
- Other Characters Window: Manage outfit control for other characters
- Outfit Preview Window: Preview your outfits before equipping

## 6. Notes

- To use the outfit preview system, you need to have images with the same name as your outfit in the preview directory you set in the configuration window.
- The plugin may not work properly if you are locked out by a friend using the outfit lock feature.

That's it! You're now ready to use the Outfit Manager plugin to manage your character's outfits in-game.
