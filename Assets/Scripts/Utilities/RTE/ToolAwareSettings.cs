using NoteEditor.Views;
using UnityEngine;

[DisallowMultipleComponent]
public class ToolAwareSettings : MonoBehaviour
{
    [Header("ToolAware 参数")]
    public NoteEditorView noteEditor;
    public int dragToolIndex = 3;
}