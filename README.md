# Get Prereq Classes and Forge Enhancements

A **vibe-coded** one‑click script that farms, ranks, and equips everything needed for **Ultras-v3** challenges. No coding knowledge required – just press run.

---

## Overview

This script automatically prepares your account for any Ultra boss by:

- **Checking** which classes, reputations, forge tiers, and boost items you already have.
- **Farming and ranking up** all required classes:
  - **Lord of Order** is done **first** (daily quests + rank 10).
  - Bard, Shaman, StoneCrusher, ArchPaladin, King's Echo, ArchFiend, Dragon Of Time, Verus DoomKnight.
- **Farming Alchemy (rank 8)** and **Good (rank 10)** reputations.
- **Unlocking the minimum weapon enhancements** for all Ultras:
  - **Blade of Awe** → unlocks **Health Vamp, Mana Vamp, Awe Blast**.
  - **Lacerate**, **Praxis**, **Hero's Valiance** from the Forge questline.
- **Unlocking all helm and cape forge tiers**.
- **Auto‑equipping** the best %+ all‑monster weapon and %+ tagged armor/pet from inventory/bank.
- **Optionally farming** Hollowborn Reaper's Scythe (40% all) and/or Polly Roger (30% tagged) if no suitable boost items are found.
- **Logging progress** and stopping cleanly after a final status report.

---

## What This Script Provides (Weapon Enhancements)

| Enhancement | Unlocked Via | Used In |
|-------------|--------------|---------|
| **Lacerate** | Forge questline | Most taunters and DPS |
| **Praxis** | Forge questline | ArchPaladin, Verus DoomKnight, supports |
| **Health Vamp** | Blade of Awe | Dage, Gramiel, Kolr, etc. |
| **Mana Vamp** | Blade of Awe | Many classes (primary or fallback) |
| **Awe Blast** | Blade of Awe | Lord of Order, supports in Darkon/Tyndarius |

> ✅ This is the **minimum required set** – enough to clear all Ultras. For optimal damage, consider farming Valiance, Elysium, etc. later.

---

## Requirements

- Skua (latest version)

---

## Installation

1. Download `GetPrerequisites.cs`.
2. Place it in: `Skua\Scripts\Ultrasv3\DependenciesUltras\`
3. Restart Skua.

---

## Configuration

| Option | Description | Default |
|--------|-------------|---------|
| **AutoEquipBoosts** | Equips the best all‑monster weapon and all‑race tagged armor/pet from inventory/bank. | `false` |
| **FarmMissingBoosts** | If no suitable boost is found, farms Scythe (40% all) and/or Polly (30% tagged). If disabled, just recommends them. | `false` |

---

## Usage

1. Open Skua → navigate to the script (`Ultrasv3/DependenciesUltras`).
2. Click **Start Script**.
3. Set the options as desired.
4. Run the script – it will:
   - Show a progress check.
   - **Do Lord of Order daily quests first** (if needed).
   - Farm missing classes, reputations, and forge tiers.
   - Unlock Blade of Awe (Awe enhancements).
   - Unlock Lacerate and Praxis.
   - Handle boost items (auto‑equip or farm fallbacks).
   - Display a final status table and stop.

---

## ⚠️ Disclaimer

**Use at your own risk.** Botting may violate the Terms of Service of AdventureQuest Worlds (AQW). This script is provided for **educational and informational purposes only**. The author is not responsible for any account actions taken by Artix Entertainment, including but not limited to warnings, suspensions, or bans. By using this script, you accept full responsibility for your actions.

---

> Built with AI, driven by vibes. Good luck in Ultras-v3!
