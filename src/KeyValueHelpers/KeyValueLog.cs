using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KeyValueHelpers
{
    public enum KeyValueLogType
    {
        Info,
        Warning,
        Error
    }

    public class KeyValueLog
    {
        public KeyValueLogType LogType { get; private set; }
        public DateTime When { get; private set; }
        public FileInfo File { get; private set; }
        public string What { get; private set; }

        public static KeyValueLog New(KeyValueLogType type, FileInfo file, string what)
        {
            var result = new KeyValueLog()
            {
                LogType = type,
                When = DateTime.Now,
                File = file,
                What = what
            };

            return result;
        }
    }
}
