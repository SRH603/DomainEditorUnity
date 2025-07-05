using System.Text;
using UnityEngine;

namespace Battlehub.Storage
{
    public class BinaryData : ScriptableObject
    {
        private static readonly byte[] m_empty = new byte[0];

        public byte[] Bytes
        {
            get;
            set;
        }

        private void Awake()
        {
            Bytes = m_empty;
        }

        public static implicit operator byte[](BinaryData d) => d.Bytes;
        public static explicit operator string(BinaryData d) => d.GetString();

        public static BinaryData FromBytes(byte[] bytes)
        {
            var binaryData = CreateInstance<BinaryData>();
            binaryData.Bytes = bytes;
            return binaryData;
        }

        public static BinaryData FromString(string str)
        {
            var binaryData = CreateInstance<BinaryData>();
            binaryData.SetString(str);
            return binaryData;
        }
    }

    public static class BinaryDataExt
    {
        public static string GetString(this BinaryData data)
        {
            return Encoding.UTF8.GetString(data.Bytes);
        }

        public static void SetString(this BinaryData data, string str)
        {
           data.Bytes = Encoding.UTF8.GetBytes(str);
        }
    }
}

