/*
name: Get Prereq Classes and Forge (Joe Flow)
description: Standalone wrapper that farms prerequisites in the same order as FarmerJoeDoAll, with LoO first, Hero's Valiance in Forge Weapon, and boost equipment equipped before any farming.
tags: prereq, prerequisites, forge, classes, ultras, joe, farmerjoe, bard, lordoforder, herovaliance
*/

//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreAdvanced.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Other/Classes/VerusDoomKnight.cs
//cs_include Scripts/Other/Classes/REP-based/Shaman.cs
//cs_include Scripts/Other/Classes/REP-based/StoneCrusher.cs
//cs_include Scripts/Other/Classes/KingsEcho.cs
//cs_include Scripts/Good/ArchPaladin.cs
//cs_include Scripts/Other/Classes/DragonOfTime.cs
//cs_include Scripts/Nation/Various/ArchFiend.cs
//cs_include Scripts/Dailies/LordOfOrder.cs
//cs_include Scripts/Farm/REP/AlchemyREP.cs
//cs_include Scripts/Farm/REP/GoodREP.cs
//cs_include Scripts/Enhancement/UnlockForgeEnhancements.cs
//cs_include Scripts/Hollowborn/HollowbornReapersScythe.cs
//cs_include Scripts/Seasonal/TalkLikeaPirateDay/CelestialPirateCommander[PollyRogers].cs
//cs_include Scripts/Good/GearOfAwe/BladeOfAwe.cs
//cs_include Scripts/Other/Classes/REP-based/Bard.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skua.Core.Interfaces;
using Skua.Core.Models.Items;
using Skua.Core.Options;

public class GetPrerequisitesWithJoeFlow
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
    private PrerequisiteManager Mgr => _mgr ??= new PrerequisiteManager();
    private PrerequisiteManager? _mgr;

    public string OptionsStorage = "PrereqOptions";
    public bool DontPreconfigure = true;

    public List<IOption> Options = new()
    {
        new Option<bool>("AutoEquipBoosts", "Auto-Equip Best Boosts",
            "Equips the best all‑monster weapon and all‑race armor/pet immediately at the start (if they exist).", false),
        new Option<bool>("FarmMissingBoosts", "Farm Missing Boost Items",
            "If no suitable boost item is found, farms Hollowborn Reaper's Scythe (40% all) and/or Polly Roger (30% all tagged) at the very end of the script.", false),
        CoreBots.Instance.SkipOptions,
    };

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        bool autoEquip = Bot.Config!.Get<bool>("AutoEquipBoosts");
        bool farmMissing = Bot.Config!.Get<bool>("FarmMissingBoosts");

        // ──────────── INITIAL STATUS REPORT ────────────
        Mgr.PrintInitialStatus();

        // ──────────── EQUIP EXISTING BOOSTS (if enabled) ────────────
        if (autoEquip)
        {
            Core.Logger("[Joe Flow] Auto‑equipping best boost items (existing only)...", "Info");
            Mgr.EquipBestBoostItems();
        }

        // ──────────── PHASE 0: LORD OF ORDER (FIRST) ────────────
        Core.Logger("[Joe Flow] Phase 0: Lord of Order – starting daily quests early", "Info");
        Mgr.EnsureLordOfOrder(rankUp: true);

        // ──────────── PHASE 1: EARLY CLASSES ────────────
        Core.Logger("[Joe Flow] Phase 1: Early core classes", "Info");
        Mgr.EnsureBard(rankUp: true);
        Mgr.EnsureShaman(rankUp: true);
        Mgr.EnsureStoneCrusher(rankUp: true);
        Mgr.EnsureArchPaladin(rankUp: true);

        // ──────────── PHASE 2: MID CLASSES ────────────
        Core.Logger("[Joe Flow] Phase 2: Mid-tier classes", "Info");
        Mgr.EnsureVerusDoomKnight(rankUp: true);
        Mgr.EnsureDragonOfTime(rankUp: true);
        Mgr.EnsureKingEcho(rankUp: true);
        Mgr.EnsureArchFiend(rankUp: true);

        // ──────────── PHASE 3: REPUTATIONS ────────────
        Core.Logger("[Joe Flow] Phase 3: Reputation farming (Alchemy & Good)", "Info");
        Mgr.EnsureAlchemyReputation(8);
        Mgr.EnsureGoodReputation(10);

        // ──────────── PHASE 4: FORGE, BLADE OF AWE, AND HERO'S VALIANCE ────────────
        Core.Logger("[Joe Flow] Phase 4: Forge, Blade of Awe, and Hero's Valiance", "Info");
        Mgr.EnsureBladeOfAwe();
        Mgr.EnsureForgeEnhancements();      // now handles Lacerate, Praxis, and Hero's Valiance together

        // ──────────── PHASE 5: FARM MISSING BOOSTS (if enabled) ────────────
        if (farmMissing)
        {
            Core.Logger("[Joe Flow] Phase 5: Farming missing boost items...", "Info");
            Mgr.FarmMissingBoostItems(equipAfterFarming: autoEquip);
        }

        // ──────────── FINAL STATUS REPORT ────────────
        Mgr.PrintFinalStatus();

        Core.SetOptions(false);
        Bot.Stop();
    }
}

// ─────────────────────────────────────────────────────────────
// PREREQUISITE MANAGER – holds all logic
// ─────────────────────────────────────────────────────────────

public class PrerequisiteManager
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private CoreBots Core => CoreBots.Instance;
    private CoreAdvanced Adv => _Adv ??= new();
    private CoreAdvanced? _Adv;
    private CoreFarms Farm => _Farm ??= new();
    private CoreFarms? _Farm;

    private static VerusDoomKnightClass VDK => _VDK ??= new();
    private static VerusDoomKnightClass? _VDK;
    private static Shaman Sham => _Sham ??= new();
    private static Shaman? _Sham;
    private static StoneCrusher SC => _SC ??= new();
    private static StoneCrusher? _SC;
    private static KingsEcho KE => _KE ??= new();
    private static KingsEcho? _KE;
    private static ArchPaladin AP => _AP ??= new();
    private static ArchPaladin? _AP;
    private static DragonOfTime DoT => _DoT ??= new();
    private static DragonOfTime? _DoT;
    private static ArchFiend AF => _AF ??= new();
    private static ArchFiend? _AF;
    private static UnlockForgeEnhancements Forge => _Forge ??= new();
    private static UnlockForgeEnhancements? _Forge;
    private static LordOfOrder LOO => _LOO ??= new();
    private static LordOfOrder? _LOO;
    private static AlchemyREP AlchemyRep => _AlchemyRep ??= new();
    private static AlchemyREP? _AlchemyRep;
    private static GoodREP GoodRep => _GoodRep ??= new();
    private static GoodREP? _GoodRep;
    private static HollowbornScythe HBS => _HBS ??= new();
    private static HollowbornScythe? _HBS;
    private static CelestialPirateCommander CPC => _CPC ??= new();
    private static CelestialPirateCommander? _CPC;
    private static BladeOfAwe BoA => _BoA ??= new();
    private static BladeOfAwe? _BoA;
    private static Bard BardInstance => _BardInstance ??= new();
    private static Bard? _BardInstance;

    private static readonly int[] LordOfOrderQuestIds =
    {
        7156, 7157, 7158, 7159, 7160, 7161, 7162, 7163, 7164, 7165
    };

    private static readonly string[] RequiredClasses =
    [
        "Bard",
        "Verus DoomKnight",
        "Shaman",
        "StoneCrusher",
        "King's Echo",
        "ArchPaladin",
        "Dragon of Time",
        "ArchFiend"
    ];

    private static readonly string[] RaceKeys = ["Human", "Undead", "Dragonkin", "Chaos", "Elemental"];

    private static readonly ItemCategory[] WeaponCategories =
    [
        ItemCategory.Sword, ItemCategory.Axe, ItemCategory.Dagger,
        ItemCategory.Gun, ItemCategory.HandGun, ItemCategory.Rifle,
        ItemCategory.Bow, ItemCategory.Mace, ItemCategory.Gauntlet,
        ItemCategory.Polearm, ItemCategory.Staff, ItemCategory.Wand,
        ItemCategory.Whip
    ];

    // ─── HELPERS ──────────────────────────────────────────────────────

    private float NormaliseBoost(float raw) => raw > 1f ? raw - 1f : raw;

    private bool IsClassComplete(string className)
    {
        Core.Unbank(className);
        if (!Core.CheckInventory(className))
            return false;
        int rank = Core.CheckClassRank(ClassName: className);
        return rank >= 9;
    }

    private bool ShouldRankUpClass(string className)
    {
        Core.Unbank(className);
        if (!Core.CheckInventory(className))
            return false;
        int rank = Core.CheckClassRank(ClassName: className);
        return rank < 9;
    }

    private bool IsClassMissing(string className)
    {
        Core.Unbank(className);
        return !Core.CheckInventory(className);
    }

    private bool HasBladeOfAwe()
    {
        return Core.CheckInventory("Blade of Awe", toInv: true) || Core.CheckInventory("Blade of Awe");
    }

    private int GetLordOfOrderProgress()
    {
        int count = 0;
        foreach (int questId in LordOfOrderQuestIds)
            if (Core.isCompletedBefore(questId)) count++;
        return count;
    }

    private bool IsLordOfOrderComplete()
    {
        return IsClassComplete("Lord Of Order");
    }

    private int ReputationRank(string repName) => Bot.Reputation.GetRank(repName);
    private bool HasReputation(string repName, int requiredRank) => ReputationRank(repName) >= requiredRank;

    // Combined check for all three weapon forge enhancements
    private bool AreForgeWeaponsComplete()
    {
        return Adv.uLacerate() && Adv.uPraxis() && Adv.uValiance();
    }

    private bool AllPrereqsComplete()
    {
        return IsLordOfOrderComplete()
            && IsClassComplete("Verus DoomKnight")
            && IsClassComplete("Shaman")
            && IsClassComplete("StoneCrusher")
            && IsClassComplete("King's Echo")
            && IsClassComplete("ArchPaladin")
            && IsClassComplete("Dragon of Time")
            && IsClassComplete("ArchFiend")
            && IsClassComplete("Bard")
            && HasReputation("Alchemy", 8)
            && HasReputation("Good", 10)
            && AreForgeWeaponsComplete()
            && Adv.uForgeHelm()
            && Adv.uForgeCape()
            && HasBladeOfAwe();
    }

    // ─── ACQUISITION HELPERS ─────────────────────────────────────────

    private void AcquireClass(string className, bool rankUp, Action getClass, Action? rankUpAction = null)
    {
        Core.Unbank(className);

        if (IsClassMissing(className))
        {
            Core.Logger($"[PrereqMgr] Doing {className} class...", "Info");
            getClass();
        }
        else if (rankUp && ShouldRankUpClass(className))
        {
            Core.Logger($"Ranking up {className} to rank 10...", "Info");
            if (rankUpAction != null)
                rankUpAction();
            else
                Adv.RankUpClass(className);
        }
        else if (IsClassComplete(className))
        {
            Core.Logger($"Skipping {className}; already owned and rank 10.", "Info");
        }
        else
        {
            Core.Logger($"[PrereqMgr] Doing {className} class...", "Info");
            getClass();
        }
    }

    // ─── PUBLIC ENSURE METHODS ──────────────────────────────────────

    public void EnsureBard(bool rankUp = true)
    {
        AcquireClass("Bard", rankUp, () => BardInstance.GetBard(rankUpClass: rankUp));
    }

    public void EnsureVerusDoomKnight(bool rankUp = true)
    {
        AcquireClass("Verus DoomKnight", rankUp, () => VDK.GetClass(rankup: rankUp));
    }

    public void EnsureShaman(bool rankUp = true)
    {
        AcquireClass("Shaman", rankUp, () => Sham.GetShaman(rankUpClass: rankUp));
    }

    public void EnsureStoneCrusher(bool rankUp = true)
    {
        AcquireClass("StoneCrusher", rankUp, () => SC.GetSC(rankUpClass: rankUp));
    }

    public void EnsureKingEcho(bool rankUp = true)
    {
        AcquireClass("King's Echo", rankUp, () => KE.GetKE(rankup: rankUp));
    }

    public void EnsureArchPaladin(bool rankUp = true)
    {
        AcquireClass("ArchPaladin", rankUp, () => AP.GetAP(rankUpClass: rankUp));
    }

    public void EnsureDragonOfTime(bool rankUp = true)
    {
        AcquireClass("Dragon of Time", rankUp, () => DoT.GetDoT(rankUpClass: rankUp));
    }

    public void EnsureArchFiend(bool rankUp = true)
    {
        AcquireClass("ArchFiend", rankUp, () => AF.GetArchfiend(rankUp));
    }

    public void EnsureLordOfOrder(bool rankUp = true)
    {
        if (IsLordOfOrderComplete())
        {
            Core.Logger("Skipping Lord Of Order; already complete.", "Info");
            return;
        }
        Core.Logger("[PrereqMgr] Doing Lord Of Order daily class (start as early as possible)...", "Info");
        LOO.GetLoO(rankUp);
    }

    public void EnsureAlchemyReputation(int targetRank = 8)
    {
        int alchRank = ReputationRank("Alchemy");
        if (alchRank >= targetRank)
        {
            Core.Logger($"Skipping Alchemy reputation; already rank {alchRank} (>= {targetRank}).", "Info");
            return;
        }
        Core.Logger($"[PrereqMgr] Getting Alchemy reputation to rank {targetRank} (current: {alchRank})...", "Info");
        AlchemyRep.ScriptMain(Bot);
        Core.SetOptions();
    }

    public void EnsureGoodReputation(int targetRank = 10)
    {
        int goodRank = ReputationRank("Good");
        if (goodRank >= targetRank)
        {
            Core.Logger($"Skipping Good reputation; already rank {goodRank} (>= {targetRank}).", "Info");
            return;
        }
        Core.Logger($"[PrereqMgr] Getting Good reputation to rank {targetRank} (current: {goodRank})...", "Info");
        GoodRep.ScriptMain(Bot);
        Core.SetOptions();
    }

    public void EnsureBladeOfAwe()
    {
        if (HasBladeOfAwe())
        {
            Core.Logger("Skipping Blade of Awe; already obtained.", "Info");
            return;
        }
        Core.Logger("[PrereqMgr] Getting Blade of Awe (unlocks Awe enhancements)...", "Info");
        BoA.GetBoA();
    }

    public void EnsureForgeEnhancements()
    {
        bool weaponsComplete = AreForgeWeaponsComplete();
        bool helmComplete = Adv.uForgeHelm();
        bool capeComplete = Adv.uForgeCape();

        if (weaponsComplete && helmComplete && capeComplete)
        {
            Core.Logger("Skipping forge enhancements; all required forge tiers (including Hero's Valiance) are already unlocked.", "Info");
            return;
        }

        Core.Logger("[PrereqMgr] Doing forge enhancements (including Hero's Valiance)...", "Info");
        Forge.ForgeUnlocks();

        // After ForgeUnlocks, check if Valiance is still missing and log a warning
        if (!Adv.uValiance())
        {
            Core.Logger("[PrereqMgr] Hero's Valiance not unlocked by ForgeUnlocks; may need manual unlocking.", "Warning");
        }
        else
        {
            Core.Logger("[PrereqMgr] Hero's Valiance successfully unlocked.", "Info");
        }
    }

    // ─── BOOST ITEMS ──────────────────────────────────────────────────

    private List<InventoryItem> GetAllItems()
    {
        var items = new List<InventoryItem>();
        if (Bot.Inventory.Items != null)
            items.AddRange(Bot.Inventory.Items);
        if (Bot.Bank.Items != null)
            items.AddRange(Bot.Bank.Items);
        return items;
    }

    private InventoryItem? FindBestWeapon(List<InventoryItem> items)
    {
        var candidates = items
            .Where(i => WeaponCategories.Contains(i.Category) || Adv.WeaponCatagories.Contains(i.Category))
            .Select(i => new { Item = i, Boost = Core.GetBoostFloat(i, "dmgAll") })
            .Where(x => x.Boost >= 1.4f)
            .OrderByDescending(x => x.Boost)
            .ToList();
        return candidates.FirstOrDefault(x => x.Item.Equipped)?.Item ?? candidates.FirstOrDefault()?.Item;
    }

    private InventoryItem? FindBestArmorOrPet(List<InventoryItem> items)
    {
        var candidates = items
            .Where(i => i.Category == ItemCategory.Armor ||
                        (i.CategoryString?.Equals("Pet", StringComparison.OrdinalIgnoreCase) == true))
            .Select(i => new {
                Item = i,
                Boosts = RaceKeys.Select(r => Core.GetBoostFloat(i, r)).ToList(),
                IsEquipped = i.Equipped
            })
            .Where(x => x.Boosts.All(b => b >= 1.3f))
            .OrderByDescending(x => x.Boosts.Sum())
            .ToList();

        return candidates.FirstOrDefault(x => x.IsEquipped)?.Item ?? candidates.FirstOrDefault()?.Item;
    }

    private void EquipItem(InventoryItem item)
    {
        if (item == null) return;

        if (Bot.Bank.Contains(item.ID))
        {
            Core.Logger($"    → Moving {item.Name} from bank to inventory...", "Info");
            Bot.Bank.ToInventory(item.ID);
            Bot.Wait.ForBankToInventory(item.ID);
            Bot.Sleep(500);
        }

        if (Bot.Inventory.Contains(item.ID) && !Bot.Inventory.IsEquipped(item.ID))
        {
            Core.Logger($"    → Equipping {item.Name}...", "Info");
            Bot.Inventory.EquipItem(item.ID);
            Bot.Wait.ForItemEquip(item.ID);
            Bot.Sleep(500);
        }
    }

    private void EnsureLevel100()
    {
        int currentLevel = Bot.Player.Level;
        if (currentLevel >= 100)
        {
            Core.Logger($"[PrereqMgr] Level {currentLevel} — already 100+.", "Info");
            return;
        }

        Core.Logger($"[PrereqMgr] Level {currentLevel} — need 100. Farming XP...", "Warning");
        Farm.Experience(100);
        if (Bot.Player.Level < 100)
            Core.Logger("[PrereqMgr] Failed to reach level 100. Scythe requires level 100.", "Error");
        else
            Core.Logger($"[PrereqMgr] Successfully reached level {Bot.Player.Level}!", "Info");
    }

    // ─── PUBLIC BOOST METHODS ──────────────────────────────────────

    /// <summary>
    /// Equips the best existing boost items (weapon and armor/pet) without farming.
    /// </summary>
    public void EquipBestBoostItems()
    {
        if (Bot.Bank.Items == null || Bot.Bank.Items.Count == 0)
        {
            Bot.Bank.Load();
            Bot.Wait.ForTrue(() => (Bot.Bank.Items?.Count ?? 0) > 0, 20);
        }

        var allItems = GetAllItems();

        var weapon = FindBestWeapon(allItems);
        if (weapon != null)
        {
            float norm = NormaliseBoost(Core.GetBoostFloat(weapon, "dmgAll"));
            Core.Logger($"  ✅ Found weapon: {weapon.Name} (+{norm:P0} all damage) – equipping.", "Info");
            EquipItem(weapon);
        }
        else
        {
            Core.Logger("  ❌ No 40%+ all‑damage weapon found (will farm later if enabled).", "Warning");
        }

        var armorPet = FindBestArmorOrPet(allItems);
        if (armorPet != null)
        {
            var boosts = RaceKeys.Select(r => NormaliseBoost(Core.GetBoostFloat(armorPet, r)));
            var vals = RaceKeys.Zip(boosts, (r, v) => $"{r}={v:P0}");
            Core.Logger($"  ✅ Found armor/pet: {armorPet.Name} ({string.Join(", ", vals)}) – equipping.", "Info");
            EquipItem(armorPet);
        }
        else
        {
            Core.Logger("  ❌ No 30%+ all‑race armor/pet found (will farm later if enabled).", "Warning");
        }
    }

    /// <summary>
    /// Farms missing boost items (Hollowborn Reaper's Scythe and/or Polly Roger) if needed.
    /// </summary>
    /// <param name="equipAfterFarming">If true, equips the newly farmed items.</param>
    public void FarmMissingBoostItems(bool equipAfterFarming)
    {
        Core.Logger("[PrereqMgr] Farming missing boost items...", "Info");

        if (Bot.Bank.Items == null || Bot.Bank.Items.Count == 0)
        {
            Bot.Bank.Load();
            Bot.Wait.ForTrue(() => (Bot.Bank.Items?.Count ?? 0) > 0, 20);
        }

        var allItems = GetAllItems();

        // Check weapon
        var weapon = FindBestWeapon(allItems);
        if (weapon == null)
        {
            Core.Logger("  → No 40%+ weapon found. Farming Hollowborn Reaper's Scythe...", "Info");
            EnsureLevel100();
            HBS.ScriptMain(Bot);
            allItems = GetAllItems(); // refresh
            weapon = FindBestWeapon(allItems);
            if (weapon != null)
            {
                Core.Logger($"  ✅ Farmed: {weapon.Name} (+{NormaliseBoost(Core.GetBoostFloat(weapon, "dmgAll")):P0})", "Info");
                if (equipAfterFarming)
                    EquipItem(weapon);
            }
            else
            {
                Core.Logger("  ❌ Failed to farm a 40%+ weapon.", "Warning");
            }
        }
        else
        {
            Core.Logger($"  ✅ Already have weapon: {weapon.Name} (+{NormaliseBoost(Core.GetBoostFloat(weapon, "dmgAll")):P0})", "Info");
        }

        // Check armor/pet
        var armorPet = FindBestArmorOrPet(allItems);
        if (armorPet == null)
        {
            Core.Logger("  → No 30%+ all‑race armor/pet found. Farming Polly Roger...", "Info");
            CPC.ScriptMain(Bot);
            allItems = GetAllItems(); // refresh
            armorPet = FindBestArmorOrPet(allItems);
            if (armorPet != null)
            {
                Core.Logger($"  ✅ Farmed: {armorPet.Name} (30%+ all races)", "Info");
                if (equipAfterFarming)
                    EquipItem(armorPet);
            }
            else
            {
                Core.Logger("  ❌ Failed to farm a 30%+ all‑race armor/pet.", "Warning");
            }
        }
        else
        {
            Core.Logger($"  ✅ Already have armor/pet: {armorPet.Name}", "Info");
        }
    }

    // ─── STATUS REPORTS ──────────────────────────────────────────────

    public void PrintInitialStatus()
    {
        Core.Logger("─────────────────────────────────────────────", "Info");
        Core.Logger("📋 INITIAL PREREQUISITE STATUS", "Info");
        Core.Logger("─────────────────────────────────────────────", "Info");
        PrintStatusSummary();
    }

    public void PrintFinalStatus()
    {
        Core.Logger("─────────────────────────────────────────────", "Info");
        Core.Logger("✅ FINAL PREREQUISITE STATUS", "Info");
        Core.Logger("─────────────────────────────────────────────", "Info");
        PrintStatusSummary();
        ShowFinalMessageBox();
    }

    private void PrintStatusSummary()
    {
        bool allOk = AllPrereqsComplete();

        Core.Logger($"Overall Status: {(allOk ? "✅ ALL COMPLETE" : "❌ MISSING ITEMS")}", allOk ? "Info" : "Warning");

        // Classes
        foreach (string cls in RequiredClasses)
        {
            bool complete = IsClassComplete(cls);
            Core.Logger($"  {(complete ? "✅" : "❌")} {cls}: rank 10" + (complete ? "" : " (missing/not rank 10)"), complete ? "Info" : "Warning");
        }
        bool looComplete = IsLordOfOrderComplete();
        Core.Logger($"  {(looComplete ? "✅" : "❌")} Lord Of Order: rank 10" + (looComplete ? "" : " (in progress or missing)"), looComplete ? "Info" : "Warning");

        // Reputations
        int alchRank = ReputationRank("Alchemy");
        bool alchOk = alchRank >= 8;
        Core.Logger($"  {(alchOk ? "✅" : "❌")} Alchemy: rank {alchRank}" + (alchOk ? "" : " (need rank 8)"), alchOk ? "Info" : "Warning");

        int goodRank = ReputationRank("Good");
        bool goodOk = goodRank >= 10;
        Core.Logger($"  {(goodOk ? "✅" : "❌")} Good: rank {goodRank}" + (goodOk ? "" : " (need rank 10)"), goodOk ? "Info" : "Warning");

        // Forge & Awe
        bool hasBoA = HasBladeOfAwe();
        Core.Logger($"  {(hasBoA ? "✅" : "❌")} Blade of Awe (Awe enhancements)" + (hasBoA ? "" : " (missing)"), hasBoA ? "Info" : "Warning");

        bool weaponsComplete = AreForgeWeaponsComplete();
        Core.Logger($"  {(weaponsComplete ? "✅" : "❌")} Forge Weapon (Lacerate, Praxis, Valiance)" + (weaponsComplete ? "" : " (not fully unlocked)"), weaponsComplete ? "Info" : "Warning");

        bool helmDone = Adv.uForgeHelm();
        Core.Logger($"  {(helmDone ? "✅" : "❌")} Forge Helm (all tiers)" + (helmDone ? "" : " (not fully unlocked)"), helmDone ? "Info" : "Warning");

        bool capeDone = Adv.uForgeCape();
        Core.Logger($"  {(capeDone ? "✅" : "❌")} Forge Cape (all tiers)" + (capeDone ? "" : " (not fully unlocked)"), capeDone ? "Info" : "Warning");

        // Level & Gold
        bool hasLevel100 = Bot.Player.Level >= 100;
        long gold = Bot.Player.Gold;
        bool hasGold = gold >= 10_000_000;
        Core.Logger($"  {(hasLevel100 ? "✅" : "❌")} Level 100: {Bot.Player.Level}" + (hasLevel100 ? "" : " / 100"), hasLevel100 ? "Info" : "Warning");
        Core.Logger($"  {(hasGold ? "✅" : "❌")} Gold: {gold:N0} >= 10,000,000" + (hasGold ? "" : " (need 10M)"), hasGold ? "Info" : "Warning");

        // Boost items (current state)
        var allItems = GetAllItems();
        var weapon = FindBestWeapon(allItems);
        var armorPet = FindBestArmorOrPet(allItems);
        bool hasWeapon = weapon != null;
        bool hasArmorPet = armorPet != null;

        string weaponStatus;
        if (hasWeapon)
        {
            float boost = NormaliseBoost(Core.GetBoostFloat(weapon, "dmgAll"));
            weaponStatus = $"{weapon.Name} (+{boost:P0})";
        }
        else
        {
            weaponStatus = "none found";
        }
        Core.Logger($"  {(hasWeapon ? "✅" : "❌")} Weapon: {weaponStatus}", hasWeapon ? "Info" : "Warning");

        string armorStatus = hasArmorPet ? $"{armorPet.Name} (30%+ all races)" : "none found";
        Core.Logger($"  {(hasArmorPet ? "✅" : "❌")} Armor/Pet: {armorStatus}", hasArmorPet ? "Info" : "Warning");

        Core.Logger("─────────────────────────────────────────────", "Info");
    }

    private void ShowFinalMessageBox()
    {
        var allItems = GetAllItems();
        var weapon = FindBestWeapon(allItems);
        var armorPet = FindBestArmorOrPet(allItems);
        bool hasWeapon = weapon != null;
        bool hasArmorPet = armorPet != null;
        bool hasLevel100 = Bot.Player.Level >= 100;
        long gold = Bot.Player.Gold;
        bool hasGold = gold >= 10_000_000;

        float weaponBoost = hasWeapon ? NormaliseBoost(Core.GetBoostFloat(weapon, "dmgAll")) : 0f;

        string armorPetType = "None";
        string armorPetName = "None";
        float armorBoost = 0f;
        bool uniformBoost = false;
        string raceBoostsStr = "";

        if (hasArmorPet)
        {
            armorPetType = armorPet.Category == ItemCategory.Armor ? "Armor" : "Pet";
            armorPetName = armorPet.Name;
            var raceBoosts = RaceKeys.Select(r => NormaliseBoost(Core.GetBoostFloat(armorPet, r))).ToList();
            raceBoostsStr = string.Join(", ", RaceKeys.Zip(raceBoosts, (r, v) => $"{r} {v:P0}"));
            uniformBoost = raceBoosts.All(v => Math.Abs(v - raceBoosts[0]) < 0.01f);
            armorBoost = uniformBoost ? raceBoosts[0] : 0f;
        }

        float totalBoost = 0f;
        bool canComputeTotal = hasWeapon && hasArmorPet && uniformBoost;
        if (canComputeTotal)
            totalBoost = (1 + weaponBoost) * (1 + armorBoost) - 1;

        string ign = Core.Username();

        var sb = new StringBuilder();
        sb.AppendLine($"✅ ULTRAS PREREQUISITE STATUS – {ign}");
        sb.AppendLine();

        sb.AppendLine("📚 CLASSES:");
        foreach (string cls in RequiredClasses)
        {
            bool complete = IsClassComplete(cls);
            sb.AppendLine($"  {(complete ? "✅" : "❌")} {cls}: rank 10" + (complete ? "" : " (needs rank 10)"));
        }
        bool looComplete = IsLordOfOrderComplete();
        sb.AppendLine($"  {(looComplete ? "✅" : "❌")} Lord Of Order: rank 10" + (looComplete ? "" : " (needs rank 10)"));
        sb.AppendLine();

        sb.AppendLine("📈 REPUTATIONS:");
        int alchRank = ReputationRank("Alchemy");
        bool alchOk = alchRank >= 8;
        sb.AppendLine($"  {(alchOk ? "✅" : "❌")} Alchemy: rank {alchRank}" + (alchOk ? "" : " (need rank 8)"));
        int goodRank = ReputationRank("Good");
        bool goodOk = goodRank >= 10;
        sb.AppendLine($"  {(goodOk ? "✅" : "❌")} Good: rank {goodRank}" + (goodOk ? "" : " (need rank 10)"));
        sb.AppendLine();

        sb.AppendLine("🔧 FORGE & AWE:");
        bool hasBoA = HasBladeOfAwe();
        sb.AppendLine($"  {(hasBoA ? "✅" : "❌")} Blade of Awe (Awe enhancements)" + (hasBoA ? "" : " (needs farming)"));

        bool weaponsComplete = AreForgeWeaponsComplete();
        sb.AppendLine($"  {(weaponsComplete ? "✅" : "❌")} Weapon: Lacerate, Praxis, Valiance" + (weaponsComplete ? "" : " (not fully unlocked)"));

        bool helmDone = Adv.uForgeHelm();
        sb.AppendLine($"  {(helmDone ? "✅" : "❌")} Helm: all tiers" + (helmDone ? "" : " (not fully unlocked)"));

        bool capeDone = Adv.uForgeCape();
        sb.AppendLine($"  {(capeDone ? "✅" : "❌")} Cape: all tiers" + (capeDone ? "" : " (not fully unlocked)"));
        sb.AppendLine();

        sb.AppendLine("💰 LEVEL & GOLD:");
        sb.AppendLine($"  {(hasLevel100 ? "✅" : "❌")} Level 100: {Bot.Player.Level}" + (hasLevel100 ? "" : " / 100"));
        sb.AppendLine($"  {(hasGold ? "✅" : "❌")} Gold: {gold:N0} >= 10,000,000" + (hasGold ? "" : " (need 10M)"));
        sb.AppendLine();

        sb.AppendLine("⚔️ BOOST ITEMS:");
        if (hasWeapon)
            sb.AppendLine($"  ✅ Weapon: {weapon.Name} (+{weaponBoost:P0} all)");
        else
            sb.AppendLine($"  ❌ Weapon: no 40%+ all-damage weapon");

        if (hasArmorPet)
            sb.AppendLine($"  ✅ {armorPetType}: {armorPetName} ({raceBoostsStr})");
        else
            sb.AppendLine($"  ❌ Armor/Pet: no 30%+ tagged boost item");

        if (canComputeTotal)
            sb.AppendLine($"     Total Boost: +{totalBoost:P0} (multiplicative)");
        sb.AppendLine();

        sb.AppendLine($"⚙️  CONFIG:");
        bool autoEquip = Bot.Config!.Get<bool>("AutoEquipBoosts");
        bool farmMissing = Bot.Config!.Get<bool>("FarmMissingBoosts");
        sb.AppendLine($"  • AutoEquipBoosts:    {(autoEquip ? "✅" : "❌")}");
        sb.AppendLine($"  • FarmMissingBoosts:  {(farmMissing ? "✅" : "❌")} (farms at the end if needed)");
        sb.AppendLine();

        if (!hasGold || !hasLevel100 || !hasWeapon || !hasArmorPet || !weaponsComplete)
        {
            sb.AppendLine("🔧 RECOMMENDED ACTIONS:");
            if (!hasGold) sb.AppendLine("  • Run ArmyPrismata.cs (gold)");
            if (!hasLevel100 && !farmMissing) sb.AppendLine("  • Enable FarmMissingBoosts or farm XP");
            if (!hasWeapon) sb.AppendLine("  • Run HollowbornReapersScythe.cs (or enable FarmMissingBoosts)");
            if (!hasArmorPet) sb.AppendLine("  • Run CelestialPirateCommander[PollyRogers].cs (or enable FarmMissingBoosts)");
            if (!weaponsComplete) sb.AppendLine("  • Unlock Lacerate, Praxis, and Hero's Valiance (forge progression)");
        }
        else
            sb.AppendLine("🎉 All good – proceed to Ultras!");
        Bot.ShowMessageBox(sb.ToString(), "Ultras Prerequisites (Joe Flow)");
    }
}
