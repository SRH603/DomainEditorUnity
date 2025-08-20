using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blackout.UI
{
    /// <summary>
    /// A serializable data class that represents an animation curve.
    /// </summary>
    [Serializable]
    public class AnimationCurveData
    {
        public List<KeyframeData> keyframes;

        public AnimationCurveData()
        {
            keyframes = new List<KeyframeData>();
        }
        
        public AnimationCurveData(AnimationCurve curve)
        {
            keyframes = new List<KeyframeData>();
            
            for (int i = 0; i < curve.keys.Length; i++)
            {
                Keyframe keyframe = curve.keys[i];
                keyframes.Add(keyframe);
            }
        }
        
        public AnimationCurveData(List<CurveKeyframe> keyframes)
        {
            this.keyframes = new List<KeyframeData>();
            
            for (int i = 0; i < keyframes.Count; i++)
            {
                this.keyframes.Add(keyframes[i].Data);
            }
        }
        
        /// <summary>
        /// Apply this data to the specified curve
        /// </summary>
        /// <param name="curve"></param>
        public void ApplyToCurve(AnimationCurve curve)
        {
            Keyframe[] keys = curve.keys;
            
            Array.Resize(ref keys, keyframes.Count);

            for (int i = 0; i < keyframes.Count; i++)
                keys[i] = keyframes[i];
            
            curve.keys = keys;
        }

        public static implicit operator AnimationCurve(AnimationCurveData data)
        {
            Keyframe[] keys = new Keyframe[data.keyframes.Count];
            
            for (int i = 0; i < data.keyframes.Count; i++)
                keys[i] = data.keyframes[i];

            return new AnimationCurve(keys);
        }
    }
}