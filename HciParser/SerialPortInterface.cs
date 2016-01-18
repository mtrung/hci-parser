using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace HciParser
{
    /// <summary> 
    /// Interfaces with a serial port. There should only be one instance 
    /// of this class for each serial port to be used. 
    /// </summary> 
    public class SerialPortInterface
    {
        private SerialPort _serialPort = new SerialPort();
        private int _baudRate = 115200;
        private int _dataBits = 8;
        private Handshake _handshake = Handshake.None;
        private Parity _parity = Parity.None;
        private string _portName = "COM1";
        private StopBits _stopBits = StopBits.One;
        public int portIndex;

        /// <summary> 
        /// Holds data received until we get a terminator. 
        /// </summary> 
        private string tString = string.Empty;
        /// <summary> 
        /// End of transmition byte in this case EOT (ASCII 4). 
        /// </summary> 
        private byte _terminator = 0x4;

        public int BaudRate { get { return _baudRate; } set { _baudRate = value; } }
        public int DataBits { get { return _dataBits; } set { _dataBits = value; } }
        public Handshake Handshake { get { return _handshake; } set { _handshake = value; } }
        public Parity Parity { get { return _parity; } set { _parity = value; } }
        public string PortName { get { return _portName; } set { _portName = value; } }


        public static void GetPortNames()
        {
            // Get a list of serial port names. 
            string[] ports = SerialPort.GetPortNames();
            if (ports == null || ports.Length == 0)
            {
                Console.WriteLine("No serial ports were found");
                return;
            }

            Console.WriteLine("The following serial ports were found:");
            // Display each port name to the console. 
            foreach (string port in ports)
            {
                Console.WriteLine(port);
            }
            //Console.ReadLine();
        }

        public static void DetectAndOpenPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports == null)
                return;

            SerialPortInterface spi;
            for (int i = 0; i < ports.Length; ++i)
            {
                if (i >= 2)
                    break;
                spi = new SerialPortInterface();
                spi.PortName = ports[i];
                spi.portIndex = i;
                spi.Open();
            }
        }


        void print(string s)
        {
            Output.printComLog(PortName, s, portIndex);
        }

        public bool Open()
        {
            try
            {
                _serialPort.BaudRate = _baudRate;
                _serialPort.DataBits = _dataBits;
                _serialPort.Handshake = _handshake;
                _serialPort.Parity = _parity;
                _serialPort.PortName = _portName;
                _serialPort.StopBits = _stopBits;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);

                if (_serialPort.IsOpen)
                {
                    print("Already opened");
                }

                _serialPort.Open();
            }
            catch (Exception ex) 
            {
                print("Open error: " + ex.Message);
                return false; 
            }

            print("Opened");
            return true;
        }

        byte[] buffer = new byte[512];
        public bool IsAsciiMode = false;
        int processedLen;
        int buffer_write= 0;

        static public bool ShowSerialBuffer = false;

        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Initialize a buffer to hold the received data 
            if (buffer == null || (buffer.Length) < _serialPort.ReadBufferSize + buffer_write)
            {
                byte[] buffer2 = new byte[_serialPort.ReadBufferSize + buffer_write];
                if (buffer_write > 0)
                    Buffer.BlockCopy(buffer, 0, buffer2, 0, buffer_write);
                buffer = buffer2;
            }

            //There is no accurate method for checking how many bytes are read 
            //unless you check the return from the Read method 
            int bytesRead = _serialPort.Read(buffer, buffer_write, _serialPort.ReadBufferSize);
            bytesRead += buffer_write;

            if (IsAsciiMode)
            {
                //For the example assume the data we are received is ASCII data. 
                tString += Encoding.ASCII.GetString(buffer, 0, bytesRead);
                //Check if string contains the terminator  
                if (tString.IndexOf((char)_terminator) > -1)
                {
                    //If tString does contain terminator we cannot assume that it is the last character received 
                    string workingString = tString.Substring(0, tString.IndexOf((char)_terminator));
                    //Remove the data up to the terminator from tString 
                    tString = tString.Substring(tString.IndexOf((char)_terminator));
                    //Do something with workingString 
                    Console.WriteLine(workingString);
                }
            }
            else
            {
                //  ...must lock output to prevent data mixing w/ other ports
                lock (Output.obj)
                {
                    //  ...print out the wrapped byte count
                    string byteCountStr = buffer_write > 0 ?
                        buffer_write.ToString() + "+" + (bytesRead-buffer_write).ToString() : 
                        bytesRead.ToString();

                    if (ShowSerialBuffer)
                    {
                        tString = BitConverter.ToString(buffer, 0, bytesRead);                        
                        print(byteCountStr + " - " + tString);
                    }
                    else
                    {
                        print(byteCountStr);
                    }

                    processedLen = Parser.parse(buffer, 0, bytesRead);

                    //  ...move the remain data to begin of the buffer
                    if (processedLen < bytesRead)
                    {
                        buffer_write = bytesRead - processedLen;
                        Buffer.BlockCopy(buffer, processedLen, buffer, 0, buffer_write);
                    }
                    else
                        buffer_write = 0;
                }                
            }
        }
    } 
}
