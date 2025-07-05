using Battlehub.RTCommon;
using Battlehub.RTEditor.Models;
using Battlehub.RTHandles;
using Battlehub.RTSL.Interface;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityWeld.Binding;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTEditor.ViewModels
{
    [Binding]
    public class SceneViewModel : ViewModel
    {
        private Ray m_pointer;
        [Binding]
        public Ray Pointer
        {
            get { return m_pointer; }
            set { m_pointer = value; }
        }

        private Transform m_cameraTransform;
        [Binding]
        public Transform CameraTransform
        {
            get { return m_cameraTransform; }
            set { m_cameraTransform = value; }
        }

        private object m_dragItem;
        private GameObject m_dropTarget;
        private HashSet<Transform> m_prefabInstanceTransforms;
        private GameObject m_prefabInstance;
        protected GameObject PrefabInstance
        {
            get { return m_prefabInstance; }
            set { m_prefabInstance = value; }
        }

        private Plane m_dragPlane;
        protected Plane DragPlane
        {
            get { return m_dragPlane; }
            set { m_dragPlane = value; }
        }

        private IPlacementModel m_placement;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_placement = IOC.Resolve<IPlacementModel>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_placement = null;
        }

        #region Bound UnityEvent Handlers

        public override void OnDelete()
        {
            Editor.Delete(Editor.Selection.gameObjects);
        }

        public override void OnDuplicate()
        {
            Editor.Duplicate(Editor.Selection.gameObjects);
        }

        private bool m_isCyclicNestingDetected;
        public override async void OnExternalObjectEnter()
        {
            base.OnExternalObjectEnter();
            if (m_prefabInstance != null)
            {
                return;
            }

            object dragObject = ExternalDragObjects.FirstOrDefault();
            if (dragObject is IAsset)
            {
                IAsset asset = (IAsset)Editor.DragDrop.DragObjects[0];
                if (Editor.IsPrefab(asset.ID))
                {
                    CanDropExternalObjects = true;

                    using var b = SetBusy();
                    try
                    {
                        var result = await Editor.InstantiateAssetsAsync(new[] { asset.ID });
                        if (result.IsCyclicNestingDetected)
                        {
                            m_isCyclicNestingDetected = true;
                        }
                        else
                        { 
                            CreateDragPlane();
                            if (!GetPointOnDragPlane(out Vector3 point))
                            {
                                point = Vector3.zero;
                            }

                            m_prefabInstance = result.Instances[0];
                            m_prefabInstance.transform.position = point;
                            m_prefabInstanceTransforms = new HashSet<Transform>(m_prefabInstance.GetComponentsInChildren<Transform>(true));
                            if (!Editor.DragDrop.InProgress)
                            {
                                RecordUndo();
                                m_prefabInstance = null;
                                m_prefabInstanceTransforms = null;
                            }
                        }  
                    }
                    catch(Exception e)
                    {
                        ShowUnableToInstantiateAssets(e);
                        CanDropExternalObjects = false;
                    }
                }
                else
                {
                    if(Editor.GetType(asset.ID) == typeof(Material))
                    {
                        m_dragItem = asset;
                    }
                }
            }
            else if (dragObject is IToolCmd)
            {
                CanDropExternalObjects = true;
            }
            else
            {
                #pragma warning disable CS0612 // Type or member is obsolete
                await HandleExternalObjectEnterLegacy(dragObject);
                #pragma warning restore CS0612 // Type or member is obsolete
            }
        }

        public override void OnExternalObjectLeave()
        {
            base.OnExternalObjectLeave();

            if (!Editor.IsBusy)
            {
                m_isCyclicNestingDetected = false;
                CanDropExternalObjects = false;
            }

            if (m_prefabInstance != null)
            {
                Editor.ReleaseAsync(new[] { m_prefabInstance });

                m_prefabInstance = null;
                m_prefabInstanceTransforms = null;
            }

            m_dragItem = null;
            m_dropTarget = null;
        }

        public override void OnExternalObjectDrag()
        {
            base.OnExternalObjectDrag();
            Vector3 point;
            if (GetPointOnDragPlane(out point))
            {
                if (m_prefabInstance != null)
                {
                    m_prefabInstance.transform.position = point;

                    RaycastHit hit = Physics.RaycastAll(Pointer).Where(h => !m_prefabInstanceTransforms.Contains(h.transform)).FirstOrDefault();
                    if (hit.transform != null)
                    {
                        m_prefabInstance.transform.position = hit.point;
                    }
                }
            }

            if (m_dragItem != null)
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(Pointer, out hitInfo, float.MaxValue, Editor.CameraLayerSettings.RaycastMask))
                {
                    MeshRenderer renderer = hitInfo.collider.GetComponentInChildren<MeshRenderer>();
                    SkinnedMeshRenderer sRenderer = hitInfo.collider.GetComponentInChildren<SkinnedMeshRenderer>();

                    if (renderer != null || sRenderer != null)
                    {
                        CanDropExternalObjects = true;
                        m_dropTarget = hitInfo.transform.gameObject;
                    }
                    else
                    {
                        CanDropExternalObjects = false;
                        m_dropTarget = null;
                    }
                }
                else
                {
                    CanDropExternalObjects = false;
                    m_dropTarget = null;
                }
            }
        }

        public override void OnExternalObjectDrop()
        {
            base.OnExternalObjectDrop();
            if (m_isCyclicNestingDetected)
            {
                ShowCyclicNestingDetected();
                m_isCyclicNestingDetected = false;
                return;
            }

            if (m_prefabInstance != null)
            {                
                Editor.SetDirtyAsync(PrefabInstance.transform);

                RecordUndo();
                m_prefabInstance = null;
                m_prefabInstanceTransforms = null;
            }

            if (m_dropTarget != null)
            {
                HandleExternalAssetDrop();
                m_dropTarget = null;
                m_dragItem = null;
            }

            IToolCmd cmd = Editor.DragDrop.DragObjects.OfType<IToolCmd>().FirstOrDefault();
            if (cmd != null)
            {
                object result = cmd.Run();
                GameObject go = result as GameObject;
                if(go == null)
                {
                    ExposeToEditor exposeToEditor = result as ExposeToEditor;
                    if(exposeToEditor != null)
                    {
                        go = exposeToEditor.gameObject;
                    }
                }

                if (go != null)
                {
                    CreateDragPlane();
                    Vector3 point;
                    if (GetPointOnDragPlane(out point))
                    {
                        RaycastHit hit = Physics.RaycastAll(Pointer).FirstOrDefault();
                        if (hit.transform != null)
                        {
                            point = hit.point;
                        }

                        ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
                        go.transform.position = point + Vector3.up * exposeToEditor.Bounds.extents.y;

                        IRuntimeSelectionComponent selectionComponent = m_placement.GetSelectionComponent();
                        if (selectionComponent.CanSelect)
                        {
                            bool wasEnabled = Undo.Enabled;
                            Undo.Enabled = false;
                            Selection.activeGameObject = null;
                            Selection.activeGameObject = go;
                            Undo.Enabled = wasEnabled;
                        }
                    }
                }
            }
        }
        #endregion

        #region Methods

        private void ShowUnableToInstantiateAssets(Exception e)
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.MessageBox(Localization.GetString("ID_RTEditor_HierarchyView_UnableToLoadAssetItems", "Unable to instantiate assets"), e.Message);
            Debug.LogException(e);
        }

        private void ShowCyclicNestingDetected()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.MessageBox(
                Localization.GetString("ID_RTEditor_HierarchyView_CyclicNestingDetected", "Cyclic nesting detected"),
                Localization.GetString("ID_RTEditor_HierarchyView_CyclicNestingNotSupported", "Cyclic nesting of prefabs is not supported"));
        }

        protected void CreateDragPlane()
        {
            m_dragPlane = m_placement.GetDragPlane(CameraTransform);
        }

        protected virtual Plane GetDragPlane(IScenePivot scenePivot, Vector3 up)
        {
            return m_placement.GetDragPlane(up, scenePivot.SecondaryPivot);
        }

        protected virtual bool GetPointOnDragPlane(out Vector3 point)
        {
            return m_placement.GetPointOnDragPlane(m_dragPlane, Pointer, out point);
        }

        private async void HandleExternalAssetDrop()
        {
            var dropTarget = m_dropTarget;
            MeshRenderer renderer = dropTarget.GetComponentInChildren<MeshRenderer>();
            SkinnedMeshRenderer sRenderer = dropTarget.GetComponentInChildren<SkinnedMeshRenderer>();

            if (renderer == null && sRenderer == null)
            {
                return;
            }

            UnityObject obj;
            if (m_dragItem is IAsset)
            {
                var asset = (IAsset)m_dragItem;
                obj = await Editor.LoadAssetAsync(asset.ID) as UnityObject;
            }
            else
            {
                #pragma warning disable CS0612 // Type or member is obsolete
                obj = await LoadAssetAsyncLegacy();
                #pragma warning restore CS0612 // Type or member is obsolete
            }

            if (obj is Material)
            {
                if (renderer != null)
                {
                    Undo.BeginRecordValue(renderer, Strong.PropertyInfo((MeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                    Material[] materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; ++i)
                    {
                        materials[i] = (Material)obj;
                    }
                    renderer.sharedMaterials = materials;
                    _ = Editor.SetDirtyAsync(renderer);
                }

                if (sRenderer != null)
                {
                    Undo.BeginRecordValue(sRenderer, Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                    Material[] materials = sRenderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; ++i)
                    {
                        materials[i] = (Material)obj;
                    }
                    sRenderer.sharedMaterials = materials;
                    _ = Editor.SetDirtyAsync(sRenderer);
                }

                if (renderer != null || sRenderer != null)
                {
                    Undo.BeginRecord();
                }

                if (renderer != null)
                {
                    Undo.EndRecordValue(renderer, Strong.PropertyInfo((MeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                }

                if (sRenderer != null)
                {
                    Undo.EndRecordValue(sRenderer, Strong.PropertyInfo((SkinnedMeshRenderer x) => x.sharedMaterials, "sharedMaterials"));
                }

                if (renderer != null || sRenderer != null)
                {
                    Undo.EndRecord();
                }

                _ = Editor.SetDirtyAsync(dropTarget.transform);
            }
        }

        protected virtual void RecordUndo()
        {
            ExposeToEditor exposeToEditor = m_prefabInstance.GetComponent<ExposeToEditor>();

            Undo.BeginRecord();
            Undo.RegisterCreatedObjects(new[] { exposeToEditor });

            IRuntimeSelectionComponent selectionComponent = m_placement.GetSelectionComponent();
            if (selectionComponent.CanSelect)
            {
                Selection.activeGameObject = m_prefabInstance;
            }
            Undo.EndRecord();
        }

        #endregion

        #region Legacy

        [Obsolete]
        private async Task HandleExternalObjectEnterLegacy(object dragObject)
        {
            
            if (ProjectItem.IsAssetItem(dragObject))
            {
                var project = IOC.Resolve<RTSL.Interface.IProjectAsync>();

                ProjectItem assetItem = (ProjectItem)Editor.DragDrop.DragObjects[0];
                if (project.Utils.ToType(assetItem) == typeof(GameObject))
                {
                    CanDropExternalObjects = true;

                    Editor.IsBusy = true;
                    UnityObject[] objects;
                    try
                    {
                        objects = await project.Safe.LoadAsync(new[] { assetItem });
                    }
                    finally
                    {
                        Editor.IsBusy = false;
                    }

                    OnAssetItemLoaded(objects);
                    m_dragItem = null;
                }
                else if (project.Utils.ToType(assetItem) == typeof(Material))
                {
                    m_dragItem = assetItem;
                }
            }
        }

        [Obsolete]
        private async Task<UnityObject> LoadAssetAsyncLegacy()
        {
            ProjectItem assetItem = (ProjectItem)Editor.DragDrop.DragObjects[0];
            Editor.IsBusy = true;
            UnityObject obj;
            try
            {
                var project = IOC.Resolve<RTSL.Interface.IProjectAsync>();
                obj = (await project.Safe.LoadAsync(new[] { assetItem }))[0];
                return obj;
            }
            catch (Exception e)
            {
                IWindowManager wm = IOC.Resolve<IWindowManager>();
                if (wm != null)
                {
                    wm.MessageBox("Unable to load asset item ", e.Message);
                    Debug.LogException(e);
                }
            }
            finally
            {
                Editor.IsBusy = false;
            }

            return null;
        }

        [Obsolete]
        protected virtual void OnAssetItemLoaded(UnityObject[] objects)
        {
            GameObject prefab = objects[0] as GameObject;
            if (prefab == null)
            {
                return;
            }

            CreateDragPlane();

            bool wasPrefabEnabled = prefab.activeSelf;
            prefab.SetActive(false);

            Vector3 point;
            if (GetPointOnDragPlane(out point))
            {
                m_prefabInstance = InstantiatePrefab(prefab, point, prefab.GetComponent<Transform>().rotation);
            }
            else
            {
                m_prefabInstance = InstantiatePrefab(prefab, Vector3.zero, prefab.GetComponent<Transform>().rotation);
            }
            m_prefabInstance.hideFlags = HideFlags.None;
            Editor.AddGameObjectToHierarchy(m_prefabInstance);

            m_prefabInstanceTransforms = new HashSet<Transform>(m_prefabInstance.GetComponentsInChildren<Transform>(true));

            prefab.SetActive(wasPrefabEnabled);

            ExposeToEditor exposeToEditor = ExposePrefabInstance(m_prefabInstance);
            exposeToEditor.SetName(prefab.name);

            OnActivatePrefabInstance(m_prefabInstance);

            if (!Editor.DragDrop.InProgress)
            {
                RecordUndo();
                m_prefabInstance = null;
                m_prefabInstanceTransforms = null;
            }
        }

        [Obsolete]
        protected virtual GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Instantiate(prefab, position, rotation);
        }

        [Obsolete]
        protected virtual ExposeToEditor ExposePrefabInstance(GameObject prefabInstance)
        {
            ExposeToEditor exposeToEditor = prefabInstance.GetComponent<ExposeToEditor>();
            if (exposeToEditor == null)
            {
                exposeToEditor = prefabInstance.AddComponent<ExposeToEditor>();
            }
            return exposeToEditor;
        }

        [Obsolete]
        protected virtual void OnActivatePrefabInstance(GameObject prefabInstance)
        {
            prefabInstance.SetActive(true);
        }
  

        #endregion

    }

}
