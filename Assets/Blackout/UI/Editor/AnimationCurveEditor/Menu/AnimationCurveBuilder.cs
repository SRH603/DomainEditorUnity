using Blackout.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;

namespace BlackoutEditor.UI
{
    public static class AnimationCurveBuilder
    {
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

        public const string HelpKeysText = "Mouse Scroll \nCtrl + Mouse Scroll\nShift + Mouse Scroll\nMiddle Mouse + Drag \nCtrl + Middle Mouse + Drag\nShift + Middle Mouse + Drag\nLeft Mouse (on node)\nLeft Mouse + Drag (on node)\nLeft Mouse + Drag (on tangents)\nLeft Mouse x2 (on line)\nRight Mouse (on node)";
        public const string DividersText = "-\n-\n-\n-\n-\n-\n-\n-\n-\n-\n-";
        public const string ActionsText = "Zoom \nZoom (X axis)\nZoom (Y axis)\nPan\nZoom (X axis)\nZoom (Y axis)\nSelect keyframe\nMove keyframe\nRotate tangents\nCreate keyframe\nOpen node submenu";

        public static readonly AnimationCurve DefaultCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        
        /// <summary>
        /// Create the animation curve button.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     AnimationCurveButton
        ///         - Mask
        ///             - CurveButtonRenderer
        /// </remarks>
        /// <param name="editor">Reference to a AnimationCurveEditor in the scene.</param>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateCurveButton(AnimationCurveEditor editor, BlackoutBuilder.Resources resources)
        {
            // Create GOs Hierarchy
            GameObject root = BlackoutBuilder.CreateUIElementRoot("Animation Curve Button", ButtonElementSize);
            //root.gameObject.SetActive(false);

            GameObject mask = BlackoutBuilder.CreateUIObject("Mask", root);
            GameObject curveImage = BlackoutBuilder.CreateUIObject("Curve Image", mask);

            BlackoutBuilder.SetRectTransform(root.transform, Center, Center, Center, ButtonElementSize);
            
            // Background
            BlackoutBuilder.CreateImage(root, resources.background, CurveButtonBackground, true);
            
            // Mask
            Image maskImage = BlackoutBuilder.CreateMask(mask, resources.mask);
            BlackoutBuilder.SetRectTransform(maskImage.transform, Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            
            // Curve Render
            curveImage.AddComponent<CanvasRenderer>();
            
            ButtonCurveRenderer curveRenderer = curveImage.AddComponent<ButtonCurveRenderer>();
            curveRenderer.color = CurveDefaultColor;
            curveRenderer.Content = (RectTransform)maskImage.transform;
            curveRenderer.Curve = DefaultCurve;
            curveRenderer.LineThickness = 2;
            curveRenderer.raycastTarget = false;

            BlackoutBuilder.SetRectTransform(curveRenderer.transform, Center, Vector2.zero, Vector2.one, new Vector2(3, 3), new Vector2(-3, -3));
           
            AnimationCurveButton button = root.AddComponent<AnimationCurveButton>();
            button.CurveRenderer = curveRenderer;
            button.UpdateMode = AnimationCurveButton.CurveUpdateMode.OnEndEdit;
            button.Editor = editor;
            button.Curve = DefaultCurve;
            //root.SetActive(true);
            
            return root;
        }
       
        
        /// <summary>
        /// Create the Curve Editor.
        /// </summary>
        /// <remarks>
        /// Hierarchy:
        /// (root)
        ///     Animation Curve Editor
        ///         - Header
        ///             - Text
        ///         - Helpers
        ///             - Undo
        ///                 - Text
        ///             - Redo
        ///                 - Text
        ///             - Help
        ///                 - Text
        ///                 - Help Menu
        ///                     - Keys
        ///                     - Dividers
        ///                     - Actions
        ///                     - Title
        ///         - Inset
        ///             - Top
        ///                 - ScrollView
        ///                     - Viewport
        ///                         - Content
        ///                             - Grid
        ///                                 - Curve
        ///                                 - Keyframe
        ///                                     - Left Tangent
        ///                                         - Line
        ///                                         - Handle
        ///                                     - Right Tangent
        ///                                         - Line
        ///                                         - Handle
        ///                                     - Handle
        ///                                 - Keyframe Values
        ///                                     - Time
        ///                                         - Time
        ///                                         - InputField
        ///                                             - Text Area
        ///                                                 - Text
        ///                                     - Divider
        ///                                     - Value
        ///                                         - Value
        ///                                         - InputField
        ///                                             - Text Area
        ///                                                 - Text
        ///                                 - Keyframe Editor
        ///                                     - Delete
        ///                                         - Text
        ///                                     - Edit
        ///                                         - Text
        ///                                     - Divider
        ///                                     - Clamped Auto
        ///                                         - Checkmark
        ///                                             - Checkmark
        ///                                         - Text
        ///                                     - Auto
        ///                                         - Checkmark
        ///                                             - Checkmark
        ///                                         - Text
        ///                                     - Free Smooth
        ///                                         - Checkmark
        ///                                             - Checkmark
        ///                                         - Text
        ///                                     - Flat
        ///                                         - Checkmark
        ///                                             - Checkmark
        ///                                         - Text
        ///                                     - Broken
        ///                                         - Checkmark
        ///                                             - Checkmark
        ///                                         - Text
        ///                                     - Divider
        ///                                     - Left Tangent
        ///                                         - Text
        ///                                         - Arrow
        ///                                     - Right Tangent
        ///                                         - Text
        ///                                         - Arrow
        ///                                     - Both Tangents
        ///                                         - Text
        ///                                         - Arrow
        ///                                     - Tangent Editor
        ///                                         - Free
        ///                                             - Background
        ///                                                 - Checkmark
        ///                                             - Text
        ///                                         - Linear
        ///                                             - Background
        ///                                                 - Checkmark
        ///                                             - Text
        ///                                         - Constant
        ///                                             - Background
        ///                                                 - Checkmark
        ///                                             - Text
        ///                                         - Weighted
        ///                                             - Background
        ///                                                 - Checkmark
        ///                                             - Text
        ///                                 - Quick Actions
        ///                                     - Normalize
        ///                                         - Text
        ///                                     - Flip Horizontal
        ///                                         - Text
        ///                                     - Flip Vertical
        ///                                         - Text
        ///                                 - Quick Actions
        ///                     - Scrollbar Horizontal
        ///                         - Sliding Area
        ///                             - Handle  
        ///                     - Scrollbar Vertical
        ///                         - Sliding Area
        ///                             - Handle
        ///                     - Vertical Axis
        ///                         - Template
        ///                     - Horizontal Axis
        ///                         - Template
        ///             - Bottom
        ///                 - Mask
        ///                     - Template
        ///                         - Mask
        ///                             - Curve Image
        ///         - Resizers
        ///             - Left
        ///             - Right
        ///             - Top
        ///             - Bottom
        ///             - Top Left
        ///             - Top Right
        ///             - Bottom Left
        ///             - Bottom Right
        /// </remarks>
        /// <param name="resources">The resources to use for creation.</param>
        /// <returns>The root GameObject of the created element.</returns>
        public static GameObject CreateCurveEditor(BlackoutBuilder.Resources resources) 
        {
            // Create GOs Hierarchy
            GameObject root = BlackoutBuilder.CreateUIElementRoot("Animation Curve Editor", EditorElementSize);
            
            BlackoutBuilder.CreateImage(root, null, HeaderColor, true);

            LegacyAnimationCurveEditor editor = root.AddComponent<LegacyAnimationCurveEditor>();
            PopupCanvas popupCanvas = root.AddComponent<PopupCanvas>();
            UndoHandler undoHandler = editor.UndoHandler = root.AddComponent<UndoHandler>();
            
            popupCanvas.SortingOrder = 30000;
            popupCanvas.CreateBlockingElement = true;
            
            // Header
            CreateHeader(root, editor);
            
            // Helpers
            CreateHelpers(root, undoHandler, resources);
            
            // Inset
            CreateInset(root, editor, resources);
            
            // Resizers
            CreateResizers(root);
            
            return root;
        }
        
        private static void CreateHeader(GameObject parent, LegacyAnimationCurveEditor editor)
        {
            GameObject header = BlackoutBuilder.CreateUIObject("Header", parent);
            BlackoutBuilder.CreateImage(header, null, HeaderColor, true);
            header.AddComponent<DraggableWindow>();

            BlackoutBuilder.SetRectTransform(header.transform, 
                Center, 
                new Vector2(0, 1), 
                new Vector2(1, 1), 
                new Vector2(3, -27.5f), 
                new Vector2(-132.5f, -2.5f)).
                anchoredPosition = new Vector2(-64.75f, -15f);
            
            GameObject title = BlackoutBuilder.CreateUIObject("Title", header);
            BlackoutBuilder.FullStretch(title.transform, new Vector2(5f, 0f));
            editor.WindowTitle = BlackoutBuilder.CreateText(title, TextColor, "Curve", 14, TextAnchor.MiddleLeft);
        }
        
        public static void CreateHelpers(GameObject parent, UndoHandler undoHandler, BlackoutBuilder.Resources resources)
        {
            GameObject helpers = BlackoutBuilder.CreateUIObject("Helpers", parent);

            BlackoutBuilder.SetRectTransform(helpers.transform, 
                Center, 
                Vector2.one, 
                Vector2.one, 
                new Vector2(-132.5f, -25f), 
                new Vector2(-2.5f, -5f));
            
            // Undo
            GameObject undo = BlackoutBuilder.CreateUIObject("Undo", helpers);
            BlackoutBuilder.SetRectTransform(undo.transform,
                Center,
                new Vector2(1, 0.5f),
                new Vector2(1, 0.5f),
                new Vector2(-130, -10),
                new Vector2(-80, 10));
            
            
            GameObject undoText = BlackoutBuilder.CreateUIObject("Text", undo);
            BlackoutBuilder.FullStretch(undoText.transform);
            Text undoLabel = BlackoutBuilder.CreateText(undoText, TextColor, "Undo");
            
            BlackoutBuilder.CreateImage(undo, resources.standard, ButtonColor, true);

            BlackoutBuilder.CreateButton(undo, new UnityAction(undoHandler.PerformUndo));
            
            GraphicHoverColor undoHover = undo.AddComponent<GraphicHoverColor>();
            undoHover.Graphic = undoLabel;
            
            // Redo
            GameObject redo = BlackoutBuilder.CreateUIObject("Redo", helpers);
            BlackoutBuilder.SetRectTransform(redo.transform,
                Center,
                new Vector2(1, 0.5f),
                new Vector2(1, 0.5f),
                new Vector2(-75, -10),
                new Vector2(-25, 10));
            
            GameObject redoText = BlackoutBuilder.CreateUIObject("Text", redo);
            BlackoutBuilder.FullStretch(redoText.transform);
            Text redoLabel = BlackoutBuilder.CreateText(redoText, TextColor, "Redo");
            
            BlackoutBuilder.CreateImage(redo, resources.standard, ButtonColor, true);
           
            BlackoutBuilder.CreateButton(redo, undoHandler.PerformRedo);
            
            GraphicHoverColor redoHover = redo.AddComponent<GraphicHoverColor>();
            redoHover.Graphic = redoLabel;

            // Help
            GameObject help = BlackoutBuilder.CreateUIObject("Help", helpers);
            BlackoutBuilder.SetRectTransform(help.transform,
                Center,
                new Vector2(1, 0.5f),
                new Vector2(1, 0.5f),
                new Vector2(-20, -10),
                new Vector2(0, 10));
            
            GameObject helpText = BlackoutBuilder.CreateUIObject("Text", help);
            BlackoutBuilder.FullStretch(helpText.transform);
            
            Text helpLabel = BlackoutBuilder.CreateText(helpText, TextColor, "?") ;
            
            BlackoutBuilder.CreateImage(help, resources.standard, ButtonColor, true);
            
            // Help Foldout
            GameObject foldout = BlackoutBuilder.CreateUIObject("Help Foldout", help);
            BlackoutBuilder.SetRectTransform(foldout.transform, 
                Vector2.one,
                Center,
                Center,
                new Vector2(-340, -170),
                new Vector2(-10, 10));

            BlackoutBuilder.CreateImage(foldout, resources.standard, TextColor, true);
            
            PopupCanvas popoutCanvas = foldout.AddComponent<PopupCanvas>();
            popoutCanvas.SortingOrder = 30002;
            popoutCanvas.CreateBlockingElement = false;
            
            GameObject keys = BlackoutBuilder.CreateUIObject("Keys", foldout);
            GameObject dividers = BlackoutBuilder.CreateUIObject("Dividers", foldout);
            GameObject actions = BlackoutBuilder.CreateUIObject("Actions", foldout);
            GameObject title = BlackoutBuilder.CreateUIObject("Title", foldout);

            BlackoutBuilder.SetRectTransform(keys.transform, 
                Center, 
                Vector2.zero, 
                Vector2.one, 
                new Vector2(5, 5), 
                new Vector2(-145, -20));
            
            BlackoutBuilder.SetRectTransform(dividers.transform,
                Center,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 1f),
                new Vector2(20f, 0),
                new Vector2(30f, -20));
            
            BlackoutBuilder.SetRectTransform(actions.transform,
                Center,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 1f),
                new Vector2(40, 5),
                new Vector2(165, -20));
            
            BlackoutBuilder.SetRectTransform(title.transform,
                Center,
                new Vector2(0, 1),
                Vector2.one,
                new Vector2(5, -20),
                Vector2.zero);
            
            BlackoutBuilder.CreateText(keys, ButtonColor, HelpKeysText, 12, TextAnchor.MiddleLeft);
            BlackoutBuilder.CreateText(dividers, ButtonColor, DividersText, 12);
            BlackoutBuilder.CreateText(actions, ButtonColor, ActionsText, 12, TextAnchor.MiddleLeft);
            BlackoutBuilder.CreateText(title, ButtonColor, "<b>Controls</b>", 14, TextAnchor.MiddleLeft);
            
            CurveEditorHelpButton helpButton = help.AddComponent<CurveEditorHelpButton>();
            helpButton.Menu = foldout;
            
            GraphicHoverColor helpHover = help.AddComponent<GraphicHoverColor>();
            helpHover.Graphic = helpLabel;
            
            foldout.SetActive(false);
        }

        private static void CreateInset(GameObject parent, LegacyAnimationCurveEditor editor, BlackoutBuilder.Resources resources)
        {
            GameObject inset = BlackoutBuilder.CreateUIObject("Inset", parent);
            BlackoutBuilder.SetRectTransform(inset.transform,
                Center,
                new Vector2(0, 0),
                new Vector2(1, 1),
                new Vector2(2.5f, 2.5f),
                new Vector2(-2.5f, -27.5f));
            
            CreateInsetTop(inset, editor, resources);
            
            CreateInsetBottom(inset, editor,resources);
        }

        public static void CreateInsetTop(GameObject parent, LegacyAnimationCurveEditor editor, BlackoutBuilder.Resources resources)
        {
            GameObject top = BlackoutBuilder.CreateUIObject("Top", parent);
            BlackoutBuilder.SetRectTransform(top.transform,
                Center,
                Vector2.zero,
                Vector2.one,
                new Vector2(0, 31),
                Vector2.zero);

            BlackoutBuilder.CreateImage(top, null, ButtonColor, false);
            
            #region ScrollView
            GameObject scrollView = BlackoutBuilder.CreateUIObject("Scroll View", top);
            BlackoutBuilder.FullStretch(scrollView.transform);
            
            CurveScrollRect scrollRect = editor.ScrollRect = scrollView.AddComponent<CurveScrollRect>();
            
            // Viewport
            GameObject viewport = BlackoutBuilder.CreateUIObject("Viewport", scrollView);
            SetupViewport(viewport);
            scrollRect.Viewport = (RectTransform)viewport.transform;
            
            // Content
            GameObject content = BlackoutBuilder.CreateUIObject("Content", viewport);
            scrollRect.Content = BlackoutBuilder.SetRectTransform(content.transform,
                Center,
                Center,
                Center,
                new Vector2(483, 440));
            #endregion
            
            #region Grid
            GameObject grid = BlackoutBuilder.CreateUIObject("Grid", content);
            scrollRect.Grid = BlackoutBuilder.SetRectTransform(grid.transform,
                Center,
                Vector2.zero, 
                Vector2.one, 
                Vector2.zero,
                Vector2.zero);

            BlackoutBuilder.CreateImage(grid, null, Color.white, true);

            CurveGrid curveGrid = editor.Grid = grid.AddComponent<CurveGrid>();
            curveGrid.ScrollRect = scrollRect;
            curveGrid.Editor = editor;
            #endregion
            
            #region Curve
            GameObject curve = BlackoutBuilder.CreateUIObject("Curve", grid);
            BlackoutBuilder.SetRectTransform(curve.transform,
                Center,
                Center,
                Center,
                new Vector2(500, 500));
            
            curve.AddComponent<CanvasRenderer>();

            GridCurveRenderer renderer = editor.CurveRenderer = curve.AddComponent<GridCurveRenderer>();
            renderer.color = CurveDefaultColor;
            renderer.Content = scrollRect.Grid;
            renderer.Curve = DefaultCurve;
            #endregion
            
            #region Keyframe Popups
            // Keyframe
            editor.KeyframeTemplate = CreateKeyframeTemplate(grid, resources);

            // Keyframe Values Editor
            CreateKeyframeValueEditor(grid, editor, resources);

            // Tangent Editor Popup
            CreateKeyframeTangentEditor(grid, editor, resources);
            
            // Quick Action Popup
            CreateQuickActionMenu(grid, editor, resources);
            #endregion
            
            #region Scrollbar Horizontal
            GameObject scrollbarGoH = BlackoutBuilder.CreateUIObject("Scrollbar Horizontal", scrollView);
            BlackoutBuilder.CreateImage(scrollbarGoH, resources.background, ButtonColorTransparent, true);
            BlackoutBuilder.SetRectTransform(scrollbarGoH.transform,
                Vector2.zero,
                Vector2.zero,
                new Vector2(1, 0),
                Vector2.zero,
                new Vector2(-12, 12));
            
            Scrollbar scrollbarHorizontal = scrollRect.HorizontalScrollbar = SetupScrollbar(scrollbarGoH, resources);
            scrollbarHorizontal.direction = Scrollbar.Direction.LeftToRight;
            scrollRect.HorizontalScrollbarSpacing = -12;
            #endregion
            
            #region Scrollbar Vertical
            GameObject scrollbarGoV = BlackoutBuilder.CreateUIObject("Scrollbar Vertical", scrollView);
            BlackoutBuilder.CreateImage(scrollbarGoV, resources.background, ButtonColorTransparent, true);
            BlackoutBuilder.SetRectTransform(scrollbarGoV.transform,
                Vector2.one,
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(-12, 0),
                Vector2.zero);
            
            Scrollbar scrollbarVertical = scrollRect.VerticalScrollbar = SetupScrollbar(scrollbarGoV, resources);
            scrollbarVertical.direction = Scrollbar.Direction.BottomToTop;
            #endregion
            
            #region Vertical Axis
            GameObject verticalAxis = BlackoutBuilder.CreateUIObject("Vertical Axis", scrollView);
            BlackoutBuilder.SetRectTransform(verticalAxis.transform,
                Center,
                Vector2.zero,
                new Vector2(0, 1),
                new Vector2(0, 12),
                new Vector2(20, 0));

            RectMask2D axisMaskV = verticalAxis.AddComponent<RectMask2D>();
            #if UNITY_2019_4_OR_NEWER
            axisMaskV.padding = new Vector4(0, 20, 0, 20);
            axisMaskV.softness = new Vector2Int(10, 10);
            #endif
            
            GameObject verticalAxisTemplate = BlackoutBuilder.CreateUIObject("Template", verticalAxis);
            BlackoutBuilder.SetRectTransform(verticalAxisTemplate.transform,
                Center,
                Center,
                Center,
                 new Vector2(-25, -10),
                new Vector2(25, 10))
                .localRotation = Quaternion.Euler(0, 0, 90);
            
            Text verticalTemplateLabel = BlackoutBuilder.CreateText(verticalAxisTemplate, TextColorTransparent, "0.5");
            
            verticalAxisTemplate.SetActive(false);
            
            LegacyCurveGridMarkers verticalMarkers = verticalAxis.AddComponent<LegacyCurveGridMarkers>();
            verticalMarkers.Direction = LegacyCurveGridMarkers.MovementDirection.Vertical;
            verticalMarkers.Editor = editor;
            verticalMarkers.ScrollRect = scrollRect;
            verticalMarkers.Template = verticalTemplateLabel;
            #endregion
            
            #region Horizontal Axis
            GameObject horizontalAxis = BlackoutBuilder.CreateUIObject("Horizontal Axis", scrollView);
            BlackoutBuilder.SetRectTransform(horizontalAxis.transform,
                Center,
                Vector2.zero,
                new Vector2(1, 0),
                new Vector2(0, 12),
                new Vector2(-12, 32));

            RectMask2D axisMaskH = horizontalAxis.AddComponent<RectMask2D>();
            #if UNITY_2019_4_OR_NEWER
            axisMaskH.padding = new Vector4(20, 0, 20, 0);
            axisMaskH.softness = new Vector2Int(10, 10);
            #endif
            
            GameObject horizontalAxisTemplate = BlackoutBuilder.CreateUIObject("Template", horizontalAxis);
            BlackoutBuilder.SetRectTransform(horizontalAxisTemplate.transform,
                    Center,
                    Center,
                    Center,
                    new Vector2(-25, -10),
                    new Vector2(25, 10));
            
            Text horizontalTemplateLabel = BlackoutBuilder.CreateText(horizontalAxisTemplate, TextColorTransparent, "0.5");
            
            horizontalAxisTemplate.SetActive(false);
            
            LegacyCurveGridMarkers horizontalMarkers = horizontalAxis.AddComponent<LegacyCurveGridMarkers>();
            horizontalMarkers.Direction = LegacyCurveGridMarkers.MovementDirection.Horizontal;
            horizontalMarkers.Editor = editor;
            horizontalMarkers.ScrollRect = scrollRect;
            horizontalMarkers.Template = horizontalTemplateLabel;
            #endregion
            
            CurveClickDetector curveClickDetector = curve.AddComponent<CurveClickDetector>();
            curveClickDetector.Menu = editor.CurveQuickActions;
        }

        public static void SetupViewport(GameObject viewport)
        {
            RectMask2D viewportMask = viewport.AddComponent<RectMask2D>();
            #if UNITY_2019_4_OR_NEWER
            viewportMask.padding = new Vector4(20, 20, 0, 0);
            viewportMask.softness = new Vector2Int(20, 20);
            #endif
            
            BlackoutBuilder.SetRectTransform(viewport.transform, 
                new Vector2(0, 1), 
                Vector2.zero, 
                Vector2.one, 
                new Vector2(0, 12), 
                new Vector2(-12, 0));
        }

        private static CurveKeyframe CreateKeyframeTemplate(GameObject parent, BlackoutBuilder.Resources resources)
        {
            GameObject keyframe = BlackoutBuilder.CreateUIObject("Keyframe", parent);
            BlackoutBuilder.SetRectTransform(keyframe.transform,
                Center,
                Center,
                Center,
                new Vector2(18, 18));
            
            BlackoutBuilder.CreateImage(keyframe, null, Color.clear, true);
            
            CurveKeyframe curveKeyframe = keyframe.AddComponent<CurveKeyframe>();
            
            GameObject leftTangent = BlackoutBuilder.CreateUIObject("Left Tangent", keyframe);
            BlackoutBuilder.SetRectTransform(leftTangent.transform,
                new Vector2(1, 0.5f),
                Center,
                Center,
                new Vector2(-50, -1),
                new Vector2(0, 1));
            
            GameObject leftTangentLine = BlackoutBuilder.CreateUIObject("Line", leftTangent);
            BlackoutBuilder.SetRectTransform(leftTangentLine.transform,
                new Vector2(0, 0.5f),
                Vector2.zero, 
                Vector2.one, 
                Vector2.zero,
                Vector2.zero);

            BlackoutBuilder.CreateImage(leftTangentLine, null, TextColor, false);
            
            GameObject leftTangentHandle = BlackoutBuilder.CreateUIObject("Handle", leftTangent);
            BlackoutBuilder.SetRectTransform(leftTangentHandle.transform,
                Center,
                new Vector2(0f, 0.5f), 
                new Vector2(0f, 0.5f), 
                new Vector2(-4.5f, -4.5f),
                new Vector2(4.5f, 4.5f));

            BlackoutBuilder.CreateImage(leftTangentHandle, resources.knob, TextColor, true, Image.Type.Simple);

            CurveTangent leftCurveTangent = leftTangent.AddComponent<CurveTangent>();
            leftCurveTangent.RectTransform = (RectTransform)leftTangent.transform;
            leftCurveTangent.Keyframe = curveKeyframe;
            leftCurveTangent.Direction = CurveTangent.Side.Left;

            curveKeyframe.LeftTangent = leftCurveTangent;
            
            GameObject rightTangent = BlackoutBuilder.CreateUIObject("Right Tangent", keyframe);
            BlackoutBuilder.SetRectTransform(rightTangent.transform,
                new Vector2(0, 0.5f),
                Center,
                Center,
                new Vector2(0, -1),
                new Vector2(50, 1));
            
            GameObject rightTangentLine = BlackoutBuilder.CreateUIObject("Line", rightTangent);
            BlackoutBuilder.SetRectTransform(rightTangentLine.transform,
                new Vector2(0, 0.5f),
                Vector2.zero, 
                Vector2.one, 
                Vector2.zero,
                Vector2.zero);

            BlackoutBuilder.CreateImage(rightTangentLine, null, TextColor, false);
            
            GameObject rightTangentHandle = BlackoutBuilder.CreateUIObject("Handle", rightTangent);
            BlackoutBuilder.SetRectTransform(rightTangentHandle.transform,
                Center,
                new Vector2(1f, 0.5f), 
                new Vector2(1f, 0.5f), 
                new Vector2(-4.5f, -4.5f),
                new Vector2(4.5f, 4.5f));

            BlackoutBuilder.CreateImage(rightTangentHandle, resources.knob, TextColor, true, Image.Type.Simple);

            CurveTangent rightCurveTangent = rightTangent.AddComponent<CurveTangent>();
            rightCurveTangent.RectTransform = (RectTransform)rightTangent.transform;
            rightCurveTangent.Keyframe = curveKeyframe;
            rightCurveTangent.Direction = CurveTangent.Side.Right;

            curveKeyframe.RightTangent = rightCurveTangent;
            
            GameObject handle = BlackoutBuilder.CreateUIObject("Handle", keyframe);
            BlackoutBuilder.SetRectTransform(handle.transform,
                Center,
                Center,
                Center,
                new Vector2(16, 16));
            
            curveKeyframe.Handle = BlackoutBuilder.CreateImage(handle, resources.knob, CurveDefaultColor, true, Image.Type.Simple);

            keyframe.SetActive(false);
            leftTangent.SetActive(false);
            rightTangent.SetActive(false);
            return curveKeyframe;
        }

        private static void CreateKeyframeValueEditor(GameObject parent, LegacyAnimationCurveEditor editor, BlackoutBuilder.Resources resources)
        {
            GameObject keyframeValueEditor = editor.KeyframeValuesPopup = BlackoutBuilder.CreateUIObject("Keyframe Values", parent);
            BlackoutBuilder.SetRectTransform(keyframeValueEditor.transform,
                Center,
                Center,
                Center,
                new Vector2(100, 38));

            BlackoutBuilder.CreateImage(keyframeValueEditor, resources.standard, TextColor, true);

            PopupCanvas popupCanvas = keyframeValueEditor.AddComponent<PopupCanvas>();
            popupCanvas.SortingOrder = 30001;
            popupCanvas.CreateBlockingElement = true;
                
            GameObject time = BlackoutBuilder.CreateUIObject("Time", keyframeValueEditor);
            BlackoutBuilder.SetRectTransform(time.transform,
                Center,
                new Vector2(0, 1),
                Vector2.one,
                new Vector2(2, -17),
                new Vector2(-2, -2));

            editor.TimeInput = CreateKeyframeValueField(time, "Time", resources);
            
            GameObject divider = BlackoutBuilder.CreateUIObject("Divider", keyframeValueEditor);
            BlackoutBuilder.SetRectTransform(divider.transform,
                Center,
                new Vector2(0, 0.5f),
                new Vector2(1, 0.5f),
                new Vector2(2, -0.5f),
                new Vector2(-2, 0.5f));

            BlackoutBuilder.CreateImage(divider, null, TransparentBlack, false);
            
            GameObject value = BlackoutBuilder.CreateUIObject("Value", keyframeValueEditor);
            BlackoutBuilder.SetRectTransform(value.transform,
                Center,
                Vector2.zero, 
                new Vector2(1, 0),
                new Vector2(2, 2),
                new Vector2(-2, 17));

            editor.ValueInput = CreateKeyframeValueField(value, "Value", resources);
            
            keyframeValueEditor.SetActive(false);
        }

        private static InputField CreateKeyframeValueField(GameObject parent, string name, BlackoutBuilder.Resources resources)
        {
            GameObject label = BlackoutBuilder.CreateUIObject(name, parent);
            BlackoutBuilder.SetRectTransform(label.transform,
                Center,
                Vector2.zero,
                Vector2.one,
                new Vector2(3, 0),
                Vector2.zero);

            BlackoutBuilder.CreateText(label, ButtonColor, name, 12, TextAnchor.MiddleLeft);

            GameObject inputFieldGo = BlackoutBuilder.CreateUIObject("InputField", parent);
            BlackoutBuilder.SetRectTransform(inputFieldGo.transform,
                Center,
                Vector2.zero, 
                Vector2.one,
                new Vector2(40, 0),
                Vector2.zero);
            
            GameObject textArea = BlackoutBuilder.CreateUIObject("Text Area", inputFieldGo);
            BlackoutBuilder.SetRectTransform(textArea.transform,
                Center,
                Vector2.zero,
                Vector2.one,
                new Vector2(5, 0),
                new Vector2(-5, 0));
            
            RectMask2D mask = textArea.AddComponent<RectMask2D>();
            #if UNITY_2019_4_OR_NEWER
            mask.padding = new Vector4(-8, -5, -8, -5);
            #endif
            
            GameObject text = BlackoutBuilder.CreateUIObject("Text", textArea);
            BlackoutBuilder.FullStretch(text.transform);

            Text textComponent = BlackoutBuilder.CreateText(text, ButtonColor, string.Empty, 12, TextAnchor.MiddleRight);

            Graphic targetGraphic = BlackoutBuilder.CreateImage(inputFieldGo, resources.standard, TransparentBlack, true);
            
            InputField inputField = BlackoutBuilder.CreateInputField(inputFieldGo, targetGraphic, textComponent, (int)InputField.ContentType.DecimalNumber);
            
            BlackoutBuilder.SetColorTransitionValues(inputField);

            return inputField;
        }
        
        private static void CreateKeyframeTangentEditor(GameObject parent, AnimationCurveEditor editor, BlackoutBuilder.Resources resources)
        {
            GameObject keyframeEditor = BlackoutBuilder.CreateUIObject("Keyframe Editor", parent);
            BlackoutBuilder.SetRectTransform(keyframeEditor.transform,
                Center,
                Center,
                Center,
                new Vector2(140, 176));
            
            BlackoutBuilder.CreateImage(keyframeEditor, resources.standard, TextColor, true);
            
            KeyframeEditorPopup popupComponent = keyframeEditor.AddComponent<KeyframeEditorPopup>();
            popupComponent.editor = editor;
            editor.KeyframeEditor = popupComponent;
            
            PopupCanvas popupCanvas = keyframeEditor.AddComponent<PopupCanvas>();
            popupCanvas.SortingOrder = 30002;
            popupCanvas.CreateBlockingElement = true;

            popupComponent.DeleteButton = CreateTangentMenuButton(keyframeEditor, "Delete", editor, new Vector2(2, -19), new Vector2(138, -2));
            popupComponent.EditButton = CreateTangentMenuButton(keyframeEditor, "Edit", editor, new Vector2(2, -36), new Vector2(138, -19));

            CreateTangentMenuDivider(keyframeEditor, new Vector2(2, -37), new Vector2(138, -36));
            
            popupComponent.ClampedAutoToggle = CreateTangentMenuToggle(keyframeEditor, resources, "Clamped Auto", editor, new Vector2(2, -54), new Vector2(138, -37));
            popupComponent.AutoToggle = CreateTangentMenuToggle(keyframeEditor, resources, "Auto", editor, new Vector2(2, -71), new Vector2(138, -54));
            popupComponent.FreeSmoothToggle = CreateTangentMenuToggle(keyframeEditor, resources, "Free Smooth", editor, new Vector2(2, -88), new Vector2(138, -71));
            popupComponent.FlatToggle = CreateTangentMenuToggle(keyframeEditor, resources, "Flat", editor, new Vector2(2, -105), new Vector2(138, -88));
            popupComponent.BrokenToggle = CreateTangentMenuToggle(keyframeEditor, resources, "Broken", editor, new Vector2(2, -122), new Vector2(138, -105));
            
            CreateTangentMenuDivider(keyframeEditor, new Vector2(2, -123), new Vector2(138, -122));
            
            popupComponent.LeftTangentButton = CreateTangentFoldoutButton(keyframeEditor, "Left Tangent", editor, popupComponent, CurveTangent.Side.Left, new Vector2(2, -140), new Vector2(138, -123));
            popupComponent.RightTangentButton = CreateTangentFoldoutButton(keyframeEditor, "Right Tangent", editor, popupComponent, CurveTangent.Side.Right, new Vector2(2, -157), new Vector2(138, -140));
            popupComponent.BothTangentButton = CreateTangentFoldoutButton(keyframeEditor, "Both Tangents", editor, popupComponent, CurveTangent.Side.Both, new Vector2(2, -174), new Vector2(138, -157));
            
            CreateTangentSubMenu(keyframeEditor, resources, editor, popupComponent);
            
            keyframeEditor.SetActive(false);
        }

        private static void CreateTangentSubMenu(GameObject parent, BlackoutBuilder.Resources resources, AnimationCurveEditor editor, KeyframeEditorPopup popupComponent)
        {
            GameObject tangentEditor = BlackoutBuilder.CreateUIObject("Tangent Editor", parent);
            BlackoutBuilder.SetRectTransform(tangentEditor.transform,
                new Vector2(0, 1),
                Center,
                Center,
                new Vector2(66, -90),
                new Vector2(156, -18));

            BlackoutBuilder.CreateImage(tangentEditor, resources.standard, TextColor, true);

            PopupCanvas popupCanvas = tangentEditor.AddComponent<PopupCanvas>();
            popupCanvas.SortingOrder = 30003;
            popupCanvas.CreateBlockingElement = false;

            popupComponent.TangentFreeToggle = CreateTangentMenuToggle(tangentEditor, resources, "Free", editor, new Vector2(2, -19), new Vector2(88, -2), true);
            popupComponent.TangentLinearToggle = CreateTangentMenuToggle(tangentEditor, resources, "Linear", editor, new Vector2(2, -36), new Vector2(88, -19), true);
            popupComponent.TangentConstantToggle = CreateTangentMenuToggle(tangentEditor, resources, "Constant", editor, new Vector2(2, -53), new Vector2(88, -36), true);
            popupComponent.TangentWeightedToggle = CreateTangentMenuToggle(tangentEditor, resources, "Weighted", editor, new Vector2(2, -70), new Vector2(88, -53), true);
            popupComponent.TangentMenu = tangentEditor;
            
            tangentEditor.SetActive(false);
        }

        public static void CreateQuickActionMenu(GameObject parent, AnimationCurveEditor editor, BlackoutBuilder.Resources resources)
        {
            GameObject menu = BlackoutBuilder.CreateUIObject("Quick Actions", parent);
            BlackoutBuilder.SetRectTransform(menu.transform,
                Center,
                Center,
                Center,
                new Vector2(140, 55));

            BlackoutBuilder.CreateImage(menu, resources.standard, TextColor, true);
            
            PopupCanvas popupCanvas = menu.AddComponent<PopupCanvas>();
            popupCanvas.SortingOrder = 30002;
            popupCanvas.CreateBlockingElement = true;
            
            CurveQuickActions curveQuickActions = menu.AddComponent<CurveQuickActions>();
            curveQuickActions.Editor = editor;
            curveQuickActions.RectTransform = (RectTransform)menu.transform;
            
            curveQuickActions.NormalizeButton = CreateTangentMenuButton(menu, "Normalize", editor, new Vector2(2, -19), new Vector2(138, -2));
            curveQuickActions.FlipHorizontalButton = CreateTangentMenuButton(menu, "Flip Horizontal", editor, new Vector2(2, -36), new Vector2(138, -19));
            curveQuickActions.FlipVerticalButton = CreateTangentMenuButton(menu, "Flip Vertical", editor, new Vector2(2, -53), new Vector2(138, -36));

            editor.CurveQuickActions = menu;
            
            menu.SetActive(false);
        }
        
        private static TangentMenuButton CreateTangentMenuButton(GameObject parent, string name, AnimationCurveEditor editor, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject button = BlackoutBuilder.CreateUIObject(name, parent);
            
            BlackoutBuilder.SetRectTransform(button.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0, 1),
                new Vector2(0, 1),
                offsetMin,
                offsetMax);

            Image graphic = BlackoutBuilder.CreateImage(button, null, Color.white, true);

            GameObject text = BlackoutBuilder.CreateUIObject("Text", button);
            BlackoutBuilder.FullStretch(text.transform, new Vector2(17, 0));

            BlackoutBuilder.CreateText(text, ButtonColor, name, 12, TextAnchor.MiddleLeft);
            
            TangentMenuButton menuButton = button.AddComponent<TangentMenuButton>();
            menuButton.Editor = editor;
            menuButton.Graphic = graphic;
            return menuButton;
        }
        
        private static TangentToggleButton CreateTangentMenuToggle(GameObject parent, BlackoutBuilder.Resources resources, string name, AnimationCurveEditor editor, Vector2 offsetMin, Vector2 offsetMax, bool subMenu = false)
        {
            GameObject toggle = BlackoutBuilder.CreateUIObject(name, parent);
            
            BlackoutBuilder.SetRectTransform(toggle.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0, 1),
                new Vector2(0, 1),
                offsetMin,
                offsetMax);

            Image graphic = BlackoutBuilder.CreateImage(toggle, null, Color.white, true);
            
            TangentToggleButton toggleButton = toggle.AddComponent<TangentToggleButton>();
            toggleButton.Editor = editor;
            toggleButton.Graphic = graphic;
            
            GameObject background = BlackoutBuilder.CreateUIObject("Background", toggle);
            BlackoutBuilder.SetRectTransform(background.transform,
                Center,
                subMenu ? new Vector2(0, 0.5f) : Center,
                subMenu ? new Vector2(0, 0.5f) : Center,
                subMenu ? new Vector2(0, -7.5f) : new Vector2(-68, -7.5f),
                subMenu ? new Vector2(15, 7.5f) : new Vector2(-53, 7.5f));
            
            toggleButton.GraphicBox = BlackoutBuilder.CreateImage(background, null, TangentHoverColor, false);
            
            GameObject checkmark = BlackoutBuilder.CreateUIObject("Checkmark", background);
            BlackoutBuilder.FullStretch(checkmark.transform);
            toggleButton.GraphicCheckmark = BlackoutBuilder.CreateImage(checkmark, resources.checkmark, ButtonColor, false, Image.Type.Simple);

            GameObject text = BlackoutBuilder.CreateUIObject("Text", toggle);
            BlackoutBuilder.FullStretch(text.transform, new Vector2(17, 0));

            BlackoutBuilder.CreateText(text, ButtonColor, name, 12, TextAnchor.MiddleLeft);
            
            return toggleButton;
        }

        private static TangentFoldoutButton CreateTangentFoldoutButton(GameObject parent, string name, AnimationCurveEditor editor, KeyframeEditorPopup popupComponent, CurveTangent.Side side, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject button = BlackoutBuilder.CreateUIObject(name, parent);
            
            BlackoutBuilder.SetRectTransform(button.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0, 1),
                new Vector2(0, 1),
                offsetMin,
                offsetMax);
            
            Image graphic = BlackoutBuilder.CreateImage(button, null, Color.white, true);

            GameObject text = BlackoutBuilder.CreateUIObject("Text", button);
            BlackoutBuilder.FullStretch(text.transform, new Vector2(17, 0));

            BlackoutBuilder.CreateText(text, ButtonColor, name, 12, TextAnchor.MiddleLeft);
           
            GameObject arrow = BlackoutBuilder.CreateUIObject("Arrow", button);
            BlackoutBuilder.FullStretch(arrow.transform, new Vector2(20, 0));

            BlackoutBuilder.CreateText(arrow, ButtonColor, ">", 14, TextAnchor.MiddleRight);
            
            TangentFoldoutButton foldoutButton = button.AddComponent<TangentFoldoutButton>();
            foldoutButton.Editor = editor;
            foldoutButton.Graphic = graphic;
            foldoutButton.EditorPopup = popupComponent;
            foldoutButton.Side = side;
            return foldoutButton;
        }
        
        public static void CreateTangentMenuDivider(GameObject parent, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject button = BlackoutBuilder.CreateUIObject("Divider", parent);
            
            BlackoutBuilder.SetRectTransform(button.transform,
                new Vector2(0.5f, 1f),
                new Vector2(0, 1),
                new Vector2(0, 1),
                offsetMin,
                offsetMax);

            BlackoutBuilder.CreateImage(button, null, TransparentBlack, false);
        }
        
        public static Scrollbar SetupScrollbar(GameObject scrollbarGo, BlackoutBuilder.Resources resources)
        {
            GameObject slidingArea = BlackoutBuilder.CreateUIObject("Sliding Area", scrollbarGo);
            BlackoutBuilder.SetRectTransform(slidingArea.transform,
                Center,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            
            GameObject handle = BlackoutBuilder.CreateUIObject("Handle", slidingArea);
            BlackoutBuilder.SetRectTransform(handle.transform,
                Center,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            
            BlackoutBuilder.CreateImage(handle, resources.standard, TextColorTransparent, true);

            Scrollbar scrollbar = scrollbarGo.AddComponent<Scrollbar>();
            scrollbar.handleRect = (RectTransform)handle.transform;
            BlackoutBuilder.SetColorTransitionValues(scrollbar);

            return scrollbar;
        }

        public static void CreateInsetBottom(GameObject parent, AnimationCurveEditor editor, BlackoutBuilder.Resources resources)
        {
            GameObject bottom = BlackoutBuilder.CreateUIObject("Bottom", parent);
            BlackoutBuilder.SetRectTransform(bottom.transform,
                Center,
                Vector2.zero,
                new Vector2(1, 0),
                Vector2.zero,
                new Vector2(0, 31));
            
            BlackoutBuilder.CreateImage(bottom, null, HeaderColor, false);
            
            GameObject mask = BlackoutBuilder.CreateUIObject("Mask", bottom);
            BlackoutBuilder.FullStretch(mask.transform);
            BlackoutBuilder.CreateMask(mask, resources.mask);

            HorizontalLayoutGroup layoutGroup = mask.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.padding = new RectOffset(3, 3, 3, 3);
            layoutGroup.spacing = 5f;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            
            GameObject template = BlackoutBuilder.CreateUIObject("Template", mask);
            BlackoutBuilder.SetRectTransform(template.transform,
                Center,
                new Vector2(0, 1),
                new Vector2(0, 1),
                new Vector2(3, -28),
                new Vector2(53, -3));
            
            BlackoutBuilder.CreateImage(template, resources.standard, CurveButtonBackground, true);

            GameObject templateMask = BlackoutBuilder.CreateUIObject("Mask", template);
            BlackoutBuilder.CreateMask(templateMask, resources.mask);
            BlackoutBuilder.FullStretch(templateMask.transform);
            
            GameObject curveObject = BlackoutBuilder.CreateUIObject("Curve Image", templateMask);
            BlackoutBuilder.SetRectTransform(curveObject.transform,
                Center,
                Vector2.zero,
                Vector2.one,
                new Vector2(3, 3),
                new Vector2(-3, -3));
            
            curveObject.AddComponent<CanvasRenderer>();

            ButtonCurveRenderer renderer = curveObject.AddComponent<ButtonCurveRenderer>();
            renderer.color = CurveDefaultColor;
            renderer.LineThickness = 2;
            renderer.Content = (RectTransform)templateMask.transform;
            renderer.raycastTarget = false;
            
            AnimationCurveButton button = template.AddComponent<AnimationCurveButton>();
            button.Editor = editor;
            button.CurveRenderer = renderer;
            button.UpdateMode = AnimationCurveButton.CurveUpdateMode.None;

            template.AddComponent<GraphicHoverColor>().Graphic = renderer;
            template.SetActive(false);
            
            editor.PresetTemplate = button;
        }

        public static void CreateResizers(GameObject parent)
        {
            GameObject resizers = BlackoutBuilder.CreateUIObject("Resizers", parent);
            BlackoutBuilder.FullStretch(resizers.transform);

            ResizeCursor resizeCursor = resizers.AddComponent<ResizeCursor>();
            RectTransform windowRect = (RectTransform)parent.transform;
            
            GameObject left = BlackoutBuilder.CreateUIObject("Left", resizers);
            GameObject right = BlackoutBuilder.CreateUIObject("Right", resizers);
            GameObject top = BlackoutBuilder.CreateUIObject("Top", resizers);
            GameObject bottom = BlackoutBuilder.CreateUIObject("Bottom", resizers);
            GameObject topLeft = BlackoutBuilder.CreateUIObject("Top Left", resizers);
            GameObject topRight = BlackoutBuilder.CreateUIObject("Top Right", resizers);
            GameObject bottomLeft = BlackoutBuilder.CreateUIObject("Bottom Left", resizers);
            GameObject bottomRight = BlackoutBuilder.CreateUIObject("Bottom Right", resizers);

            BlackoutBuilder.CreateImage(left, null, ClearColor, true);
            BlackoutBuilder.CreateImage(right, null, ClearColor, true);
            BlackoutBuilder.CreateImage(top, null, ClearColor, true);
            BlackoutBuilder.CreateImage(bottom, null, ClearColor, true);
            BlackoutBuilder.CreateImage(topLeft, null, ClearColor, true);
            BlackoutBuilder.CreateImage(topRight, null, ClearColor, true);
            BlackoutBuilder.CreateImage(bottomLeft, null, ClearColor, true);
            BlackoutBuilder.CreateImage(bottomRight, null, ClearColor, true);

            ResizeHandle handle = left.AddComponent<ResizeHandle>();
            handle.CursorType = ResizeCursorType.ResizeEW;
            handle.Direction = new Vector2Int(-1, 0);
            handle.WindowRectTransform = windowRect;
            handle.ResizeCursor = resizeCursor;
            
            handle = right.AddComponent<ResizeHandle>();
            handle.CursorType = ResizeCursorType.ResizeEW;
            handle.Direction = new Vector2Int(1, 0);
            handle.WindowRectTransform = windowRect;
            handle.ResizeCursor = resizeCursor;
            
            handle = top.AddComponent<ResizeHandle>();
            handle.CursorType = ResizeCursorType.ResizeNS;
            handle.Direction = new Vector2Int(0, 1);
            handle.WindowRectTransform = windowRect;
            handle.ResizeCursor = resizeCursor;
            
            handle = bottom.AddComponent<ResizeHandle>();
            handle.CursorType = ResizeCursorType.ResizeNS;
            handle.Direction = new Vector2Int(0, -1);
            handle.WindowRectTransform = windowRect;
            handle.ResizeCursor = resizeCursor;
            
            handle = topLeft.AddComponent<ResizeHandle>();
            handle.CursorType = ResizeCursorType.ResizeNWSE;
            handle.Direction = new Vector2Int(-1, 1);
            handle.WindowRectTransform = windowRect;
            handle.ResizeCursor = resizeCursor;
            
            handle = topRight.AddComponent<ResizeHandle>();
            handle.CursorType = ResizeCursorType.ResizeNESW;
            handle.Direction = new Vector2Int(1, 1);
            handle.WindowRectTransform = windowRect;
            handle.ResizeCursor = resizeCursor;
            
            handle = bottomRight.AddComponent<ResizeHandle>();
            handle.CursorType = ResizeCursorType.ResizeNWSE;
            handle.Direction = new Vector2Int(1, -1);
            handle.WindowRectTransform = windowRect;
            handle.ResizeCursor = resizeCursor;
            
            handle = bottomLeft.AddComponent<ResizeHandle>();
            handle.CursorType = ResizeCursorType.ResizeNESW;
            handle.Direction = new Vector2Int(-1, -1);
            handle.WindowRectTransform = windowRect;
            handle.ResizeCursor = resizeCursor;
            
            BlackoutBuilder.SetRectTransform(left.transform, 
                Center, 
                new Vector2(0, 0), 
                new Vector2(0, 1), 
                new Vector2(-3, 6), 
                new Vector2(3, -6));
            
            BlackoutBuilder.SetRectTransform(right.transform,
                Center, 
                new Vector2(1, 0), 
                new Vector2(1, 1), 
                new Vector2(-3, 6), 
                new Vector2(3, -6));
            
            BlackoutBuilder.SetRectTransform(top.transform,
                Center, 
                new Vector2(0, 1), 
                new Vector2(1, 1), 
                new Vector2(6, -3), 
                new Vector2(-6, 3));
            
            BlackoutBuilder.SetRectTransform(bottom.transform,
                new Vector2(0.5f, 0f), 
                new Vector2(0, 0), 
                new Vector2(1, 0), 
                new Vector2(6, -3), 
                new Vector2(-6, 3));
            
            BlackoutBuilder.SetRectTransform(topLeft.transform,
                new Vector2(0.5f, 0f), 
                new Vector2(0, 1), 
                new Vector2(0, 1), 
                new Vector2(-3, -6), 
                new Vector2(6, 3));
            
            BlackoutBuilder.SetRectTransform(topRight.transform,
                new Vector2(0.5f, 0f), 
                new Vector2(1, 1), 
                new Vector2(1, 1), 
                new Vector2(-6, -6), 
                new Vector2(3, 3));
            
            BlackoutBuilder.SetRectTransform(bottomRight.transform,
                new Vector2(0.5f, 0f), 
                new Vector2(1, 0), 
                new Vector2(1, 0), 
                new Vector2(-6, -3), 
                new Vector2(3, 6));
            
            BlackoutBuilder.SetRectTransform(bottomLeft.transform,
                new Vector2(0.5f, 0f), 
                new Vector2(0, 0), 
                new Vector2(0, 0), 
                new Vector2(-3, -3), 
                new Vector2(6, 6));
        }
    }
}