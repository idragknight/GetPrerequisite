# Get Prereq Classes and Forge Enhancements

One‑click script to farm, rank, and equip everything needed for **Ultras-v3** challenges in AQW.

---

## What it does

- Checks and farms missing classes: Verus DoomKnight, Shaman, StoneCrusher, King's Echo, ArchPaladin, Dragon of Time, ArchFiend.
- Unlocks Lord of Order (completes dailies, ranks to 10).
- Farms Alchemy (rank 8) and Good (rank 10) reputations.
- Unlocks all required forge enhancements (Lacerate, Praxis, Helm, Cape).
- Auto‑equips best 40%+ weapon and 30%+ tagged armor/pet from inventory/bank.
- Optionally farms Hollowborn Reaper's Scythe (51% all) and/or Polly Roger (30% tagged) if missing.
- Logs progress and stops cleanly after a final status report.

---

## Requirements

- Skua (latest version)
- Access to required quests (storylines not farmed by this script)

---

## Installation

1. Download `GetPrerequisites.cs`.
2. Place it in: `Skua\Scripts\Ultrasv3\DependenciesUltras\`
3. Restart Skua.

---

## Configuration

Two options (toggle in Skua before running):

| Option | Default |
|--------|---------|
| AutoEquipBoosts | `true` |
| FarmMissingBoosts | `true` |

---

## Usage

Run the script. It will:

- Show a progress check.
- Farm what’s missing.
- Auto‑equip boost items (and farm fallbacks if enabled).
- Display a final status table and stop.

---

## Dependencies (cs_include)
