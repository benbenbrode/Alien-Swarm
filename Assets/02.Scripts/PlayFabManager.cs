using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;  // �̱��� �ν��Ͻ�

    private void Awake()
    {
        // �̹� �ν��Ͻ��� �����ϸ� �ڽ��� �ı��ϰ�, �ƴϸ� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // �� ���� �� �ı����� �ʵ��� ����
        }
        else
        {
            Destroy(gameObject);  // �ߺ��� �Ŵ����� ���� ��� �ڽ��� �ı�
        }
    }

    // 1. ȸ������ �޼���
    public void RegisterUser(string username, string email, string password)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Email = email,
            Password = password,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Registration successful! UserId: " + result.PlayFabId);
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogError("Registration failed: " + error.GenerateErrorReport());
    }

    // 2. �α��� �޼���
    public void LoginUser(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful! UserId: " + result.PlayFabId);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }

    // 3. ������ ������ �����ϴ� �޼���
    public void SaveItemCounts(int itemACount, int itemBCount, int itemCCount)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "ItemA", itemACount.ToString() },
                { "ItemB", itemBCount.ToString() },
                { "ItemC", itemCCount.ToString() }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    private void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Item counts saved successfully!");
    }

    private void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Failed to save item counts: " + error.GenerateErrorReport());
    }
}
