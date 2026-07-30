# Get Prereq Classes and Forge Enhancements

A **Vibe** one‑click script to farm, rank, and equip everything needed for **Ultras-v3** challenges in AQW.

---

## Overview

This script automatically:
- Checks which classes, reputations, forge tiers, and boost items you already have.
- **Completes Lord of Order daily quests (and ranks to 10) first**, then proceeds to farm missing classes (Verus DoomKnight, Shaman, StoneCrusher, King's Echo, ArchPaladin, Dragon of Time, ArchFiend) and ranks them to 10.
- Farms Alchemy (rank 8) and Good (rank 10) reputations.
- Unlocks all required forge enhancements (Lacerate, Praxis, Helm, Cape).
- Auto‑equips the best 40%+ all‑monster weapon and 30%+ tagged armor/pet from your inventory/bank.
- Optionally farms Hollowborn Reaper's Scythe (40% all) and/or Polly Roger (30% tagged) if missing.
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

| Option | Description | Default |
|--------|-------------|---------|
| AutoEquipBoosts | Equips the best all‑monster weapon and all‑race tagged armor/pet found in inventory/bank. | `false` |
| FarmMissingBoosts | If no suitable boost item is found, farms Scythe (40% all) and/or Polly (30% tagged). If disabled, just recommends them. | `false` |

---

## Usage

1. Open Skua and navigate to the script (`Ultrasv3/DependenciesUltras`).
2. Set the options as desired.
3. Click **Run**.
4. The script will:
   - Show a progress check.
   - **Do Lord of Order daily quests first** (if not already completed/rank 10).
   - Farm missing classes, reputations, and forge tiers.
   - Auto‑equip boost items (and farm fallbacks if enabled).
   - Display a final status table and stop.
