using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

public class LoginMgr : MonoBehaviour
{
    // UI ��ҵ�
    public InputField idInputField;
    public InputField passwordInputField;
    public Text messageText;

    public GameObject panel1; // �α��� �г�
    public GameObject panel2; // ȸ������ �г�

    // �̸��� ������ �˻��ϴ� ���� ǥ����
    private readonly Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    // �α��� ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    public void OnLoginButtonClick()
    {
        string email = idInputField.text;
        string password = passwordInputField.text;

        // �̸��� ���� �˻�
        if (!IsValidEmail(email))
        {
            messageText.text = "���̵�� �̸��� �����̾�� �մϴ�.";
            return;
        }

        // ��й�ȣ ���� �˻�
        if (password.Length < 6)
        {
            messageText.text = "��й�ȣ�� 6�ڸ� �̻��̾�� �մϴ�.";
            return;
        }

        // ��ȿ�� �˻縦 ����ϸ� �α��� �õ�
        LoginUser(email, password);
    }

    // �̸��� ��ȿ�� �˻� �޼���
    private bool IsValidEmail(string email)
    {
        return emailRegex.IsMatch(email);
    }

    // PlayFab �α��� �޼���
    private void LoginUser(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    // �α��� ���� �ݹ�
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful! UserId: " + result.PlayFabId);
        messageText.text = "�α��� ����! �κ�� �̵��մϴ�.";

        // Lobby ������ �̵�
        SceneManager.LoadScene("Lobby");
    }

    // �α��� ���� �ݹ�
    private void OnLoginFailure(PlayFabError error)
    {
        messageText.text = "�α��� ����";
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
