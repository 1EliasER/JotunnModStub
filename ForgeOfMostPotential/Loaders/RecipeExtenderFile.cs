using System;
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;

public class RecipeExtenderFile
{
    public List<RecipeDefinition> Recipes { get; set; }
        = new List<RecipeDefinition>();

    public void LoadFromJson(string json)
    {
        object parsed = SimpleJson.SimpleJson.DeserializeObject(json);

        var root = parsed as IDictionary<string, object>;
        var recipes = root["recipes"] as IList<object>;

        foreach (object item in recipes)
        {
            var entry = item as IDictionary<string, object>;

            var definition = new RecipeDefinition
            {
                Item = (string)entry["item"],
                Amount = Convert.ToInt32(entry["amount"]),
                CraftingStation = (string)entry["craftingStation"]
            };

            var requirements =
                entry["requirements"] as IList<object>;

            foreach (object requirementItem in requirements)
            {
                var requirement =
                    requirementItem as IDictionary<string, object>;

                definition.Requirements.Add(
                    new RecipeRequirementDefinition
                    {
                        Item = (string)requirement["item"],
                        Amount = Convert.ToInt32(requirement["amount"]),
                        AmountPerLevel = Convert.ToInt32(
                            requirement["amountPerLevel"])
                    });
            }

            Recipes.Add(definition);
        }
    }

    public List<CustomRecipe> GetCustomRecipList()
    {
        var list = new List<CustomRecipe>();

        foreach (var recipe in Recipes)
        {
            var requirementList = new List<RequirementConfig>();
            foreach (var requirement in recipe.Requirements)
            {
                requirementList.Add(new RequirementConfig(requirement.Item, requirement.Amount));
            }

            RecipeConfig recipeConfig = new RecipeConfig
            {
                Item = recipe.Item,
                Amount = recipe.Amount,
                CraftingStation = recipe.CraftingStation,
                Requirements = requirementList.ToArray()
            };

            list.Add(new CustomRecipe(recipeConfig));
            
        }

        return list;
    }
}
