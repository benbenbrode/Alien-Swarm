using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class PlayFabUi : MonoBehaviour
{
    // UI 요소들
    public InputField idInputField;
    public InputField passwordInputField;
    public InputField nicknameInputField;
    public Text warningText;

    // 이메일 형식을 검사하는 정규 표현식
    private readonly Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    // 회원가입 버튼 클릭 시 호출되는 메서드
    public void OnRegisterButtonClick()
    {
        string email = idInputField.text;
        string password = passwordInputField.text;
        string nickname = nicknameInputField.text;

        // 입력 유효성 검사
        if (!IsValidEmail(email))
        {
            warningText.text = "아이디는 이메일 형식이어야 합니다.";
            return;
        }

        if (password.Length < 6)
        {
            warningText.text = "비밀번호는 6자리 이상이어야 합니다.";
            return;
        }

        if (string.IsNullOrEmpty(nickname))
        {
            warningText.text = "닉네임을 입력해주세요.";
            return;
        }

        // 유효성 검사를 통과하면 PlayFab에 회원가입 요청
        RegisterUser(email, password, nickname);
    }

    // 이메일 유효성 검사 메서드
    private bool IsValidEmail(string email)
    {
        return emailRegex.IsMatch(email);
    }

    // PlayFab 회원가입 메서드
    private void RegisterUser(string email, string password, string nickname)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Username = nickname,
            Email = email,
            Password = password,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }

    // 회원가입 성공 콜백
    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        warningText.text = "회원가입 성공!";
        Debug.Log("Registration successful! UserId: " + result.PlayFabId);

        // 초기 데이터 설정 (게임 점수, 게임 머니, 스킬 개수, 닉네임)
        SaveInitialData();

        ClearInputFields();
    }

    // 초기 데이터를 PlayFab에 저장하는 메서드
    private void SaveInitialData()
    {
        var data = new Dictionary<string, string>
        {
            { "BestScore", "0" },             // 게임 점수 초기값
            { "UserGold", "0" },              // 게임 머니 초기값
            { "SkItem_0", "1" },              // 스킬 1 초기값
            { "SkItem_1", "1" },              // 스킬 2 초기값
            { "SkItem_2", "1" },              // 스킬 3 초기값
            { "NickName", nicknameInputField.text } // 입력받은 닉네임 저장
        };

        var request = new UpdateUserDataRequest { Data = data };
        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    // 데이터 저장 성공 시 호출
    private void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Initial data saved successfully to PlayFab.");
    }

    // 데이터 저장 실패 시 호출
    private void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Failed to save initial data to PlayFab: " + error.GenerateErrorReport());
    }

    // 회원가입 실패 콜백
    private void OnRegisterFailure(PlayFabError error)
    {
        // Check if the username is unavailable or another game error indicating username duplication
        if (error.Error == PlayFabErrorCode.UsernameNotAvailable ||
            error.Error == PlayFabErrorCode.EmailAddressNotAvailable) // Add other relevant error codes as needed
        {
            warningText.text = "중복된 아이디가 있습니다.";
        }
        else
        {
            warningText.text = "회원가입 실패";
        }

        Debug.LogError("Registration failed: " + error.GenerateErrorReport());
        ClearInputFields();
    }

    // 입력 필드를 초기화하는 메서드
    private void ClearInputFields()
    {
        idInputField.text = string.Empty;
        passwordInputField.text = string.Empty;
        nicknameInputField.text = string.Empty;
    }

}
