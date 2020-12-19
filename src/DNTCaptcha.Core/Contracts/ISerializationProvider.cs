namespace DNTCaptcha.Core.Contracts
{
    /// <summary>
    /// Serialization Provider
    /// </summary>
    public interface ISerializationProvider
    {
        /// <summary>
        /// Serialize the given data to an string.
        /// </summary>
        string Serialize(object data);

        /// <summary>
        /// Deserialize the given string to an object.
        /// </summary>
        T? Deserialize<T>(string data);
    }
}