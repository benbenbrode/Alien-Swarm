using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public enum SkillType
{
    Skill_0 = 0, // 30% ����
    Skill_1,     // ����ź
    Skill_2,     // ��ȣ��
    SkCount
}

public class GlobalValue
{
    // ���� ������ �ʵ�
    public static string g_Unique_ID = "";      // ������ ������ȣ
    public static string g_NickName = "";       // ������ ����
    public static int g_BestScore = 0;          // ���� ����
    public static int g_UserGold = 0;           // ���� �Ӵ�
    public static int[] g_SkillCount = new int[3]; // ������ ���� ��
    public static int g_BestFloor = 1;          // ���� ����(Ŭ����) �ǹ� ���� 
    public static int g_CurFloorNum = 1;        // ���� �ǹ� ����
    public static float g_mouse = 1;        // ����

    // ���� �����͸� PlayFab���� �ҷ����� �޼���
    public static void LoadGameData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataLoadSuccess, OnDataLoadFailure);
    }

    // PlayFab���� ������ �ε� ���� �� ȣ��
    private static void OnDataLoadSuccess(GetUserDataResult result)
    {
        if (result.Data == null)
        {
            Debug.Log("No data found for the user, initializing defaults.");
            InitializeDefaultData();
            return;
        }

        // �����Ͱ� �ִ� ��� �� �ʵ带 �ҷ�����
        g_NickName = result.Data.ContainsKey("NickName") ? result.Data["NickName"].Value : "SBS����";
        g_BestScore = result.Data.ContainsKey("BestScore") ? int.Parse(result.Data["BestScore"].Value) : 0;
        g_UserGold = result.Data.ContainsKey("UserGold") ? int.Parse(result.Data["UserGold"].Value) : 0;

        // ��ų ���� �ε�
        for (int i = 0; i < g_SkillCount.Length; i++)
        {
            string skillKey = $"SkItem_{i}";
            g_SkillCount[i] = result.Data.ContainsKey(skillKey) ? int.Parse(result.Data[skillKey].Value) : 1;
        }

        // ���� ���� �ε�
        g_BestFloor = result.Data.ContainsKey("BestFloorNum") ? int.Parse(result.Data["BestFloorNum"].Value) : 1;
        g_CurFloorNum = result.Data.ContainsKey("CurFloorNum") ? int.Parse(result.Data["CurFloorNum"].Value) : 1;

        Debug.Log("Data loaded successfully from PlayFab.");
    }

    // PlayFab���� ������ �ε� ���� �� ȣ��
    private static void OnDataLoadFailure(PlayFabError error)
    {
        Debug.LogError("Failed to load data from PlayFab: " + error.GenerateErrorReport());
        InitializeDefaultData(); // �⺻�� ����
    }

    // �⺻�� �ʱ�ȭ �޼���
    private static void InitializeDefaultData()
    {
        g_NickName = "SBS����";
        g_BestScore = 0;
        g_UserGold = 0;
        g_SkillCount = new int[] { 1, 1, 1 };
        g_BestFloor = 1;
        g_CurFloorNum = 1;

        SaveGameData(); // �⺻���� PlayFab�� ����
    }

    // ���� �����͸� PlayFab�� �����ϴ� �޼���
    public static void SaveGameData()
    {
        var data = new Dictionary<string, string>
        {
            { "NickName", g_NickName },
            { "BestScore", g_BestScore.ToString() },
            { "UserGold", g_UserGold.ToString() },
            { "BestFloorNum", g_BestFloor.ToString() },
            { "CurFloorNum", g_CurFloorNum.ToString() }
        };

        // �� ��ų�� ������ �����Ϳ� �߰�
        for (int i = 0; i < g_SkillCount.Length; i++)
        {
            data[$"SkItem_{i}"] = g_SkillCount[i].ToString();
        }

        var request = new UpdateUserDataRequest { Data = data };
        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    // ������ ���� ���� �� ȣ��
    private static void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Data saved successfully to PlayFab.");
    }

    // ������ ���� ���� �� ȣ��
    private static void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Failed to save data to PlayFab: " + error.GenerateErrorReport());
    }


}
