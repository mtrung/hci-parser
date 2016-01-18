using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HciParser
{
    class HciLlc
    {
        static StringBuilder mSb = new StringBuilder();

        static bool isPartialPacket(byte llc_len, int startPos, int buffer_length)
        {
            return llc_len > buffer_length - (startPos + 1);
        }

        //  ...return processed len
        static public int parseLlc(byte[] buffer, int startPos, int buffer_length)
        {
            byte llc_len = buffer[startPos];
            byte llc_header = buffer[startPos + 1];

            //  ...invalid len -> return 1 to ignore
            if (llc_len > 0x20 || llc_len == 0)
                return 1;

            //  ...partial packet -> 0 byte processed
            if (isPartialPacket(llc_len, startPos, buffer_length))
                return 0;

            //  ...print out HCI LLC packet
            mSb.Clear();
            mSb.Append(" - ");
            mSb.Append(BitConverter.ToString(buffer, startPos, llc_len+1));
            Console.WriteLine(mSb.ToString());

            mSb.Clear();
            mSb.Append("\t");

            bool toPrint = true;

            byte type = Parser.getLeftMost2Bits(llc_header);
            switch (type)
            {
                //  ...I Frame
                case 2:
                    if (llc_len - 3 > 0)
                    {
                        Hci.parseHcpPacket(buffer, startPos + 2, llc_len - 3);
                        toPrint = false; // let HCI print its own msg
                    }
                    break;

                case 3:
                    bool isUframe = Parser.isBitSet(llc_header, 5);
                    if (isUframe) 
                        parseUFrame(llc_header);
                    else  //  ...S frame
                    {
                        byte SFrameType = (byte)((llc_header & 0x18) >> 3);
                        parseSFrame(SFrameType);
                    }
                    break;

                default:
                    mSb.Append("Unknown frame");
                    break;
            }
            
            if (toPrint)
            {
                mSb.Append("\n");
                Console.WriteLine(mSb.ToString());
            }

            return llc_len + 1;
        }

        static bool parseSFrame(byte SFrameType)
        {
            switch (SFrameType)
            {
                case 0: mSb.Append("RR");
                    break;

                case 1: mSb.Append("REJ");
                    break;

                case 2: mSb.Append("RNR");
                    break;

                case 3: mSb.Append("SREJ");
                    break;
            }
            return true;
        }

        static bool parseUFrame(byte llc_header)
        {
            bool isUFrameRset = (llc_header & 0x19) == 0x19;
            if (isUFrameRset)
            {
                mSb.Append("RSET");
            }
            else
            {
                bool isUFrameUa = (llc_header & 0x06) == 0x06;
                if (isUFrameUa)
                {
                    mSb.Append("UA");
                }
            }
            return true;
        }

    }
}
