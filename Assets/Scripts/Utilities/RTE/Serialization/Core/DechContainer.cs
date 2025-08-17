using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

public static class DechContainer
{
    const string MAGIC = "DECH";
    const byte VER_MAJOR = 1;
    const byte VER_MINOR_WRITE = 1; // 写入 1.1
    const ushort FLAGS = 0b0011;    // Deflate + AES-CBC

    // —— v1.0 payload: [U32 JSON][I32 AudioType][U64 AudioLen][AudioBytes]
    public enum LegacyAudioType : int { WAV = 1 }

    // —— v1.1 payload: [U32 JSON][U16 ExtLen][UTF8 Ext][U64 AudioLen][AudioBytes]
    public static byte[] SerializeJson(GameDataDTO dto) => Encoding.UTF8.GetBytes(JsonUtility.ToJson(dto, false));
    public static GameDataDTO DeserializeJson(byte[] bytes) => JsonUtility.FromJson<GameDataDTO>(Encoding.UTF8.GetString(bytes));

    public static void PackAndWrite(string path, GameDataDTO dto, string audioExtNoDot, byte[] audioBytes)
    {
        var payload = BuildPayloadV11(dto, audioExtNoDot, audioBytes);
        var deflated = Deflate(payload);

        var salt = DechCrypto.RandomBytes(16);
        DechCrypto.DeriveKeys(salt, out var encKey, out var macKey);
        var iv = DechCrypto.RandomBytes(16);
        var cipher = DechCrypto.AesCbcEncrypt(encKey, iv, deflated);

        var headerForMac = BuildHeaderForMac(VER_MINOR_WRITE, salt, iv, (ulong)cipher.LongLength);
        var hmac = DechCrypto.Hmac(macKey, headerForMac, cipher);

        Array.Copy(hmac, 0, headerForMac, 48, 32);
        using var fs = File.Create(path);
        fs.Write(headerForMac, 0, headerForMac.Length);
        fs.Write(cipher, 0, cipher.Length);
    }

    public static (GameDataDTO dto, string audioExtNoDot, byte[] audioBytes) ReadAndUnpack(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);

        if (Encoding.ASCII.GetString(br.ReadBytes(4)) != MAGIC) throw new Exception("Invalid DECH magic");
        byte verMajor = br.ReadByte(), verMinor = br.ReadByte();
        ushort flags = br.ReadUInt16();
        if (verMajor != 1) throw new Exception($"Unsupported version {verMajor}.{verMinor}");
        if (flags != FLAGS) throw new Exception("DECH flags mismatch");

        var salt = br.ReadBytes(16);
        var iv   = br.ReadBytes(16);
        var cipherLen = br.ReadUInt64();
        var hmac = br.ReadBytes(32);
        var cipher = br.ReadBytes((int)cipherLen);

        var headerForMac = BuildHeaderForMac(verMinor, salt, iv, cipherLen);
        DechCrypto.DeriveKeys(salt, out var encKey, out var macKey);
        var expected = DechCrypto.Hmac(macKey, headerForMac, cipher);
        if (!DechCrypto.FixedTimeEquals(hmac, expected)) throw new Exception("HMAC verification failed");

        var deflated = DechCrypto.AesCbcDecrypt(encKey, iv, cipher);
        var payload  = Inflate(deflated);

        return (verMinor >= 1) ? UnpackPayloadV11(payload) : UnpackPayloadV10(payload);
    }

    // ---------------- v1.1 payload ----------------
    static byte[] BuildPayloadV11(GameDataDTO dto, string audioExtNoDot, byte[] audio)
    {
        var json = SerializeJson(dto);
        var ext = (audioExtNoDot ?? "wav").Trim().TrimStart('.').ToLowerInvariant();
        var extBytes = Encoding.UTF8.GetBytes(ext);
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, Encoding.UTF8, true);
        bw.Write((UInt32)json.Length);
        bw.Write(json);
        bw.Write((UInt16)extBytes.Length);
        bw.Write(extBytes);
        bw.Write((UInt64)audio.LongLength);
        bw.Write(audio);
        return ms.ToArray();
    }
    static (GameDataDTO, string, byte[]) UnpackPayloadV11(byte[] raw)
    {
        using var ms = new MemoryStream(raw);
        using var br = new BinaryReader(ms, Encoding.UTF8, true);
        var jsonLen = br.ReadUInt32();
        var json    = br.ReadBytes((int)jsonLen);
        var extLen  = br.ReadUInt16();
        var ext     = Encoding.UTF8.GetString(br.ReadBytes(extLen));
        var aLen    = br.ReadUInt64();
        var audio   = br.ReadBytes((int)aLen);
        return (DeserializeJson(json), ext, audio);
    }

    // ---------------- v1.0 兼容 ----------------
    static (GameDataDTO, string, byte[]) UnpackPayloadV10(byte[] raw)
    {
        using var ms = new MemoryStream(raw);
        using var br = new BinaryReader(ms, Encoding.UTF8, true);
        var jsonLen = br.ReadUInt32();
        var json    = br.ReadBytes((int)jsonLen);
        var at      = (LegacyAudioType)br.ReadInt32();
        var aLen    = br.ReadUInt64();
        var audio   = br.ReadBytes((int)aLen);
        // v1.0 只有 WAV
        return (DeserializeJson(json), "wav", audio);
    }

    // ---------------- 压缩/解压 & Header ----------------
    static byte[] Deflate(byte[] raw)
    {
        using var ms = new MemoryStream();
        using (var ds = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionLevel.Optimal, true))
            ds.Write(raw, 0, raw.Length);
        return ms.ToArray();
    }
    static byte[] Inflate(byte[] deflated)
    {
        using var input = new MemoryStream(deflated);
        using var ds = new System.IO.Compression.DeflateStream(input, System.IO.Compression.CompressionMode.Decompress, true);
        using var output = new MemoryStream();
        ds.CopyTo(output);
        return output.ToArray();
    }

    static byte[] BuildHeaderForMac(byte verMinor, byte[] salt, byte[] iv, ulong cipherLen)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, Encoding.UTF8, true);
        bw.Write(Encoding.ASCII.GetBytes(MAGIC));
        bw.Write(VER_MAJOR); bw.Write(verMinor);
        bw.Write(FLAGS);
        bw.Write(salt);
        bw.Write(iv);
        bw.Write(cipherLen);
        bw.Write(new byte[32]); // HMAC 占位
        return ms.ToArray();
    }
}
