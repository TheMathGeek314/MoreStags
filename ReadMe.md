# MoreStags

This rando connection adds new possible stag locations and shuffles which ones are active in a given seed.
If enabled, some new stag locations will be added, and some vanilla stag locations may be removed.
These are brand new locations and are not pooled with the rest of the rando check locations.

## Settings
- **Enabled** - Toggles whether the set of active stags is vanilla or shuffled
	- This is different from randomizing stags. Shuffled stags can behave as either vanilla stags or as rando checks based on the Rando 4 stag setting.
- **Selection** - Choose between a *Balanced* or *Random* selection process
	- *Balanced* - Stags will be chosen from each of 20 areas without repeating an area until all have been used (repeating this process up to 40, 60, etc)
	- *Random* - No smart algorithm will be used, stags will be chosen completely at random
- **Quantity** - The number of stags active in your seed
	- This ranges from the default 11 up to all 114 active at once
	- The actual number of active stags may be lower than this value if set higher than the filter settings will allow
	- Setting this to 21 while using the Balanced setting will guarantee one stag in every area (20 areas + Dirtmouth)
- **Prefer Non Vanilla** - Removes vanilla stags from the selection pool
	- Dirtmouth and Stag Nest may ignore this setting
- **Remove Cursed Locations** - Removes *Stag-God_Spring* from the selection pool


## Misc
- Dirtmouth will always be active from the start, regardless of settings
- Stag Nest will be active unless either enemy pogo skips or transition rando are enabled
- If more than 11 stags are active including Stag Nest, Stag Nest will be available after unlocking 9 other stags (not including Dirtmouth)
- Some stag locations overlap with enemy placements. These enemies will be removed when arriving via stag, but they will exist as normal when entering from another transition
	- In rare cases, this could cause impossible seeds when using TheRealJournalRando, especially in decoupled room randos

## Compatibility / Interop
- **DebugMod**'s *All Stags* bind will grant all active stags, rather than all vanilla stags
- If **CondensedSpoilerLogger** is enabled:
	- The condensed log will include all active stags in the usual Stag Stations section
	- A new file "*MoreStagsActiveSpoiler.txt*" will provide a list of active stags even if stags are not randomized
- **QoL**'s *Stag Arrive* setting works with all new locations
- **RandoSettingsManager** works as expected

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
Hive Entryway, Honey Tree, Library, Stash

#### Deepnest
Labyrinth, Garpedes, Shadow Gauntlet, Galien, Failed Tramway, Weavers' Den

#### Ancient Basin

#### Abyss

#### Howling Cliffs

#### White Palace

#### Godhome
