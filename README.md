# Alien-Swarm


게임 장르 : 3D FPS
---

게임 소개 : 
몰려오는 외계인을 처치하며 8분후에 등장하는 최종보스를 공략하는 게임
---

게임 규칙 : 
게임이 시작되면 외계인이 주기적으로 스폰되어 플레이어를 추격합니다.
외계인은 시간이 지날수록 점점 빠르게 스폰됩니다.
플레이어의 채력이 0이 되면 게임 오버입니다.
맵에 존재하는 외계인 수가 200마리에 도달하면 게임 오버됩니다.
게임 시작 후 8분이 지나면 보스가 등잡합니다.
보스를 처치하면 게임ㅇ 클리어입니다.
---

개발 목적 : 직접 3D TPS게임을 구현해보고 다른 장르의 요소(뱀파이어 서바이벌, 랜덤디펜스)를 결합하여 나만의 게임을 만들어보기 위해
---

사용 엔진 : UNITY 2022.3.15f1
---

시연 영상 : 
---

주요 활용 기술
---
#01) PlayFab을 활용하여 회원가입, 로그인, 아이템 및 골드 저장 구현
<details>
<summary>예시 코드</summary>
  
```csharp
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


```

</details>
