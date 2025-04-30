using Newtonsoft.Json.Linq;

namespace Billy.Function.Parsing;

public class GenericParser
{
    public static T ParseJson<T>(string jsonContent) where T : new()
    {
        // Parse JSON using JObject
        JObject jsonObject = JObject.Parse(jsonContent);
        
        // Create a new instance of the target type
        T result = new T();
        
        // Get all properties of the target type
        var properties = typeof(T).GetProperties();
        
        // Process each property
        foreach (var property in properties)
        {
            // Check if the property exists in the JSON
            if (jsonObject.TryGetValue(property.Name, out JToken jsonValue))
            {
                // Get the property type
                Type propertyType = property.PropertyType;
                
                try
                {
                    // Set the property value based on its type
                    if (propertyType == typeof(string))
                    {
                        string value = jsonValue["valueString"]?.ToString();
                        property.SetValue(result, value);
                    }
                    else if (propertyType == typeof(short) || propertyType == typeof(ushort) || propertyType == typeof(int) || propertyType == typeof(long))
                    {
                        long value = jsonValue["valueNumber"]?.Value<long>() ?? 0;
                        property.SetValue(result, Convert.ChangeType(value, propertyType));
                    }
                    else if (propertyType == typeof(float) || propertyType == typeof(double) || propertyType == typeof(decimal))
                    {
                        decimal value = jsonValue["valueNumber"]?.Value<decimal>() ?? 0;
                        property.SetValue(result, Convert.ChangeType(value, propertyType));
                    }
                    else if (propertyType == typeof(DateTime))
                    {
                        string dateStr = jsonValue["valueDate"]?.ToString();
                        if (!string.IsNullOrEmpty(dateStr))
                        {
                            DateTime value = DateTime.Parse(dateStr);
                            property.SetValue(result, value);
                        }
                    }
                    else if (propertyType == typeof(bool))
                    {
                        bool value = jsonValue["valueBoolean"]?.Value<bool>() ?? false;
                        property.SetValue(result, value);
                    }
                    // Add other types as needed
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to set property {property.Name}: {ex.Message}");
                }
            }
        }
        
        return result;
    }
}
