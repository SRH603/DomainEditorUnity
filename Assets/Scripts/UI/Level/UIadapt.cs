using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIadapt : MonoBehaviour
{

    public AspectRatioFitter UIImage;
    public Canvas canvas;
    public float Ratio = 1.41f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // 获取Canvas组件
        
        if (canvas != null)
        {
            // 获取Canvas的RectTransform
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            // 获取画布的宽度和高度
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            Ratio = canvasWidth / canvasHeight;

            // 输出画布的宽度和高度
            //Debug.Log("Canvas Width: " + canvasWidth);
            //Debug.Log("Canvas Height: " + canvasHeight);


        }
        else
        {
            //Debug.LogError("Canvas component not found on this GameObject.");
        }

        if (Ratio <= 1.6)
        {
            UIImage.aspectRatio = Ratio;
        }
        else
        {
            UIImage.aspectRatio = Mathf.Log(Ratio - 0.6f) * Mathf.Log(Ratio - 0.6f) + 1.6f;
        }

        
    }
}
