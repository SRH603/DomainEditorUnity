using Battlehub.UIControls;
using TMPro;
using UnityEngine;

namespace Battlehub.RTEditor.Views
{
    [DefaultExecutionOrder(1)]
    public class EditorPresenterInputFieldBehaviour : MonoBehaviour
    {
        private VirtualizingItemContainer m_itemContainer;
        private TMP_InputField m_inputField;
     
        private void OnEnable()
        {
            m_inputField = GetComponent<TMP_InputField>();
            m_itemContainer = GetComponentInParent<VirtualizingItemContainer>();

            VirtualizingItemContainer.BeginEdit += OnBeginEdit;
            if (m_inputField != null)
            {
                m_inputField.onEndEdit.AddListener(OnEndEdit);
            }
        }

        private void OnDisable()
        {
            VirtualizingItemContainer.BeginEdit -= OnBeginEdit;
            if (m_inputField != null)
            { 
                m_inputField.onEndEdit.RemoveListener(OnEndEdit);
            }
        }

        private void OnBeginEdit(object sender, System.EventArgs e)
        {
            if (!ReferenceEquals(sender, m_itemContainer))
            {
                return;
            }

            m_inputField.ActivateInputField();
            m_inputField.Select();
        }

        private void OnEndEdit(string value)
        {
            m_itemContainer.IsEditing = false;
        }
    }
}

