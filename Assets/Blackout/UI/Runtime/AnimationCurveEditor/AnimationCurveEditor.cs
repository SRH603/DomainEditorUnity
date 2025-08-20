using Blackout.Pool;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
#pragma warning disable CS0618 // Type or member is obsolete

namespace Blackout.UI
{
    /// <summary>
    /// The main class of the curve editor. This class handles all the individual components used for editing curves
    /// </summary>
    public abstract class AnimationCurveEditor : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField, Tooltip("A curve settings asset that contains all the settings for the curve editor")]
        private CurveEditorSettings settings;
        
        [Header("Curve")]
        [SerializeField, Tooltip("The curve to edit. This should be set via code when opening the curve editor")]
        private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [SerializeField, Tooltip("When enabled, each keyframes time and value components will be clamped between 0.0 and 1.0")]
        private bool clampCurve = false;
        
        [Header("Components")]
        [SerializeField]
        private CurveScrollRect scrollRect;
        
        [SerializeField]
        private CurveGrid curveGrid;
        
        [SerializeField]
        private GridCurveRenderer curveRenderer;

        [SerializeField, Tooltip("Reference to the keyframe tangent editor popup")]
        private KeyframeEditorPopup keyframeEditor;
        
        [SerializeField, Tooltip("Reference to the quick action menu popup")]
        private GameObject curveQuickActions;
        
        [SerializeField, Tooltip("Reference to the UndoHandler component")]
        private UndoHandler undoHandler;
        
        [Header("Keyframes")]
        [SerializeField, Tooltip("Reference to the keyframe template object")]
        private CurveKeyframe keyframeTemplate;

        [SerializeField, Tooltip("Reference to the keyframe values popup")]
        private GameObject keyframeValues;

        [SerializeField, Tooltip("The distance away from the keyframe marker the input and value fields will be placed")]
        private Vector2 valueInputOffset = new Vector2(60f, 60f);
        

        [Header("Presets")]
        [SerializeField, Tooltip("Reference to the preset template button")]
        private AnimationCurveButton presetTemplate;
        
        #endregion
        
        #region Private Fields

        private CurveKeyframe _selectedKeyframe;

        private CurveUndoStep _curveUndoStep;
        
        private readonly List<CurveKeyframe> _keyframes = new List<CurveKeyframe>();
        private readonly Stack<CurveKeyframe> _pooledKeyframes = new Stack<CurveKeyframe>();
        
        #endregion
        
        #region Events
        
        public event Action OnValueChanged;
        
        public event Action OnEndEdit;
        
        #endregion
        
        #region Properties
        
        public AnimationCurve Curve
        {
            get => curve;
            set => curve = value;
        }

        public UndoHandler UndoHandler
        {
            get => undoHandler;
            set => undoHandler = value;
        }

        public CurveEditorSettings Settings
        {
            get => settings;
            set => settings = value;
        }
        
        public CurveScrollRect ScrollRect
        {
            get => scrollRect;
            set => scrollRect = value;
        }
        
        public GridCurveRenderer CurveRenderer
        {
            get => curveRenderer;
            set => curveRenderer = value;
        }
        
        public CurveKeyframe KeyframeTemplate
        {
            get => keyframeTemplate;
            set => keyframeTemplate = value;
        }

        public CurveGrid Grid
        {
            get => curveGrid;
            set => curveGrid = value;
        }

        public GameObject KeyframeValuesPopup
        {
            get => keyframeValues;
            set => keyframeValues = value;
        }

        public KeyframeEditorPopup KeyframeEditor
        {
            get => keyframeEditor;
            set => keyframeEditor = value;
        }
        
        public GameObject CurveQuickActions
        {
            get => curveQuickActions;
            set => curveQuickActions = value;
        }
        
        public AnimationCurveButton PresetTemplate
        {
            get => presetTemplate;
            set => presetTemplate = value;
        }
        
        public bool ClampCurve
        {
            get => clampCurve;
            set => clampCurve = value;
        }

        public int KeyframeCount => _keyframes.Count;

        
        #endregion

        #region Unity

        private void Awake()
        {
            if (!settings)
            {
                Debug.LogError("CurveEditorSettings not set in AnimationCurveEditor.");
                return;
            }
            
            LoadPresets();

            undoHandler.OnUndoAction += RebuildCurve;
            undoHandler.OnRedoAction += RebuildCurve;
        }

        protected virtual void Start()
        {
            scrollRect.OnScaleChanged.AddListener(OnGridScaleChanged);
        }

        private void OnDisable()
        {
            if (_selectedKeyframe)
                _selectedKeyframe.Deselect();
            
            PoolKeyframes();
            
            keyframeEditor.gameObject.SetActive(false);
            keyframeValues.SetActive(false);
            curveQuickActions.SetActive(false);
            
            OnEndEdit?.Invoke();
            
            OnValueChanged = null;
            OnEndEdit = null;
        }
        
        #endregion

        #region Functions

        /// <summary>
        /// Sets the title of the window
        /// </summary>
        /// <param name="s"></param>
        public abstract void SetWindowTitle(string s);
        
        /// <summary>
        /// Opens the editor and sets the curve to edit
        /// </summary>
        /// <param name="animationCurve"></param>
        public void EditCurve(AnimationCurve animationCurve)
        {
            gameObject.SetActive(true);
            
            PoolKeyframes();
            
            curve = animationCurve;
            curveRenderer.SetCurve(curve);

            Vector2 minRange = curveRenderer.MinRange * (settings.gridPixelsPerCell * 10f);
            Vector2 maxRange = curveRenderer.MaxRange * (settings.gridPixelsPerCell * 10f);

            float width = (maxRange.x - minRange.x) + (settings.gridPixelsPerCell * 2f);
            float height = (maxRange.y - minRange.y) + (settings.gridPixelsPerCell * 2f);
            
            InitializeKeyframes();
            
            scrollRect.ZoomToFit(width, height);

            RecordState(false);
        }
        
        /// <summary>
        /// Get the min/max range of the curve
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void GetCurveRange(out Vector2 min, out Vector2 max)
        {
            min = curveRenderer.MinRange;
            max = curveRenderer.MaxRange;
        }
        
        private void OnGridScaleChanged(Vector2 v) => curveRenderer.Redraw();
        
        #endregion
        
        #region Keyframes
        
        /// <summary>
        /// Build keyframes from the AnimationCurve
        /// </summary>
        private void InitializeKeyframes()
        {
            Keyframe[] keys = curve.keys;
            for (int i = 0; i < keys.Length; i++)
                CreateKeyframe(keys[i], i);
        }

        /// <summary>
        /// Create a CurveKeyframe object
        /// </summary>
        /// <param name="curveKey"></param>
        /// <param name="idx"></param>
        private CurveKeyframe CreateKeyframe(Keyframe curveKey, int idx)
        {
            CurveKeyframe keyframe;

            if (_pooledKeyframes.Count > 0)
                keyframe = _pooledKeyframes.Pop();
            else keyframe = Instantiate(keyframeTemplate, keyframeTemplate.transform.parent);
                
            keyframe.gameObject.SetActive(true);
                
            keyframe.KeyframeIndex = idx;
            keyframe.Keyframe = curveKey;
                
            _keyframes.Insert(idx, keyframe);

            return keyframe;
        }
        
        /// <summary>
        /// Recycle all the active keyframes
        /// </summary>
        private void PoolKeyframes()
        {
            for (int i = _keyframes.Count - 1; i >= 0; i--)
                PoolKeyframe(_keyframes[i]);
            
            _keyframes.Clear();
        }
        
        /// <summary>
        /// Move the keyframe to the pool
        /// </summary>
        /// <param name="keyframe"></param>
        private void PoolKeyframe(CurveKeyframe keyframe)
        {
            keyframe.gameObject.SetActive(false);
            _pooledKeyframes.Push(keyframe);
            _keyframes.Remove(keyframe);
        }

        /// <summary>
        /// Get the keyframe at the specified index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public CurveKeyframe GetKeyframe(int idx)
        {
            if (idx < 0 || idx >= _keyframes.Count)
                return null;

            return _keyframes[idx];
        }

        /// <summary>
        /// Check if the time is between the start and end keyframes
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool CanInsertAtTime(float time) 
            => time > _keyframes[0].Time && time < _keyframes[_keyframes.Count - 1].Time;

        /// <summary>
        /// Insert a keyframe the the specified time and value
        /// </summary>
        /// <param name="time"></param>
        /// <param name="value"></param>
        public void InsertKeyframe(float time, float value)
        {
            bool hasInserted = false;
            CurveKeyframe newKeyframe = null;
            
            // Loop through all the curves keyframe to find the correct position
            for (int i = 1; i < _keyframes.Count; i++)
            {
                if (hasInserted)
                {
                    // The keyframe has already been inserted, update the index for the remaining keyframes
                    _keyframes[i].KeyframeIndex += 1;
                    continue;
                }
                
                // Get the previous and next keyframes
                CurveKeyframe previous = _keyframes[i - 1];
                CurveKeyframe next = _keyframes[i];
                
                if (time > previous.Time && time < next.Time)
                {
                    // Insert a new keyframe into the keyframe list
                    Keyframe keyframe = new Keyframe(time, value)
                    {
                        tangentMode = TangentUtility.EncodeTangents(TangentMode.Auto)
                    };
                    
                    // Create a new CurveKeyframe object with this index
                    newKeyframe = CreateKeyframe(keyframe, i);
                    
                    hasInserted = true;
                }
            }

            // If this happens, something went wrong
            if (!newKeyframe)
                return;
            
            // Select the new keyframe and update the tangents
            SelectKeyframe(newKeyframe);
            newKeyframe.SetBothTangents(TangentMode.Auto, false);
            
            // Rebuild the keyframe array and send it back to the curve
            List<Keyframe> keyframes = ListPool<Keyframe>.Get();
            keyframes.AddRange(curve.keys);
            keyframes.Insert(newKeyframe.KeyframeIndex, newKeyframe.Keyframe);
            curve.keys = keyframes.ToArray();
            ListPool<Keyframe>.Release(keyframes);
            
            // Update the keyframe data and surrounding keyframes
            newKeyframe.FinalizeUpdate();
            
            // Redraw the curve
            curveRenderer.MarkDirty();
            
            RecordState(true);
        }
        
        /// <summary>
        /// Sets the selected keyframe and deselects the previous one
        /// </summary>
        /// <param name="keyframe"></param>
        public void SelectKeyframe(CurveKeyframe keyframe)
        {
            if (_selectedKeyframe == keyframe)
                return;

            if (_selectedKeyframe)
            {
                keyframeValues.SetActive(false);
                _selectedKeyframe.Deselect();
            }

            if (keyframe)
            {
                _selectedKeyframe = keyframe;
                _selectedKeyframe.Select();

                UpdateKeyframeInputs(
                    keyframe.Time.ToString("0.0###", CultureInfo.CurrentCulture), 
                    keyframe.Value.ToString("0.0###", CultureInfo.CurrentCulture));
            }
            else
            {
                keyframeValues.SetActive(false);
                _selectedKeyframe = null;
            }
        }

        protected abstract void UpdateKeyframeInputs(string time, string value);

        /// <summary>
        /// De-select the specified keyframe, if it is selected
        /// </summary>
        /// <param name="keyframe"></param>
        public void DeselectKeyframe(CurveKeyframe keyframe)
        {
            if (!_selectedKeyframe || _selectedKeyframe != keyframe)
                return;
            
            keyframeValues.SetActive(false);
            
            _selectedKeyframe.Deselect();
            _selectedKeyframe = null;
        }

        /// <summary>
        /// Check if the specified keyframe is selected
        /// </summary>
        /// <param name="keyframe"></param>
        /// <returns></returns>
        public bool IsSelectedKeyframe(CurveKeyframe keyframe)
            => _selectedKeyframe == keyframe;

        /// <summary>
        /// Update the keyframe in the curve and redraw the curve.
        /// </summary>
        /// <param name="keyframe"></param>
        public void UpdateKeyframe(CurveKeyframe keyframe)
        {
            CurveKeyframe lastKey = GetKeyframe(keyframe.KeyframeIndex - 1);
            CurveKeyframe nextKey = GetKeyframe(keyframe.KeyframeIndex + 1);
            
            float time = keyframe.Time;
            int idx = keyframe.KeyframeIndex;

            Keyframe[] keys = curve.keys;
            
            if (lastKey && time < lastKey.Time)
            {
                // If this keyframes time is less than the sibling to the left, switch the keyframes
                int lastIdx = lastKey.KeyframeIndex;

                keys[lastIdx] = keyframe.Keyframe;
                keys[idx] = lastKey.Keyframe;
                
                keyframe.KeyframeIndex = lastIdx;
                lastKey.KeyframeIndex = idx;

                _keyframes[lastIdx] = keyframe;
                _keyframes[idx] = lastKey;
            }
            else if (nextKey && time > nextKey.Time)
            {
                // If this keyframes time is more than the sibling to the right, switch the keyframes
                int nextIdx = nextKey.KeyframeIndex;
                
                keys[nextIdx] = keyframe.Keyframe;
                keys[idx] = nextKey.Keyframe;
                
                keyframe.KeyframeIndex = nextIdx;
                nextKey.KeyframeIndex = idx;
                
                _keyframes[nextIdx] = keyframe;
                _keyframes[idx] = nextKey;
            }
            else keys[keyframe.KeyframeIndex] = keyframe.Keyframe;
            
            curve.keys = keys;
        }

        /// <summary>
        /// Redraws the curve and send out the onValueChanged event.
        /// </summary>
        public void UpdateCurve()
        {
            curveRenderer.MarkDirty();

            if (_selectedKeyframe)
            {
                UpdateKeyframeInputs(
                    _selectedKeyframe.Time.ToString("0.0###", CultureInfo.CurrentCulture),
                    _selectedKeyframe.Value.ToString("0.0###", CultureInfo.CurrentCulture));
            }

            OnValueChanged?.Invoke();
        }

        /// <summary>
        /// Delete the specified keyframe from the curve
        /// </summary>
        /// <param name="keyframe"></param>
        public void DeleteKeyframe(CurveKeyframe keyframe)
        {
            int idx = keyframe.KeyframeIndex;
            
            curve.RemoveKey(idx);
            
            PoolKeyframe(keyframe);

            // Update the remaining keyframe indexes
            for (int i = idx; i < _keyframes.Count; i++)
                _keyframes[i].KeyframeIndex = i;
            
            curveRenderer.MarkDirty();
            
            OnValueChanged?.Invoke();
            
            RecordState(true);
        }

        /// <summary>
        /// Converts a normalized input to the range of the curve
        /// </summary>
        /// <param name="input"></param>
        public Vector2 ConvertNormalizedToCurveRange(Vector2 input)
        {
            Vector2 minRange = curveRenderer.MinRange;
            Vector2 maxRange = curveRenderer.MaxRange;
            
            return new Vector2(Mathf.Lerp(minRange.x, maxRange.x, input.x), Mathf.Lerp(minRange.y, maxRange.y, input.y));
        }
        #endregion

        #region Keyframe Value Input
        
        /// <summary>
        /// Toggle the keyframe values popup and position it relative to the selected keyframe.
        /// </summary>
        /// <param name="active"></param>
        public void ToggleKeyframeValuesPopup(bool active)
        {
            if (!active)
            {
                keyframeValues.SetActive(false);
                return;
            }

            if (!_selectedKeyframe)
                return;
            
            SetTransformToKeyframe(_selectedKeyframe, (RectTransform)keyframeValues.transform, valueInputOffset);
            
            keyframeValues.SetActive(true);
        }
        
        /// <summary>
        /// Set the selected keyframe time (callback from input field)
        /// </summary>
        /// <param name="s"></param>
        protected void SetKeyframeTime(string s)
        {
            if (!_selectedKeyframe)
                return;
            
            float v = float.Parse(s, CultureInfo.CurrentCulture);

            if (!Mathf.Approximately(v, _selectedKeyframe.Time))
            {
                _selectedKeyframe.Time = v;
                UpdateKeyframe(_selectedKeyframe);
                _selectedKeyframe.FinalizeUpdate();
            }
            
            RecordState(true);
        }
        
        /// <summary>
        /// Set the selected keyframe value (callback from input field)
        /// </summary>
        /// <param name="s"></param>
        protected void SetKeyframeValue(string s)
        {
            if (!_selectedKeyframe)
                return;
            
            float v = float.Parse(s, CultureInfo.CurrentCulture);

            if (!Mathf.Approximately(v, _selectedKeyframe.Value))
            {
                _selectedKeyframe.Value = v;
                UpdateKeyframe(_selectedKeyframe);
                _selectedKeyframe.FinalizeUpdate();
            }
            
            RecordState(true);
        }
        
        #endregion
        
        #region Keyframe Editor Popup
        
        /// <summary>
        /// Open the keyframe editor popup
        /// </summary>
        /// <param name="keyframe"></param>
        public void OpenKeyframeEditor(CurveKeyframe keyframe)
        {
            if (!keyframe)
                return;
            
            SelectKeyframe(keyframe);
            
            SetTransformToKeyframe(_selectedKeyframe, (RectTransform)keyframeEditor.transform, Vector2.zero);
            
            keyframeEditor.gameObject.SetActive(true);
            keyframeEditor.SetKeyframe(keyframe);
        }
        
        #endregion

        #region Utility
        /// <summary>
        /// Parent the rect transform to the keyframe and position it relative to the viewport.
        /// </summary>
        /// <param name="keyframe"></param>
        /// <param name="rectTransform"></param>
        /// <param name="offset"></param>
        private void SetTransformToKeyframe(CurveKeyframe keyframe, RectTransform rectTransform, Vector2 offset)
        {
            RectTransform viewport = scrollRect.Viewport;

            Vector2 posInViewport = viewport.InverseTransformPoint(keyframe.transform.position);
            Rect rect = viewport.rect;
            
            posInViewport.x -= rect.width * 0.5f;
            posInViewport.y += rect.height * 0.5f;
            
            rect = rectTransform.rect;
            
            Vector2 offsetPos = new Vector2(
                posInViewport.x < 0 ? rect.width + offset.x : -(rect.width + offset.x),
                posInViewport.y < 0 ? rect.height + offset.y : -(rect.height - offset.y)) * 0.5f;
            
            rectTransform.SetParent(keyframe.transform);
            rectTransform.SetAsLastSibling();
            
            rectTransform.localPosition = offsetPos;
            rectTransform.localScale = Vector3.one;
        }
        #endregion
        
        #region Presets

        private void LoadPresets()
        {
            if (settings.curvePresets.Count == 0)
                return;

            foreach (AnimationCurve animationCurve in settings.curvePresets)
                AddPreset(animationCurve);
        }

        /// <summary>
        /// Add a AnimationCurve preset to the editor
        /// </summary>
        /// <param name="animationCurve"></param>
        public void AddPreset(AnimationCurve animationCurve)
        {
            if (animationCurve.keys.Length < 2)
                return;
            
            AnimationCurveButton button = Instantiate(presetTemplate, presetTemplate.transform.parent);
            button.Curve = animationCurve;
            button.OnClick.AddListener(LoadPreset);
            button.OverrideOnClick = true;
            button.gameObject.SetActive(true);
        }

        /// <summary>
        /// Loads a preset animation curve to the editor
        /// </summary>
        /// <param name="animationCurve"></param>
        private void LoadPreset(AnimationCurve animationCurve)
        {
            keyframeEditor.gameObject.SetActive(false);
            keyframeValues.SetActive(false);

            // Copy the keys from the preset to the curve
            Keyframe[] keys = curve.keys;
            Keyframe[] otherKeys = animationCurve.keys;
            
            Array.Resize(ref keys, otherKeys.Length);
            
            for (int i = 0; i < otherKeys.Length; i++)
                keys[i] = otherKeys[i];

            curve.keys = keys;
            
            // Reset all the keyframes in the editor
            PoolKeyframes();
            InitializeKeyframes();
            
            // Redraw the curve
            curveRenderer.MarkDirty();
            
            OnValueChanged?.Invoke();
            
            RecordState(true);
        }
        
        #endregion
        
        #region Undo Handling

        /// <summary>
        /// Record the current state of the curve
        /// </summary>
        /// <param name="finalize">Finalize will will the current state data as post-modification and send the step off to the undo handler</param>
        public void RecordState(bool finalize)
        {
            if (finalize)
            {
                _curveUndoStep.RecordAfter();
                undoHandler.AddStep(_curveUndoStep);
                
                _curveUndoStep = null;
                
                RecordState(false);
            }
            else
            {
                _curveUndoStep = CurveUndoStep.Get();
                _curveUndoStep.Target = curve;
                _curveUndoStep.RecordBefore();
            }
        }

        /// <summary>
        /// Recycles all the curve components and rebuild it
        /// </summary>
        public void RebuildCurve()
        {
            PoolKeyframes();
            InitializeKeyframes();
            
            curveRenderer.MarkDirty();

            OnValueChanged?.Invoke();
        }
        #endregion
        
        [Serializable]
        public class AnimationCurveEvent : UnityEvent<AnimationCurve>
        {
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (!settings)
                settings = CurveSettingsUtility.GetOrCreateDefaultSettings();
            
            if (curve == null)
                curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            if (!curveGrid)
                curveGrid = GetComponentInChildren<CurveGrid>();
            
            if (!curveRenderer)
                curveRenderer = GetComponentInChildren<GridCurveRenderer>();
            
            if (!scrollRect)
                scrollRect = GetComponentInChildren<CurveScrollRect>();
            
            if (!keyframeEditor)
                keyframeEditor = GetComponentInChildren<KeyframeEditorPopup>();
            
            if (curveRenderer)
                curveRenderer.SetCurve(curve);
        }
        #endif
    }
}