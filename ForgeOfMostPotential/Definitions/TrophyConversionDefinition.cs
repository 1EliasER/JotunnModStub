using System.Collections.Generic;

public class TrophyConversionDefinition
{

    public string ConsumedItem { get; }
    public string Biome { get; }
    public string ProvidedItem { get; }
    public int ResultAmount { get; }

    public TrophyConversionDefinition(
        string consumedItem,
        string biome,
        string providedItem,
        int resultAmount)
    {
        ConsumedItem = consumedItem;
        Biome = biome;
        ProvidedItem = providedItem;
        ResultAmount = resultAmount;
    }
}
