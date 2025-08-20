using UnityEngine;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// The main class of the curve editor using legacy UI components. This class handles all the individual components used for editing curves
    /// </summary>
    public class LegacyAnimationCurveEditor : AnimationCurveEditor
    {
        #region Serialized Fields
        [Header("Components (Legacy)")]
        [SerializeField, Tooltip("Reference to the window title generic text component")]
        private Text windowTitle;
        
        [Header("Keyframes (Legacy)")]
        [SerializeField, Tooltip("Reference to the time generic input component in the keyframe values popup")]
        private InputField timeInput;
        
        [SerializeField, Tooltip("Reference to the value generic input component in the keyframe values popup")]
        private InputField valueInput;
        #endregion
        
        #region Properties
        public Text WindowTitle
        {
            get => windowTitle;
            set => windowTitle = value;
        }
        
        public InputField TimeInput
        {
            get => timeInput;
            set => timeInput = value;
        }
        
        public InputField ValueInput
        {
            get => valueInput;
            set => valueInput = value;
        }

        #endregion

        protected override void Start()
        {
            timeInput.onEndEdit.AddListener(SetKeyframeTime);
            valueInput.onEndEdit.AddListener(SetKeyframeValue);
            base.Start();
        }

        public override void SetWindowTitle(string s)
        {
            windowTitle.text = s;
        }
        
        protected override void UpdateKeyframeInputs(string time, string value)
        {
            timeInput.text = time;
            valueInput.text = value;
        }
    }
}