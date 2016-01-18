using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Timers;

namespace HciParser
{
    class Program
    {
        static string programName = "PN544 Monitor 1.0.0";

        static void Main(string[] args)
        {
            Console.Title = programName;
            Console.WriteLine(programName + "\n\nType 'q' to exit; 'help' for supported commands");
            SerialPortInterface.GetPortNames();
            Output.printComment("Start");
            SerialPortInterface.DetectAndOpenPorts();

            string readInput;
            do
            {
                readInput = Console.ReadLine();
                readInput = readInput.Trim();
                if (readInput.Length == 0)
                    continue;

                if (readInput == "clear" || readInput == "c")
                {
                    Console.Clear();
                    continue;
                }
                if (readInput == "verbose" || readInput == "v")
                {
                    SerialPortInterface.ShowSerialBuffer = true;
                    continue;
                }
                if (readInput == "no-verbose" || readInput == "-v")
                {
                    SerialPortInterface.ShowSerialBuffer = false;
                    continue;
                }
                if (readInput == "help")
                {
                    Output.printPlain("\nSupported commands:\nexit|quit|q:\t quit\nclear|c:\t clear screen\nverbose|v:\t show raw serial buffer\nno-verbose|-v:\t do not show raw serial buffer (default)\ns|save|w|write fileame:\t write (overwrite) log to disk\na|append filename:\t append log to file\n");
                    continue;
                }

                if (readInput == "exit" ||
                    readInput == "quit" || readInput == "q")
                    break;

                string[] words = readInput.Split(' ');
                if (words != null && words.Length == 2)
                {
                    if (words[0] == "s" || words[0] == "w" ||
                        words[0] == "save" || words[0] == "write" ||
                        words[0] == "a" || words[0] == "append")
                    {
                        string filename = words[1];
                        Output.printComment("Log will be saved to "+filename);
                        Output.OpenFile(filename, words[0] == "a");
                        continue;
                    }
                }

                //  ...other inputs are comments
                Output.printComment(readInput);
            } 
            while (true);

            Output.printComment("End");
        }

    }
}
