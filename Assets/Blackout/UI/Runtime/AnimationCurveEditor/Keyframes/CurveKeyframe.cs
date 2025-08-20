using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// The class that controls manipulation of keyframes on the grid
    /// </summary>
    public class CurveKeyframe : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region Serialized Fields
        
        [SerializeField]
        private Image handle;
       
        [SerializeField]
        private CurveTangent leftTangent;
        
        [SerializeField]
        private CurveTangent rightTangent;
        
        #endregion
        
        #region Private Fields

        [NonSerialized]
        public AnimationCurveEditor editor;
        
        private RectTransform _rectTransform;
       
        private KeyframeData _data;
        public int _keyframeIdx;
        private bool _brokenTangents;
        
        #endregion
        
        #region Properties
        
        public Keyframe Keyframe
        {
            get => _data;
            set
            {
                _data = value;
                SetPositionFromKeyframeData();
            }
        }
        
        public KeyframeData Data
        {
            get => _data;
            set
            {
                _data = value;
                SetPositionFromKeyframeData();
            }
        }
        
        public int KeyframeIndex
        {
            get => _keyframeIdx;
            set => _keyframeIdx = value;
        }

        public float Time
        {
            get => _data.time;
            set
            {
                _data.time = value;
                SetPositionFromKeyframeData();
            }
        }
        
        public float Value
        {
            get => _data.value;
            set
            {
                _data.value = value;
                SetPositionFromKeyframeData();
            }
        }
        
        public bool BrokenTangents
        {
            get => _brokenTangents;
            set => _brokenTangents = value;
        }

        public Image Handle
        {
            get => handle;
            set => handle = value;
        }

        public CurveTangent LeftTangent
        {
            get => leftTangent;
            set => leftTangent = value;
        }
        
        public CurveTangent RightTangent
        {
            get => rightTangent;
            set => rightTangent = value;
        }

        public AnimationCurveEditor Editor
        {
            get => editor;
            set => editor = value;
        }
        
        #endregion

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        private void OnEnable()
        {
            if (!editor)
                editor = ComponentUtility.GetComponentInParent<AnimationCurveEditor>(gameObject);
            
            editor.ScrollRect.OnScaleChanged.AddListener(OnScaleChanged);
            OnScaleChanged(editor.ScrollRect.Grid.localScale);
        }

        private void OnDisable()
        {
            editor.DeselectKeyframe(this);
            
            editor.ScrollRect.OnScaleChanged.RemoveListener(OnScaleChanged);
        }

        private void OnScaleChanged(Vector2 v)
            => _rectTransform.localScale = new Vector3(1f / v.x, 1f / v.y, 1f);
            
        /// <summary>
        /// Select this keyframe
        /// </summary>
        public void Select()
        {
            handle.color = editor.Settings.keyframeSelectedColor;
            
            TangentMode left, right;
            TangentUtility.DecodeTangents(_data.tangentMode, out left, out right);
            
            leftTangent.gameObject.SetActive(_keyframeIdx > 0 && left != TangentMode.Constant && left != TangentMode.Linear);
            rightTangent.gameObject.SetActive(_keyframeIdx < editor.KeyframeCount - 1 && right != TangentMode.Constant && right != TangentMode.Linear);
        }
        
        /// <summary>
        /// Deselect this keyframe
        /// </summary>
        public void Deselect()
        {
            handle.color = editor.Settings.keyframeDeselectedColor;
            leftTangent.gameObject.SetActive(false);
            rightTangent.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Updates keyframe data and surrounding tangents and sends the data back to the editor
        /// </summary>
        public void FinalizeUpdate()
        {
            UpdateSurroundingTangents(this);
            editor.UpdateCurve();
        }
        
        #region Keyframes
        /// <summary>
        /// Update the keyframe and tangents position in the grid from the keyframe data.
        /// </summary>
        private void SetPositionFromKeyframeData() 
        {
            float gridSize = editor.Settings.gridPixelsPerCell * 10f;
            float position = -0.5f * gridSize;

            _rectTransform.localPosition = new Vector3(
                position + (_data.time * gridSize),
                position + (_data.value * gridSize), 0f);
            
            leftTangent.transform.localRotation = TangentUtility.TangentToRotation(_data.inTangent);
            rightTangent.transform.localRotation = TangentUtility.TangentToRotation(_data.outTangent);
            
            BrokenTangents = _data.inTangent != _data.outTangent;
            
            leftTangent.Weighted = _data.weightedMode == WeightedMode.In || _data.weightedMode == WeightedMode.Both;
            leftTangent.Weight = _data.inWeight;
            
            rightTangent.Weighted = _data.weightedMode == WeightedMode.Out || _data.weightedMode == WeightedMode.Both;
            rightTangent.Weight = _data.outWeight;
        }
        
        /// <summary>
        /// Update the keyframe data from the position of the keyframe and tangents.
        /// </summary>
        private void UpdateKeyframeData()
        {
            float gridSize = editor.Settings.gridPixelsPerCell * 10f;
            float position = -0.5f * gridSize;
            float oneOver = 1f / gridSize;

            Vector3 localPosition = transform.localPosition;
            float time = (localPosition.x - position) * oneOver;
            float value = (localPosition.y - position) * oneOver;

            if (editor.ClampCurve)
            {
                float clampedTime = Mathf.Clamp(time, _keyframeIdx == 0 ? 0f : 0.01f, _keyframeIdx == editor.KeyframeCount - 1 ? 1f : 0.99f);
                float clampedValue = Mathf.Clamp01(value);
                
                if (clampedTime != time)
                {
                    localPosition.x = position + (_data.time * gridSize);
                    time = clampedTime;
                }

                if (clampedValue != value)
                {
                    localPosition.y = position + (_data.value * gridSize);
                    value = clampedValue;
                }
            }
            
            transform.localPosition = localPosition;
            
            _data.time = time;
            _data.value = value;
        }
        #endregion

        #region Tangents
        /// <summary>
        /// Marks the tangents as broken so they can be moved individually
        /// </summary>
        /// <param name="v"></param>
        public void SetBrokenTangents(bool v)
        {
            BrokenTangents = v;

            TangentMode left, right;
            TangentUtility.DecodeTangents(_data.tangentMode, out left, out right);
            
            if (left == TangentMode.ClampedAuto || left == TangentMode.Auto)
                left = TangentMode.Free;
            
            if (right == TangentMode.ClampedAuto || right == TangentMode.Auto)
                right = TangentMode.Free;

            _data.tangentMode = TangentUtility.EncodeTangents(left, right);
        }
        
        /// <summary>
        /// Flatten the tangents
        /// </summary>
        public void SetFlattenTangents()
        {
            BrokenTangents = false;
            
            _data.tangentMode = TangentUtility.EncodeTangents(TangentMode.Free);
            
            leftTangent.transform.localRotation = Quaternion.identity;
            rightTangent.transform.localRotation = Quaternion.identity;

            leftTangent.Weight = rightTangent.Weight = 1f / 3f;
            
            FinalizeUpdate();
        }

        /// <summary>
        /// Set tangent mode for both sides and update accordingly
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="sendUpdate"></param>
        public void SetBothTangents(TangentMode mode, bool sendUpdate = true)
        {
            _data.tangentMode = TangentUtility.EncodeTangents(mode);

            if (mode == TangentMode.Free)
            {
                /*if (_keyframeIdx == 0 || _keyframeIdx == editor.KeyframeCount - 1)
                    return;*/
                
                UpdateTangentsAuto();
            }
            
            if (sendUpdate)
                FinalizeUpdate();
        }

        /// <summary>
        /// Set the tangent mode for one side and update accordingly
        /// </summary>
        /// <param name="isLeft"></param>
        /// <param name="mode"></param>
        public void SetSideTangent(bool isLeft, TangentMode mode)
        {
            TangentMode left, right;
            TangentUtility.DecodeTangents(_data.tangentMode, out left, out right);

            if (isLeft)
                left = mode;
            else right = mode;
            
            _data.tangentMode = TangentUtility.EncodeTangents(left, right);

            if (mode == TangentMode.Free)
            {
                CurveKeyframe otherKeyframe = isLeft ? editor.GetKeyframe(_keyframeIdx - 1) : editor.GetKeyframe(_keyframeIdx + 1);
                if (otherKeyframe)
                    AimTangentTowardsKeyframe(otherKeyframe);
            }
            
            FinalizeUpdate();
        }
        #endregion
        
        #region Tangent Manipulation
        /// <summary>
        /// Updates tangent handles and data based on the tangent mode
        /// </summary>
        private void UpdateTangents()
        {
            TangentMode left, right;
            TangentUtility.DecodeTangents(_data.tangentMode, out left, out right);

            // Update tangent handle rotations based on their tangent mode
            if (left == TangentMode.ClampedAuto || right == TangentMode.ClampedAuto)
                UpdateTangentsClampedAuto();
            
            if (left == TangentMode.Auto || right == TangentMode.Auto)
                UpdateTangentsAuto();
            
            if (left == TangentMode.Linear || right == TangentMode.Linear)
                UpdateTangentsLinear(left, right);

            if (left == TangentMode.Constant || right == TangentMode.Constant)
                UpdateTangentsConstant(left, right);
            
            // Update tangent data from the tangent handles
            float tangent = TangentUtility.DegreesToTangent(leftTangent.transform.eulerAngles.z);
            _data.inTangent = tangent > 500 ? Mathf.Infinity : tangent < -500 ? -Mathf.Infinity : tangent;
            _data.inWeight = tangent > 500 || tangent < -500 ? 0 : (leftTangent.Weighted ? leftTangent.Weight : 0f);
            
            tangent = TangentUtility.DegreesToTangent(rightTangent.transform.eulerAngles.z);
            _data.outTangent = tangent > 500 ? Mathf.Infinity : tangent < -500 ? -Mathf.Infinity : tangent;
            _data.outWeight = tangent > 500 || tangent < -500 ? 0 : (rightTangent.Weighted ? rightTangent.Weight : 0f);
            
            _data.weightedMode = (WeightedMode)(leftTangent.Weighted ? 1 : 0) + (rightTangent.Weighted ? 2 : 0);
            
            if (editor.IsSelectedKeyframe(this))
            {
                leftTangent.gameObject.SetActive(_keyframeIdx > 0 && left != TangentMode.Constant && left != TangentMode.Linear);
                rightTangent.gameObject.SetActive(_keyframeIdx < editor.KeyframeCount - 1 && right != TangentMode.Constant && right != TangentMode.Linear);
            }
        }

        /// <summary>
        /// Update tangents in auto mode
        /// </summary>
        private void UpdateTangentsClampedAuto()
        {
            CurveKeyframe previous = editor.GetKeyframe(KeyframeIndex - 1);
            CurveKeyframe next = editor.GetKeyframe(KeyframeIndex + 1);

            if (!previous)
            {
                AimTangentTowardsKeyframe(next);
                return;
            }
    
            if (!next)
            {
                AimTangentTowardsKeyframe(previous);
                return;
            }

            float dx1 = Time - previous.Time;
            float dy1 = Value - previous.Value;
            float dx2 = next.Time - Time;
            float dy2 = next.Value - Value;

            float tangent = 0f;
            if (Mathf.Abs(dx1) > Mathf.Epsilon && Mathf.Abs(dx2) > Mathf.Epsilon)
            {
                float slope1 = dy1 / dx1;
                float slope2 = dy2 / dx2;

                // Compute a weighted average that favors a more horizontal slope
                float averageSlope = (slope1 + slope2) / 2f;
                float weight = Mathf.Max(1f - Mathf.Abs(averageSlope), 0f);
                tangent = weight * averageSlope;
            }

            // Clamping logic to prevent overshooting
            float minTangent = Mathf.Min(dy1 / dx1, dy2 / dx2);
            float maxTangent = Mathf.Max(dy1 / dx1, dy2 / dx2);
            tangent = Mathf.Clamp(tangent, minTangent, maxTangent);

            Quaternion rotation = TangentUtility.TangentToRotation(tangent);

            leftTangent.transform.rotation = rotation;
            rightTangent.transform.rotation = rotation;
        }

        /// <summary>
        /// Update tangents in auto mode
        /// </summary>
        private void UpdateTangentsAuto()
        {
            CurveKeyframe previous = editor.GetKeyframe(KeyframeIndex - 1);
            CurveKeyframe next = editor.GetKeyframe(KeyframeIndex + 1);

            if (!previous)
            {
                AimTangentTowardsKeyframe(next);
                return;
            }
            
            if (!next)
            {
                AimTangentTowardsKeyframe(previous);
                return;
            }
            
            float tangent = 0f;
                
            float dx1 = Time - previous.Time;
            float dy1 = Value - previous.Value;
            float dx2 = next.Time - Time;
            float dy2 = next.Value - Value;
            
            if (Mathf.Abs(dx1) > Mathf.Epsilon && Mathf.Abs(dx2) > Mathf.Epsilon)
            {
                float slope1 = dy1 / dx1;
                float slope2 = dy2 / dx2;
                tangent = (slope1 + slope2) / 2f;  // Average the slopes for a smoother transition
            }

            Quaternion rotation = TangentUtility.TangentToRotation(tangent);
            
            leftTangent.transform.rotation = rotation;
            rightTangent.transform.rotation = rotation;
        }
        
        /// <summary>
        /// Update tangents in constant mode
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private void UpdateTangentsConstant(TangentMode left, TangentMode right)
        {
            if (left == TangentMode.Constant)
            {
                leftTangent.transform.rotation = Quaternion.Euler(0f, 0f, -90.001f);
                leftTangent.Weighted = false;
                leftTangent.Weight = 0f;
            }

            if (right == TangentMode.Constant)
            {
                rightTangent.transform.rotation = Quaternion.Euler(0f, 0f, -90.001f);
                rightTangent.Weighted = false;
                rightTangent.Weight = 0f;
            }
        }
        
        /// <summary>
        /// Update tangents rotation in linear mode
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private void UpdateTangentsLinear(TangentMode left, TangentMode right)
        {
            if (left == TangentMode.Linear)
            {
                CurveKeyframe previous = editor.GetKeyframe(KeyframeIndex - 1);
                if (previous)
                    AimTangentTowardsKeyframe(previous);
            }

            if (right == TangentMode.Linear)
            {
                CurveKeyframe next = editor.GetKeyframe(KeyframeIndex + 1);
                if (next)
                    AimTangentTowardsKeyframe(next);
            }
        }

        /// <summary>
        /// Point one sides tangent towards another keyframe
        /// </summary>
        /// <param name="other"></param>
        private void AimTangentTowardsKeyframe(CurveKeyframe other)
        {
            bool isPrevious = other.KeyframeIndex < _keyframeIdx;
            
            float dx = isPrevious ? Time - other.Time : other.Time - Time;
            float dy = isPrevious ? Value - other.Value : other.Value - Value;

            float tangent = 0f;
            if (Mathf.Abs(dx) > Mathf.Epsilon)
                tangent = dy / dx;

            if (isPrevious)
                leftTangent.transform.rotation = TangentUtility.TangentToRotation(tangent);
            else rightTangent.transform.rotation = TangentUtility.TangentToRotation(tangent);
        }
        
        /// <summary>
        /// Update all keyframes going outwards from this keyframe
        /// </summary>
        private void UpdateSurroundingTangents(CurveKeyframe from)
        {
            UpdateTangents();
            
            editor.UpdateKeyframe(this);
            
            CurveKeyframe previous = editor.GetKeyframe(_keyframeIdx - 1);
            CurveKeyframe next = editor.GetKeyframe(_keyframeIdx + 1);
            
            if (previous && previous != from)
                previous.UpdateSurroundingTangents(this);
            
            if (next && next != from)
                next.UpdateSurroundingTangents(this);
        }
        
        /// <summary>
        /// Set the tangent mode and rotation from the tangent handle.
        /// </summary>
        /// <param name="tangent"></param>
        /// <param name="tangentMode"></param>
        public void UpdateTangentData(CurveTangent tangent, int tangentMode)
        {
            _data.tangentMode = tangentMode;

            if (!_brokenTangents)
            {
                // If tangents are not broken, set the other tangent rotation to match
                CurveTangent other = tangent.side == CurveTangent.Side.Left ? rightTangent : leftTangent;
                other.transform.rotation = tangent.transform.rotation;
            }

            FinalizeUpdate();
        }
        
        #endregion

        #region Event Callbacks
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                editor.SelectKeyframe(this);
            
            else if (eventData.button == PointerEventData.InputButton.Right)
                editor.OpenKeyframeEditor(this);
        }
        
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            editor.SelectKeyframe(this);
            editor.ToggleKeyframeValuesPopup(true);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(editor.ScrollRect.Viewport, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint))
            {
                if (_rectTransform.position == worldPoint)
                    return;
                
                _rectTransform.position = worldPoint;
                
                UpdateKeyframeData();
                
                FinalizeUpdate();
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            editor.ToggleKeyframeValuesPopup(false);
            editor.RecordState(true);
        }
        #endregion
    }
}