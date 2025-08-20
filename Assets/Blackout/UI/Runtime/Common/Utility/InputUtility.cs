using UnityEngine;

namespace Blackout.UI
{
    public static class InputUtility 
    {
        public static bool Ctrl() => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        
        public static bool Shift() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        public static bool Alt() => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
    }
}
