
using System.Collections.Generic;

public class RecipeDefinition
{
    public string Item { get; set; }
    public int Amount { get; set; }
    public string CraftingStation { get; set; }

    public List<RecipeRequirementDefinition> Requirements { get; set; }
        = new List<RecipeRequirementDefinition>();
}

public class RecipeRequirementDefinition
{
    public string Item { get; set; }
    public int Amount { get; set; }
    public int AmountPerLevel { get; set; }
}
