using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JsonTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string json = "{ \"name\": \"Alice\", \"age\": 25, \"isStudent\": false, \"scores\": [90, 85, 88], \"city\": null }";

            // Deserialize JSON to Dictionary<string, object>
            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            Dictionary<string, object> result = new Dictionary<string, object>();
            // Convert array elements properly
            foreach (var key in dict.Keys)
            {
                result[key] = ConvertJsonElement(dict[key]);
            }

            // Print results
            foreach (var kvp in result)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value} (Type: {kvp.Value?.GetType()})");

                // If value is a list, print its elements
                if (kvp.Value is List<object> list)
                {
                    Console.WriteLine($"  Array Elements: {string.Join(", ", list)}");
                }
            }

            Console.ReadKey();
        }

        static object ConvertJsonElement(object value)
        {
            switch (value)
            {
                case Newtonsoft.Json.Linq.JArray array:
                    return array.ToObject<List<object>>(); // Convert to List<object>
                case Newtonsoft.Json.Linq.JValue val:
                    return val.Value; // Extract actual value
                default:
                    return value; // Return as-is
            }
        }
    }
}