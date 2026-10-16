using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace ForgeOfMostPotential
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class ForgeOfMostPotential : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.ForgeOfMostPotential";
        public const string PluginName = "ForgeOfMostPotential";
        public const string PluginVersion = "0.0.1";
        public static float UpgraderChanceReductionMultiplier = 0.15f; // Reduces the chance of upgrading the item by this value per level
        public static float UpgraderChanceIncreamentAdder = 0.0f; // A flat increase to chance to prevent upgrade chance to close in on zero
        public static float UpgraderChanceStarting = 1f; // The starting chance (if 2, it will start calculation from 200% and reduce it for every level)
        public static int UpgraderChanceStartingLevel = 0; // The starting level of the item before reduction is used.

        private readonly Harmony harmony = new Harmony(PluginGUID);

        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private List<TrophyConversionDefinition> _trophyConversions = new List<TrophyConversionDefinition>();

        private void Awake()
        {
            PrefabManager.OnVanillaPrefabsAvailable += AddAshMaterials;
            AddIdolRecipes();
            _trophyConversions = LoadTrophyConversions();

            foreach (var definition in _trophyConversions)
            {
                Logger.LogInfo(
                    $"Loaded trophy conversion: {definition.ConsumedItem} -> " +
                    $"{definition.ProvidedItem} x{definition.ResultAmount}");
            }

            ItemManager.OnItemsRegistered += RegisterTrophyConversions;
 
            harmony.PatchAll();

            Jotunn.Logger.LogInfo("ModStub loaded with Infinite Stamina!");
        }

        void OnDestroy()
        {
            ItemManager.OnItemsRegistered -= RegisterTrophyConversions;
            harmony?.UnpatchSelf();
        }


        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipe))]
        public static class InstantUpgradePatch
        {
            private static void Prefix(InventoryGui __instance)
            {
                // For testing
                //__instance.m_upgraderDuration = 0.1f;
                //__instance.m_upgraderDurationPerLevel = 0f;
            }
        }

        private void AddAshMaterials()
        {
            CustomItem woodlandAsh = new CustomItem(
                "WoodlandAsh",
                "SpiceOceans");

            woodlandAsh.ItemDrop.m_itemData.m_shared.m_name = "Woodland Ash";
            woodlandAsh.ItemDrop.m_itemData.m_shared.m_description =
                "Ash of crushed bones from creatures of the Meadows.";

            CustomItem bronzeAsh = new CustomItem(
                "BronzeAsh",
                "SpiceMountains");

            bronzeAsh.ItemDrop.m_itemData.m_shared.m_name = "Bronze Ash";
            bronzeAsh.ItemDrop.m_itemData.m_shared.m_description =
                "Ash of crushed bones from creatures of the Black Forest.";

            CustomItem ironAsh = new CustomItem(
                "IronAsh",
                "SpicePlains");

            ironAsh.ItemDrop.m_itemData.m_shared.m_name = "Iron Ash";
            ironAsh.ItemDrop.m_itemData.m_shared.m_description =
                "Ash of crushed bones from creatures of the Swamp.";

            CustomItem silverAsh = new CustomItem(
                "SilverAsh",
                "SpiceMistlands");

            silverAsh.ItemDrop.m_itemData.m_shared.m_name = "Silver Ash";
            silverAsh.ItemDrop.m_itemData.m_shared.m_description =
                "Ash of crushed bones from creatures of the Mountains.";


            CustomItem blackMetalAsh = new CustomItem(
                "BlackMetalAsh",
                "SpiceForests");

            blackMetalAsh.ItemDrop.m_itemData.m_shared.m_name = "Blackmetal Ash";
            blackMetalAsh.ItemDrop.m_itemData.m_shared.m_description =
                "Ash of crushed bones from creatures of the Plains.";

            CustomItem EitrAsh = new CustomItem(
                "EitrAsh",
                "SpicePlains");

            EitrAsh.ItemDrop.m_itemData.m_shared.m_name = "Eitr Ash";
            EitrAsh.ItemDrop.m_itemData.m_shared.m_description =
                "Ash of crushed bones from creatures of the Mistlands.";

            CustomItem FlametalAsh = new CustomItem(
                "FlametalAsh",
                "SpiceAshlands");

            FlametalAsh.ItemDrop.m_itemData.m_shared.m_name = "Flametal Ash";
            FlametalAsh.ItemDrop.m_itemData.m_shared.m_description =
                "Ash of crushed bones from creatures of the Ashlands.";

            CustomItem BloodgoldAsh = new CustomItem(
                "BloodgoldAsh",
                "SpiceDeepNorth");

            BloodgoldAsh.ItemDrop.m_itemData.m_shared.m_name = "Bloodgold Ash";
            BloodgoldAsh.ItemDrop.m_itemData.m_shared.m_description =
                "Ash of crushed bones from creatures of the Deep North.";

            ItemManager.Instance.AddItem(woodlandAsh);
            ItemManager.Instance.AddItem(bronzeAsh);
            ItemManager.Instance.AddItem(ironAsh);
            ItemManager.Instance.AddItem(silverAsh);
            ItemManager.Instance.AddItem(blackMetalAsh);
            ItemManager.Instance.AddItem(EitrAsh);
            ItemManager.Instance.AddItem(FlametalAsh);
            ItemManager.Instance.AddItem(BloodgoldAsh);

            PrefabManager.OnVanillaPrefabsAvailable -= AddAshMaterials;
        }

        private void AddIdolRecipes()
        {
            string path = Path.Combine(
                BepInEx.Paths.BepInExRootPath,
                "scripts",
                "assets",
                "recipes.json");

            if (!File.Exists(path))
                path = Path.Combine(
                BepInEx.Paths.BepInExRootPath,
                "plugins",
                "ForgeOfMostPotential",
                "assets",
                "recipes.json");

            if (!File.Exists(path))
            {
                Logger.LogError("Trophy conversion JSON not found: " + path);
                return;
            }
            string json = File.ReadAllText(path);

            RecipeExtenderFile file = new RecipeExtenderFile();
            file.LoadFromJson(json);

            List<CustomRecipe> recipes = file.GetCustomRecipList();
            foreach (CustomRecipe recipe in recipes)
            {
                ItemManager.Instance.AddRecipe(recipe);
                Logger.LogInfo("Added recipe for " + recipe.Recipe.name);
            }

        }

        private List<TrophyConversionDefinition> LoadTrophyConversions()
        {
            string path = Path.Combine(
                BepInEx.Paths.BepInExRootPath,
                "scripts",
                "assets",
                "trophy_conversions.json");

            if (!File.Exists(path))
                path = Path.Combine(
                BepInEx.Paths.BepInExRootPath,
                "plugins",
                "ForgeOfMostPotential",
                "assets",
                "trophy_conversions.json");

            if (!File.Exists(path))
            {
                Logger.LogError("Trophy conversion JSON not found: " + path);
                return new List<TrophyConversionDefinition>();
            }

            string json = File.ReadAllText(path);

            TrophyConversionFile data = new TrophyConversionFile();
            data.LoadFromJson(json);

            Logger.LogInfo(
                "Loaded " + data.Conversions.Count +
                " trophy conversions from JSON.");

            return data.Conversions;
        }

        private void RegisterTrophyConversions()
        {
            GameObject incineratorPrefab =
                PrefabManager.Instance.GetPrefab("incinerator");

            if (incineratorPrefab == null)
            {
                Logger.LogError("Could not find the Obliterator prefab.");
                return;
            }

            Incinerator incinerator =
                incineratorPrefab.GetComponent<Incinerator>();

            if (incinerator == null)
            {
                Logger.LogError("Obliterator prefab has no Incinerator component.");
                return;
            }

            foreach (TrophyConversionDefinition definition in _trophyConversions)
            {
                if (ConversionAlreadyExists(incinerator, definition))
                {
                    Logger.LogInfo(
                        "Conversion already exists for " + definition.ConsumedItem);
                    continue;
                }

                GameObject trophyPrefab =
                    ObjectDB.instance.GetItemPrefab(definition.ConsumedItem);

                GameObject powderPrefab =
                    ObjectDB.instance.GetItemPrefab(definition.ProvidedItem);

                if (trophyPrefab == null || powderPrefab == null)
                {
                    Logger.LogError(
                        "Missing item prefab for " + definition.ConsumedItem +
                        " or " + definition.ProvidedItem);
                    continue;
                }

                ItemDrop trophyItem = trophyPrefab.GetComponent<ItemDrop>();
                ItemDrop powderItem = powderPrefab.GetComponent<ItemDrop>();

                if (trophyItem == null || powderItem == null)
                {
                    Logger.LogError("An item prefab has no ItemDrop component.");
                    continue;
                }

                Incinerator.IncineratorConversion conversion =
                    new Incinerator.IncineratorConversion
                    {
                        m_requirements = new List<Incinerator.Requirement>
                        {
                        new Incinerator.Requirement
                        {
                            m_resItem = trophyItem,
                            m_amount = 1
                        }
                        },
                        m_result = powderItem,
                        m_resultAmount = definition.ResultAmount,
                        m_priority = 100,
                        m_requireOnlyOneIngredient = true
                    };

                incinerator.m_conversions.Add(conversion);

                Logger.LogInfo(
                    "Registered " + definition.ConsumedItem +
                    " -> " + definition.ProvidedItem +
                    " x" + definition.ResultAmount);
            }
        }


        private bool ConversionAlreadyExists(
        Incinerator incinerator,
        TrophyConversionDefinition definition)
        {
            foreach (Incinerator.IncineratorConversion conversion
                in incinerator.m_conversions)
            {
                if (conversion.m_requirements == null ||
                    conversion.m_requirements.Count != 1 ||
                    conversion.m_requirements[0].m_resItem == null ||
                    conversion.m_result == null)
                {
                    continue;
                }

                bool sameTrophy =
                    conversion.m_requirements[0].m_resItem.name ==
                    definition.ConsumedItem;

                bool sameOutput =
                    conversion.m_result.name == definition.ProvidedItem;

                if (sameTrophy && sameOutput)
                {
                    return true;
                }
            }

            return false;
        }


        private static float CalculateUpgraderSuccessChance(int currentQuality)
        {
            return Mathf.Clamp(UpgraderChanceStarting * Mathf.Pow((1 - UpgraderChanceReductionMultiplier), currentQuality - UpgraderChanceStartingLevel) + UpgraderChanceIncreamentAdder, 0,1);
        }

        [HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
        public static class DoCrafting_Patch
        {
            static bool Prefix(InventoryGui __instance)
            {
                Player player = Player.m_localPlayer;
                if (player == null) return true;

                if (__instance.m_craftUpgradeItem == null)
                {
                    return true;
                }

                Recipe recipe = __instance.m_craftRecipe;
                if (recipe == null) return false;

                int currentQuality = __instance.m_craftUpgradeItem.m_quality;
                int targetQuality = currentQuality + 1;

                CraftingStation station = player.GetCurrentCraftingStation();
                bool isUpgrader = station && station.m_upgrader;

                // Check max quality limits if not at an upgrader
                if (targetQuality > recipe.m_item.m_itemData.m_shared.m_maxQuality && !isUpgrader)
                {
                    return false;
                }

                bool cheated = player.NoCostCheat() || ZoneSystem.instance.GetGlobalKey(GlobalKeys.NoCraftCost);

                // Check requirements
                if (!player.HaveRequirements(recipe, false, targetQuality, 1) && !cheated)
                {
                    player.Message(MessageHud.MessageType.Center, "$msg_missingrequirement", 0, null, false);
                    return false;
                }

                // 1. Capture idol details before it gets consumed
                ItemDrop.ItemData idolItemData = null;
                string idolPrefabName = "";
                Vector2i idolGridPos = new Vector2i(-1, -1);
                int idolVariant = 0;

                foreach (Piece.Requirement req in recipe.m_resources)
                {
                    if (req.m_upgraderResource && req.m_resItem != null)
                    {
                        idolPrefabName = req.m_resItem.gameObject.name;
                        idolItemData = player.GetInventory().GetItem(req.m_resItem.m_itemData.m_shared.m_name, -1, false);
                        break;
                    }
                }

                if (idolItemData != null)
                {
                    idolGridPos = idolItemData.m_gridPos;
                    idolVariant = idolItemData.m_variant;
                }

                // Save weapon details before modifying inventory
                Vector2i weaponGridPos = __instance.m_craftUpgradeItem.m_gridPos;
                int weaponVariant = __instance.m_craftUpgradeItem.m_variant;
                long playerID = player.GetPlayerID();
                string playerName = player.GetPlayerName();
                string weaponPrefabName = recipe.m_item.gameObject.name;

                // 2. Roll success chance: 0.85 ^ currentQuality
                bool success = true;
                if (isUpgrader)
                {
                    float successChance = CalculateUpgraderSuccessChance(currentQuality);
                    //float successChance = (float)(Math.Pow(1.0 - UpgraderChanceReductionMultiplier, currentQuality) + UpgraderChanceIncreamentAdder);
                    success = UnityEngine.Random.value <= successChance;
                }

                int finalWeaponQuality = success ? targetQuality : currentQuality;

                // 3. Update weapon in inventory (upgrade on success, retain on failure)
                player.UnequipItem(__instance.m_craftUpgradeItem, true);
                player.GetInventory().RemoveItem(__instance.m_craftUpgradeItem);
                player.GetInventory().AddItem(weaponPrefabName, 1, finalWeaponQuality, weaponVariant, playerID, playerName, weaponGridPos, cheated, false, true);

                // 4. Consume resources normally if not cheating
                if (!cheated)
                {
                    // Valheim natively consumes all recipe resources (materials + original idol)
                    player.ConsumeResources(recipe.m_resources, targetQuality, -1, 1);

                    // If upgrade FAILED, find X in the idol name, decrease it by 1, and spawn that downgraded prefab
                    if (!success && !string.IsNullOrEmpty(idolPrefabName))
                    {
                        string downgradedPrefabName = idolPrefabName;
                        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(idolPrefabName, @"\d+");
                        if (match.Success)
                        {
                            int currentLevel = int.Parse(match.Value);
                            int newLevel = Math.Max(1, currentLevel - 1);
                            downgradedPrefabName = System.Text.RegularExpressions.Regex.Replace(idolPrefabName, @"\d+", newLevel.ToString());

                            if (currentLevel > 0) 
                            {
                                player.GetInventory().AddItem(downgradedPrefabName, 1, 1, idolVariant, playerID, playerName, idolGridPos, cheated, false, true);
                                ZLog.Log($"Upgrade failed on {weaponPrefabName}. Idol downgraded from {idolPrefabName} to {downgradedPrefabName} and returned.");
                            }
                        }
;
                    }
                    else if (success)
                    {
                        ZLog.Log($"Upgrade succeeded on {weaponPrefabName}. Idol consumed normally.");
                    }
                }

                // 5. Update UI and trigger station effects/messages
                __instance.UpdateCraftingPanel(false);

                if (recipe.m_craftingStation != null && recipe.m_craftingStation.m_craftingSkill != Skills.SkillType.None)
                {
                    player.RaiseSkill(recipe.m_craftingStation.m_craftingSkill, 1f);
                }

                if (station)
                {
                    if (success)
                    {
                        station.m_craftItemDoneEffects.Create(player.transform.position, Quaternion.identity, null, 1f, -1, default(ZDOID));
                        player.Message(MessageHud.MessageType.Center, $"<color=green>Upgrade Successful!</color>", 0, null, true);
                    }
                    else
                    {
                        station.m_craftItemDoneFailEffects.Create(player.transform.position, Quaternion.identity, null, 1f, -1, default(ZDOID));
                        player.Message(MessageHud.MessageType.Center, $"<color=orange>Upgrade Failed! Idol downgraded.</color>", 0, null, true);
                    }
                }

                Game.instance.GetPlayerProfile().IncrementStat(PlayerStatType.CraftsOrUpgrades, 1f, cheated);
                Game.instance.GetPlayerProfile().IncrementStat(PlayerStatType.Upgrades, 1f, cheated);

                return false;
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "UpdateRecipe")]
        public static class UpdateRecipe_Patch
        {
            static void Postfix(InventoryGui __instance)
            {
                Player player = Player.m_localPlayer;
                if (player == null) return;

                Recipe recipe = __instance.m_selectedRecipe.Recipe;
                ItemDrop.ItemData upgradeItem = __instance.m_selectedRecipe.ItemData;

                if (recipe != null && upgradeItem != null)
                {
                    CraftingStation currentStation = player.GetCurrentCraftingStation();
                    bool isUpgrader = currentStation && currentStation.m_upgrader;

                    if (isUpgrader)
                    {
                        int itemLevel = upgradeItem.m_quality;

                        float successChancePercent = CalculateUpgraderSuccessChance(itemLevel) * 100f;
                        float breakChancePercent = 100f - successChancePercent;

                        string chanceInfo = $"\n\n<color=white>--- Refinement Odds ---</color>\n" +
                                            $"<color=green>Success Chance: {successChancePercent:F1}%</color>\n" +
                                            $"<color=yellow>Retain Chance: {breakChancePercent:F1}%</color>";

                        if (__instance.m_recipeDecription != null)
                        {
                            // Only add the text if it isn't already appended
                            if (!__instance.m_recipeDecription.text.Contains("--- Refinement Odds ---"))
                            {
                                __instance.m_recipeDecription.text += chanceInfo;
                            }
                        }
                    }
                }
            }
        }
    }
}

