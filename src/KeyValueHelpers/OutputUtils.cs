using System;
using System.Collections.Generic;
using System.Text;

namespace KeyValueHelpers
{
    internal static class OutputUtils
    {
        // TODO: toLog copy stream
        public static void Out(this string message, bool underLine = false)
        {
            Console.WriteLine(message);
            if (underLine)
                Console.WriteLine(string.Empty.PadRight(message.Length, '='));
        }

        public static void OutGreen(this string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void OutRed(this string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void OutYellow(this string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void OutUnderline(this string message)
        {
            message.Out(true);
        }

        public static string Request(this string question)
        {
            Console.WriteLine(question + ": ");
            return Console.ReadLine();
        }
    }
}
