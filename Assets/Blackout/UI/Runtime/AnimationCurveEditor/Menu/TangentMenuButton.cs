using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Blackout.UI
{
    /// <summary>
    /// Custom button used in the tangent menu
    /// </summary>
    public class TangentMenuButton : TangentHoverColorTransition, IPointerClickHandler, IPointerDownHandler
    {
        [SerializeField]
        private bool interactable = true;

        [SerializeField]
        private ButtonClickedEvent onClick = new ButtonClickedEvent();

        
        public bool Interactable
        {
            get => interactable;
            set => interactable = value;
        }
        
        public ButtonClickedEvent OnClick
        {
            get => onClick;
            set => onClick = value;
        }
        
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }
        
        // Unity 2019 has a bug where OnPointerClick is not called here if OnPointerDown
        // is not implemented when the parent has a script with pointer events...
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
        }
        
        private void Press()
        {
            if (!gameObject.activeSelf || !Interactable)
                return;

            UISystemProfilerApi.AddMarker("TangentMenuButton.OnClick", this);
            onClick.Invoke();
        }
        
        [Serializable]
        public class ButtonClickedEvent : UnityEvent {}
    }
}