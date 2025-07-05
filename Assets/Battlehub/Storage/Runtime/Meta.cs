using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Battlehub.Storage
{
    [ProtoContract]
    public class LinkMap<TID, TFID>  where TID : IEquatable<TID> where TFID : IEquatable<TFID>
    {
        [ProtoMember(1)]
        public Dictionary<TID, TID> AssetIDToInstanceID { get; set; } = new Dictionary<TID, TID>();
    }

    [ProtoContract]
    public struct Meta<TID, TFID> : IMeta<TID, TFID> where TID : IEquatable<TID> where TFID : IEquatable<TFID>
    {
        [ProtoMember(1)]
        public TID ID { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        //[ProtoMember(3)]
        //public Guid TypeID { get; set; }

        [ProtoMember(4)]
        public HashSet<TID> OutboundDependencies { get; set; }

        [ProtoMember(5)]
        public HashSet<TID> InboundDependencies { get; set; }

        [ProtoMember(6)]
        public TFID ThumbnailFileID { get; set; }

        [ProtoMember(7)]
        public TFID DataFileID { get; set; }

        public TFID FileID { get; set; }

        [ProtoMember(8)]
        public Dictionary<TID, TID> Links { get; set; }

        [ProtoMember(9)]
        public Dictionary<TID, LinkMap<TID, TID>> LinkMaps { get; set; }

        public Dictionary<TID, TID> GetLinkMap(TID id)
        {
            return LinkMaps[id].AssetIDToInstanceID;
        }

        public void AddLinkMap(TID id, Dictionary<TID, TID> linkMap)
        {
            if (LinkMaps == null)
            {
                LinkMaps = new Dictionary<TID, LinkMap<TID, TID>>();
            }

            LinkMaps.Add(id, new LinkMap<TID, TID> { AssetIDToInstanceID = linkMap });
        }

        public void ClearLinkMaps()
        {
            LinkMaps = null;
        }


        [ProtoMember(10)]
        public int TypeID { get; set; }

        [ProtoMember(11)]
        public string LoaderID { get; set; }

        [ProtoMember(12)]
        public HashSet<TID> MarkedAsDestroyed
        {
            get; 
            set;
        }

        public bool HasLinks()
        {
            return Links != null && Links.Count > 0;
        }

        public bool HasOutboundDependencies()
        {
            return OutboundDependencies != null && OutboundDependencies.Count > 0;
        }

        public bool HasInboundDependencies()
        {
            return InboundDependencies != null && InboundDependencies.Count > 0;
        }

        public bool HasMarkAsDestroyed()
        {
            return MarkedAsDestroyed != null && MarkedAsDestroyed.Count > 0;
        }

        public Meta(Meta<TID, TFID> other)
        {
            ID = other.ID;
            Name = other.Name;
            TypeID = other.TypeID;
            OutboundDependencies = other.OutboundDependencies;
            InboundDependencies = other.InboundDependencies;
            ThumbnailFileID = other.ThumbnailFileID;
            DataFileID = other.DataFileID;
            FileID = other.FileID;
            Links = other.Links;
            LinkMaps = other.LinkMaps;
            LoaderID = other.LoaderID;
            MarkedAsDestroyed = other.MarkedAsDestroyed;
        }
    }
}
