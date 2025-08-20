using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blackout.UI
{
    /// <summary>
    /// Renders this element above everything else, and closes the window when the user clicks outside of its bounds
    /// </summary>
    public class PopupCanvas : MonoBehaviour
    {
        #region Serialized Fields
       
        [SerializeField, Tooltip("The sorting order of the popup canvas.")]
        private int sortingOrder = 30000;

        [SerializeField, Tooltip("When enabled, a blocking element is created so that when a used clicks outside of the bounds the window closes automatically")]
        private bool createBlockingElement = true;
        
        #endregion
        
        #region Private Fields
        
        private Canvas _popupCanvas;
        private GameObject _blocker;
        private bool _canvasSetup;
        
        #endregion
        
        #region Properties

        public int SortingOrder
        {
            get => sortingOrder; 
            set => sortingOrder=value;
        }
        
        public bool CreateBlockingElement
        {
            get => createBlockingElement; 
            set => createBlockingElement=value;
        }
        
        #endregion

        private void OnEnable()
        {
            if (!_canvasSetup)
                SetupCanvas();

            Canvas rootCanvas = CanvasUtility.GetRootCanvas(gameObject);
            
            if (createBlockingElement)
                _blocker = CreateBlocker(rootCanvas);
        }

        private void OnDisable()
        {
            if (_blocker != null)
                Destroy(_blocker);

            _blocker = null;
        }
        
        private void Hide()
            => gameObject.SetActive(false);
        
        private void SetupCanvas()
        {
            Canvas parentCanvas = null;
            Transform parentTransform = transform.parent;
            while (parentTransform != null)
            {
                parentCanvas = parentTransform.GetComponent<Canvas>();
                if (parentCanvas != null)
                    break;

                parentTransform = parentTransform.parent;
            }

            _popupCanvas = GetOrAddComponent<Canvas>(gameObject);
            _popupCanvas.overrideSorting = true;
            _popupCanvas.sortingOrder = sortingOrder;

            if (parentCanvas != null)
            {
                BaseRaycaster[] components = parentCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    Type type = components[i].GetType();
                    if (!gameObject.GetComponent(type))
                        gameObject.AddComponent(type);
                }
            }
            else GetOrAddComponent<GraphicRaycaster>(gameObject);
            
            GetOrAddComponent<CanvasGroup>(gameObject);

            _canvasSetup = true;
        }

        private GameObject CreateBlocker(Canvas rootCanvas)
        {
            GameObject blocker = new GameObject("Blocker");

            RectTransform blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            Canvas blockerCanvas = blocker.AddComponent<Canvas>();
            blockerCanvas.overrideSorting = true;
            blockerCanvas.sortingLayerID = _popupCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = _popupCanvas.sortingOrder - 1;

            if (rootCanvas != null)
            {
                BaseRaycaster[] components = rootCanvas.GetComponents<BaseRaycaster>();
                for (int i = 0; i < components.Length; i++)
                {
                    Type type = components[i].GetType();
                    if (blocker.GetComponent(type) == null)
                        blocker.AddComponent(type);
                }
            }
            else GetOrAddComponent<GraphicRaycaster>(blocker);
            
            Image blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = Color.clear;

            Button blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(Hide);

            return blocker;
        }

        private T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T t = go.GetComponent<T>();
            if (!t)
                t = go.AddComponent<T>();
            return t;
        }
    }
}