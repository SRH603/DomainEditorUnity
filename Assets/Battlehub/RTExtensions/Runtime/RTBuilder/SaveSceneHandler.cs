using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using Battlehub.RTEditor.Models;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battlehub.RTBuilder
{
    public class SaveSceneHandler : EditorExtension
    {
        private IRuntimeEditor m_editor;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            m_editor = IOC.Resolve<IRuntimeEditor>();
            if (m_editor.CompatibilityMode != CompatibilityMode.None)
            {
                if (m_editor != null && m_editor.CompatibilityMode != CompatibilityMode.LegacyRTSL)
                {
                    m_editor.BeforeCreateAsset += OnBeforeCreateAsset;
                }
                else
                {
                    SubscribeLegacy();
                }
            }
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            if (m_editor.CompatibilityMode != CompatibilityMode.None)
            {
                if (m_editor != null && m_editor.CompatibilityMode != CompatibilityMode.LegacyRTSL)
                {
                    m_editor.BeforeCreateAsset -= OnBeforeCreateAsset;
                }
                else
                {
                    UnsubscribeLegacy();
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (m_editor != null && m_editor.CompatibilityMode != CompatibilityMode.None)
            {
                if (m_editor != null && m_editor.CompatibilityMode != CompatibilityMode.LegacyRTSL)
                {
                    m_editor.BeforeCreateAsset -= OnBeforeCreateAsset;
                }
                else
                {
                    UnsubscribeLegacy();
                }
            }
        }

        private void OnBeforeCreateAsset(object sender, BeforeCreateAssetEventArgs e)
        {
            if (ReferenceEquals(e.Object, m_editor.CurrentScene))
            {
                DontSavePBMeshes();
            }
            else if (e.Object is GameObject)
            {
                DontSavePBMeshes((GameObject)e.Object);
            }
        }

        private static void DontSavePBMeshes(GameObject root = null)
        {
            IEnumerable<PBMesh> pbMeshes = root == null ?
                Resources.FindObjectsOfTypeAll<PBMesh>() :
                root.GetComponentsInChildren<PBMesh>(true);

            pbMeshes = pbMeshes.Where(mesh => !mesh.gameObject.IsPrefab());

            foreach (PBMesh pbMesh in pbMeshes)
            {
                MeshFilter filter = pbMesh.GetComponent<MeshFilter>();
                if (filter != null)
                {
                    //Do not save probuilderized meshes
                    filter.sharedMesh.hideFlags = HideFlags.DontSave;
                }
            }
        }

        #region Legacy

        private void SubscribeLegacy()
        {
#pragma warning disable CS0612 // Type or member is obsolete
             m_project = IOC.Resolve<RTSL.Interface.IProjectAsync>();
#pragma warning restore CS0612 // Type or member is obsolete
            if (m_project != null)
            {
                m_project.Events.BeginSave += BeginSave;
            }
        }

        private void UnsubscribeLegacy()
        {
            if (m_project != null)
            {
                m_project.Events.BeginSave -= BeginSave;
            }
        }


        private RTSL.Interface.IProjectAsync m_project;
    
        private void BeginSave(object sender, RTSL.Interface.ProjectEventArgs<object[]> e)
        {
            object[] result = e.Payload;
            if (result != null && result.Length > 0 && result[0] is Scene)
            {
                DontSavePBMeshes();
            }
        }

        #endregion
    }
}
