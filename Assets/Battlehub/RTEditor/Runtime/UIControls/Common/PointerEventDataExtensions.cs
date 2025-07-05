using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace Battlehub.UIControls.Common
{
    public static class PointerEventDataExtensions 
    {
        public static bool IsDefaultPointerId(this PointerEventData data)
        {
#if ENABLE_INPUT_SYSTEM
            ExtendedPointerEventData extendedData = data as ExtendedPointerEventData;
            if(extendedData != null)
            {
                return extendedData.button == PointerEventData.InputButton.Left;
            }
#endif

            return data.pointerId == 0 || data.pointerId == -1;
        }
    }
}

