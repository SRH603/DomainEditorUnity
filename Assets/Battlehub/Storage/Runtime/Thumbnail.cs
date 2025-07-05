using ProtoBuf;

namespace Battlehub.Storage
{
    [ProtoContract]
    public struct Thumbnail : IThumbnail
    {
        [ProtoMember(1)]
        public byte[] Data
        {
            get;
            set;
        }

        public Thumbnail(byte[] data)
        {
            Data = data;
        }
    }
}
