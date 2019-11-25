using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleHelpers
{
    public static class OutputUtils
    {
        // TODO: toLog copy stream
        public static void Out(this string message, bool underLine = false)
        {
            Console.WriteLine(message);
            if (underLine)
                Console.WriteLine(string.Empty.PadRight(message.Length, '='));
        }

        public static void OutUnderline(this string message)
        {
            message.Out(true);
        }
    }
}
