namespace DistCommon.Utils
{
    using System;
    using System.IO;

    public static class JFI
    {
        public static T GetObject<T>(string filename)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
            }
            catch (IOException)
            {
                return default(T);
            }
        }
    }
}
