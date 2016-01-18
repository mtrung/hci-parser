using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HciParser
{
    class Parser
    {
        //  return total processed bytes
        static public int parse(byte[] buffer, int startPos, int buffer_length)
        {
            int singleProcessedLen = 0;
            int currPos = startPos;
            while (currPos < buffer_length)
            {
                singleProcessedLen = HciLlc.parseLlc(buffer, currPos, buffer_length);
                currPos += singleProcessedLen;

                //  ...stop at 1st error
                if (singleProcessedLen == 0)
                    return currPos - startPos;
            }
            return currPos - startPos;
        }

        public static byte sLeftTwoBitMask = 0xC0;
        public static byte sRightSixBitMask = 0x3F;

        public static byte getLeftMost2Bits(byte input)
        {
            byte ret = (byte)((input & sLeftTwoBitMask) >> 6);
            return ret;
        }

        public static bool bit_test_mask(byte val, byte bitmask)
        {
            return ((val & bitmask) == bitmask);
        }
        
        public static bool isBitSet(byte val, byte n)
        {
            return bit_test_mask(val, (byte)(1<<n));
        }

    }
}
