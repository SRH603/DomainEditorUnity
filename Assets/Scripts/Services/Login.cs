using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
//using UnityEditor.PackageManager;
using UnityEngine.UI;


public class Login : MonoBehaviour
{
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;

    public Button loginButton;
    public Button registerButton;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //LoginWithCustomID();
        registerButton.onClick.AddListener(RegisterWithEmailAddress);
        loginButton.onClick.AddListener(LoginWithEmailAddress);

    }

    void LoginWithCustomID()
    {
        var request = new LoginWithCustomIDRequest()
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log(result.PlayFabId);
        Debug.Log(result.NewlyCreated);
        Debug.Log("登录成功");
    }

    private void OnLoginFailure(PlayFabError result)
    {
        Debug.Log("登录失败");
    }

    void RegisterWithEmailAddress()
    {
        var request = new RegisterPlayFabUserRequest()
        {
            Email = emailInputField.text,
            Password = passwordInputField.text,
            RequireBothUsernameAndEmail = false  // 启用邮箱验证
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }
    // 发送验证邮件
    private void SendVerificationEmail(string email)
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = email,
            TitleId = PlayFabSettings.TitleId // 必须提供 TitleId
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnEmailSentSuccess, OnEmailSentFailure);
    }
    private void OnEmailSentSuccess(SendAccountRecoveryEmailResult result)
    {
        Debug.Log("邮箱验证邮件已发送！");
    }

    private void OnEmailSentFailure(PlayFabError error)
    {
        Debug.LogError("发送验证邮件失败: " + error.GenerateErrorReport());
    }
    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("注册成功");
        // 发送验证邮件
        SendVerificationEmail(emailInputField.text);
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogError("注册失败" + error.ErrorMessage);
    }

    private void LoginWithEmailAddress()
    {
        var request = new LoginWithEmailAddressRequest()
        {
            Email = emailInputField.text,
            Password = passwordInputField.text,
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginEmailSuccess, OnLoginEmailFailure);
    }

    private void OnLoginEmailSuccess(LoginResult result)
    {
        var PlayFabId = result.PlayFabId;
        var NewlyCreated = result.NewlyCreated;
        var str = string.Format("PlayFabId:{0}, NewlyCreated:{1}",PlayFabId ,NewlyCreated);
        InitializePlayerArchiveData();
        Debug.Log("登录成功" + str);
    }

    private void OnLoginEmailFailure(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage);
    }
    
    // 用于客户端调用 Cloud Script 以初始化玩家存档
    public void InitializePlayerArchiveData()
    {
        // 获取当前玩家的 PlayFabId
        string playerId = PlayFabSettings.staticPlayer.PlayFabId;

        // 创建 Cloud Script 请求
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "createArchive",  // Cloud Script 中定义的函数名称
            FunctionParameter = new { PlayFabId = playerId },  // 传递当前玩家的 PlayFabId
            GeneratePlayStreamEvent = true  // 生成 PlayStream 事件
        };

        // 调用 PlayFab Cloud Script
        PlayFabClientAPI.ExecuteCloudScript(request, OnCloudScriptSuccess, OnCloudScriptError);
    }

    // 成功回调
    private void OnCloudScriptSuccess(ExecuteCloudScriptResult result)
    {
        Debug.Log("Cloud Script executed successfully! Player archive data initialized.");
    }

    // 错误回调
    private void OnCloudScriptError(PlayFabError error)
    {
        Debug.LogError("Error executing Cloud Script: " + error.GenerateErrorReport());
    }
}
