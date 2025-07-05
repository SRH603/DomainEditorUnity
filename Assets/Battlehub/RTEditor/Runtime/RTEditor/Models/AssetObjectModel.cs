using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor.Models
{
    public interface IAsset
    {
        public ID ID
        {
            get;
        }
    }

    public class Asset : ScriptableObject, IAsset
    {
        public ID ID
        {
            get;
            set;
        }
    }

    public interface IAssetObjectModel
    {
        bool TryGetAsset(ID id, out Asset asset);

        bool HasAsset(ID id) { return TryGetAsset(id, out Asset _); }

        Asset GetAsset(ID id) { return TryGetAsset(id, out Asset asset) ? asset : null; }
    }

    public class AssetsObjectModel : IAssetObjectModel
    {
        private IAssetDatabaseModel m_assetDatabase;

        private readonly Dictionary<ID, Asset> m_idToAsset = new Dictionary<ID, Asset>();

        public AssetsObjectModel(IAssetDatabaseModel assetDatabase)
        {
            m_assetDatabase = assetDatabase;
            m_assetDatabase.LoadProject += OnLoadProject;
            m_assetDatabase.UnloadProject += OnUnloadProject;
            m_assetDatabase.CreateAsset += OnCreateAsset;
            m_assetDatabase.CreateFolder += OnCreateFolder;
            m_assetDatabase.DuplicateAssets += OnDuplicateAssets;
            m_assetDatabase.MoveAssets += OnMoveAssets;
            m_assetDatabase.DeleteAssets += OnDeleteAssets;
        }

        public void Dispose()
        {
            m_assetDatabase.LoadProject -= OnLoadProject;
            m_assetDatabase.UnloadProject -= OnUnloadProject;
            m_assetDatabase.CreateAsset -= OnCreateAsset;
            m_assetDatabase.CreateFolder -= OnCreateFolder;
            m_assetDatabase.DuplicateAssets -= OnDuplicateAssets;
            m_assetDatabase.MoveAssets -= OnMoveAssets;
            m_assetDatabase.DeleteAssets -= OnDeleteAssets;
            
            m_assetDatabase = null;
        }

        public bool TryGetAsset(ID id, out Asset asset)
        {
            return m_idToAsset.TryGetValue(id, out asset);
        }

        private void CreateAsset(ID id, string name)
        {
            var asset = ScriptableObject.CreateInstance<Asset>();
            asset.name = name;
            asset.ID = id;
            m_idToAsset.Add(asset.ID, asset);

            var parentID = m_assetDatabase.GetParent(asset.ID);
            if (parentID != ID.Empty && !m_idToAsset.ContainsKey(parentID))
            {
                CreateAsset(parentID, m_assetDatabase.GetName(parentID));
            }
        }

        private void DestroyAsset(ID id)
        {
            if (m_idToAsset.TryGetValue(id, out var asset))
            {
                m_idToAsset.Remove(id);
                UnityEngine.Object.Destroy(asset);
            }
        }

        private void CreateAssets(ID id)
        {
            CreateAsset(id, m_assetDatabase.GetName(id));
            var children = m_assetDatabase.GetChildren(id, sortByName: false);
            foreach (ID childID in children)
            {
                CreateAssets(childID);
            }
        }

        private void DestroyAssets()
        {
            foreach(var id in m_idToAsset.Keys)
            {
                if (m_idToAsset.TryGetValue(id, out var asset))
                {
                    UnityEngine.Object.Destroy(asset);
                }
            }
            m_idToAsset.Clear();
        }

        private void OnLoadProject(object sender, EventArgs e)
        {
            CreateAssets(m_assetDatabase.RootFolderID);
        }

        private void OnUnloadProject(object sender, EventArgs e)
        {
            DestroyAssets();
        }

        private void OnCreateAsset(object sender, CreateAssetEventArgs e)
        {
            DestroyAsset(e.OverwrittenAssetID);
            CreateAsset(e.AssetID, m_assetDatabase.GetName(e.AssetID));
        }

        private void OnCreateFolder(object sender, CreateFolderEventArgs e)
        {
            CreateAsset(e.AssetID, m_assetDatabase.GetName(e.AssetID));
        }

        private void OnDuplicateAssets(object sender, DuplicateAssetsEventArgs e)
        {
            for (int i = 0; i < e.AssetID.Count; ++i)
            {
                CreateAsset(e.AssetID[i], m_assetDatabase.GetName(e.AssetID[i]));

                var children = m_assetDatabase.GetChildren(e.AssetID[i]);

                foreach (var childID in children)
                {
                    CreateAsset(childID, m_assetDatabase.GetName(childID));
                }
            }
        }

        private void OnMoveAssets(object sender, MoveAssetsEventArgs e)
        {
            for (int i = 0; i < e.AssetID.Count; ++i)
            {
                DestroyAsset(e.AssetID[i]);

                var children = e.ChildrenID[i];
                foreach (var childID in children)
                {
                    DestroyAsset(childID);
                }

                var id = m_assetDatabase.GetAssetID(e.NewPath[i]);
                CreateAsset(id, m_assetDatabase.GetName(id));

                foreach (var childID in m_assetDatabase.GetChildren(id, sortByName:false, recursive:true))
                {
                    CreateAsset(childID, m_assetDatabase.GetName(childID));
                }
            }
        }

        private void OnDeleteAssets(object sender, DeleteAssetsEventArgs e)
        {
            for (int i = 0; i < e.AssetID.Count; ++i)
            {
                DestroyAsset(e.AssetID[i]);

                var children = e.ChildrenID[i];

                foreach (var childID in children)
                {
                    DestroyAsset(childID);
                }
            }
        }
    }
}
