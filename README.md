# Dungeon-Generation
Modular Dungeon Generation
<a href="General">General</a>

## General plans for game
<span id="General"></span>
1) Menu (Play (single and multiplayer), Settings, Stats, Exit)
2) Overworld town before actual dungeon
3) General set up for story based gameplay for levels 1-100
4) 2D sprites for living entities in the 3D environment
5) Enchantment and Curse system for loot based off of floor difficulty
6) Time bonus mechanic
7) "Doors" (Zones that trigger next room?)
8) Room focus for camera angle? (Camera view not determined yet) 
9) Small stackable permanent upgrades purchasable with coins
10) One time run items purchasable with coins


## Plans for the DungeonGen Script

1) Method to make custom "Rooms" of varying sizes
2) Method to vary hallway lengths (likely determined before hallway creation) 
3) Finish making the end room method and logic, for placing the final room (boss) (Custom boss room method?)
4) Add components to "Rooms" for random dungeon aesthetics and interactives
5) Add checks so instead of creating dungeons based off of iterations it has a minimum room count based on difficulty
6) Make some hallways create entrances when intersecting with a room instead of deadend
7) Puzzle Rooms
8) Figure out how to make clusters (My guess is that is generates a whole dungeon, and when it gets to the last node it'll begin generating a new dungeon entirely from the last node, repeat for x iterations)


## Aesthetics of a dungeon

1) Bones/Skeletons
2) Torches/Candles
3) Rocks/Rubble
4) Plants
    - Trees
    - Vines
    - Moss/Grass
5) Chains
6) Crates/Barrels
7) Water/Drains
8) Furniture
    - Table
    - Chair
    - Carpet
9) Fireplace


## Interactive in a dungeon

1) Chests
2) Levers
3) Loot
    - Coins
    - Weapons
    - Potions
4) Traps
5) Doors (?)
6) Shrines


## Aesthetics of a Village

1) Buildings
    - Silos
    - Wells
    - Huts/Houses
2) Animals
3) walkways
4) Church
5) Trees/Rocks


## Interactive in a Village

1) Villagers
    - Shopkeeper
    - Old adventurer
    - Contracter
    - BlackSmith
    - Seer
2) LeaderBoard (May just keep in stats)
3) Tavern

## Known Bugs (To fix)

- Fix wall blockers from canceling iterations (Happens when dungeon builds inward on itself)

## Changes I remember to mention

### [3/19/2018]
- Adjusted DeadEndFrequencyCorrection to actually work (Changes DE tags to anything else if nodeCount is too low during main DunGen generation phase)

### [3/20/2018]
- Started "RandomHallway" method which takes in pendingNode to build from and also takes a random length size
(Most code is from DunGen script so I have to compartmentalize DunGen into smaller methods)
