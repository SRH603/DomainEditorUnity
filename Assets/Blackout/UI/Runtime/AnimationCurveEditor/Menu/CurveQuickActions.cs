using UnityEngine;
#pragma warning disable CS0618 // Type or member is obsolete

namespace Blackout.UI
{
    /// <summary>
    /// Controller for the quick action menu
    /// </summary>
    public class CurveQuickActions : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private AnimationCurveEditor editor;

        [SerializeField]
        private RectTransform rectTransform;
        
        [SerializeField]
        private TangentMenuButton normalizeButton;
        
        [SerializeField]
        private TangentMenuButton flipHorizontalButton;
        
        [SerializeField]
        private TangentMenuButton flipVerticalButton;
        
        #endregion
        
        #region Properties
        
        public AnimationCurveEditor Editor
        {
            get => editor;
            set => editor = value;
        }
        
        public RectTransform RectTransform
        {
            get => rectTransform;
            set => rectTransform = value;
        }

        public TangentMenuButton NormalizeButton
        {
            get => normalizeButton;
            set => normalizeButton = value;
        }
        
        public TangentMenuButton FlipHorizontalButton
        {
            get => flipHorizontalButton;
            set => flipHorizontalButton = value;
        }
        
        public TangentMenuButton FlipVerticalButton
        {
            get => flipVerticalButton;
            set => flipVerticalButton = value;
        }
        
        #endregion
        
        private void Awake()
        {
            normalizeButton.OnClick.AddListener(NormalizeCurve);
            flipHorizontalButton.OnClick.AddListener(FlipHorizontal);
            flipVerticalButton.OnClick.AddListener(FlipVertical);
        }

        private void OnEnable()
        {
            // Rescale this so the size is consistent
            Vector3 parentScale = rectTransform.parent.localScale;
            rectTransform.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f);
        }

        /// <summary>
        /// Normalizes the points in the curve to be between 0 and 1.
        /// </summary>
        private void NormalizeCurve()
        {
            Vector2 min, max;
            editor.GetCurveRange(out min, out max);

            Vector2 range = new Vector2(max.x - min.x, max.y - min.y);
            
            Keyframe[] keys = editor.Curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                Keyframe key = keys[i];
                key.time = (key.time - min.x) / range.x;
                key.value = (key.value - min.y) / range.y;
                keys[i] = key;
            }
            
            editor.Curve.keys = keys;
            editor.RecordState(true);
            editor.RebuildCurve();
            
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Flips the curve horizontally.
        /// </summary>
        private void FlipHorizontal()
        {
            Keyframe[] keys = editor.Curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                Keyframe key = keys[i];
                key.time = 1f - key.time;

                TangentMode left, right;
                TangentUtility.DecodeTangents(key.tangentMode, out left, out right);
                
                float tempInTangent = key.inTangent;
                
                key.inTangent = -key.outTangent;
                key.outTangent = -tempInTangent;

                key.tangentMode = TangentUtility.EncodeTangents(right, left);
                
                keys[i] = key;
            }
            
            editor.Curve.keys = keys;
            editor.RecordState(true);
            editor.RebuildCurve();
            
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Flips the curve vertically
        /// </summary>
        private void FlipVertical()
        {
            Keyframe[] keys = editor.Curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                Keyframe key = keys[i];
                
                key.value = 1f - key.value;
                
                TangentMode left, right;
                TangentUtility.DecodeTangents(key.tangentMode, out left, out right);
                
                if (left != TangentMode.Constant)
                    key.inTangent = -key.inTangent;
                
                if (right != TangentMode.Constant)
                    key.outTangent = -key.outTangent;
                
                keys[i] = key;
            }
            
            editor.Curve.keys = keys;
            editor.RecordState(true);
            editor.RebuildCurve();
            
            gameObject.SetActive(false);
        }
    }
}