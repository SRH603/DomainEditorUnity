using Blackout.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;

namespace BlackoutEditor.UI
{
    public static class BlackoutBuilder
    {
        /// <summary>
        /// Object used to pass resources to use for the default controls.
        /// </summary>
        public struct Resources
        {
            public Sprite standard;
            public Sprite background;
            public Sprite inputField;
            public Sprite knob;
            public Sprite checkmark;
            public Sprite dropdown;
            public Sprite mask;
        }
        
        public static readonly Vector2 ButtonElementSize = new Vector2(160f, 30f);
        public static readonly Vector2 EditorElementSize = new Vector2(500, 500);
        public static readonly Vector2 Center = Vector2.one * 0.5f;
        
        public static readonly Color TextColor = new Color(0.8078431f, 0.8078431f, 0.8078431f, 1f);
        public static readonly Color TextColorTransparent = new Color(0.8078431f, 0.8078431f, 0.8078431f, 0.5f);

        public static readonly Color ButtonColor = new Color(0.1019608f, 0.09803922f, 0.1019608f, 1f);
        public static readonly Color ButtonColorTransparent = new Color(0.1019608f, 0.09803922f, 0.1019608f, 0.5f);

        public static readonly Color TransparentBlack = new Color(0, 0, 0, 0.125f);
        public static readonly Color TangentHoverColor = new Color(0f, 0.5686275f, 1f, 0.45f);

        public static readonly Color HeaderColor = new Color(0.2196079f, 0.2235294f, 0.2352941f, 1f);
        public static readonly Color CurveButtonBackground = new Color(0.1019608f, 0.09803922f, 0.1019608f, 1f);
        public static readonly Color CurveDefaultColor = new Color(0.454902f, 1f, 0f, 1f);
        public static readonly Color ClearColor = new Color(1f, 1f, 1f, 0f);
        
        public static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        public static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go;
        }

        public static void SetColorTransitionValues(Selectable selectable)
        {
            ColorBlock colors = selectable.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor     = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor    = new Color(0.521f, 0.521f, 0.521f);
        }

        public static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

#if UNITY_EDITOR
            Undo.SetTransformParent(child.transform, parent.transform, "");
#else
            child.transform.SetParent(parent.transform, false);
#endif
            SetLayerRecursively(child, parent.layer);
        }

        public static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
        
        public static Image CreateImage(GameObject gameObject, Sprite sprite, Color color, bool raycastTarget, Image.Type type = Image.Type.Sliced)
        {
            Image image = gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = type;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static Image CreateMask(GameObject gameObject, Sprite sprite)
        {
            Image maskImage = gameObject.AddComponent<Image>();
            maskImage.sprite = sprite;
            maskImage.type = Image.Type.Sliced;
            maskImage.color = Color.white;
            maskImage.raycastTarget = false;

            gameObject.AddComponent<Mask>().showMaskGraphic = false;
            return maskImage;
        }

        public static Text CreateText(GameObject gameObject, Color color, string text, int fontSize = 14, TextAnchor alignment = TextAnchor.MiddleCenter) 
        {
            Text label = gameObject.AddComponent<Text>();
            label.text = text;
            label.color = color;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.raycastTarget = false;
            return label;
        }
        
        public static InputField CreateInputField(GameObject gameObject, Graphic targetGraphic, Text textComponent, int contentType)
        {
            InputField inputField = gameObject.AddComponent<InputField>();
            inputField.targetGraphic = targetGraphic;
            inputField.textComponent = textComponent;
            inputField.contentType = (InputField.ContentType)contentType;

            return inputField;
        }
        
        public static Button CreateButton(GameObject gameObject, UnityAction callback)
        {
            Button button = gameObject.AddComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick, callback);
            SetColorTransitionValues(button);
            return button;
        }
        
        public static RectTransform SetRectTransform(Transform transform, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            RectTransform rectTransform = (RectTransform)transform;
            rectTransform.pivot = pivot;
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.sizeDelta = sizeDelta;

            return rectTransform;
        }
        
        public static RectTransform SetRectTransform(Transform transform, Vector2 pivot, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rectTransform = (RectTransform)transform;
            rectTransform.pivot = pivot;
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;

            return rectTransform;
        }
        
        public static void FullStretch(Transform transform, Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            RectTransform rectTransform = (RectTransform)transform;
            rectTransform.pivot = Center;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
    }
}