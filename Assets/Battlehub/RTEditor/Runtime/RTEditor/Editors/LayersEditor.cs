using Battlehub.RTCommon;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class LayersEditor : MonoBehaviour
    {
        [SerializeField]
        private Transform m_editorsPanel = null;
        [SerializeField]
        private GameObject m_editorPrefab = null;
        private LayersInfo m_layersInfo;
        private bool m_isDirty = false;
        private IRuntimeEditor m_editor;
        private const string k_layersInfoKey = "Battlehub.RTEditor.LayersInfo";

        private void Awake()
        {
            m_editor = IOC.Resolve<IRuntimeEditor>();

            m_layersInfo = (LayersInfo)m_editor.Selection.activeObject;

            foreach (LayersInfo.Layer layer in m_layersInfo.Layers)
            {
                GameObject editor = Instantiate(m_editorPrefab, m_editorsPanel, false);

                TextMeshProUGUI text = editor.GetComponentInChildren<TextMeshProUGUI>(true);
                if (text != null)
                {
                    text.text = layer.Index + ": ";
                }

                StringEditor stringEditor = editor.GetComponentInChildren<StringEditor>(true);
                if (stringEditor != null)
                {
                    if (layer.Index <= 5)
                    {
                        TMP_InputField inputField = stringEditor.GetComponentInChildren<TMP_InputField>(true);
                        inputField.selectionColor = new Color(0, 0, 0, 0);
                        inputField.readOnly = true;
                    }

                    stringEditor.Init(layer, layer, Strong.MemberInfo((LayersInfo.Layer x) => x.Name), null, string.Empty, null, () => m_isDirty = true, null, false);
                }
            }
        }

        private void OnDestroy()
        {
            if (m_isDirty)
            {
                IRTE editor = IOC.Resolve<IRTE>();
                if (editor != null)
                {
                    EndEdit();
                }
            }
        }

        private void OnApplicationQuit()
        {
            m_isDirty = false;
        }

        private static string m_currentProject;

        private static LayersInfo m_loadedLayers;
        public static LayersInfo LoadedLayers
        {
            get { return m_loadedLayers; }
        }

        public static async void LoadLayers(Action<LayersInfo> callback)
        {
            IRTE editor = IOC.Resolve<IRTE>();
            await LoadLayersAsync(callback);
        }

        public static async void BeginEdit()
        {
            IRTE editor = IOC.Resolve<IRTE>();
            await LoadLayersAsync(loadedLayers =>
            {
                editor.Selection.activeObject = loadedLayers;
            });
        }

        private static string InitLayersInfo()
        {
            IRTE rte = IOC.Resolve<IRTE>();
            int layersMask = rte.CameraLayerSettings.RaycastMask & ~(1 << rte.CameraLayerSettings.UIBackgroundLayer);

            m_loadedLayers = ScriptableObject.CreateInstance<LayersInfo>();
            m_loadedLayers.Layers = new List<LayersInfo.Layer>
            {
                new LayersInfo.Layer("Default", 0),
                new LayersInfo.Layer("Transparent FX", 1),
                new LayersInfo.Layer("Ignore Raycast", 2),
                new LayersInfo.Layer("Water", 4),
            };

            for (int i = 8; i < 32; ++i)
            {
                if ((layersMask & (1 << i)) != 0)
                {
                    m_loadedLayers.Layers.Add(new LayersInfo.Layer(i == 10 ? "UI" : LayerMask.LayerToName(i), i));
                }
            }

            return JsonUtility.ToJson(m_loadedLayers);
        }

        private static void LoadLayersInfo(string layersInfo)
        {
            m_loadedLayers = ScriptableObject.CreateInstance<LayersInfo>();
            JsonUtility.FromJsonOverwrite(layersInfo, m_loadedLayers);

            foreach (LayersInfo.Layer layer in m_loadedLayers.Layers)
            {
                if (string.IsNullOrEmpty(layer.Name))
                {
                    layer.Name = LayerMask.LayerToName(layer.Index);
                }
            }
        }

        public static async Task LoadLayersAsync(Action<LayersInfo> callback)
        {
            var editor = IOC.Resolve<IRuntimeEditor>();
            if (!editor.IsProjectLoaded)
            {
                LoadLayersInfo(InitLayersInfo());
                callback?.Invoke(m_loadedLayers);
                return;
            }

            if (m_loadedLayers == null || editor.ProjectID != m_currentProject)
            {
                m_currentProject = editor.ProjectID;

                string layersInfo = await editor.GetValueAsync<string>(k_layersInfoKey);
                if (string.IsNullOrEmpty(layersInfo))
                {
                    layersInfo = InitLayersInfo();
                    await editor.SetValueAsync(k_layersInfoKey, layersInfo);
                }
                else
                {
                    LoadLayersInfo(layersInfo);
                }
            }
            callback?.Invoke(m_loadedLayers);
        }

        private async void EndEdit()
        {
            var editor = IOC.Resolve<IRuntimeEditor>();
            string layersInfo = JsonUtility.ToJson(m_layersInfo);
            await editor.SetValueAsync(k_layersInfoKey, layersInfo);
        }
    }
}
