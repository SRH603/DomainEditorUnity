// Assets/Scripts/DomainEchoing/Core/DechCrypto.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class DechCrypto
{
    // 32字节主密钥——请替换为你自己的，并考虑“拆分/混淆”防静态分析
    static readonly byte[] MASTER_KEY = Encoding.UTF8.GetBytes("domain-echoing-master-key-32bytes!!");

    public static void DeriveKeys(byte[] salt, out byte[] encKey, out byte[] macKey)
    {
        using var h = new HMACSHA256(MASTER_KEY);
        var prk = h.ComputeHash(salt); // PRK = HMAC(MK, salt)

        using var h1 = new HMACSHA256(prk);
        encKey = h1.ComputeHash(Encoding.ASCII.GetBytes("DECH-ENC"));

        using var h2 = new HMACSHA256(prk);
        macKey = h2.ComputeHash(Encoding.ASCII.GetBytes("DECH-MAC"));
    }

    public static byte[] RandomBytes(int n) { var b=new byte[n]; RandomNumberGenerator.Fill(b); return b; }

    public static byte[] Hmac(byte[] key, byte[] dataA, byte[] dataB)
    {
        using var h = new HMACSHA256(key);
        h.TransformBlock(dataA, 0, dataA.Length, null, 0);
        h.TransformFinalBlock(dataB, 0, dataB.Length);
        return h.Hash;
    }

    public static bool FixedTimeEquals(byte[] a, byte[] b)
    {
        if (a==null||b==null||a.Length!=b.Length) return false;
        int diff=0; for(int i=0;i<a.Length;i++) diff |= a[i]^b[i];
        return diff==0;
    }

    public static byte[] AesCbcEncrypt(byte[] key, byte[] iv, byte[] plain)
    {
        using var aes = Aes.Create(); // 兼容性好（Mono/IL2CPP）
        aes.KeySize=256; aes.BlockSize=128; aes.Mode=CipherMode.CBC; aes.Padding=PaddingMode.PKCS7;
        aes.Key=key; aes.IV=iv;
        using var ms=new MemoryStream();
        using(var cs=new CryptoStream(ms,aes.CreateEncryptor(),CryptoStreamMode.Write))
            cs.Write(plain,0,plain.Length);
        return ms.ToArray();
    }

    public static byte[] AesCbcDecrypt(byte[] key, byte[] iv, byte[] cipher)
    {
        using var aes = Aes.Create();
        aes.KeySize=256; aes.BlockSize=128; aes.Mode=CipherMode.CBC; aes.Padding=PaddingMode.PKCS7;
        aes.Key=key; aes.IV=iv;
        using var input=new MemoryStream(cipher);
        using var cs=new CryptoStream(input,aes.CreateDecryptor(),CryptoStreamMode.Read);
        using var output=new MemoryStream();
        cs.CopyTo(output);
        return output.ToArray();
    }
}
