/*
name: Get Prereq Classes and Forge Enhancements
description: Standalone wrapper that uses the existing class and forge prerequisite scripts for Ultras v3.
tags: prereq, prerequisites, forge, classes, ultras
*/

//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreAdvanced.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skua.Core.Interfaces;
using Skua.Core.Models.Items;
using Skua.Core.Options;

public class PrereqClassesAndForge
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

    // NEW: Blade of Awe (unlocks Awe enhancements)
    private static BladeOfAwe BoA => _BoA ??= new();
    private static BladeOfAwe? _BoA;

    private static readonly int[] LordOfOrderQuestIds =
    {
        7156, 7157, 7158, 7159, 7160, 7161, 7162, 7163, 7164, 7165
    };

    private static readonly string[] RequiredClasses =
    [
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

    private readonly List<string> _messageLines = new();

    public string OptionsStorage = "PrereqOptions";
    public bool DontPreconfigure = true;

    public List<IOption> Options = new()
    {
        new Option<bool>("AutoEquipBoosts", "Auto-Equip Best Boosts",
            "Automatically equips the best all monster weapon and all race tagged armor/pet found in inventory/bank.", false),
        new Option<bool>("FarmMissingBoosts", "Farm Missing Boost Items",
            "If no suitable boost item is found, farms Hollowborn Reaper's Scythe (40% all monster) and/or Polly Roger (30% all tagged). If false, just recommends them.", false),
        CoreBots.Instance.SkipOptions,
    };

    // ─── HELPERS ──────────────────────────────────────────────────────

    private float NormaliseBoost(float raw) => raw > 1f ? raw - 1f : raw;

    // Returns true if class is owned and rank >= 9 (rank 10)
    private bool IsClassComplete(string className)
    {
        Core.Unbank(className);
        if (!Core.CheckInventory(className))
            return false;
        int rank = Core.CheckClassRank(ClassName: className);
        return rank >= 9;
    }

    // Returns true if class is owned and rank < 9 (needs ranking up)
    private bool ShouldRankUpClass(string className)
    {
        Core.Unbank(className);
        if (!Core.CheckInventory(className))
            return false;
        int rank = Core.CheckClassRank(ClassName: className);
        return rank < 9;
    }

    // Returns true if class is not owned at all
    private bool IsClassMissing(string className)
    {
        Core.Unbank(className);
        return !Core.CheckInventory(className);
    }

    // NEW: Check if Blade of Awe is owned (unlocks Awe enhancements)
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
            && HasReputation("Alchemy", 8)
            && HasReputation("Good", 10)
            && Adv.uLacerate()
            && Adv.uPraxis()
            && Adv.uForgeHelm()
            && Adv.uForgeCape()
            && HasBladeOfAwe();   // <-- NEW: ensures Awe enhancements are unlocked
    }

    // ─── ACQUISITION ──────────────────────────────────────────────────

    private void AcquireClass(string className, bool rankUp, Action getClass, Action? rankUpAction = null)
    {
        Core.Unbank(className);

        // 1. If class is missing, farm it
        if (IsClassMissing(className))
        {
            Core.Logger($"[PrereqClassesAndForge] Doing {className} class...", "Info");
            getClass();
        }
        // 2. If class is owned but not rank 10, rank it up
        else if (rankUp && ShouldRankUpClass(className))
        {
            Core.Logger($"Ranking up {className} to rank 10...", "Info");
            if (rankUpAction != null)
                rankUpAction();
            else
                Adv.RankUpClass(className);
        }
        // 3. If class is owned and rank 10, skip
        else if (IsClassComplete(className))
        {
            Core.Logger($"Skipping {className}; already owned and rank 10.", "Info");
        }
        // Fallback: if something went wrong, try farming
        else
        {
            Core.Logger($"[PrereqClassesAndForge] Doing {className} class...", "Info");
            getClass();
        }
    }

    // ─── PUBLIC METHODS ──────────────────────────────────────────────

    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        if (Bot.Bank.Items == null || Bot.Bank.Items.Count == 0)
        {
            Bot.Bank.Load();
            Bot.Wait.ForTrue(() => (Bot.Bank.Items?.Count ?? 0) > 0, 20);
        }

        PrintProgress();

        bool autoEquip = Bot.Config!.Get<bool>("AutoEquipBoosts");
        bool farmMissing = Bot.Config!.Get<bool>("FarmMissingBoosts");

        if (AllPrereqsComplete())
        {
            Core.Logger("[PrereqClassesAndForge] All prerequisite classes and forge enhancements are already complete. Nothing to do.", "Info");
            if (autoEquip || farmMissing)
                HandleBoostItems(autoEquip, farmMissing);
            ShowPostPrereqPrompt();
            Core.SetOptions(false);
            Bot.Stop();
            return;
        }

        GetAll();

        if (autoEquip || farmMissing)
            HandleBoostItems(autoEquip, farmMissing);

        ShowPostPrereqPrompt();
        Core.SetOptions(false);
        Bot.Stop();
    }

    public void PrintProgress()
    {
        Core.Logger("[PrereqClassesAndForge] Progress check:", "Info");
        _messageLines.Clear();

        void LogStatus(string name, bool complete, string detail = "")
        {
            string line = complete ? $"✓ {name}: complete (rank 10)" : $"• {name}: needs to be done (rank 9)";
            if (!complete && !string.IsNullOrEmpty(detail))
                line += $" — {detail}";
            _messageLines.Add(line);
            Core.Logger(line, complete ? "Info" : "Warning");
        }

        // Classes – check ownership and rank
        LogStatus("Verus DoomKnight", IsClassComplete("Verus DoomKnight"));
        LogStatus("Shaman", IsClassComplete("Shaman"));
        LogStatus("StoneCrusher", IsClassComplete("StoneCrusher"));
        LogStatus("King's Echo", IsClassComplete("King's Echo"));
        LogStatus("ArchPaladin", IsClassComplete("ArchPaladin"));
        LogStatus("Dragon of Time", IsClassComplete("Dragon of Time"));
        LogStatus("ArchFiend", IsClassComplete("ArchFiend"));

        // Lord Of Order
        bool looComplete = IsLordOfOrderComplete();
        string looDetail = looComplete ? "" : $"{GetLordOfOrderProgress()}/{LordOfOrderQuestIds.Length} quests done";
        LogStatus("Lord Of Order", looComplete, looDetail);

        // Reputations
        int alchRank = ReputationRank("Alchemy");
        bool alchOk = alchRank >= 8;
        LogStatus("Alchemy reputation", alchOk, alchOk ? "" : $"rank {alchRank}/8");

        int goodRank = ReputationRank("Good");
        bool goodOk = goodRank >= 10;
        LogStatus("Good reputation", goodOk, goodOk ? "" : $"rank {goodRank}/10");

        // NEW: Blade of Awe (unlocks Awe enhancements)
        bool hasBoA = HasBladeOfAwe();
        LogStatus("Blade of Awe (Awe enhancements)", hasBoA, hasBoA ? "" : "needs to be obtained");

        // Forge
        bool weaponDone = Adv.uLacerate() && Adv.uPraxis();
        bool helmDone = Adv.uForgeHelm();
        bool capeDone = Adv.uForgeCape();
        LogStatus("Forge weapon enhancements", weaponDone);
        LogStatus("Forge helm enhancements", helmDone);
        LogStatus("Forge cape enhancements", capeDone);

        // Optional
        LogStatus("Hollowborn Reaper's Scythe (optional)", Core.CheckInventory("Hollowborn Reaper's Scythe"));
        LogStatus("Polly Roger (optional)", Core.CheckInventory("Polly Roger"));
    }

    public void GetAll(bool rankUp = true)
    {
        Core.Logger("[PrereqClassesAndForge] Getting prerequisite classes...", "Info");

        if (IsLordOfOrderComplete())
        {
            Core.Logger("Skipping Lord Of Order; already complete.", "Info");
        }
        else
        {
            int looProgress = GetLordOfOrderProgress();
            Core.Logger($"[PrereqClassesAndForge] Lord Of Order progress: {looProgress}/{LordOfOrderQuestIds.Length}", "Info");
            Core.Logger("[PrereqClassesAndForge] Doing Lord Of Order daily class first...", "Info");
            LOO.GetLoO(rankUp);
        }

        AcquireClass("Verus DoomKnight", rankUp, () => VDK.GetClass(rankup: rankUp));
        AcquireClass("Shaman", rankUp, () => Sham.GetShaman(rankUpClass: rankUp));
        AcquireClass("StoneCrusher", rankUp, () => SC.GetSC(rankUpClass: rankUp));
        AcquireClass("King's Echo", rankUp, () => KE.GetKE(rankup: rankUp));
        AcquireClass("ArchPaladin", rankUp, () => AP.GetAP(rankUpClass: rankUp));
        AcquireClass("Dragon of Time", rankUp, () => DoT.GetDoT(rankUpClass: rankUp));
        AcquireClass("ArchFiend", rankUp, () => AF.GetArchfiend(rankUp));

        GetReputation();

        // NEW: Get Blade of Awe (unlocks Awe enhancements)
        if (HasBladeOfAwe())
        {
            Core.Logger("Skipping Blade of Awe; already obtained.", "Info");
        }
        else
        {
            Core.Logger("[PrereqClassesAndForge] Getting Blade of Awe (unlocks Awe enhancements)...", "Info");
            BoA.GetBoA(); // This farms rep to rank 6 and gets the blade
        }

        Core.Logger("[PrereqClassesAndForge] Getting forge enhancements...", "Info");
        if (Adv.uLacerate() && Adv.uPraxis() && Adv.uForgeHelm() && Adv.uForgeCape())
        {
            Core.Logger("Skipping forge enhancements; all required forge tiers are already unlocked.", "Info");
        }
        else
        {
            Core.Logger("[PrereqClassesAndForge] Doing forge enhancements...", "Info");
            Forge.ForgeUnlocks();
        }
    }

    public void GetClassesOnly(bool rankUp = true)
    {
        AcquireClass("Verus DoomKnight", rankUp, () => VDK.GetClass(rankup: rankUp));
        AcquireClass("Shaman", rankUp, () => Sham.GetShaman(rankUpClass: rankUp));
        AcquireClass("StoneCrusher", rankUp, () => SC.GetSC(rankUpClass: rankUp));
        AcquireClass("King's Echo", rankUp, () => KE.GetKE(rankup: rankUp));
        AcquireClass("ArchPaladin", rankUp, () => AP.GetAP(rankUpClass: rankUp));
        AcquireClass("Dragon of Time", rankUp, () => DoT.GetDoT(rankUpClass: rankUp));
        AcquireClass("ArchFiend", rankUp, () => AF.GetArchfiend(rankUp));
    }

    public void GetReputation()
    {
        int alchRank = ReputationRank("Alchemy");
        if (alchRank >= 8)
        {
            Core.Logger("Skipping Alchemy reputation; already rank 8 or higher.", "Info");
        }
        else
        {
            Core.Logger($"[PrereqClassesAndForge] Getting Alchemy reputation to rank 8 (current rank: {alchRank})...", "Info");
            AlchemyRep.ScriptMain(Bot);
            Core.SetOptions();
        }

        int goodRank = ReputationRank("Good");
        if (goodRank >= 10)
        {
            Core.Logger("Skipping Good reputation; already rank 10 or higher.", "Info");
        }
        else
        {
            Core.Logger($"[PrereqClassesAndForge] Getting Good reputation to rank 10 (current rank: {goodRank})...", "Info");
            GoodRep.ScriptMain(Bot);
            Core.SetOptions();
        }
    }

    public void GetForgeEnhancements()
    {
        Core.Logger("[PrereqClassesAndForge] Doing forge enhancements...", "Info");
        Forge.ForgeUnlocks();
    }

    // ─── BOOST ITEM HANDLING ──────────────────────────────────────────

    private void HandleBoostItems(bool autoEquip, bool farmMissing)
    {
        Core.Logger("[PrereqClassesAndForge] Checking for boost items...", "Info");

        if (Bot.Bank.Items == null || Bot.Bank.Items.Count == 0)
        {
            Bot.Bank.Load();
            Bot.Wait.ForTrue(() => (Bot.Bank.Items?.Count ?? 0) > 0, 20);
        }

        var allItems = GetAllItems();

        var bestWeapon = FindBestWeapon(allItems);
        bool hasWeapon = bestWeapon != null;

        if (hasWeapon)
        {
            float raw = Core.GetBoostFloat(bestWeapon, "dmgAll");
            float norm = NormaliseBoost(raw);
            Core.Logger($"  ✅ Found weapon: {bestWeapon.Name} ({norm:P0} all damage)", "Info");
            if (autoEquip)
                EquipItem(bestWeapon);
        }
        else
        {
            Core.Logger("  ❌ No 40%+ all-damage weapon found.", "Warning");
            if (farmMissing)
            {
                Core.Logger("  → Farming Hollowborn Reaper's Scythe...", "Info");
                EnsureLevel100();
                HBS.ScriptMain(Bot);
                allItems = GetAllItems();
                bestWeapon = FindBestWeapon(allItems);
                if (bestWeapon != null && autoEquip)
                {
                    Core.Logger($"  ✅ Farmed and equipping: {bestWeapon.Name}", "Info");
                    EquipItem(bestWeapon);
                }
            }
        }

        var bestBoostItem = FindBestArmorOrPet(allItems);
        bool hasBoost = bestBoostItem != null;

        if (hasBoost)
        {
            var normalisedBoosts = RaceKeys.Select(r => NormaliseBoost(Core.GetBoostFloat(bestBoostItem, r)));
            var vals = RaceKeys.Zip(normalisedBoosts, (r, v) => $"{r}={v:P0}");
            Core.Logger($"  ✅ Found boost item: {bestBoostItem.Name} ({string.Join(", ", vals)})", "Info");
            if (autoEquip)
                EquipItem(bestBoostItem);
        }
        else
        {
            Core.Logger("  ❌ No armor or pet with 30%+ all race boosts found.", "Warning");
            if (farmMissing)
            {
                Core.Logger("  → Farming Polly Roger as fallback...", "Info");
                CPC.ScriptMain(Bot);
                allItems = GetAllItems();
                bestBoostItem = FindBestArmorOrPet(allItems);
                if (bestBoostItem != null && autoEquip)
                {
                    Core.Logger($"  ✅ Farmed and equipping: {bestBoostItem.Name}", "Info");
                    EquipItem(bestBoostItem);
                }
            }
        }

        allItems = GetAllItems();
        bool finalHasWeapon = FindBestWeapon(allItems) != null;
        bool finalHasBoost = FindBestArmorOrPet(allItems) != null;

        Core.Logger("[PrereqClassesAndForge] Boost item status:", "Info");
        Core.Logger($"  • 40%+ weapon: {(finalHasWeapon ? "✅" : "❌")}", finalHasWeapon ? "Info" : "Warning");
        Core.Logger($"  • 30%+ armor/pet: {(finalHasBoost ? "✅" : "❌")}", finalHasBoost ? "Info" : "Warning");
    }

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
            Core.Logger($"[PrereqClassesAndForge] Level {currentLevel} — already 100+.", "Info");
            return;
        }

        Core.Logger($"[PrereqClassesAndForge] Level {currentLevel} — need 100. Farming XP...", "Warning");
        Farm.Experience(100);
        if (Bot.Player.Level < 100)
            Core.Logger("[PrereqClassesAndForge] Failed to reach level 100. Scythe requires level 100.", "Error");
        else
            Core.Logger($"[PrereqClassesAndForge] Successfully reached level {Bot.Player.Level}!", "Info");
    }

    // ─── FINAL STATUS REPORT ──────────────────────────────────────────

    private void ShowPostPrereqPrompt()
    {
        bool autoEquip = Bot.Config!.Get<bool>("AutoEquipBoosts");
        bool farmMissing = Bot.Config!.Get<bool>("FarmMissingBoosts");

        var allItems = GetAllItems();
        var weapon = FindBestWeapon(allItems);
        var armorPet = FindBestArmorOrPet(allItems);
        bool hasWeapon = weapon != null;
        bool hasArmorPet = armorPet != null;
        bool hasLevel100 = Bot.Player.Level >= 100;
        long gold = Bot.Player.Gold;
        bool hasGold = gold >= 10_000_000;

        string ign = Core.Username();

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

        Core.Logger("", "Info");
        Core.Logger($"Report for: {ign}", "Info");
        Core.Logger("════════════════════════════════════════════════════════════════════════", "Info");
        Core.Logger("PREREQUISITE          STATUS     DETAIL", "Info");
        Core.Logger("────────────────────────────────────────────────────────────────────────", "Info");

        Core.Logger($"{"Gold",-22} {(hasGold ? "✅" : "❌"),-8} {gold:N0} >= 10,000,000", hasGold ? "Info" : "Warning");
        Core.Logger($"{"Level 100",-22} {(hasLevel100 ? "✅" : "❌"),-8} {Bot.Player.Level} / 100", hasLevel100 ? "Info" : "Warning");

        if (hasWeapon)
            Core.Logger($"{"Weapon",-22} {"✅",-8} +{weaponBoost:P0} {weapon.Name}", "Info");
        else
            Core.Logger($"{"Weapon",-22} {"❌",-8} None", "Warning");

        if (hasArmorPet)
        {
            string boostDisplay = uniformBoost ? $"+{armorBoost:P0}" : "(varied)";
            Core.Logger($"{armorPetType,-22} {"✅",-8} {boostDisplay} {armorPetName} ({raceBoostsStr})", "Info");
        }
        else
            Core.Logger($"{"Armor/Pet",-22} {"❌",-8} None", "Warning");

        if (canComputeTotal)
            Core.Logger($"{"Total Boost",-22} {"",-8} +{totalBoost:P0} (multiplicative: {weaponBoost:P0} × {armorBoost:P0})", "Info");
        else if (hasWeapon && hasArmorPet && !uniformBoost)
            Core.Logger($"{"Total Boost",-22} {"",-8} varies by race – see above", "Info");
        else
            Core.Logger($"{"Total Boost",-22} {"",-8} N/A (missing item)", "Info");

        Core.Logger("────────────────────────────────────────────────────────────────────────", "Info");
        Core.Logger($"AutoEquipBoosts:      {(autoEquip ? "✅ ENABLED" : "❌ DISABLED")}", "Info");
        Core.Logger($"FarmMissingBoosts:    {(farmMissing ? "✅ ENABLED" : "❌ DISABLED")}", "Info");
        Core.Logger("════════════════════════════════════════════════════════════════════════", "Info");

        if (!hasGold || !hasLevel100 || !hasWeapon || !hasArmorPet)
        {
            Core.Logger("⚠️  RECOMMENDED ACTIONS:", "Warning");
            if (!hasGold) Core.Logger("  • Run ArmyPrismata.cs (gold)", "Info");
            if (!hasLevel100 && !farmMissing) Core.Logger("  • Enable FarmMissingBoosts or farm XP manually", "Info");
            if (!hasWeapon) Core.Logger("  • Run HollowbornReapersScythe.cs", "Info");
            if (!hasArmorPet) Core.Logger("  • Run CelestialPirateCommander[PollyRogers].cs", "Info");
        }
        else
            Core.Logger("🎉 All prerequisites complete – ready for Ultras v3!", "Info");

        Core.Logger("", "Info");

        // ─── MESSAGE BOX ──────────────────────────────────────────────

        var sb = new StringBuilder();
        sb.AppendLine($"✅ ULTRAS‑V3 PREREQUISITE STATUS – {ign}");
        sb.AppendLine();

        // Classes
        sb.AppendLine("📚 CLASSES:");
        foreach (string cls in RequiredClasses)
        {
            bool complete = IsClassComplete(cls);
            sb.AppendLine($"  {(complete ? "✅" : "❌")} {cls}: rank 10" + (complete ? "" : " (needs rank 10)"));
        }
        bool looComplete = IsLordOfOrderComplete();
        sb.AppendLine($"  {(looComplete ? "✅" : "❌")} Lord Of Order: rank 10" + (looComplete ? "" : " (needs rank 10)"));
        sb.AppendLine();

        // Reputations
        sb.AppendLine("📈 REPUTATIONS:");
        int alchRank = ReputationRank("Alchemy");
        bool alchOk = alchRank >= 8;
        sb.AppendLine($"  {(alchOk ? "✅" : "❌")} Alchemy: rank {alchRank}" + (alchOk ? "" : " (need rank 8)"));

        int goodRank = ReputationRank("Good");
        bool goodOk = goodRank >= 10;
        sb.AppendLine($"  {(goodOk ? "✅" : "❌")} Good: rank {goodRank}" + (goodOk ? "" : " (need rank 10)"));
        sb.AppendLine();

        // Forge & Awe
        sb.AppendLine("🔧 FORGE & AWE:");
        bool hasBoA = HasBladeOfAwe();
        sb.AppendLine($"  {(hasBoA ? "✅" : "❌")} Blade of Awe (unlocks Awe enhancements)" + (hasBoA ? "" : " (needs farming)"));

        bool weaponDone = Adv.uLacerate() && Adv.uPraxis();
        sb.AppendLine($"  {(weaponDone ? "✅" : "❌")} Weapon: Lacerate & Praxis" + (weaponDone ? "" : " (not fully unlocked)"));
        bool helmDone = Adv.uForgeHelm();
        sb.AppendLine($"  {(helmDone ? "✅" : "❌")} Helm: all tiers" + (helmDone ? "" : " (not fully unlocked)"));
        bool capeDone = Adv.uForgeCape();
        sb.AppendLine($"  {(capeDone ? "✅" : "❌")} Cape: all tiers" + (capeDone ? "" : " (not fully unlocked)"));
        sb.AppendLine();

        // Level & Gold
        sb.AppendLine("💰 LEVEL & GOLD:");
        sb.AppendLine($"  {(hasLevel100 ? "✅" : "❌")} Level 100: {Bot.Player.Level}" + (hasLevel100 ? "" : " / 100"));
        sb.AppendLine($"  {(hasGold ? "✅" : "❌")} Gold: {gold:N0} >= 10,000,000" + (hasGold ? "" : " (need 10M)"));
        sb.AppendLine();

        // Boost items
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
        sb.AppendLine($"  • AutoEquipBoosts:    {(autoEquip ? "✅" : "❌")}");
        sb.AppendLine($"  • FarmMissingBoosts:  {(farmMissing ? "✅" : "❌")}");
        sb.AppendLine();

        if (!hasGold || !hasLevel100 || !hasWeapon || !hasArmorPet)
        {
            sb.AppendLine("🔧 RECOMMENDED ACTIONS:");
            if (!hasGold) sb.AppendLine("  • Run ArmyPrismata.cs (gold)");
            if (!hasLevel100 && !farmMissing) sb.AppendLine("  • Enable FarmMissingBoosts or farm XP");
            if (!hasWeapon) sb.AppendLine("  • Run HollowbornReapersScythe.cs");
            if (!hasArmorPet) sb.AppendLine("  • Run CelestialPirateCommander[PollyRogers].cs");
        }
        else
            sb.AppendLine("🎉 All good – proceed to Ultras v3!");

        Bot.ShowMessageBox(sb.ToString(), "Ultras-v3 Prerequisites");
    }
}