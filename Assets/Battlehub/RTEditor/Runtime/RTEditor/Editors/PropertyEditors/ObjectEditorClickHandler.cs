using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public class ObjectEditorClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent Click = new UnityEvent();
        public UnityEvent DoubleClick = new UnityEvent();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                DoubleClick.Invoke();
            }

            Click.Invoke();
        }
    }
}

