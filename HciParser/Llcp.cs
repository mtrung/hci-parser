using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HciParser
{
    class Llcp
    {
        static StringBuilder mSb;

        public static void setSb(StringBuilder sb)
        {
            mSb = sb;
        }
        public static void parseLlcpLlc(byte[] buffer, int startPos, int len, StringBuilder sb)
        {
            mSb = sb;

            byte byte1 = buffer[startPos];
            byte byte2 = buffer[startPos + 1];

            byte dsap = (byte)( byte1 >> 2 );   // left 6 bits
            byte ssap = (byte)( byte2 & 0x3F ); // right 6 bits

            int type1 = ((byte1 & 0x3) << 2);   // right most 2 bits
            int type2 = Parser.getLeftMost2Bits(byte2);
            byte ptype = (byte)(type1 + type2);

            sb.Append("\n\t\t");
            sb.Append(getPtypeStr(ptype));
            sb.Append(" - SSAP=");
            sb.Append(ssap.ToString());
            sb.Append(" DSAP=");
            sb.Append(dsap.ToString());

            parseLlcp(ptype, buffer, startPos, len);

            mSb = null;
        }

        static string getPtypeStr(byte ptype)
        {
            switch (ptype)
            {
                case 0: return "SYMM";
                case 1: return "PAX";
                case 2: return "AGF";
                case 3: return "UI";
                case 4: return "CONNECT";
                case 5: return "Disconnect";
                case 6: return "Connection Complete";
                case 7: return "Disconnected Mode";
                case 8: return "FRMR";
                case 9: return "reserved";
                case 10: return "reserved";
                case 11: return "reserved";
                case 12: return "I";
                case 13: return "Receive Ready";
                case 14: return "Receive Not Ready";
                case 15: return "reserved";
            }
            return "";
        }

        static void parseLlcp(byte ptype, byte[] buffer, int startPos, int len)
        {
            switch (ptype)
            {
                case 0:
                    Console.Beep();
                    break;
                case 1: break;
                case 2: break;
                case 3: break;
                case 4: break;
                case 5: break;
                case 6: break;
                case 7: break;
                case 8: break;
                case 9: break;
                case 10: break;
                case 11: break;
                case 12:
                    Snep.parse();
                    break;
                case 13: break;
                case 14: break;
                case 15: break;
            }
        }

    }
}
