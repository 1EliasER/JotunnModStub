using System;
using System.Collections.Generic;

public class TrophyConversionFile
{
    public List<TrophyConversionDefinition> Conversions { get; }
        = new List<TrophyConversionDefinition>();

    public void LoadFromJson(string json)
    {
        object parsed = SimpleJson.SimpleJson.DeserializeObject(json);

        var root = parsed as IDictionary<string, object>;
        var conversions = root["conversions"] as IList<object>;

        foreach (object item in conversions)
        {


            var entry = item as IDictionary<string, object>;

            var definition = new TrophyConversionDefinition(
                (string)entry["consumedItem"],
                (string)entry["biome"],
                (string)entry["providedItem"],
                Convert.ToInt32(entry["resultAmount"]));

            Conversions.Add(definition);
        }
    }
}
