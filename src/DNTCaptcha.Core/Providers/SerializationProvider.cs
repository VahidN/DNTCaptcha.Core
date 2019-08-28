using DNTCaptcha.Core.Contracts;
#if NETCOREAPP3_0
using System.Text.Json;
#else
using Newtonsoft.Json;
#endif

namespace DNTCaptcha.Core.Providers
{
    /// <summary>
    /// Serialization Provider
    /// </summary>
    public class SerializationProvider : ISerializationProvider
    {
        /// <summary>
        /// Serialize the given data to an string.
        /// </summary>
        public string Serialize(object data)
        {
#if NETCOREAPP3_0
            return JsonSerializer.Serialize(data,
                                             new JsonSerializerOptions
                                             {
                                                 WriteIndented = false,
                                                 IgnoreNullValues = true
                                             });
#else
            return JsonConvert.SerializeObject(data,
                                                new JsonSerializerSettings
                                                {
                                                    Formatting = Formatting.None,
                                                    NullValueHandling = NullValueHandling.Ignore,
                                                    DefaultValueHandling = DefaultValueHandling.Ignore
                                                });
#endif
        }

        /// <summary>
        /// Deserialize the given string to an object.
        /// </summary>
        public T Deserialize<T>(string data)
        {
#if NETCOREAPP3_0
            return JsonSerializer.Deserialize<T>(data);
#else
            return JsonConvert.DeserializeObject<T>(data);
#endif
        }
    }
}