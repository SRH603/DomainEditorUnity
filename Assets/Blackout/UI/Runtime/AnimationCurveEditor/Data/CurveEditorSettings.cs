using System.Collections.Generic;
using UnityEngine;

namespace Blackout.UI
{
    /// <summary>
    /// Contains all the visual data of the curve editor in one place
    /// </summary>
    [CreateAssetMenu(fileName = "CurveEditorSettings", menuName = "Blackout/Curve Editor Settings")]
    public class CurveEditorSettings : ScriptableObject
    {
        [Header("Keyframes")]
        [SerializeField, Tooltip("The color of a deselected keyframe node")]
        public Color keyframeDeselectedColor = new Color(0.4535961f, 1, 0, 1);
        
        [SerializeField, Tooltip("The color of a selected keyframe node")]
        public Color keyframeSelectedColor = new Color(1, 1, 1, 1);
        
        [Header("Grid Render")]
        [SerializeField, Tooltip("The color of the main lines of the grid. These are the lines located on whole numbers")]
        public Color gridPrimaryColor = new Color(1f, 1f, 1f, 0.5f);
        
        [SerializeField, Tooltip("The color of the secondary lines of the grid")]
        public Color gridSecondaryColor = new Color(1f, 1f, 1f, 0.25f);

        [SerializeField, Tooltip("The pixel thickness of the rendered grid")]
        public float gridLineThickness = 2f;
        
        [SerializeField, Tooltip("The number of pixels per grid cell")]
        public int gridPixelsPerCell = 50;

        
        [Header("Curve Render")]
        [SerializeField, Tooltip("The color of the rendered curve")]
        public Color curveColor = new Color(0.4535961f, 1, 0, 1);
        
        [SerializeField, Tooltip("The pixel thickness of the rendered curve")]
        public float curveThickness = 2f;

        [SerializeField, Tooltip("The thickness in pixels around the line that are determined to be valid clicks")]
        public float curveClickThickness = 10f;
        
        [SerializeField, Tooltip("The thickness in pixels around the line that are determined to be valid clicks")]
        public float curveDoubleClickTime = 0.3f;
        
        
        [Header("Keyframe Editor Popup")]
        [SerializeField, Tooltip("The hover color of the buttons in the keyframe editor popup")]
        public Color keyframeEditorButtonColor = new Color(0f, 0.57f, 1f, 0.45f);
        
        [SerializeField, Tooltip("The fade duration of the buttons in the keyframe editor popup")]
        public float keyframeEditorButtonFadeDuration = 0.05f;
        
        
        [Header("Presets")]
        [SerializeField, Tooltip("The default curve presets that are loaded when the editor is opened")]
        public List<AnimationCurve> curvePresets = new List<AnimationCurve>()
        {
            AnimationCurve.Linear(0f, 1f, 1f, 1f),
            AnimationCurve.Linear(0f, 0f, 1f, 1f),
            new AnimationCurve(new Keyframe[2]
            {
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(1f, 1f, 2f, 2f)
            }),
            new AnimationCurve(new Keyframe[2]
            {
                new Keyframe(0f, 0f, 2f, 2f),
                new Keyframe(1f, 1f, 0f, 0f)
            }),
            new AnimationCurve(new Keyframe[2]
            {
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(1f, 1f, 0f, 0f)
            })
        };
    }
}
