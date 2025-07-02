using UnityEngine;
using UnityEngine.EventSystems;

public class ChapterScrollbar : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    //private bool isDragging = false;
    public ChapterPicker chapterPicker;

    public void OnBeginDrag(PointerEventData eventData)
    {
        //isDragging = true;
        chapterPicker.OnBeginDrag();
        //Debug.Log("开始拖动");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //isDragging = false;
        chapterPicker.OnEndDrag();
        //Debug.Log("停止拖动");
    }
}
