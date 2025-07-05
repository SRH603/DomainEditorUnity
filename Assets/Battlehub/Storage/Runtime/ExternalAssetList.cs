using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Battlehub.Storage
{
    [Serializable]
    public class ExternalAssetListItem
    {
        [SerializeField, HideInInspector]
        private string name;

        [SerializeField]
        private UnityObject m_asset;

        [SerializeField]
        private string m_guid;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public UnityObject Asset
        {
            get { return m_asset; }
            set { m_asset = value; }
        }

        public Guid ID
        {
            get
            {
                if (Guid.TryParse(m_guid, out var id) && id != Guid.Empty)
                {
                    return id;
                }

                return Guid.Empty;
            }
            set
            {
                m_guid = value.ToString();
            }
        }
    }

    [CreateAssetMenu(fileName = "ExternalAssetList", menuName = "Runtime Asset Database/External Asset List")]
    public class ExternalAssetList : ScriptableObject
    {
        [SerializeField]
        private bool m_showGuids;

        [SerializeField]
        private ExternalAssetListItem[] m_items;

        public ExternalAssetListItem[] Items
        {
            get { return m_items; }
            set { m_items = value; }
        }

        private HashSet<UnityObject> m_existingAssets = new HashSet<UnityObject>();

        public void OnValidate()
        {
            ExternalAssetListItem prevItem = null;

            for (int i = 0; i < m_items.Length; ++i)
            {
                var item = m_items[i];

                if (item.ID == Guid.Empty)
                {
                    item.ID = Guid.NewGuid();
                }
                else if (prevItem != null && prevItem.ID == item.ID)
                {
                    item.ID = Guid.NewGuid();
                }

                if(item.Asset != null && !m_existingAssets.Add(item.Asset))
                {
                    item.Asset = null;
                }

                var asset = item.Asset;
                var idString = m_showGuids ? item.ID.ToString() + " -> " : string.Empty;
                item.Name = asset == null ?
                    $"{idString}None (Object)" : 
                    $"{idString}{asset.name} ({asset.GetType().Name})";

                prevItem = item;
            }

            m_existingAssets.Clear();
        }
    }
}
