using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class ContentManage : MonoBehaviour
{
    public Button addButton; // 用于关联UI按钮
    private string currencyCode = "EC"; // 这里是你在PlayFab中创建的货币代码

    void Start()
    {

        // 监听按钮点击事件
        addButton.onClick.AddListener(OnAddCurrencyClicked);
    }
    void OnAddCurrencyClicked()
    {
        // 创建货币请求
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = currencyCode,
            Amount = 10 // 增加 10 个货币
        };

        // 调用PlayFab API来添加货币
        PlayFabClientAPI.AddUserVirtualCurrency(request, OnCurrencyAdded, OnError);
    }

    // 货币添加成功回调
    void OnCurrencyAdded(ModifyUserVirtualCurrencyResult result)
    {
        Debug.Log("Successfully added currency! New balance: " + result.Balance);
    }

    // 错误回调
    void OnError(PlayFabError error)
    {
        Debug.LogError("Error adding currency: " + error.GenerateErrorReport());
    }
    
}
