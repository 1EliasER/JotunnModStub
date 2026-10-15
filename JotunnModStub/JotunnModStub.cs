using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine; // Add this line

namespace JotunnModStub
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class JotunnModStub : BaseUnityPlugin
    {
        public const string PluginGUID = "com.jotunn.jotunnmodstub";
        public const string PluginName = "JotunnModStub";
        public const string PluginVersion = "0.0.1";

        private readonly Harmony harmony = new Harmony(PluginGUID);

        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private readonly List<TrophyConversionDefinition> _trophyConversions =
new List<TrophyConversionDefinition>
{
        new TrophyConversionDefinition(
            "TrophyNeck", "Meadows", "Stone", 1),

        new TrophyConversionDefinition(
            "TrophyBoar", "Meadows", "Stone", 1),

        new TrophyConversionDefinition(
            "TrophyDeer", "Meadows", "Stone", 2)


};

        private void Awake()
        {
            ItemManager.OnItemsRegistered += RegisterTrophyConversions;
            Jotunn.Logger.LogInfo("ModStub loaded with Infinite Stamina!");
            //CreateItemWithMultipleRecipes();//CreateClonedItem();
            //AddWoodToFineWoodConversion();
            //AddTrophyConversions();
            // Activate Harmony patches
            harmony.PatchAll();
            //AddRecipes();
        }

        void OnDestroy()
        {
            ItemManager.OnItemsRegistered -= RegisterTrophyConversions;
            harmony?.UnpatchSelf();
        }

        /**
        [Serializable]
        public class ExtendedRecipe
        {
            public string item;           // Prefab name of the item
            public string name;           // Localization token key for item name
            public string nameValue;      // Actual display name
            public string description;    // Item description
            public string descriptionToken; // Localization token key for description

            public int amount = 1;        // How many are crafted
            public string craftingStation = "piece_workbench";
            public int minStationLevel = 1;

            // Requirements (Ingredients)
            public RecipeRequirement[] requirements;
        }

        [Serializable]
        public class RecipeRequirement
        {
            public string item;           // e.g., "Wood", "Resin"
            public int amount;            // e.g., 1
        }
        **/

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
                        "Conversion already exists for " + definition.TrophyItem);
                    continue;
                }

                GameObject trophyPrefab =
                    ObjectDB.instance.GetItemPrefab(definition.TrophyItem);

                GameObject powderPrefab =
                    ObjectDB.instance.GetItemPrefab(definition.PowderItem);

                if (trophyPrefab == null || powderPrefab == null)
                {
                    Logger.LogError(
                        "Missing item prefab for " + definition.TrophyItem +
                        " or " + definition.PowderItem);
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
                        m_resultAmount = definition.PowderAmount,
                        m_priority = 100,
                        m_requireOnlyOneIngredient = true
                    };

                incinerator.m_conversions.Add(conversion);

                Logger.LogInfo(
                    "Registered " + definition.TrophyItem +
                    " -> " + definition.PowderItem +
                    " x" + definition.PowderAmount);
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
                    definition.TrophyItem;

                bool sameOutput =
                    conversion.m_result.name == definition.PowderItem;

                if (sameTrophy && sameOutput)
                {
                    return true;
                }
            }

            return false;
        }


        private void AddTrophyConversions()
        {
            foreach (var definition in _trophyConversions)
            {
                var config = new IncineratorConversionConfig
                {
                    Station = Incinerators.Incinerator,
                    FromItem = definition.TrophyItem,
                    ToItem = definition.PowderItem,
                    ProducedItems = definition.PowderAmount,
                    RequireOnlyOneIngredient = true,
                    Priority = 100
                };

                config.Requirements.Add(
                    new IncineratorRequirementConfig(
                        definition.TrophyItem, 1));

                var conversion = new CustomItemConversion(config);

                if (!conversion.IsValid())
                {
                    Logger.LogWarning(
                        $"Invalid conversion for {definition.TrophyItem}");
                    continue;
                }

                bool added =
                    ItemManager.Instance.AddItemConversion(conversion);

                Logger.LogInfo(
                    $"{definition.TrophyItem} -> " +
                    $"{definition.PowderItem} x{definition.PowderAmount}; " +
                    $"Added={added}");
            }
        }

        private void AddBoarTrophyConversion2()
        {
            var config = new IncineratorConversionConfig
            {
                Station = Incinerators.Incinerator,
                ToItem = "FineWood",
                ProducedItems = 1,
                RequireOnlyOneIngredient = true,
                Priority = 100
            };

            config.Requirements.Add(
                new IncineratorRequirementConfig("TrophyBoar", 1));

            var conversion = new CustomItemConversion(config);

            Logger.LogInfo(
                $"Conversion valid: {conversion.IsValid()}");

            Logger.LogInfo(
                $"Output: {config.ToItem}, Priority: {config.Priority}");

            Logger.LogInfo(
                $"Requirements: {string.Join(", ", config.Requirements.Select(r => $"{r.Item} x{r.Amount}"))}");

            bool added = ItemManager.Instance.AddItemConversion(conversion);

            Logger.LogInfo($"Conversion added: {added}");
        }


        private void CreateClonedItem()
        {
            // 1. Configure your new item's details
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = "Super Wood";                          // Display name token or text
            itemConfig.Description = "Extremely dense and powerful wood.";
            itemConfig.Amount = 1;

            // Optional: add crafting requirements directly in the config
            itemConfig.AddRequirement("Wood", 5);
            itemConfig.AddRequirement("Resin", 2);

            // 2. Create the CustomItem by passing: (New Item ID, Source Vanilla Prefab to Clone, Config)
            CustomItem customItem = new CustomItem("SuperWood", "FineWood", itemConfig);

            // 3. Register it with Jötunn's ItemManager
            ItemManager.Instance.AddItem(customItem);
        }

        private void CreateItemWithMultipleRecipes()
        {
            // 1. Configure and register the custom item itself
            ItemConfig itemConfig = new ItemConfig();
            itemConfig.Name = "Super Wood";
            itemConfig.Description = "Extremely dense and powerful wood.";
            itemConfig.Amount = 1;
            itemConfig.StackSize = 11;

            CustomItem customItem = new CustomItem("SuperWood", "FineWood", itemConfig);
            ItemManager.Instance.AddItem(customItem);

            // 2. Define Recipe Option A: 5 Wood + 2 Resin
            RecipeConfig option1 = new RecipeConfig()
            {
                Name = "Recipe_SuperWood_Opt1",
                Item = "SuperWood",
                Amount = 1,
                CraftingStation = "piece_workbench",
                MinStationLevel = 1,
                Requirements = new RequirementConfig[]
                {
            new RequirementConfig { Item = "Wood", Amount = 5 },
            new RequirementConfig { Item = "Resin", Amount = 2 }
                }
            };
            ItemManager.Instance.AddRecipe(new CustomRecipe(option1)); // Uses ItemManager!

            // 3. Define Recipe Option B: 2 Fine Wood (Alternative option)
            RecipeConfig option2 = new RecipeConfig()
            {
                Name = "Recipe_SuperWood_Opt2",
                Item = "SuperWood",
                Amount = 1,
                CraftingStation = "piece_workbench",
                MinStationLevel = 1,
                Requirements = new RequirementConfig[]
                {
            new RequirementConfig { Item = "FineWood", Amount = 2 }
                }
            };
            ItemManager.Instance.AddRecipe(new CustomRecipe(option2)); // Uses ItemManager!
        }

        /**
        private void AddRecipes()
        {
            var extendedRecipes = ExtendedRecipeManager.LoadRecipesFromJson("ExampleRecipes/Assets/recipes.json");

            extendedRecipes.ForEach(extendedRecipe =>
            {
                // Load prefab from asset bundle
                var prefab = _embeddedResourceBundle.LoadAsset<GameObject>($"{Path.Combine(UnityBasePath, extendedRecipe.item)}.prefab");

                // Create custom item
                var customItem = new CustomItem(prefab, fixReference: true);

                // Edit item drop to set name and description
                var itemDrop = customItem.ItemDrop;
                itemDrop.m_itemData.m_shared.m_name = extendedRecipe.name;
                itemDrop.m_itemData.m_shared.m_description = extendedRecipe.description;

                // Add localizations for name and description
                LocalizationManager.Instance.AddToken(extendedRecipe.name, extendedRecipe.nameValue, forceReplace: false);
                LocalizationManager.Instance.AddToken(extendedRecipe.descriptionToken, extendedRecipe.description, forceReplace: false);

                // Add item with recipe
                customItem.Recipe = ExtendedRecipe.Convert(extendedRecipe);
                ItemManager.Instance.AddItem(customItem);
            });
        }**/


        // Patch player stamina consumption
        [HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
        public static class Player_UseStamina_Patch
        {
            public static bool Prefix()
            {
                // Returning false stops stamina from decreasing
                return false;
            }
        }

        // 1. Inject recipe into ObjectDB when the game database loads
        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        public static class ObjectDB_Awake_Recipe_Patch
        {
            public static Recipe FineWoodRecipe = null;

            static void Postfix(ObjectDB __instance)
            {
                return;

                if (__instance == null || __instance.m_items.Count == 0) return;

                // Avoid duplicate creation
                if (FineWoodRecipe != null && __instance.m_recipes.Contains(FineWoodRecipe)) return;

                // Fetch Item prefabs
                GameObject fineWoodPrefab = __instance.GetItemPrefab("FineWood");
                GameObject woodPrefab = __instance.GetItemPrefab("Wood");
                GameObject resinPrefab = __instance.GetItemPrefab("Resin");

                if (!fineWoodPrefab || !woodPrefab || !resinPrefab) return;

                // Get Workbench station
                CraftingStation workbench = null;
                GameObject workbenchPrefab = ZNetScene.instance?.GetPrefab("piece_workbench");
                if (workbenchPrefab)
                {
                    workbench = workbenchPrefab.GetComponent<CraftingStation>();
                }

                // Define Requirements (1 Wood, 1 Resin)
                Piece.Requirement reqWood = new Piece.Requirement
                {
                    m_resItem = woodPrefab.GetComponent<ItemDrop>(),
                    m_amount = 1,
                    m_amountPerLevel = 0,
                    m_recover = true
                };

                Piece.Requirement reqResin = new Piece.Requirement
                {
                    m_resItem = resinPrefab.GetComponent<ItemDrop>(),
                    m_amount = 1,
                    m_amountPerLevel = 0,
                    m_recover = true
                };

                // Build Recipe instance
                FineWoodRecipe = ScriptableObject.CreateInstance<Recipe>();
                FineWoodRecipe.name = "Recipe_FineWoodFromWoodAndResin";
                FineWoodRecipe.m_item = fineWoodPrefab.GetComponent<ItemDrop>();
                FineWoodRecipe.m_amount = 1;
                FineWoodRecipe.m_craftingStation = workbench;
                FineWoodRecipe.m_minStationLevel = 1;
                FineWoodRecipe.m_resources = new Piece.Requirement[] { reqWood, reqResin };
                FineWoodRecipe.m_enabled = true;

                // Add to database
                __instance.m_recipes.Add(FineWoodRecipe);

                ZLog.Log("[Mod] FineWood custom recipe injected into ObjectDB.");
            }
        }




        // 2. Ensure player automatically unlocks the recipe upon spawning in world
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static class Player_OnSpawned_Recipe_Patch
        {
            static void Postfix(Player __instance)
            {
                return;

                if (__instance != null && ObjectDB_Awake_Recipe_Patch.FineWoodRecipe != null)
                {
                    string recipeItemName = ObjectDB_Awake_Recipe_Patch.FineWoodRecipe.m_item.m_itemData.m_shared.m_name;

                    // Check Valheim's internal HashSet directly
                    if (!__instance.m_knownRecipes.Contains(recipeItemName))
                    {
                        __instance.AddKnownRecipe(ObjectDB_Awake_Recipe_Patch.FineWoodRecipe);
                    }
                }
            }
        }

        // --- GAME LOGIC PATCH: Custom Upgrade Handler (Normal consumption, item retention, regex prefab name downgrade for idol on failure) ---
        [HarmonyPatch(typeof(InventoryGui), "DoCrafting")]
        public static class DoCrafting_Patch
        {
            static bool Prefix(InventoryGui __instance, Player player)
            {
                return true;

                // If it's a normal craft (not an upgrade), let the vanilla game handle it normally
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
                    float successChance = (float)Math.Pow(0.85, currentQuality);
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

        // --- 2. UI DISPLAY PATCH (Updates instantly when item is selected) ---
        [HarmonyPatch(typeof(InventoryGui), "UpdateRecipe")]
        public static class UpdateRecipe_Patch
        {
            static void Postfix(InventoryGui __instance, Player player)
            {
                return;

                // RecipeDataPair properties are 'Recipe' and 'ItemData'
                Recipe recipe = __instance.m_selectedRecipe.Recipe;
                ItemDrop.ItemData upgradeItem = __instance.m_selectedRecipe.ItemData;

                if (recipe != null && upgradeItem != null)
                {
                    CraftingStation currentStation = player.GetCurrentCraftingStation();
                    bool isUpgrader = currentStation && currentStation.m_upgrader;

                    if (isUpgrader)
                    {
                        int itemLevel = upgradeItem.m_quality;

                        // Calculate percentage values (0.85 ^ itemLevel)
                        float successChancePercent = (float)Math.Pow(0.85, itemLevel) * 100f;
                        float breakChancePercent = 100f - successChancePercent;

                        string chanceInfo = $"\n\n<color=white>--- Refinement Odds ---</color>\n" +
                                           $"<color=green>Success Chance: {successChancePercent:F1}%</color>\n" +
                                           $"<color=yellow>Retain Chance: {breakChancePercent:F1}%</color>";

                        if (__instance.m_recipeDecription != null)
                        {
                            __instance.m_recipeDecription.text += chanceInfo;
                        }
                    }
                }
            }
        }
    }
}