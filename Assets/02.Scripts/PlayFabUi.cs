using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class PlayFabUi : MonoBehaviour
{
    // UI ��ҵ�
    public InputField idInputField;
    public InputField passwordInputField;
    public InputField nicknameInputField;
    public Text warningText;

    // �̸��� ������ �˻��ϴ� ���� ǥ����
    private readonly Regex emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    // ȸ������ ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    public void OnRegisterButtonClick()
    {
        string email = idInputField.text;
        string password = passwordInputField.text;
        string nickname = nicknameInputField.text;

        // �Է� ��ȿ�� �˻�
        if (!IsValidEmail(email))
        {
            warningText.text = "���̵�� �̸��� �����̾�� �մϴ�.";
            return;
        }

        if (password.Length < 6)
        {
            warningText.text = "��й�ȣ�� 6�ڸ� �̻��̾�� �մϴ�.";
            return;
        }

        if (string.IsNullOrEmpty(nickname))
        {
            warningText.text = "�г����� �Է����ּ���.";
            return;
        }

        // ��ȿ�� �˻縦 ����ϸ� PlayFab�� ȸ������ ��û
        RegisterUser(email, password, nickname);
    }

    // �̸��� ��ȿ�� �˻� �޼���
    private bool IsValidEmail(string email)
    {
        return emailRegex.IsMatch(email);
    }

    // PlayFab ȸ������ �޼���
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

    // ȸ������ ���� �ݹ�
    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        warningText.text = "ȸ������ ����!";
        Debug.Log("Registration successful! UserId: " + result.PlayFabId);

        // �ʱ� ������ ���� (���� ����, ���� �Ӵ�, ��ų ����, �г���)
        SaveInitialData();

        ClearInputFields();
    }

    // �ʱ� �����͸� PlayFab�� �����ϴ� �޼���
    private void SaveInitialData()
    {
        var data = new Dictionary<string, string>
        {
            { "BestScore", "0" },             // ���� ���� �ʱⰪ
            { "UserGold", "0" },              // ���� �Ӵ� �ʱⰪ
            { "SkItem_0", "1" },              // ��ų 1 �ʱⰪ
            { "SkItem_1", "1" },              // ��ų 2 �ʱⰪ
            { "SkItem_2", "1" },              // ��ų 3 �ʱⰪ
            { "NickName", nicknameInputField.text } // �Է¹��� �г��� ����
        };

        var request = new UpdateUserDataRequest { Data = data };
        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    // ������ ���� ���� �� ȣ��
    private void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Initial data saved successfully to PlayFab.");
    }

    // ������ ���� ���� �� ȣ��
    private void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Failed to save initial data to PlayFab: " + error.GenerateErrorReport());
    }

    // ȸ������ ���� �ݹ�
    private void OnRegisterFailure(PlayFabError error)
    {
        // Check if the username is unavailable or another game error indicating username duplication
        if (error.Error == PlayFabErrorCode.UsernameNotAvailable ||
            error.Error == PlayFabErrorCode.EmailAddressNotAvailable) // Add other relevant error codes as needed
        {
            warningText.text = "�ߺ��� ���̵� �ֽ��ϴ�.";
        }
        else
        {
            warningText.text = "ȸ������ ����";
        }

        Debug.LogError("Registration failed: " + error.GenerateErrorReport());
        ClearInputFields();
    }

    // �Է� �ʵ带 �ʱ�ȭ�ϴ� �޼���
    private void ClearInputFields()
    {
        idInputField.text = string.Empty;
        passwordInputField.text = string.Empty;
        nicknameInputField.text = string.Empty;
    }

}
