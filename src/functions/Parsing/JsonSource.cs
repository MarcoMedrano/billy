using Newtonsoft.Json;

namespace Billy.Function.Parsing;

public class JsonSourceValue<T>
{
    [JsonProperty("valueString")]
    public string ValueString { get; set; }
    
    [JsonProperty("valueNumber")]
    public decimal ValueNumber { get; set; }
    
    [JsonProperty("valueDate")]
    public string ValueDate { get; set; }
    
    public T Value => typeof(T) == typeof(string) ? 
        (T)(object)ValueString : 
        typeof(T) == typeof(decimal) ? 
        (T)(object)ValueNumber : 
        default(T);
}

public class JsonSourceArray
{
    [JsonProperty("valueArray")]
    public List<JsonSourceArrayItem> ValueArray { get; set; }
}

public class JsonSourceArrayItem
{
    [JsonProperty("valueObject")]
    public JsonSourceItem ValueObject { get; set; }
}