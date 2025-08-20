using Blackout.Pool;
using System.Text;
using UnityEngine;

namespace Blackout.UI
{
    /// <summary>
    /// A custom undo type that handles animation curves
    /// </summary>
    public class CurveUndoStep : IUndoStep
    {
        private static readonly ObjectPool<CurveUndoStep> StepPool;
        private static readonly ObjectPool<AnimationCurveData> CurveDataPool;
        private static readonly ObjectPool<KeyframeData> KeyframeDataPool;

        private AnimationCurve _target;
        private AnimationCurveData _before;
        private AnimationCurveData _after;

        /// <summary>
        /// The target AnimationCurve
        /// </summary>
        public AnimationCurve Target
        {
            get => _target;
            set => _target = value;
        }
        
        /// <summary>
        /// AnimationCurve data before changes
        /// </summary>
        public AnimationCurveData Before
        {
            get => _before;
            set => _before = value;
        }
        
        /// <summary>
        /// AnimationCurve data after changes
        /// </summary>
        public AnimationCurveData After
        {
            get => _after;
            set => _after = value;
        }

        /// <summary>
        /// Setup object pools for all the data classes used
        /// </summary>
        static CurveUndoStep()
        {
            StepPool = new ObjectPool<CurveUndoStep>(()=> new CurveUndoStep(), OnGetAction);
            
            CurveDataPool = new ObjectPool<AnimationCurveData>(() => new AnimationCurveData());
            
            KeyframeDataPool = new ObjectPool<KeyframeData>(() => new KeyframeData());
        }

        /// <summary>
        /// Applies the value of the before data to the animation curve
        /// </summary>
        public void PerformUndo()
        {
            if (_target == null)
                return;
            
            _before.ApplyToCurve(_target);
        }

        /// <summary>
        /// Applies the value of the after data to the animation curve
        /// </summary>
        public void PerformRedo()
        {
            if (_target == null)
                return;
            
            _after.ApplyToCurve(_target);
        }
        
        /// <summary>
        /// Free all the data used by this undo step back to the pool
        /// </summary>
        public void Free()
        {
            _target = null;
            CurveDataPool.Release(_before);
            CurveDataPool.Release(_after);
            
            StepPool.Release(this);
        }

        /// <summary>
        /// Records the target animation curve to the before data
        /// </summary>
        public void RecordBefore()
            => FreeExistingAndRebuild(_before);

        /// <summary>
        /// Records the target animation curve to the after data
        /// </summary>
        public void RecordAfter()
            => FreeExistingAndRebuild(_after);

        private void FreeExistingAndRebuild(AnimationCurveData data)
        {
            for (int i = 0; i < data.keyframes.Count; i++)
            {
                KeyframeData keyframeData = data.keyframes[i];
                KeyframeDataPool.Release(keyframeData);
            }
            data.keyframes.Clear();

            Keyframe[] keys = _target.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                KeyframeData keyframeData = KeyframeDataPool.Get();
                keyframeData.CopyFrom(keys[i]);
                data.keyframes.Add(keyframeData);
            }
        }

        /// <summary>
        /// Get a CurveUndoStep from the pool
        /// </summary>
        /// <returns></returns>
        public static CurveUndoStep Get()
            => StepPool.Get();
        
        private static void OnGetAction(CurveUndoStep curveUndoStep)
        {
            curveUndoStep._before = CurveDataPool.Get();
            curveUndoStep._after = CurveDataPool.Get();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Before");
            foreach (KeyframeData k in _before.keyframes)
            {
                sb.AppendLine($"{k.time} {k.value} - {k.inTangent} {k.outTangent} - {k.inWeight} {k.outWeight} - {k.weightedMode} {k.tangentMode}");
            }
            
            sb.AppendLine("After");
            foreach (KeyframeData k in _after.keyframes)
            {
                sb.AppendLine($"{k.time} {k.value} - {k.inTangent} {k.outTangent} - {k.inWeight} {k.outWeight} - {k.weightedMode} {k.tangentMode}");
            }

            return sb.ToString();
        }
    }
}
