using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Custom toggle used in the tangent menu
    /// </summary>
    public class TangentToggleButton : TangentHoverColorTransition, IPointerClickHandler, IPointerDownHandler
    {
        #region Serialized Fields
        
        [Tooltip("Is the toggle currently on or off?")]
        [SerializeField]
        private bool isOn;
        
        [SerializeField]
        private Graphic graphicBox;  
        
        [SerializeField]
        private Graphic graphicCheckmark;  
        
        [SerializeField]
        private ToggleEvent onValueChanged = new ToggleEvent();

        #endregion
        
        #region Properties
        
        public bool IsOn
        {
            get => isOn;
            set => Set(value);
        }
        
        public ToggleEvent OnValueChanged
        {
            get => onValueChanged;
            set => onValueChanged = value;
        }
        
        public Graphic GraphicBox
        {
            get => graphicBox;
            set => graphicBox = value;
        }
        
        public Graphic GraphicCheckmark
        {
            get => graphicCheckmark;
            set => graphicCheckmark = value;
        }
        
        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
            graphicBox.canvasRenderer.SetAlpha(isOn ? 1f : 0f);
            graphicCheckmark.canvasRenderer.SetAlpha(isOn ? 1f : 0f);
        }

        public void SetIsOnWithoutNotify(bool value) 
            => Set(value, false);

        private void Set(bool value, bool sendCallback = true)
        {
            if (isOn == value)
                return;

            isOn = value;

            PlayEffect();
            if (sendCallback)
            {
                UISystemProfilerApi.AddMarker("TangentToggleButton.IsOn", this);
                onValueChanged.Invoke(isOn);
            }
        }
       
        /// <summary>
        /// Play the appropriate effect.
        /// </summary>
        private void PlayEffect()
        {
            if (graphicBox)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    graphicBox.canvasRenderer.SetAlpha(isOn ? 1f : 0f);
                else
                #endif
                    graphicBox.CrossFadeAlpha(isOn ? 1f : 0f, 0.1f, true);
            }
            
            if (graphicCheckmark)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                    graphicCheckmark.canvasRenderer.SetAlpha(isOn ? 1f : 0f);
                else
                #endif
                    graphicCheckmark.CrossFadeAlpha(isOn ? 1f : 0f, 0.1f, true);
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            IsOn = !isOn;
        }
        
        // Unity 2019 has a bug where OnPointerClick is not called here if OnPointerDown
        // is not implemented when the parent has a script with pointer events...
        public void OnPointerDown(PointerEventData eventData)
        {
        }
        
        [Serializable]
        public class ToggleEvent : UnityEvent<bool>
        {}
    }
}