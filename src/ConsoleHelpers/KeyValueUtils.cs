using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleHelpers
{
    public class KeyValueUtils
    {
        public static KeyValueData Load(string path, bool recursive = false)
        {
            var files = Directory.EnumerateFiles(path, "*.*",
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(x => x.EndsWith(".yml") || x.EndsWith(".yaml"))
                .Select(x => new { file = x, firstLine = File.ReadLines(x).FirstOrDefault() })
                .Where(x => x.firstLine != null && x.firstLine.StartsWith("kv-configurator: 1.0"))
                .Select(x => x.file);

            KeyValueData result = new KeyValueData();

            foreach (var file in files)
            {
                var config = ConfigUtils.LoadConfig(file);
                var fi = new FileInfo(file);

                if (config.application is JObject jo)
                {
                    var apps = jo.ToObject<IDictionary<string, object>>()
                                 .ToDictionary(k => k.Key, v => v.Value);

                    foreach (var app in apps)
                    {
                        var envs = (app.Value as JObject)
                            .ToObject<IDictionary<string, object>>()
                            .ToDictionary(k => k.Key, v => v.Value);

                        foreach (var env in envs)
                        {
                            var data = (env.Value as JObject)
                                .ToObject<IDictionary<string, object>>()
                                .ToDictionary(k => k.Key, v => v.Value);

                            var prefix = $"/cfg/{app.Key}/{env.Key}";

                            if (data.ContainsKey("tree"))
                                result.TreeToFlat(prefix, data["tree"] as JObject);

                            if (data.ContainsKey("files"))
                                result.LoadFiles(fi.DirectoryName, prefix, data["files"] as JObject);
                        }
                    }
                }
            }

            return result;
        }

        // TODO: errors.Out() && import.Do(endpoint: etcd)

    }//end of class

    public class KeyValueData
    {
        private Dictionary<string, string> keyValues = new Dictionary<string, string>();
        
        // list<kv_user>
        // list<kv_errors>

        public void TreeToFlat(string prefix, JObject obj)
        {
            var data = obj.ToObject<IDictionary<string, object>>()
                          .ToDictionary(k => k.Key, v => v.Value);

            foreach (var it in data)
            {
                if (it.Value is string str)
                    keyValues[prefix + "/" + it.Key] = str;

                if (it.Value is JObject jo)
                    TreeToFlat(prefix + "/" + it.Key, jo);
            }
        }

        public void LoadFiles(string ymlFolder, string prefix, JObject fileSection)
        {
            var files = fileSection
                .ToObject<IDictionary<string, object>>()
                .ToDictionary(k => k.Key, v => v.Value);

            foreach (var file in files)
            {
                var data = (file.Value as JObject)
                    .ToObject<IDictionary<string, object>>()
                    .ToDictionary(k => k.Key, v => v.Value);

                string type = data["type"] as string;
                string fileName = data["file"] as string;

                var content = File.ReadAllText($"{ymlFolder}\\{fileName}");
                keyValues[prefix + "/" + file.Key] = content;
            }
        }

    }//end of class
}
