using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HciParser
{
    class Hci
    {
        static StringBuilder mSb = new StringBuilder();

        static byte[] mBuffer4Chain = new byte[1024];
        static int mCurrChainPos = 0;

        public static bool parseHcpPacket(byte[] buffer, int startPos, int len)
        {
            byte cb_pid = buffer[startPos];
            byte pid = (byte)(cb_pid & 0x7F);

            mSb.Clear();
            mSb.Append("\t");

            //  ...CB == 1 -> normal packet
            if (Parser.isBitSet(cb_pid, 7))
            {
                parsePid(pid);

                byte type_ins = buffer[startPos + 1];
                byte type = Parser.getLeftMost2Bits(type_ins);
                byte ins = (byte)(type_ins & Parser.sRightSixBitMask);

                if (mCurrChainPos > 0)
                {
                    mSb.Append(" Chaining end ");
                    Buffer.BlockCopy(buffer, startPos + 2, mBuffer4Chain, mCurrChainPos, len - 2);
                    int chainedMsgLen = mCurrChainPos + (len - 2);// this is the len of whole chain msg
                    parseHcpMessage(type, ins, mBuffer4Chain, 0, chainedMsgLen, mSb);
                    mCurrChainPos = 0;
                }
                else
                {
                    parseHcpMessage(type, ins, buffer, startPos + 2, len - 2, mSb);
                }
            }
            else
            {
                mSb.Append("Chaining");
                parsePid(pid);
                Buffer.BlockCopy(buffer, startPos + 2, mBuffer4Chain, mCurrChainPos, len - 2);
                mCurrChainPos += (len - 2);
            }

            Console.WriteLine(mSb.ToString());
            return true;
        }

        static bool sContactlessReaderMode = false;
        static void parsePid(byte pid)
        {
            switch (pid)
            {
                case 0: mSb.Append(" link management gate "); break;
                case 1: mSb.Append(" administration gate "); break;

                case 0x21: mSb.Append(" Type B card RF gate "); break;
                case 0x22: mSb.Append(" Type B' card RF gate "); break;
                case 0x23: mSb.Append(" Type A card RF gate "); break;
                case 0x24: mSb.Append(" Type F card RF gate "); break;

                case 0x30: mSb.Append(" Initiator RF gate "); break;
                case 0x31: mSb.Append(" Target RF gate "); break;

                case 0x11: mSb.Append(" 14443 Type B reader RF gate "); sContactlessReaderMode = true; break;
                case 0x13: mSb.Append(" 14443 Type A reader RF gate "); sContactlessReaderMode = true; break;

                default:
                    mSb.Append(" Pipe "); mSb.Append(pid.ToString("X2"));mSb.Append(" ");
                    break;
            }
        }

        static void parseHcpMessage(byte type, byte ins, byte[] buffer, int startPos, int len, StringBuilder sb)
        {
            bool success = true;
            switch (type)
            {
                case 0:
                    success = parseCmd(ins, buffer, startPos, len, sb);
                    break;
                case 1:
                    success = parseEvent(ins, buffer, startPos, len, sb);
                    break;
                case 2:
                    success = parseResp(ins, buffer, startPos, len, sb);
                    break;
            }

            //  ...use default string if failed
            if (!success)
                sb.Append(msgTypeStr(type));
        }

        static string msgTypeStr(byte type)
        {
            switch (type)
            {
                case 0: return "CMD";
                case 1: return "EVT";
                case 2: return "RESP";
            }
            return "";
        }

        static bool parseEvent(byte ins, byte[] buffer, int startPos, int len, StringBuilder sb)
        {
            switch (ins)
            {
                case 0x10:
                    sb.Append(" EVT_TARGET_DISCOVERED - ");
                    sb.Append(buffer[startPos] == 3 ? "multiple" : "single");
                    return true;

                case 0x12:
                    sb.Append(" NXP_EVT_INFO_EXT_RF_FIELD - ");
                    sb.Append(buffer[startPos] == 0 ? "NO" : "YES");
                    return true;


                case 1:
                    sb.Append(" NXP_EVT_NFC_SND_DATA ");
                    // mi + data
                    // llcp startPos + 1
                    if (1 < len)
                    {
                        Llcp.parseLlcpLlc(buffer, startPos + 1, len - 1, sb);
                    }
                    return true;

                case 4:
                    sb.Append(" NXP_EVT_NFC_RCV_DATA - ");
                    // rf + mi + data

                    if (buffer[startPos] == 1)
                        sb.Append("RF Error");
                    // llcp startPos + 2
                    if (2 < len)
                    {
                        Llcp.parseLlcpLlc(buffer, startPos + 2, len - 2, sb);
                    }
                    return true;

                case 0x02:
                    sb.Append(" NXP_EVT_NFC_ACTIVATED - ");
                    sb.Append(buffer[startPos] == 0 ? "Passive" : "Active");
                    return true;

                case 0x03:
                    sb.Append(" ");
                    sb.Append("NXP_EVT_NFC_DEACTIVATED");
                    sb.Append(" ");
                    return true;
            }
            return false;
        }

        static bool isAdminGate = false;
        static bool parseCmd(byte ins, byte[] buffer, int startPos, int len, StringBuilder sb)
        {
            switch (ins)
            {
                //  ...all gates
                case 1: sb.Append("ANY_SET_PARAMETER"); return true;
                case 2: sb.Append("ANY_GET_PARAMETER"); return true;
                case 3: sb.Append("ANY_OPEN_PIPE"); return true;
                case 4: sb.Append("ANY_CLOSE_PIPE"); return true;

                case 0x10: sb.Append("WR_XCHGDATA"); return true;

                case 0x12: sb.Append("NXP_NFCI_ATTREQUEST"); return true;
                case 0x13: sb.Append("NXP_NFCI_CONTINUE_ACTIVATION"); return true;

            }

            if (isAdminGate)
            {
                switch (ins)
                {
                    case 0x10: sb.Append("ADM_CREATE_PIPE"); return true;
                    case 0x11: sb.Append("ADM_DELETE_PIPE"); return true;
                    case 0x12: sb.Append("ADM_NOTIFY_PIPE_CREATED"); return true;
                    case 0x13: sb.Append("ADM_NOTIFY_PIPE_DELETED"); return true;
                    case 0x14: sb.Append("ADM_CLEAR_ALL_PIPE"); return true;
                    case 0x15: sb.Append("ADM_NOTIFY_ALL_PIPE_CLEARED"); return true;
                }
                sb.Append("Admin gate command");
            }

            return false;
        }

        static bool parseResp(byte ins, byte[] buffer, int startPos, int len, StringBuilder sb)
        {
            switch (ins)
            {
                case 0: sb.Append("ANY_OK"); return true;
                case 1: sb.Append("ANY_E_NOT_CONNETCED"); return true;
                case 2: sb.Append("ANY_E_CMD_PAR_UNKNOWN"); return true;
                case 3: sb.Append("ANY_E_NOK"); return true;
                case 4: sb.Append("ANY_E_NO_PIPES_AVAILABLE"); return true;
                case 5: sb.Append("ANY_E_REG_PAR_UNKNOWN"); return true;
                case 6: sb.Append("ANY_E_PIPE_NOT_OPENED"); return true;
                case 7: sb.Append("ANY_E_CMD_NOT_SUPPORTED"); return true;
                case 8: sb.Append("ANY_E_INHIBITED"); return true;
                case 9: sb.Append("ANY_E_TIMEOUT"); return true;
                case 0x0A: sb.Append("ANY_E_REG_ACCESS_DENIED"); return true;
                case 0x0B: sb.Append("ANY_E_PIPE_ACCESS_DENIED"); return true;
            }
            return false;
        }
    }
}
