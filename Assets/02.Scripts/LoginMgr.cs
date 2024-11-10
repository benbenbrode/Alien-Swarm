using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class LoginMgr : MonoBehaviour
{
    // UI 요소들
    public InputField idInputField;
    public InputField passwordInputField;
    public Text messageText;

    public GameObject panel1; // 로그인 패널
    public GameObject panel2; // 회원가입 패널

    // 이메일 형식을 검사하는 정규 표현식
    private readonly Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    // 로그인 버튼 클릭 시 호출되는 메서드
    public void OnLoginButtonClick()
    {
        string email = idInputField.text;
        string password = passwordInputField.text;

        // 이메일 형식 검사
        if (!IsValidEmail(email))
        {
            messageText.text = "아이디는 이메일 형식이어야 합니다.";
            return;
        }

        // 비밀번호 길이 검사
        if (password.Length < 6)
        {
            messageText.text = "비밀번호는 6자리 이상이어야 합니다.";
            return;
        }

        // 유효성 검사를 통과하면 로그인 시도
        LoginUser(email, password);
    }

    // 이메일 유효성 검사 메서드
    private bool IsValidEmail(string email)
    {
        return emailRegex.IsMatch(email);
    }

    // PlayFab 로그인 메서드
    private void LoginUser(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    // 로그인 성공 콜백
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful! UserId: " + result.PlayFabId);
        messageText.text = "로그인 성공! 로비로 이동합니다.";

        // Lobby 씬으로 이동
        SceneManager.LoadScene("Lobby");
    }

    // 로그인 실패 콜백
    private void OnLoginFailure(PlayFabError error)
    {
        messageText.text = "로그인 실패";
    }

    public void ShowPanel1()
    {
        panel1.SetActive(true);
        panel2.SetActive(false);
    }

    public void ShowPanel2()
    {
        panel1.SetActive(false);
        panel2.SetActive(true);
    }


}
