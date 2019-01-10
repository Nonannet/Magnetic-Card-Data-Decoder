//A simple C# .NET wave reader library from http://code.google.com/p/dot-net-wave-reader/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WaveReader
{
    public class WaveData
    {
        private static readonly short BitsPerByte = 8;
        private static readonly short MaxBits = 16;
        public short[][] Samples { get; private set; }
        public short CompressionCode { get; private set; }
        public short NumberOfChannels { get; private set; }
        public int SampleRate { get; private set; }
        public int BytesPerSecond { get; private set; }
        public short BitsPerSample { get; private set; }
        public short BlockAlign { get; private set; }
        public int NumberOfFrames { get; private set; }

        /// <summary>
        /// Reads a Wave file from the input stream, but doesn't close the stream
        /// </summary>
        /// <param name="stream">Input WAVE file stream</param>
        public WaveData(Stream stream)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream))
            {
                binaryReader.ReadChars(4); //"RIFF"
                int length = binaryReader.ReadInt32();
                binaryReader.ReadChars(4); //"WAVE"
                string chunkName = new string(binaryReader.ReadChars(4)); //"fmt "
                int chunkLength = binaryReader.ReadInt32();
                this.CompressionCode = binaryReader.ReadInt16(); //1 for PCM/uncompressed
                this.NumberOfChannels = binaryReader.ReadInt16();
                this.SampleRate = binaryReader.ReadInt32();
                this.BytesPerSecond = binaryReader.ReadInt32();
                this.BlockAlign = binaryReader.ReadInt16();
                this.BitsPerSample = binaryReader.ReadInt16();
                if ((MaxBits % BitsPerSample) != 0)
                {
                    throw new Exception("The input stream uses an unhandled SignificantBitsPerSample parameter");
                }
                binaryReader.ReadChars(chunkLength - 16);
                chunkName = new string(binaryReader.ReadChars(4));
                try
                {
                    while (chunkName.ToLower() != "data")
                    {
                        binaryReader.ReadChars(binaryReader.ReadInt32());
                        chunkName = new string(binaryReader.ReadChars(4));
                    }
                }
                catch
                {
                    throw new Exception("Input stream misses the data chunk");
                }
                chunkLength = binaryReader.ReadInt32();
                this.NumberOfFrames = (chunkLength * BitsPerByte) / (this.NumberOfChannels * this.BitsPerSample);
                this.Samples = SplitChannels(binaryReader, this.NumberOfChannels, this.BitsPerSample, this.NumberOfFrames);
            }
        }

        public static short[][] SplitChannels(BinaryReader binaryReader, short numberOfChannels, short bitsPerSample, int numberOfFrames)
        {
            var samples = new short[numberOfChannels][];
            for (int channel = 0; channel < numberOfChannels; channel++)
            {
                samples[channel] = new short[numberOfFrames];
            }
            short readedBits = 0;
            short numberOfReadedBits = 0;
            for (int frame = 0; frame < numberOfFrames; frame++)
            {
                for (int channel = 0; channel < numberOfChannels; channel++)
                {
                    while (numberOfReadedBits < bitsPerSample)
                    {
                        readedBits |= (short)(Convert.ToInt16(binaryReader.ReadByte()) << numberOfReadedBits);
                        numberOfReadedBits += BitsPerByte;
                    }
                    var numberOfExcessBits = numberOfReadedBits - bitsPerSample;
                    samples[channel][frame] = (short)(readedBits >> numberOfExcessBits);
                    readedBits %= (short)(1 << numberOfExcessBits);
                    numberOfReadedBits = (short)numberOfExcessBits;
                }
            }
            return samples;
        }
    }
}
