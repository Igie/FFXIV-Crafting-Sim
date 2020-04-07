# FFXIV-Crafting-Sim

FFXIV crafting simulator using genetic algorithm to find best possible solution and more...

Currently uses https://github.com/ufx/SaintCoinach
This helps to read all game recipes and images instead of needing to update program manually after every update, in short, automatic updater.

If you like FFXIVTeamcraft crafting simulator you should like this one too.
Most basics are implemented:
Showing progress/quality increase, setting conditions(still needs refinement), modifying crafter stats, food, tea. As far as I checked progress/quality increases are accurate but do tell me if you find false values.

It uses local files to store items, recipes, images, your best rotations and stats so don't be afraid if you see a few .db files, they are made so program doesn't need to access you game files and read all the data, which takes more time.

There are still many bugs and I am working on this project everyday. Do help me with anything from program layout to efficiency of coding and optimization.

Known bugs:
Current rotation images disappear when changing crafter stats.
Sometimes when searching for rotation with already defined rotation, the defined rotation disappears.

Features:
Implementing success rate (currently uses actions with 100% rate).
Implementing fractal rotations (list of actions which don't necessarily finish recipe but rather increases progress/quality at high efficiency (most increase at smallest cost). Later they can be joined to make full rotation.
