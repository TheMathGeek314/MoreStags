# MoreStags

This rando connection adds new possible stag locations and shuffles which ones are active in a given seed.
If enabled, some new stag locations will be added, and some vanilla stag locations may be removed.
These are brand new locations and are not pooled with the rest of the rando check locations.

## Settings
- **Enabled** - Toggles whether the set of active stags is vanilla or shuffled
	- This is different from randomizing stags. Shuffled stags can behave as either vanilla stag tolls or as rando shiny checks depending on the base Randomizer stag setting.
- **Selection** - Choose between a *Balanced* or *Random* selection process
	- *Balanced* - Stags will be chosen from each of 20 areas without repeating an area until all have been used (repeating this process up to 41, 61, etc)
	- *Random* - No smart algorithm will be used, stags will be chosen completely at random
- **Quantity** - The number of stags active in your seed
	- This ranges from the default 11 up to all 114 active at once
	- The actual number of active stags may be lower than this value if set higher than the filter settings will allow
	- Setting this to 21 while using the Balanced setting will guarantee one stag in every area (20 areas + Dirtmouth)
- **Stag Nest Threshold** - The percentage of active stags required to unlock Stag Nest (if Stag Nest is active)
	- *Half*: 50%
	- *Many*: 75%
	- *Most*: 90%
	- *All*: 100%
- **Prefer Non Vanilla** - Removes vanilla stags from the selection pool
	- Dirtmouth and Stag Nest may ignore this setting
- **Remove Cursed Locations** - Removes *Stag-God_Spring*, *Stag-Blue_Room*, and *Stag-Hall_of_Gods* from the selection pool


## Misc
- Dirtmouth will always be active, regardless of settings
- Stag Nest will be active unless either enemy pogo skips or transition rando are enabled
- Some stag locations overlap with enemy placements. These enemies will be removed when arriving via stag, but they will exist as normal when entering from another transition
	- In rare cases, this could cause impossible seeds when using TheRealJournalRando, especially in decoupled room randos
- When a stag item is obtained in the same room as a stag location, in some cases you must reload the room to be able to travel to the new station
	- This only happens if you open the stag menu before picking up the item
	- This is a known bug, but no fix is planned because it would induce excessive lag every time you open the stag menu. If encountered, either leaving the room or stagging anywhere else will update the menu to show the missing entry.

## Compatibility / Interop
- **DebugMod**'s *All Stags* bind will grant all active stags, rather than all vanilla stags
- If **CondensedSpoilerLogger** is enabled:
	- The condensed log will include all active stags in the usual Stag Stations section
	- A new file "*MoreStagsActiveSpoiler.txt*" will provide a list of active stags even if stags are not randomized
- **QoL**'s *Stag Arrive* setting works with all new locations
- **RandoSettingsManager** works as expected
- **AlreadyEnoughPlayMaker** is recommended to reduce lag while scrolling but is not strictly required as a dependency

## Known Bug
- When a stag item is found in the same room as a stag location, the new item will not appear if the menu has already been opened in that room.
- Reloading the room or stagging somewhere else will properly update the stag menu.
- No fix is planned for this because doing so would induce excessive lag every time the stag menu is opened.
- (This excessive lag technically already happens on room entry, but that's pretty unavoidable)

## New Stags (by area)
#### Crossroads
Grubfather, Southwest Crossroads, Goams, East Crossroads, Myla, Brooding Mawlek, False Knight

#### Upper Greenpath
Waterfall East, Waterfall West, Central Greenpath, Moss Knight

#### Lower Greenpath
Hunter, Artist's Approach, Gulkas, Durandoos, Stone Sanctuary

#### Fog Canyon
Fog Canyon, Archives Entrance, Teacher's Archives

#### Upper Fungal
Fungal Colony, Upper Wastes, Elder Hu, Pilgrim's Way, Fungal Nexus, City Gate

#### Lower Fungal
Mantis Outskirts, Bretta, Mantis Village, Fungal Core, Fungal Elder

#### Queen's Gardens
Droppers, Garden Respite, Love Key, Loodles, Upper Gardens, White Lady

#### West City
Rafters, Triple Gate, Office, Lower Storerooms, Soul Sanctum

#### East City
East Elevator, Flooded Station, Memorial, Watcher's Spire

#### Waterways
Isma's Grove, Flukemarm, East Waterways

#### Crystal Peak
Mine Entrance, Overlook, Conga Line, Crystallised Mound, Crystal Corridor, Hallownest's Crown

#### Resting Grounds
Upper Tram, Dreamshield, Grey Mourner, Shrine of Believers

#### Kingdom's Edge
Lower Kingdom's Edge, Hoppers, Scarecrow, Quick Slash, Camp, Bardoon, Pale Lurker

#### Hive
Hive Entryway, Honey Tree, Bee Library, Honey Stash

#### Deepnest
Labyrinth, Garpedes, Shadow Gauntlet, Galien, Failed Tramway, Weavers' Den

#### Ancient Basin
Broken Bridge, Lower Tramway, Ancient Basin, Mawlurks, Monarch Wings

#### Abyss
The Abyss, Void Tendrils, Birthplace

#### Howling Cliffs
Baldur Shell, Wasteland, Mato

#### White Palace
Palace Entrance, Palace Foyer, Palace Atrium, Right Atrium Far, Right Atrium Near, Lower Left Atrium, Upper Left Atrium, Lower Balcony, Upper Balcony, Workshop, Nursery, Path of Pain 2, Path of Pain 3, Path of Pain 4

#### Godhome
Blue Room, Hall of Gods, God Spring