using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;  // 싱글턴 인스턴스

    private void Awake()
    {
        // 이미 인스턴스가 존재하면 자신을 파괴하고, 아니면 유지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 변경 시 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject);  // 중복된 매니저가 있을 경우 자신을 파괴
        }
    }

    // 1. 회원가입 메서드
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

    // 2. 로그인 메서드
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

    // 3. 아이템 개수를 저장하는 메서드
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
