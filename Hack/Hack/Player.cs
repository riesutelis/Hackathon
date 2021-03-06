﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using System.IO;
using System.Media;

namespace Hack
{

    // Three wrapper classes for the three main sections of the wav file
    class WaveHeader
    {
        public string sGroupID; // RIFF
        public uint dwFileLength; // total file length minus 8, which is taken up by RIFF
        public string sRiffType; // always WAVE

        /// <summary>
        /// Initializes a WaveHeader object with the default values.
        /// </summary>
        public WaveHeader()
        {
            dwFileLength = 0;
            sGroupID = "RIFF";
            sRiffType = "WAVE";
        }
    }

    class WaveFormatChunk
    {
        public string sChunkID;         // Four bytes: "fmt "
        public uint dwChunkSize;        // Length of header in bytes
        public ushort wFormatTag;       // 1 (MS PCM)
        public ushort wChannels;        // Number of channels
        public uint dwSamplesPerSec;    // Frequency of the audio in Hz... 44100
        public uint dwAvgBytesPerSec;   // for estimating RAM allocation
        public ushort wBlockAlign;      // sample frame size, in bytes
        public ushort wBitsPerSample;    // bits per sample

        /// <summary>
        /// Initializes a format chunk with the following properties:
        /// Sample rate: 44100 Hz
        /// Channels: Mono
        /// Bit depth: 16-bit
        /// </summary>
        public WaveFormatChunk()
        {
            sChunkID = "fmt ";
            dwChunkSize = 16;
            wFormatTag = 1;
            wChannels = 1;
            dwSamplesPerSec = 44100;
            wBitsPerSample = 16;
            wBlockAlign = (ushort)(wChannels * (wBitsPerSample / 8));
            dwAvgBytesPerSec = dwSamplesPerSec * wBlockAlign;
        }
    }

    class WaveDataChunk
    {
        public string sChunkID;     // "data"
        public uint dwChunkSize;    // Length of header in bytes
        public short[] shortArray;  // 16-bit audio

        /// <summary>
        /// Initializes a new data chunk with default values.
        /// </summary>
        public WaveDataChunk()
        {
            shortArray = new short[0];
            dwChunkSize = 0;
            sChunkID = "data";
        }
    }


    ////////////////////////////////////////////////////////////////////






    class Player
    {
        //  private DynamicSoundEffectInstance instance;

        private List<float> workingBuffer;
        private List<float> workingBuffer1;
        private List<float> workingBuffer2;
        private short[] convertedBuffer;
        private float volume = 1.0f;

        private WaveHeader header;
        private WaveFormatChunk format;
        private WaveDataChunk data;

        public string FilePath = "test.wav";

        SoundPlayer player;
        SoundPlayer beep;



        public Player()
        {
            player = new SoundPlayer(FilePath);
            beep = new SoundPlayer("volume.wav");
        }



        public void MakeMusic(float seed, float tempo, int key, float timeSignature)
        {
            int keyIndex = 0;
            //float tempo = 120;
            int phraseCount = 2;
            int scaleType = 2;
            // testing stuff
            KeySignature keyMaker = new KeySignature();
            BackingTrack bt = new BackingTrack();
            int[] testScale = new int[5] { 0, 2, 4, 7, 9 };
            workingBuffer1 = bt.makeTrack(keyMaker.CreateScale(keyIndex, scaleType), 0, tempo, phraseCount, waveform.sine, 1.0f, seed);
            
            Maestro m = new Maestro(keyIndex, scaleType, tempo);
            workingBuffer2 = m.CreateTrack(phraseCount, waveform.sine,(int)seed);

            // distortion
         //   workingBuffer1 = MixerClass.Distortion(workingBuffer1, 3.0f);

            workingBuffer = MixerClass.Mix(workingBuffer1, workingBuffer2, 0.7f, 1.55f);
            workingBuffer = MixerClass.Normalize(workingBuffer);

            // reverb I guess
       //     workingBuffer = MixerClass.Reverberation(workingBuffer, 0.7f);

            save(FilePath);
            Play();
        }



        public void Play()
        {
            player.Play();
        }



        public void ChangeVolume(float volume)
        {
            volume *= volume * volume;

            NoteGenerator tng = new NoteGenerator();
            this.volume = volume;
            workingBuffer = tng.NoteFromA3(13, 0.3f, waveform.sine);
            save("volume.wav");
            beep.Play();
        }




        // Saves the track to a wav file. A working buffer must exist.
        public void save(string filePath)
        {
            header = new WaveHeader();
            format = new WaveFormatChunk();
            data = new WaveDataChunk();

            convert();

            data.shortArray = convertedBuffer;
            // Calculate data chunk size in bytes
            data.dwChunkSize = (uint)(data.shortArray.Length * (format.wBitsPerSample / 8));


            FileStream f = new FileStream(filePath, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(f);
            // Write the header
            bw.Write(header.sGroupID.ToCharArray());
            bw.Write(header.dwFileLength);
            bw.Write(header.sRiffType.ToCharArray());

            // Write the format chunk
            bw.Write(format.sChunkID.ToCharArray());
            bw.Write(format.dwChunkSize);
            bw.Write(format.wFormatTag);
            bw.Write(format.wChannels);
            bw.Write(format.dwSamplesPerSec);
            bw.Write(format.dwAvgBytesPerSec);
            bw.Write(format.wBlockAlign);
            bw.Write(format.wBitsPerSample);

            // Write the data chunk
            bw.Write(data.sChunkID.ToCharArray());
            bw.Write(data.dwChunkSize);
            foreach (short dataPoint in data.shortArray)
            {
                bw.Write(dataPoint);
            }

            bw.Seek(4, SeekOrigin.Begin);
            uint filesize = (uint)bw.BaseStream.Length;
            bw.Write(filesize - 8);

            // Clean up
            bw.Close();
            f.Close();
        }



        // Converts the float values in th working buffer to pairs of bytes and stores them in xna buffer
        private void convert()
        {
            int bufferSize = workingBuffer.Count;
            convertedBuffer = new short[bufferSize];

            for (int i = 0; i < bufferSize; i++)
            {
                float floatSample = MathHelper.Clamp(workingBuffer[i], -1.0f, 1.0f) * volume;
                short shortSample = (short)(floatSample >= 0.0f ? short.MaxValue * floatSample : short.MinValue * floatSample * -1);
                convertedBuffer[i] = shortSample;
            }
        }




    }
}
