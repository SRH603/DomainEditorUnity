namespace Blackout.UI
{
    /// <summary>
    /// Tangent constraints on Keyframe
    /// </summary>
    public enum TangentMode
    {
        /// <summary>
        /// The tangent can be freely set by dragging the tangent handle
        /// </summary>
        Free,
        /// <summary>
        /// The tangents are automatically set to make the curve go smoothly through the key
        /// </summary>
        Auto,
        /// <summary>
        /// The tangent points towards the neighboring key
        /// </summary>
        Linear,
        /// <summary>
        /// The curve retains a constant value between two keys
        /// </summary>
        Constant,
        /// <summary>
        /// The tangents are automatically set to make the curve go smoothly through the key
        /// </summary>
        ClampedAuto,
    }
}