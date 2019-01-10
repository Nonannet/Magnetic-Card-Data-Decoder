using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magnetic_Card_Reader
{
    class MagneticCardReader
    {
        int lastTimeStamp = 0;
        double filteredInterval = 0;
        int dataPointIndex = 0;
        byte[] decodedData = new byte[1024];
        int currBitPos = 0;
        int lastPuls = 0;
        int startCountDown = 0;


 
        private int getParity(byte byteValue)
        {
            int A = byteValue;

            A ^= A >> 4;
            A ^= A >> 2;
            A ^= A >> 1;

            return A & 1;
        }


        public string getDataString()
        {
            /*char[] charTableTrack1 = {' ', '!', '"', '#', '$', '%', '&', '´', '(', ')', '*', '+', ',', '-', '.', '/',
                                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?',
                                '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
                                'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[', '\\', ']', '^', '_'};*/

            char[] charTableTrack2 = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?'};

            char[] charsData = new char[64];

            for (int i = 0; i < 64; i++ )
            {
                if (getParity(decodedData[i])==0)
                {
                    charsData[i] = '_';
                }
                else
                {
                    charsData[i] = charTableTrack2[(decodedData[i] % 16)];
                }
            }

            string outPstring = new String(charsData);

            return outPstring;
        }

        public void addNewSignalState(int timeStamp, int newState)
        {
            int shortPuls = 0;
            int timeInterv = (timeStamp - lastTimeStamp);

            if (dataPointIndex < 18)
            {
                if (lastTimeStamp > 0)
                {
                    filteredInterval = filteredInterval + (double)timeInterv / 16;
                }
            }
            else
            {
                if (timeInterv < filteredInterval * 3 / 4)
                {
                    shortPuls = 1;
                }
                else
                {
                    shortPuls = 0;
                }

                filteredInterval = filteredInterval * 0.9 + (double)(timeInterv * (shortPuls + 1)) * 0.1;

                if ((lastPuls & shortPuls) == 1)
                {
                    startCountDown--;
                    if (startCountDown < 0)
                    {
                        decodedData[currBitPos / 5] |= (byte)(1 << (currBitPos % 5));
                        currBitPos++;
                        shortPuls = 0;
                    }
                    
                }
                else if (shortPuls == 0)
                {
                    if (startCountDown < 0)
                    {
                        currBitPos++;
                    }
                }
            }

            lastPuls = shortPuls;
            lastTimeStamp = timeStamp;
            dataPointIndex++;
        }
    }
}
