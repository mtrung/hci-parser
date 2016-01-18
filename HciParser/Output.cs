using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HciParser
{
    class Output
    {
        //static string sFilename;
        static System.IO.StreamWriter sFile;
        static public void OpenFile(string filename, bool append)
        {
            if (sFile != null)
                sFile.Close();
            //sFilename = filename;
            sFile = new System.IO.StreamWriter(filename, append);
        }


        static public Object obj = new Object();

        static string sShortTimeFormat = "mm:ss.fff ";
        static string sLongTimeFormat = "yyyy-MM-dd HH:mm:ss.fff ";

        static void WriteLine(string outStr)
        {
            Console.WriteLine(outStr);
            if (sFile != null) sFile.WriteLine(outStr);
        }

        /*public static void print(Boolean isShort, string s)
        {
            Console.ResetColor();
            string timestamp = isShort ?
                DateTime.Now.ToString(sShortTimeFormat) :
                DateTime.Now.ToString(sLongTimeFormat);
            WriteLine(timestamp + s);
        }*/


        public static void printComLog(string portName, string s, int portIndex)
        {
            ConsoleColor c;
            if (portIndex == 0)
                c = ConsoleColor.Yellow;
            else c = ConsoleColor.Magenta;
            Console.ForegroundColor = c;

            StringBuilder sb = new StringBuilder(DateTime.Now.ToString(sShortTimeFormat));
            //sb.Append(" ");
            sb.Append(portName);
            sb.Append(" ");
            sb.Append(s);
            WriteLine(sb.ToString());
        }

        public static void printComment(string s)
        {
            Console.ResetColor();           
            StringBuilder sb = new StringBuilder(DateTime.Now.ToString(sLongTimeFormat));
            //sb.Append("\n");
            sb.Append(" --- ");sb.Append(s);
            sb.Append("\n");
            WriteLine(sb.ToString());
        }

        public static void printPlain(string s)
        {
            Console.ResetColor();
            WriteLine(s);
        }

    }
}
