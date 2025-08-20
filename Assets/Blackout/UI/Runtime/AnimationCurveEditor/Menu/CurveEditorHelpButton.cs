using UnityEngine;
using UnityEngine.EventSystems;

namespace Blackout.UI
{
    /// <summary>
    /// Opens the help menu when mouse hovers over
    /// </summary>
    public class CurveEditorHelpButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private GameObject menu;

        private bool _pointerInside = false;
        private float _timeInside = 0f;

        public GameObject Menu
        {
            get => menu;
            set => menu = value;
        }
        
        private void Update()
        {
            if (_pointerInside && !menu.activeSelf)
            {
                _timeInside += Time.deltaTime;

                if (_timeInside > 0.25f)
                    menu.SetActive(true);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _pointerInside = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            menu.SetActive(false);
            _pointerInside = false;
            _timeInside = 0f;
        }
    }
}