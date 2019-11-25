using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ConsoleHelpers
{
    public static class BashUtils
    {
        public static string Bash(this string cmd)
        // TODO: params - showOutput = false, enableExceptions = false
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            Console.WriteLine(cmd);

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine(result);

            return result;
        }
    }
}
