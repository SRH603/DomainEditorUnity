using UnityEngine;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Creates and positions using legacy UI text objects in the correct place to line up with the grid
    /// </summary>
    public class LegacyCurveGridMarkers : CurveGridMarkers<Text>
    {
        #region Serialized Fields
        
        [SerializeField, Tooltip("The text template to use for the value markers.")]
        private Text template;
        
        #endregion
        
        #region Properties
        
        public Text Template
        {
            get => template;
            set => template = value;
        }
        
        #endregion

        protected override Text Instantiate()
            => Instantiate(template, template.transform.parent);

        protected override void SetText(Text t, string s)
            => t.text = s;
    }
}