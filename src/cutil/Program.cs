using ConsoleHelpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cutil
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = KeyValueUtils.Load(@"root-folder", true);
        }

        static void Main2(string[] args)
        {
            "Hello greed World!".OutGreen();
            "Hello white World!".Out();
            "Hello red World!".OutRed();

            var config = ConfigUtils.LoadConfig("../../../../../sample.yaml");
            string sval = config.start;
            double dval = config.pi;
            int ival = config.integer;

            var xx = config["dev"]["user"];
            string usr = config.dev.user;

            foreach (dynamic it in config)
            {
                Console.WriteLine(it);
            }
        }
    }
}
