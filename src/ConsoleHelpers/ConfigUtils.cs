using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;

namespace ConsoleHelpers
{
    public class ConfigUtils
    {
        public static dynamic LoadConfig(string fileName)
        {
            using (var reader = new StreamReader("sample.yaml"))
            {
                var deserializer = new DeserializerBuilder().Build();
                var yamlObject = deserializer.Deserialize(reader);

                var serializer = new SerializerBuilder()
                    .JsonCompatible()
                    .Build();

                var json = serializer.Serialize(yamlObject);

                dynamic results = JsonConvert.DeserializeObject<dynamic>(json);

                return results;
            }//using
        }
    }//end of class
}
