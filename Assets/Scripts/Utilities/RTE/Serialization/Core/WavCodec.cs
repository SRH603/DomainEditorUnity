using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class WavWriter
{
    public static byte[] FromAudioClip(AudioClip clip)
    {
        int ch = clip.channels, sr = clip.frequency, frames = clip.samples;
        float[] data = new float[frames * ch]; clip.GetData(data, 0);
        int dataLen = data.Length * 2, totalLen = 44 + dataLen;
        using var ms = new MemoryStream(totalLen);
        using var bw = new BinaryWriter(ms, Encoding.UTF8, true);
        bw.Write(Encoding.ASCII.GetBytes("RIFF")); bw.Write(totalLen - 8);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));
        bw.Write(Encoding.ASCII.GetBytes("fmt ")); bw.Write(16);
        bw.Write((ushort)1); bw.Write((ushort)ch); bw.Write(sr);
        bw.Write(sr * ch * 2); bw.Write((ushort)(ch * 2)); bw.Write((ushort)16);
        bw.Write(Encoding.ASCII.GetBytes("data")); bw.Write(dataLen);
        for (int i=0;i<data.Length;i++){ var s=Mathf.Clamp(data[i],-1f,1f); short v=(short)Mathf.RoundToInt(s*32767f); bw.Write(v); }
        return ms.ToArray();
    }
}