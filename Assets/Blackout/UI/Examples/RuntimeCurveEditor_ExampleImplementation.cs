using Blackout.UI;
using UnityEngine;

namespace Blackout.UI
{
    public class RuntimeCurveEditor_ExampleImplementation : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve myCurveReference;

        // Create a reference to the AnimationCurveButton component
        [SerializeField]
        private AnimationCurveButton animationCurveButton;

        private void Start()
        {
            // Assign your AnimationCurve instance to the AnimationCurveButton.Curve property
            animationCurveButton.Curve = myCurveReference;

            // Now when the user clicks that button, it will open the curve editor using this curve
        }
    }
}