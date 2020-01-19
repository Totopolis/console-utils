using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace ConsoleHelpers
{
	public class ConfigUtils
    {
		public static dynamic LoadConfig(string fileName, bool replaceMinus = true)
		{
			using (var reader = new StreamReader(fileName))
			{
				var deserializer = new DeserializerBuilder().Build();
				var yamlObject = deserializer.Deserialize(reader);

				var serializer = new SerializerBuilder()
					.JsonCompatible()
					.Build();

				var json = serializer.Serialize(yamlObject);

				if (replaceMinus)
					using (StringReader sr = new StringReader(json))
					using (JsonTextReader jr = new JsonTextReader(sr))
					{
						JObject jo = JObject.Load(jr);
						ChangeNumericalPropertyNames(jo);
						json = jo.ToString();
					}//using

				dynamic results = JsonConvert.DeserializeObject<dynamic>(json);

				return results;
			}//using
		}

		private static void ChangeNumericalPropertyNames(JObject jo)
		{
			foreach (JProperty jp in jo.Properties().ToList())
			{
				if (jp.Value.Type == JTokenType.Object)
					ChangeNumericalPropertyNames((JObject)jp.Value);
				else if (jp.Value.Type == JTokenType.Array)
				{
					foreach (JToken child in jp.Value)
						if (child.Type == JTokenType.Object)
							ChangeNumericalPropertyNames((JObject)child);
				}

				if (jp.Name.Contains("-"))
				{
					string name = jp.Name.Replace("-", "_");
					jp.Replace(new JProperty(name, jp.Value));
				}
			}//foreach
		}
	}//end of class
}
