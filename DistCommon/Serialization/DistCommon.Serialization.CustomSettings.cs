namespace DistCommon.Serialization
{
    using System;
    using Newtonsoft.Json;

    public sealed class CustomSettings : JsonSerializerSettings
    {
        public CustomSettings() : base()
        {
            this.Converters.Add(new IPAddressConverter());
            this.Converters.Add(new IPEndPointConverter());
            this.Converters.Add(new TypeConverter());
            this.TypeNameHandling = TypeNameHandling.Auto;
        }
    }
}
