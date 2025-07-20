using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubdivWidget : MonoBehaviour
{
    [SerializeField] Button leftBtn;
    [SerializeField] Button rightBtn;
    [SerializeField] TMP_InputField input;
    [SerializeField] GridGraphic targetGrid;
    public bool isHorizontal = true;          // true = 横线细分，否则竖线

    [SerializeField] int min = 1;
    [SerializeField] int max = 12;

    void Awake()
    {
        leftBtn.onClick.AddListener(() => Change(-1));
        rightBtn.onClick.AddListener(() => Change(+1));
        input.onEndEdit.AddListener(SetFromField);
        RefreshField();
    }

    void Change(int delta)
    {
        int value = GetValue();
        value = Mathf.Clamp(value + delta, min, max);
        SetValue(value);
    }

    void SetFromField(string s)
    {
        if (int.TryParse(s, out int v))
        {
            v = Mathf.Clamp(v, min, max);
            SetValue(v);
        }
        else RefreshField();
    }

    int GetValue() => isHorizontal ? targetGrid.hSubdiv : targetGrid.vSubdiv;

    void SetValue(int v)
    {
        if (isHorizontal) targetGrid.hSubdiv = v;
        else              targetGrid.vSubdiv = v;
        targetGrid.RebuildGrid();
        RefreshField();
    }

    void RefreshField() => input.text = GetValue().ToString();
}