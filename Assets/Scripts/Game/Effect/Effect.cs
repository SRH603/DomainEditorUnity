using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public float DestroyTime;
    // Start is called before the first frame update
    void Start()
    {
        // 调用 Destroy 方法并延迟 3 秒后执行
        Invoke("DestroyObject", DestroyTime);
    }
    void DestroyObject()
    {
        // 销毁当前物体
        Destroy(gameObject);
    }
}
