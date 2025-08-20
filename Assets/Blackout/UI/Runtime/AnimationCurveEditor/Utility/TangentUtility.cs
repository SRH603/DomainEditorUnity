using UnityEngine;

namespace Blackout.UI
{
    public static class TangentUtility
    {
        /// <summary>
        /// Decode the tangent mode into left and right tangent modes.
        /// </summary>
        /// <param name="tangentMode"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void DecodeTangents(int tangentMode, out TangentMode left, out TangentMode right)
        {
            left = (TangentMode)((tangentMode & 0b1111) / 2); // Isolate lower 4 bits and divide by 2
            right = (TangentMode)((tangentMode >> 5) & 0b1111); // Shift right by 5 bits and isolate next 4 bits
        }
        
        /// <summary>
        /// Encode the specified tangent mode on to both side into a single integer.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static int EncodeTangents(TangentMode mode)
        {
            int leftValue = (int)mode * 2;
            int rightValue = (int)mode * 32;

            return leftValue | rightValue;
        }
        
        /// <summary>
        /// Encode the left and right tangent modes into a single integer.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static int EncodeTangents(TangentMode left, TangentMode right)
        {
            int leftValue = (int)left * 2;
            int rightValue = (int)right * 32;

            return leftValue | rightValue;
        }
        
        /// <summary>
        /// Convert a tangent to degrees.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float TangentToDegrees(float t)
            => Mathf.Rad2Deg * Mathf.Atan(t);
        
        /// <summary>
        /// Convert degrees to a tangent.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static float DegreesToTangent(float d)
            => Mathf.Tan(d * Mathf.PI / 180f);
        
        /// <summary>
        /// Convert a tangent to a rotation suitable for the keyframe UI.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Quaternion TangentToRotation(float t)
            => Quaternion.Euler(0f, 0f, TangentToDegrees(t));
    }
}