using UnityEngine;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Creates and updates the material for the shader that draws the grid
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class CurveGrid : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("Components")]
        [SerializeField]
        private CurveScrollRect scrollRect;

        [SerializeField]
        private AnimationCurveEditor editor;

        #endregion
        
        #region Private Fields
        
        private Graphic _graphic;
        private RectTransform _rectTransform;
        private Material _material;

        private static readonly int PrimaryColor = Shader.PropertyToID("_PrimaryColor");
        private static readonly int SecondaryColor = Shader.PropertyToID("_SecondaryColor");
        private static readonly int Thickness = Shader.PropertyToID("_Thickness");
        private static readonly int LocalScale = Shader.PropertyToID("_LocalScale");
        private static readonly int Cells = Shader.PropertyToID("Cells");
        private static readonly int CellUV = Shader.PropertyToID("_CellUV");

        #endregion
        
        #region Properties
        
        public CurveScrollRect ScrollRect
        {
            get => scrollRect;
            set => scrollRect = value;
        }
        
        public AnimationCurveEditor Editor
        {
            get => editor;
            set => editor = value;
        }
        
        #endregion
        
        private void Start()
        {
            if (!editor)
            {
                editor = ComponentUtility.GetComponentInParent<AnimationCurveEditor>(gameObject);
                
                if (!editor)
                {
                    enabled = false;
                    Debug.LogError("AnimationCurveGridShader requires an AnimationCurveEditor parent to function.");
                    return;
                }
            }
            
            if (!scrollRect)
            {
                scrollRect = ComponentUtility.GetComponentInParent<CurveScrollRect>(gameObject);

                if (!scrollRect)
                {
                    enabled = false;
                    Debug.LogError("AnimationCurveGridShader requires an AnimationCurveScrollRect parent to function.");
                    return;
                }
            }

            if (!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();

            if (!_graphic)
                _graphic = GetComponent<Graphic>();

            if (!_graphic.material || _graphic.material.shader.name != "Hidden/Blackout/AnimationCurveGrid")
            {
                _graphic.material = new Material(Shader.Find("Hidden/Blackout/AnimationCurveGrid"))
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            Vector2 size = _rectTransform.rect.size;
            Vector3 localScale = _rectTransform.localScale;

            _material = _graphic.material;

            _material.SetColor(PrimaryColor, editor.Settings.gridPrimaryColor);
            _material.SetColor(SecondaryColor, editor.Settings.gridSecondaryColor);

            float lineThickness = editor.Settings.gridLineThickness;
            float pixelsPerCell = editor.Settings.gridPixelsPerCell;
            
            _material.SetVector(Thickness, new Vector2((lineThickness / size.x) / localScale.x, (lineThickness / size.y) / localScale.y));
            _material.SetVector(Cells, size / pixelsPerCell);
            _material.SetVector(CellUV, new Vector2(pixelsPerCell, pixelsPerCell) / size);

            _material.SetVector(LocalScale, localScale);
        }

        private void Update()
        {
            if (!_material || !_rectTransform)
                return;

            
            Vector2 size = _rectTransform.rect.size;
            Vector3 localScale = _rectTransform.localScale;

            float lineThickness = editor.Settings.gridLineThickness;
            float pixelsPerCell = editor.Settings.gridPixelsPerCell;

            _material.SetVector(Cells, size / pixelsPerCell);
            _material.SetVector(CellUV, new Vector2(pixelsPerCell, pixelsPerCell) / size);
            _material.SetVector(Thickness, new Vector2((lineThickness / size.x) / localScale.x, (lineThickness / size.y) / localScale.y));

            _material.SetVector(LocalScale, localScale);
        }

#if UNITY_EDITOR
        private void OnValidate() => Start();
#endif
    }
}