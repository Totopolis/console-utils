using dotnet_etcd;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleHelpers
{
    public class KeyValueUtils
    {
        public static KeyValueData Load(string path, bool recursive = false,
            bool showProcess = false,
            KeyValueLogType outputFilter = KeyValueLogType.Info)
        {
            var files = Directory.EnumerateFiles(path, "*.*",
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(x => x.EndsWith(".yml") || x.EndsWith(".yaml"))
                .Select(x => new { file = x, firstLine = File.ReadLines(x).FirstOrDefault() })
                .Where(x => x.firstLine != null && x.firstLine.StartsWith("kv-configurator: 1.0"))
                .Select(x => x.file);

            KeyValueData result = new KeyValueData(showProcess);

            foreach (var file in files)
            {
                var config = ConfigUtils.LoadConfig(file);
                var fi = new FileInfo(file);

                if (config.etcd is JObject etcdJo)
                {
                    var data = etcdJo.GetParents();

                    var host = data["host"] as string;
                    var port = data["port"] as string;

                    result.InitEtcd(host, port);
                }
                
                if (config.enviroment is JObject envJo)
                {
                    var envs = envJo.GetParents();

                    foreach (var env in envs)
                    {
                        var envData = env.Value.GetParents();

                        if (envData.ContainsKey("user"))
                        {
                            var usrData = envData["user"].GetParents();
                            var name = usrData["name"] as string;
                            var password = usrData["pass"] as string;

                            result.AddUser(name, password, $"{env.Key}");
                        }
                    }
                }

                if (config.application is JObject appJo)
                {
                    var apps = appJo.GetParents();

                    foreach (var app in apps)
                    {
                        if (app.Value == null)
                        {
                            result.Warning(fi, $"Application {app.Key} does not contain environments");
                            break;
                        }

                        var envs = app.Value.GetParents();

                        foreach (var env in envs)
                        {
                            var envData = env.Value.GetParents();

                            var prefix = $"cfg/{env.Key}/{app.Key}";

                            if (envData.ContainsKey("tree"))
                                result.TreeToFlat(prefix, envData["tree"]);

                            if (envData.ContainsKey("files"))
                                result.LoadFiles(fi.DirectoryName, prefix, envData["files"]);

                            if (envData.ContainsKey("user"))
                            {
                                var userData = envData["user"].GetParents();

                                var name = userData["name"] as string;
                                var pass = userData["pass"] as string;

                                result.AddUser(name, pass, $"{env.Key}/{app.Key}");
                            }
                        }//envs
                    }//apps
                }

                result.Info(fi, "File processed");
            }

            return result;
        }

        public static void ImportToEtcd(KeyValueData data)
        {
            string etcdUrl = $"http://{data.EtcdHost}:{data.EtcdPort}";
            $"Import to etcd {etcdUrl}".OutUnderline();

            using (var client = new EtcdClient(etcdUrl))
            {
                "Replace key-value tree".OutUnderline();
                client.DeleteRange("cfg/");

                foreach (var kv in data.Tree)
                {
                    kv.Key.Out();
                    client.Put(kv.Key, kv.Value);
                }

                "Delete users & roles".OutUnderline();
                var users = client
                    .UserList(new Etcdserverpb.AuthUserListRequest())
                    .Users
                    .ToList();

                foreach (var name in users)
                    if (name != "root")
                    {
                        var reqDelUsr = new Etcdserverpb.AuthUserDeleteRequest() { Name = name };
                        client.UserDelete(reqDelUsr);
                        $"User {name} deleted".Out();
                    }

                var roles = client
                    .RoleList(new Etcdserverpb.AuthRoleListRequest())
                    .Roles
                    .ToList();

                foreach (var name in roles)
                    if (name != "root")
                    {
                        var reqDelRole = new Etcdserverpb.AuthRoleDeleteRequest() { Role = name };
                        client.RoleDelete(reqDelRole);
                        $"Role {name} deleted".Out();
                    }

                "Create roles".OutUnderline();

                int i = 0;
                var rolesDic = new Dictionary<string, string>(); // access vs role_name

                data.Users
                    .Select(x => x.Access)
                    .Distinct()
                    .ToList()
                    .ForEach(x =>
                    {
                        string roleName = $"role{++i}";

                        var reqRoleAdd = new Etcdserverpb.AuthRoleAddRequest() { Name = roleName };
                        client.RoleAdd(reqRoleAdd);
                        $"Role {roleName} created".Out();

                        var reqAddPerm = new Etcdserverpb.AuthRoleGrantPermissionRequest()
                        {
                            Name = roleName,
                            Perm = new Authpb.Permission()
                            {
                                Key = Google.Protobuf.ByteString.CopyFromUtf8($"cfg/{x}"),
                                RangeEnd = Google.Protobuf.ByteString.CopyFromUtf8($"cfg/{x}"),
                                PermType = Authpb.Permission.Types.Type.Read
                            }
                        };
                        client.RoleGrantPermission(reqAddPerm);
                        $"Readonly access to cfg/{x} granted".Out();

                        reqAddPerm = new Etcdserverpb.AuthRoleGrantPermissionRequest()
                        {
                            Name = roleName,
                            Perm = new Authpb.Permission()
                            {
                                Key = Google.Protobuf.ByteString.CopyFromUtf8($"app/{x}"),
                                RangeEnd = Google.Protobuf.ByteString.CopyFromUtf8($"app/{x}"),
                                PermType = Authpb.Permission.Types.Type.Readwrite
                            }
                        };
                        client.RoleGrantPermission(reqAddPerm);
                        $"Readwrite access to app/{x} granted".Out();

                        rolesDic[x] = roleName;
                    });

                "Create users and grant roles".OutUnderline();

                foreach (var user in data.Users)
                {
                    var reqAddUsr = new Etcdserverpb.AuthUserAddRequest()
                    {
                        Name = user.Name,
                        Password = user.Password
                    };

                    client.UserAdd(reqAddUsr);
                    $"User {user.Name} created".Out();

                    var reqGrantRole = new Etcdserverpb.AuthUserGrantRoleRequest()
                    {
                        User = user.Name,
                        Role = rolesDic[user.Access]
                    };
                    client.UserGrantRole(reqGrantRole);
                    $"Access to {user.Access} granted ({rolesDic[user.Access]})".Out();
                }
            }
        }

    }//end of class

    internal static class KeyValueHelper
    {
        public static Dictionary<string, object> GetParents(this object obj)
        {
            var jo = obj as JObject;
            if (jo == null)
                return new Dictionary<string, object>();

            return jo.ToObject<IDictionary<string, object>>()
                     .ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
