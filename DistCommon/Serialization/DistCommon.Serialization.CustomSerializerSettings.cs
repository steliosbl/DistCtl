namespace DistCommon.Serialization
{
    using System;
    using Newtonsoft.Json;

    public static class CustomSerializerSettings
    {
        public static void AddCustomSettings(this Newtonsoft.Json.JsonSerializerSettings settings)
        {
            settings.Converters.Add(new TypeConverter());
            settings.Converters.Add(new IPAddressConverter());
            settings.Converters.Add(new IPEndPointConverter());
            settings.TypeNameHandling = TypeNameHandling.Auto;
        }

        public sealed class SerializerSettings : JsonSerializerSettings
        {
            public SerializerSettings() : base()
            {
                this.Converters.Add(new TypeConverter());
                this.Converters.Add(new IPAddressConverter());
                this.Converters.Add(new IPEndPointConverter());
                this.TypeNameHandling = TypeNameHandling.Auto;
            }
        }
    }
}
