using System.Collections.Generic;

public class TrophyConversionDefinition
{
    public string TrophyItem { get; }
    public string Biome { get; }
    public string PowderItem { get; }
    public int PowderAmount { get; }

    public TrophyConversionDefinition(
        string trophyItem,
        string biome,
        string powderItem,
        int powderAmount)
    {
        TrophyItem = trophyItem;
        Biome = biome;
        PowderItem = powderItem;
        PowderAmount = powderAmount;
    }
}
