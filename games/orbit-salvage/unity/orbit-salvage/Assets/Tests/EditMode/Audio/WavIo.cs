using System;
using System.IO;

namespace OrbitalSalvage.AudioDsp.Tests
{
    public static class WavIo
    {
        public static float[][] ReadFloat32(string path, out int sampleRate)
        {
            byte[] bytes = File.ReadAllBytes(path);
            int channels = 0;
            sampleRate = 0;
            int bits = 0;
            int format = 0;
            int pos = 12;
            while (pos + 8 <= bytes.Length)
            {
                string id = System.Text.Encoding.ASCII.GetString(bytes, pos, 4);
                int size = BitConverter.ToInt32(bytes, pos + 4);
                int body = pos + 8;
                if (id == "fmt ")
                {
                    format = BitConverter.ToInt16(bytes, body);
                    channels = BitConverter.ToInt16(bytes, body + 2);
                    sampleRate = BitConverter.ToInt32(bytes, body + 4);
                    bits = BitConverter.ToInt16(bytes, body + 14);
                }
                else if (id == "data")
                {
                    if (format != 3 || bits != 32)
                    {
                        throw new InvalidDataException("Expected float32 WAV: " + path);
                    }

                    int frames = size / (4 * channels);
                    float[][] data = new float[channels][];
                    for (int c = 0; c < channels; c++)
                    {
                        data[c] = new float[frames];
                    }

                    for (int i = 0; i < frames; i++)
                    {
                        for (int c = 0; c < channels; c++)
                        {
                            data[c][i] = BitConverter.ToSingle(bytes, body + (i * channels + c) * 4);
                        }
                    }

                    return data;
                }

                pos = body + size + (size & 1);
            }

            throw new InvalidDataException("No data chunk: " + path);
        }

        public static void WriteFloat32(string path, float[][] data, int sampleRate)
        {
            int channels = data.Length;
            int frames = data[0].Length;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream fs = File.Create(path))
            using (BinaryWriter w = new BinaryWriter(fs))
            {
                int dataBytes = frames * channels * 4;
                w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                w.Write(36 + dataBytes);
                w.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                w.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                w.Write(16);
                w.Write((short)3);
                w.Write((short)channels);
                w.Write(sampleRate);
                w.Write(sampleRate * channels * 4);
                w.Write((short)(channels * 4));
                w.Write((short)32);
                w.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                w.Write(dataBytes);
                for (int i = 0; i < frames; i++)
                {
                    for (int c = 0; c < channels; c++)
                    {
                        w.Write(data[c][i]);
                    }
                }
            }
        }

        public static float[][] Concat(float[][] a, float[][] b, int gapFrames)
        {
            int channels = a.Length;
            float[][] result = new float[channels][];
            for (int c = 0; c < channels; c++)
            {
                result[c] = new float[a[c].Length + gapFrames + b[c].Length];
                Array.Copy(a[c], 0, result[c], 0, a[c].Length);
                Array.Copy(b[c], 0, result[c], a[c].Length + gapFrames, b[c].Length);
            }

            return result;
        }
    }
}
