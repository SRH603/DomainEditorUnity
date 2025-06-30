using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortordUI : MonoBehaviour
{

    public int sortord; // 0 默认，1 字母，2 难度，3 成绩，4 版本
    public GameObject[] Sortordtypeface;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Updatrbutton(int buttonindex)
    {
        for (int i = 0; i <= Sortordtypeface.Length - 1; ++i)
        {
            if(i == buttonindex) Sortordtypeface[i].gameObject.SetActive(true);
            else Sortordtypeface[i].gameObject.SetActive(false);
        }
    }

    public void Click()
    {
        if (sortord == Sortordtypeface.Length - 1)
        {
            sortord = 0;
        }
        else
        {
            ++sortord;
        }
        Updatrbutton(sortord);
    }

}
