using UnityEngine;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Controller for the keyframe editor popup
    /// </summary>
    public class KeyframeEditorPopup : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        public AnimationCurveEditor editor;
        
        [Header("Curve Buttons")]
        [SerializeField]
        private TangentMenuButton deleteButton;

        [SerializeField]
        private TangentMenuButton editButton;
        
        [SerializeField]
        private TangentToggleButton clampedAutoToggle;
        
        [SerializeField]
        private TangentToggleButton autoToggle;

        [SerializeField]
        private TangentToggleButton flatToggle;
        
        [SerializeField]
        private TangentToggleButton freeSmoothToggle;
        
        [SerializeField]
        private TangentToggleButton brokenToggle;

        [SerializeField]
        private TangentFoldoutButton leftTangentButton;
        
        [SerializeField]
        private TangentFoldoutButton rightTangentButton;
        
        [SerializeField]
        private TangentFoldoutButton bothTangentButton;

        [Header("Tangent Sub Menu")]
        [SerializeField]
        private GameObject tangentMenu;

        [SerializeField]
        private TangentToggleButton tangentFreeToggle;
        
        [SerializeField]
        private TangentToggleButton tangentLinearToggle;
        
        [SerializeField]
        private TangentToggleButton tangentConstantToggle;
        
        [SerializeField]
        private TangentToggleButton tangentWeightedToggle;
        #endregion
        
        #region Private Fields

        private Graphic _deleteText;
        private CurveKeyframe _keyframe;

        private CurveTangent.Side _tangentMenuSide = CurveTangent.Side.None;
        
        #endregion
        
        #region Properties

        public TangentMenuButton DeleteButton
        {
            get => deleteButton;
            set => deleteButton = value;
        }
        
        public TangentMenuButton EditButton
        {
            get => editButton;
            set => editButton = value;
        }
        
        public TangentToggleButton ClampedAutoToggle
        {
            get => clampedAutoToggle;
            set => clampedAutoToggle = value;
        }
        
        public TangentToggleButton AutoToggle
        {
            get => autoToggle;
            set => autoToggle = value;
        }
        
        public TangentToggleButton FlatToggle
        {
            get => flatToggle;
            set => flatToggle = value;
        }
        
        public TangentToggleButton FreeSmoothToggle
        {
            get => freeSmoothToggle;
            set => freeSmoothToggle = value;
        }
        
        public TangentToggleButton BrokenToggle
        {
            get => brokenToggle;
            set => brokenToggle = value;
        }
        
        public TangentFoldoutButton LeftTangentButton
        {
            get => leftTangentButton;
            set => leftTangentButton = value;
        }
        
        public TangentFoldoutButton RightTangentButton
        {
            get => rightTangentButton;
            set => rightTangentButton = value;
        }
        
        public TangentFoldoutButton BothTangentButton
        {
            get => bothTangentButton;
            set => bothTangentButton = value;
        }
        
        public GameObject TangentMenu
        {
            get => tangentMenu;
            set => tangentMenu = value;
        }
        
        public TangentToggleButton TangentFreeToggle
        {
            get => tangentFreeToggle;
            set => tangentFreeToggle = value;
        }
        
        public TangentToggleButton TangentLinearToggle
        {
            get => tangentLinearToggle;
            set => tangentLinearToggle = value;
        }
        
        public TangentToggleButton TangentConstantToggle
        {
            get => tangentConstantToggle;
            set => tangentConstantToggle = value;
        }
        
        public TangentToggleButton TangentWeightedToggle
        {
            get => tangentWeightedToggle;
            set => tangentWeightedToggle = value;
        }
        
        #endregion

        private void Awake()
        {
            _deleteText = deleteButton.transform.GetChild(0).GetComponent<Graphic>();

            SetupEventCallbacks();
            
            tangentMenu.SetActive(true);
        }
        
        private void Start()
        {
            tangentMenu.SetActive(false);
        }

        private void OnDisable()
        {
            clampedAutoToggle.SetIsOnWithoutNotify(false);
            autoToggle.SetIsOnWithoutNotify(false);
            flatToggle.SetIsOnWithoutNotify(false);
            freeSmoothToggle.SetIsOnWithoutNotify(false);
            brokenToggle.SetIsOnWithoutNotify(false);
            
            tangentFreeToggle.SetIsOnWithoutNotify(false);
            tangentLinearToggle.SetIsOnWithoutNotify(false);
            tangentConstantToggle.SetIsOnWithoutNotify(false);
            tangentWeightedToggle.SetIsOnWithoutNotify(false);
            
            tangentMenu.SetActive(false);
            _tangentMenuSide = CurveTangent.Side.None;
        }
        
        private void SetupEventCallbacks()
        {
            deleteButton.OnClick.AddListener(OnDeleteKeyframe);
            editButton.OnClick.AddListener(OnEditKeyframe);
            
            clampedAutoToggle.OnValueChanged.AddListener(OnClampedAutoToggle);
            autoToggle.OnValueChanged.AddListener(OnAutoToggle);
            flatToggle.OnValueChanged.AddListener(OnFlattenToggle);
            freeSmoothToggle.OnValueChanged.AddListener(OnFreeSmoothToggle);
            brokenToggle.OnValueChanged.AddListener(OnBrokenToggle);
            
            leftTangentButton.OnClick.AddListener(() => OpenTangentMenu((RectTransform)leftTangentButton.transform, CurveTangent.Side.Left));
            rightTangentButton.OnClick.AddListener(() => OpenTangentMenu((RectTransform)rightTangentButton.transform, CurveTangent.Side.Right));
            bothTangentButton.OnClick.AddListener(() => OpenTangentMenu((RectTransform)bothTangentButton.transform, CurveTangent.Side.Both));
            
            tangentFreeToggle.OnValueChanged.AddListener(OnToggleTangentsFree);
            tangentLinearToggle.OnValueChanged.AddListener(OnToggleTangentsLinear);
            tangentConstantToggle.OnValueChanged.AddListener(OnToggleTangentsConstant);
            tangentWeightedToggle.OnValueChanged.AddListener(OnToggleTangentsWeighted);
        }

        public void SetKeyframe(CurveKeyframe keyframe)
        {
            _keyframe = keyframe;
            
            bool canDelete = editor.KeyframeCount > 2;
            
            // If there are only 2 keyframes, disable the delete button
            deleteButton.Interactable = canDelete;
            
            // Fade the delete buttons text
            _deleteText.CrossFadeAlpha(canDelete ? 1f : 0.5f, 0f, true);

            TangentMode left, right;
            TangentUtility.DecodeTangents(_keyframe.Data.tangentMode, out left, out right);
           
            if (left == TangentMode.ClampedAuto && right == TangentMode.ClampedAuto)
                clampedAutoToggle.SetIsOnWithoutNotify(true);
            
            else if (left == TangentMode.Auto && right == TangentMode.Auto)
                autoToggle.SetIsOnWithoutNotify(true);
            
            else if (keyframe.Data.inTangent == 0f && keyframe.Data.outTangent == 0f)
                flatToggle.SetIsOnWithoutNotify(true);
            
            else if (left == TangentMode.Free && right == TangentMode.Free && !keyframe.BrokenTangents)
                freeSmoothToggle.SetIsOnWithoutNotify(true);
            
            brokenToggle.SetIsOnWithoutNotify(keyframe.BrokenTangents);
        }
        
        #region Menu Buttons
        private void OnDeleteKeyframe()
        {
            if (editor.KeyframeCount <= 2)
                return;
            
            editor.DeleteKeyframe(_keyframe);
            gameObject.SetActive(false);
        }
        
        private void OnEditKeyframe()
        {
            editor.ToggleKeyframeValuesPopup(true);
            gameObject.SetActive(false);
        }
        
        private void OnClampedAutoToggle(bool v)
        {
            _keyframe.BrokenTangents = false;
            
            _keyframe.SetBothTangents(TangentMode.ClampedAuto);
            
            editor.RecordState(true);

            gameObject.SetActive(false);
        }
        
        private void OnAutoToggle(bool v)
        {
            _keyframe.BrokenTangents = false;
            
            _keyframe.SetBothTangents(TangentMode.Auto);
            
            editor.RecordState(true);

            gameObject.SetActive(false);
        }

        private void OnFlattenToggle(bool v)
        {
            _keyframe.BrokenTangents = false;
            
            _keyframe.SetFlattenTangents();
            
            editor.RecordState(true);

            gameObject.SetActive(false);
        }
        
        private void OnFreeSmoothToggle(bool v)
        {
            _keyframe.BrokenTangents = false;
            
            _keyframe.SetBothTangents(TangentMode.Free);
            
            editor.RecordState(true);

            gameObject.SetActive(false);
        }
        
        private void OnBrokenToggle(bool v)
        {
            _keyframe.SetBrokenTangents(v);
            
            editor.RecordState(true);

            gameObject.SetActive(false);
        }
        #endregion
        
        #region Tangent Sub Menu

        public void OpenTangentMenu(RectTransform rectTransform, CurveTangent.Side side)
        {
            if (_tangentMenuSide == side)
                return;
            
            _tangentMenuSide = side;

            TangentMode left, right;
            TangentUtility.DecodeTangents(_keyframe.Data.tangentMode, out left, out right);

            tangentFreeToggle.SetIsOnWithoutNotify(_keyframe.BrokenTangents && 
                                                   (side == CurveTangent.Side.Left ? left == TangentMode.Free :
                                                   side == CurveTangent.Side.Right ? right == TangentMode.Free :
                                                   left == TangentMode.Free && right == TangentMode.Free));
            
            tangentLinearToggle.SetIsOnWithoutNotify(side == CurveTangent.Side.Left ? left == TangentMode.Linear :
                                                     side == CurveTangent.Side.Right ? right == TangentMode.Linear :
                                                     left == TangentMode.Linear && right == TangentMode.Linear);
            
            tangentConstantToggle.SetIsOnWithoutNotify(side == CurveTangent.Side.Left ? left == TangentMode.Constant :
                                                       side == CurveTangent.Side.Right ? right == TangentMode.Constant :
                                                       left == TangentMode.Constant && right == TangentMode.Constant);
            
            tangentWeightedToggle.SetIsOnWithoutNotify(side == CurveTangent.Side.Left ? _keyframe.LeftTangent.Weighted :
                                                       side == CurveTangent.Side.Right ? _keyframe.RightTangent.Weighted :
                                                       _keyframe.LeftTangent.Weighted && _keyframe.RightTangent.Weighted);
            
            tangentMenu.transform.SetParent(rectTransform);
            tangentMenu.transform.position = rectTransform.TransformPoint(rectTransform.rect.width * 0.5f, 0f, 0f);
            tangentMenu.SetActive(true);
        }

        public void CloseTangentMenu(CurveTangent.Side side)
        {
            if (_tangentMenuSide != side)
                return;
            
            tangentMenu.SetActive(false);
        }

        private void OnToggleTangentsFree(bool v)
        {
            _keyframe.BrokenTangents = true;
            
            if (_tangentMenuSide == CurveTangent.Side.Both)
                _keyframe.SetBothTangents(TangentMode.Free);
            else _keyframe.SetSideTangent(_tangentMenuSide == CurveTangent.Side.Left, TangentMode.Free);
           
            editor.RecordState(true);

            gameObject.SetActive(false);
        }

        private void OnToggleTangentsLinear(bool v)
        {
            _keyframe.BrokenTangents = true;
            
            if (_tangentMenuSide == CurveTangent.Side.Both)
                _keyframe.SetBothTangents(TangentMode.Linear);
            else _keyframe.SetSideTangent(_tangentMenuSide == CurveTangent.Side.Left, TangentMode.Linear);
            
            editor.RecordState(true);
            
            gameObject.SetActive(false);
        }

        private void OnToggleTangentsConstant(bool v)
        {
            _keyframe.BrokenTangents = true;
            
            if (_tangentMenuSide == CurveTangent.Side.Both)
                _keyframe.SetBothTangents(TangentMode.Constant);
            else _keyframe.SetSideTangent(_tangentMenuSide == CurveTangent.Side.Left, TangentMode.Constant);
            
            editor.RecordState(true);
            
            gameObject.SetActive(false);
        }
        
        private void OnToggleTangentsWeighted(bool v)
        {
            if (_tangentMenuSide == CurveTangent.Side.Left || _tangentMenuSide == CurveTangent.Side.Both)
                _keyframe.LeftTangent.Weighted = v;
            
            if (_tangentMenuSide == CurveTangent.Side.Right || _tangentMenuSide == CurveTangent.Side.Both)
                _keyframe.RightTangent.Weighted = v;
            
            editor.RecordState(true);
            
            gameObject.SetActive(false);
        }
        #endregion
        
    }
}