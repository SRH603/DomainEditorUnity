using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchIndicator : MonoBehaviour
{
    public GameObject touchIndicatorPrefab;
    private Canvas canvas;
    public Toggle toggle;
    public GameObject Console;
    //public MeshRenderer[] meshRenderers = new MeshRenderer[3];
    private List<GameObject> currentIndicators = new List<GameObject>();

    void Start()
    {
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in the scene.");
        }
    }

    void Update()
    {
        // �Ƴ���һ֡�Ĵ��ص�
        foreach (var indicator in currentIndicators)
        {
            Destroy(indicator);
        }
        currentIndicators.Clear();
        if (toggle.isOn == true)
        {
            Console.SetActive(true);
            // ����������
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
                {
                    CreateIndicatorAtPosition(touch.position);
                }
            }

            // �����������
            if (Input.mousePresent && Input.GetMouseButton(0)) // ����������
            {
                CreateIndicatorAtPosition(Input.mousePosition);
            }

        }
        else
        {
            Console.SetActive(false);
        }
            
    }

    void CreateIndicatorAtPosition(Vector2 screenPosition)
    {
        if (canvas == null) return;

        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPosition, canvas.worldCamera, out canvasPos);

        GameObject touchIndicator = Instantiate(touchIndicatorPrefab, canvas.transform);
        (touchIndicator.transform as RectTransform).anchoredPosition = canvasPos;
        // ����Raycast Target
        Image imageComponent = touchIndicator.GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.raycastTarget = false;
        }
        currentIndicators.Add(touchIndicator);
    }
}
