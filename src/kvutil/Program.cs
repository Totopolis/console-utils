using ConsoleHelpers;
using KeyValueHelpers;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;

namespace kvutil
{
    class Program
    {
        static int Main(string[] args)
        {
            // https://github.com/natemcmaster/CommandLineUtils
            var app = new CommandLineApplication();
            app.Name = "kvutil";

            app.HelpOption();
            var optionVersion = app.Option("-v|--version", "Show version", CommandOptionType.NoValue);
            var optionFolder = app.Option("-f|--folder <FOLDER>", "The root folder for kv import", CommandOptionType.SingleValue);

            //var optionRepeat = app.Option<int>("-n|--count <N>", "Repeat", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                if (optionVersion.HasValue())
                {
                    "version 0.01a".OutYellow();
                    return 0;
                }

                var dir = optionFolder.HasValue() ? optionFolder.Value() : Directory.GetCurrentDirectory();

                if (!Directory.Exists(dir))
                {
                    $"Directory {dir} not exists".OutRed();
                    return -1;
                }

                var data = KeyValueUtils.Load(dir, recursive: true, showProcess: true);
                "Data loaded".OutYellow();

                KeyValueUtils.ImportToEtcd(data);
                "Data imported".OutYellow();

                return 0;
            });

            return app.Execute(args);
        }
    }
}
