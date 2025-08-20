using System;
using UnityEngine;
#pragma warning disable CS0618 // Type or member is obsolete

namespace Blackout.UI
{
    /// <summary>
    /// A serializable data class that represents a Keyframe
    /// </summary>
    [Serializable]
    public class KeyframeData
    {
        /// <summary>
        /// The time of the keyframe
        /// </summary>
        public float time;

        /// <summary>
        /// The value of the curve at keyframe
        /// </summary>
        public float value;

        /// <summary>
        /// Sets the incoming tangent for this key. The incoming tangent affects the slope of the curve from the previous key to this key
        /// </summary>
        public float inTangent;

        /// <summary>
        /// Sets the outgoing tangent for this key. The outgoing tangent affects the slope of the curve from this key to the next key
        /// </summary>
        public float outTangent;

        /// <summary>
        /// Sets the incoming weight for this key. The incoming weight affects the slope of the curve from the previous key to this key
        /// </summary>
        public float inWeight;

        /// <summary>
        /// Sets the outgoing weight for this key. The outgoing weight affects the slope of the curve from this key to the next key
        /// </summary>
        public float outWeight;

        /// <summary>
        /// Weighted mode for the keyframe
        /// </summary>
        public WeightedMode weightedMode;
        
        public int tangentMode;
        
        public KeyframeData()
        {
            time = 0.0f;
            value = 0.0f;
            inTangent = 0.0f;
            outTangent = 0.0f;
            weightedMode = WeightedMode.None;
            inWeight = 0.0f;
            outWeight = 0.0f;
        }
        
        public KeyframeData(float time, float value)
        {
            this.time = time;
            this.value = value;
            inTangent = 0.0f;
            outTangent = 0.0f;
            weightedMode = WeightedMode.None;
            inWeight = 0.0f;
            outWeight = 0.0f;
        }

        public KeyframeData(float time, float value, float inTangent, float outTangent)
        {
              this.time = time;
              this.value = value;
              this.inTangent = inTangent;
              this.outTangent = outTangent;
              weightedMode = WeightedMode.None;
              inWeight = 0.0f;
              outWeight = 0.0f;
        }

        /// <summary>
        ///   Create a keyframe
        /// </summary>
        /// <param name="time"></param>
        /// <param name="value"></param>
        /// <param name="inTangent"></param>
        /// <param name="outTangent"></param>
        /// <param name="inWeight"></param>
        /// <param name="outWeight"></param>
        public KeyframeData(float time, float value, float inTangent, float outTangent, float inWeight, float outWeight)
        {
            this.time = time;
            this.value = value;
            this.inTangent = inTangent;
            this.outTangent = outTangent;
            this.weightedMode = WeightedMode.Both;
            this.inWeight = inWeight;
            this.outWeight = outWeight;
        }

        /// <summary>
        /// Copies a keyframes data to this data structure
        /// </summary>
        /// <param name="keyframe"></param>
        public void CopyFrom(Keyframe keyframe)
        {
            time = keyframe.time;
            value = keyframe.value;
            inTangent = keyframe.inTangent;
            outTangent = keyframe.outTangent;
            weightedMode = keyframe.weightedMode;
            inWeight = keyframe.inWeight;
            outWeight = keyframe.outWeight;
            tangentMode = keyframe.tangentMode;
        }

        public static implicit operator KeyframeData (Keyframe keyframe)
        {
            return new KeyframeData(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent, keyframe.inWeight, keyframe.outWeight)
            {
                tangentMode = keyframe.tangentMode,
                weightedMode = keyframe.weightedMode
            };
        }
        
        public static implicit operator Keyframe (KeyframeData keyframe)
        {
            return new Keyframe(keyframe.time, keyframe.value, keyframe.inTangent, keyframe.outTangent, keyframe.inWeight, keyframe.outWeight)
            {
                tangentMode = keyframe.tangentMode,
                weightedMode = keyframe.weightedMode
            };
        }

        
    }
}