using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Cross fade color when mouse is inside
    /// </summary>
    public class GraphicHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Color normalColor = new Color(0.8078431f, 0.8078431f, 0.8078431f, 1f);

        [SerializeField]
        private Color hoverColor = new Color(0.4535961f, 1, 0, 1);

        [SerializeField]
        private float duration = 0.1f;
        
        [SerializeField]
        private Graphic graphic;

        public Graphic Graphic
        {
            get => graphic;
            set => graphic = value;
        }
        
        private void OnEnable()
        {
            if (graphic)
                graphic.color = normalColor;
        }
        
        private void OnDisable()
        {
            if (graphic)
                graphic.color = normalColor;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (graphic)
                graphic.CrossFadeColor(hoverColor, duration, true, true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (graphic)
                graphic.CrossFadeColor(normalColor, duration, true, true);
        }
    }
}