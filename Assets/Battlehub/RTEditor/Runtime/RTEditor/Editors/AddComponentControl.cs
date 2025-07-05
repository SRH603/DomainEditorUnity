using Battlehub.RTCommon;
using Battlehub.UIControls;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using Battlehub.RTEditor.Models;

namespace Battlehub.RTEditor
{
    public class SelectComponentEventArgs : EventArgs
    {
        public ScriptInfo ComponentInfo
        {
            get;
            private set;
        }

        public SelectComponentEventArgs(ScriptInfo componentInfo)
        {
            ComponentInfo = componentInfo;
        }
    }

    public class AddComponentControl : MonoBehaviour
    {
        [Obsolete("Use SelectComponent")]
        public event Action<Type> ComponentSelected;

        public event EventHandler<SelectComponentEventArgs> SelectComponent;

        [SerializeField]
        private TMP_Dropdown m_dropDown = null;
        private TMP_InputField m_filter = null;
        private VirtualizingTreeView m_treeView = null;
        
        private ScriptInfo[] m_cache;
        private string m_filterText;
        private bool m_isOpened;

        private IRTE m_editor;
        private ILocalization m_lc;
        private IComponentFactoryModel m_componentFactory;

        private void Start()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_lc = IOC.Resolve<ILocalization>();
            m_componentFactory = IOC.Resolve<IComponentFactoryModel>();
        }

        private void OnDestroy()
        {
            m_editor = null;
            m_lc = null;
            m_componentFactory = null;
        }

        private void Update()
        {
            bool isOpened = m_dropDown.transform.childCount == 3;

            if(m_isOpened != isOpened)
            {
                m_isOpened = isOpened;
                if(m_isOpened)
                {
                    OnOpened();
                }
                else
                {
                    OnClosed();
                }
            }

            if(m_isOpened)
            {
                IInput input = m_editor.Input;
                if (input.GetKeyDown(KeyCode.DownArrow))
                {
                    m_treeView.Select();
                    m_treeView.IsFocused = true;
                }
                else if(input.GetKeyDown(KeyCode.Return))
                {
                    if(m_treeView.SelectedItem != null)
                    {
                        Hide();
                    }
                }
            }
        }

        private void OnOpened()
        {
            Type[] editableTypes = IOC.Resolve<IEditorsMap>().GetEditableTypes();

            m_filter = GetComponentInChildren<TMP_InputField>();
            if(m_filter != null)
            {
                m_filter.onValueChanged.AddListener(OnFilterValueChanged);
                m_filter.text = m_filterText;

                if (!m_editor.TouchInput.IsTouchSupported)
                {
                    m_filter.Select();
                }
            }

            m_treeView = GetComponentInChildren<VirtualizingTreeView>();
            m_treeView.CanDrag = false;
            m_treeView.CanReparent = false;
            m_treeView.CanReorder = false;
            m_treeView.CanSelectAll = false;
            m_treeView.CanMultiSelect = false;

            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.ItemClick += OnItemClick;
            m_cache = m_componentFactory.GetSupportedComponents().OrderBy(t => t.Name).ToArray();
            ApplyFilterInstant(m_filterText);
        }

        private void OnClosed()
        {
            if (m_filter != null)
            {
                m_filter.onValueChanged.RemoveListener(OnFilterValueChanged);
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.ItemClick -= OnItemClick;

            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ScriptInfo componentInfo = (ScriptInfo)e.Item;
            TextMeshProUGUI text = e.ItemPresenter.GetComponentInChildren<TextMeshProUGUI>(true);
            text.text = m_lc.GetString(string.Format("ID_RTEditor_CD_{0}", componentInfo.Name), componentInfo.Name);
        }

        private void OnItemClick(object sender, ItemArgs e)
        {
            StartCoroutine(CoHide());
        }

        private IEnumerator CoHide()
        {
            yield return new WaitForEndOfFrame();
            if(m_treeView.SelectedItem != null)
            {
                Hide();
            }
        }

        private void Hide()
        {
            m_dropDown.Hide();

            var componentInfo = (ScriptInfo)m_treeView.SelectedItem;

            if (ComponentSelected != null)
            {
                ComponentSelected(componentInfo.ComponentType);
            }

            if (SelectComponent != null)
            {
                SelectComponent(this, new SelectComponentEventArgs(componentInfo));
            }
        }

        private void OnFilterValueChanged(string text)
        {
            m_filterText = text;
            ApplyFilter(text);
        }

        private void ApplyFilter(string text)
        {
            if (m_coApplyFilter != null)
            {
                StopCoroutine(m_coApplyFilter);
            }
            StartCoroutine(m_coApplyFilter = CoApplyFilter(text));
        }

        private IEnumerator m_coApplyFilter;
        private IEnumerator CoApplyFilter(string filter)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            ApplyFilterInstant(filter);
        }

        private void ApplyFilterInstant(string filter)
        {
            if (m_treeView != null)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    m_treeView.Items = m_cache;
                }
                else
                {
                    m_treeView.Items = m_cache.Where(item =>  item.Name.ToLower().Contains(filter.ToLower()) || m_lc.GetString(string.Format("ID_RTEditor_CD_{0}", item.Name), item.Name).ToLower().Contains(filter.ToLower()));
                }
            }
        }
    }
}

