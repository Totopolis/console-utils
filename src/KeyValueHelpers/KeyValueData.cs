using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KeyValueHelpers
{
    public sealed class KeyValueData
    {
        public string EtcdHost { get; private set; }
        public string EtcdPort { get; private set; }

        private bool ShowProcess { get; set; }

        private Dictionary<string, string> keyValues;
        public IEnumerable<KeyValuePair<string, string>> Tree => keyValues;

        private List<KeyValueLog> logs;

        private List<KeyValueUser> users;
        public IEnumerable<KeyValueUser> Users => users;

        public enum ProcessStatus
        {
            Ok,
            Warnings,
            Errors
        }

        public ProcessStatus Status { get; private set; }
        public KeyValueLogType OutputFilter { get; set; }

        public KeyValueData(bool showProcess = false,
                            KeyValueLogType outputFilter = KeyValueLogType.Info)
        {
            ShowProcess = showProcess;
            keyValues = new Dictionary<string, string>();
            logs = new List<KeyValueLog>();
            users = new List<KeyValueUser>();
            Status = ProcessStatus.Ok;
            OutputFilter = outputFilter;
        }

        public void InitEtcd(string host, string port)
        {
            EtcdHost = host;
            EtcdPort = port;
        }

        public void Info(FileInfo file, string message)
        {
            logs.Add(KeyValueLog.New(KeyValueLogType.Info, file, message));

            if (ShowProcess && OutputFilter != KeyValueLogType.Warning
                && OutputFilter != KeyValueLogType.Error)
            {
                string fn = file != null ? file.FullName : "";
                $"INFO: {fn}".Out();
                message.Out();
            }
        }

        public void Warning(FileInfo file, string message)
        {
            if (Status == ProcessStatus.Ok)
                Status = ProcessStatus.Warnings;

            logs.Add(KeyValueLog.New(KeyValueLogType.Warning, file, message));

            if (ShowProcess && OutputFilter != KeyValueLogType.Error)
            {
                string fn = file != null ? file.FullName : "";
                $"WARNING: {fn}".OutYellow();
                message.OutYellow();
            }
        }

        public void Error(FileInfo file, string message)
        {
            Status = ProcessStatus.Errors;

            logs.Add(KeyValueLog.New(KeyValueLogType.Error, file, message));

            if (ShowProcess)
            {
                string fn = file != null ? file.FullName : "";
                $"ERROR: {fn}".OutRed();
                message.OutRed();
            }
        }

        public void TreeToFlat(string prefix, object obj)
        {
            var data = obj.GetParents();

            foreach (var it in data)
            {
                if (it.Value is string str)
                    keyValues[prefix + "/" + it.Key] = str;

                if (it.Value is JObject jo)
                    TreeToFlat(prefix + "/" + it.Key, jo);
            }
        }

        public void LoadFiles(string ymlFolder, string prefix, object fileSection)
        {
            var files = fileSection.GetParents();

            foreach (var file in files)
            {
                var data = file.Value.GetParents();

                string type = data["type"] as string;
                string fileName = data["file"] as string;

                DirectoryInfo di = new DirectoryInfo(ymlFolder);
                
                var fi = di.GetFiles()
                    .Where(x=>x.Name == fileName)
                    .FirstOrDefault();

                if (fi == null)
                    continue;

                var content = File.ReadAllText(fi.FullName);
                keyValues[prefix + "/" + file.Key] = content;
            }
        }

        public void AddUser(string name, string password, string access)
        {
            var usr = KeyValueUser.New(name, password, access);
            users.Add(usr);
        }

    }//end of class
}
