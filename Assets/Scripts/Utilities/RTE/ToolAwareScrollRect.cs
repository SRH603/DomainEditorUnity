using NoteEditor.Views;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 只有当 NoteEditorView.currentTool == selectToolIndex 且 按下右键 才允许拖拽滚动。
/// 滚轮（OnScroll）不受影响，始终生效。
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ToolAwareScrollRect : ScrollRect
    {
        public NoteEditorView noteEditor;   // 你自己挂引用
        public int selectToolIndex = 3;     // “选择”工具的索引

        // 滚轮保持原样，无需改动
        
        private new void Start()
        {
            var settings = GetComponent<ToolAwareSettings>();
            noteEditor      = settings.noteEditor;
            selectToolIndex   = settings.dragToolIndex;
        }

        // 1) 也让右键经过 Initialize 阶段
        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            // 只有“选择”模式下，右键才算“可拖拽”
            if (eventData.button == PointerEventData.InputButton.Right &&
                noteEditor.currentTool == selectToolIndex)
            {
                // 篡改一下，让基类以为是左键
                eventData.button = PointerEventData.InputButton.Left;
                base.OnInitializePotentialDrag(eventData);
                // 还原
                eventData.button = PointerEventData.InputButton.Right;
                return;
            }
            // 其他情况，按原逻辑（左键才行）
            //base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right &&
                noteEditor.currentTool == selectToolIndex)
            {
                eventData.button = PointerEventData.InputButton.Left;
                base.OnBeginDrag(eventData);
                eventData.button = PointerEventData.InputButton.Right;
                return;
            }
            // 其它情况，只支持左键拖
            //base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right &&
                noteEditor.currentTool == selectToolIndex)
            {
                eventData.button = PointerEventData.InputButton.Left;
                base.OnDrag(eventData);
                eventData.button = PointerEventData.InputButton.Right;
                return;
            }
            //base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right &&
                noteEditor.currentTool == selectToolIndex)
            {
                eventData.button = PointerEventData.InputButton.Left;
                base.OnEndDrag(eventData);
                eventData.button = PointerEventData.InputButton.Right;
                return;
            }
            //base.OnEndDrag(eventData);
        }
    }
