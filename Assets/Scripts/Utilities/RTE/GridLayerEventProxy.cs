using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NoteEditor.Views; // 调整到你实际命名空间

public class GridLayerEventProxy :
    MonoBehaviour,
    IPointerClickHandler,
    IInitializePotentialDragHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("拖拽转发目标")]
    public ScrollRect scrollRect;          // 拖到你的 ScrollRect
    public NoteEditorView noteEditor;      // 拖到 NoteEditorView 脚本
    [Tooltip("选择工具的索引")]
    public int selectToolIndex = 3;

    // —— 左键点击：生成 Note —— 
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left &&
            noteEditor.currentTool != selectToolIndex)
        {
            // 调用你原来的创建逻辑
            noteEditor.OnGridClicked(eventData);
        }
    }

    // —— 右键拖拽：转发给 ScrollRect —— 
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        scrollRect.OnInitializePotentialDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        scrollRect.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        scrollRect.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        scrollRect.OnEndDrag(eventData);
    }
}