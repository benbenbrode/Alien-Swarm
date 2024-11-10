using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public enum SkillType
{
    Skill_0 = 0, // 30% 힐링
    Skill_1,     // 수류탄
    Skill_2,     // 보호막
    SkCount
}

public class GlobalValue
{
    // 유저 데이터 필드
    public static string g_Unique_ID = "";      // 유저의 고유번호
    public static string g_NickName = "";       // 유저의 별명
    public static int g_BestScore = 0;          // 게임 점수
    public static int g_UserGold = 0;           // 게임 머니
    public static int[] g_SkillCount = new int[3]; // 아이템 보유 수
    public static int g_BestFloor = 1;          // 최종 도달(클리어) 건물 층수 
    public static int g_CurFloorNum = 1;        // 현재 건물 층수
    public static float g_mouse = 1;        // 감도

    // 유저 데이터를 PlayFab에서 불러오는 메서드
    public static void LoadGameData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataLoadSuccess, OnDataLoadFailure);
    }

    // PlayFab에서 데이터 로드 성공 시 호출
    private static void OnDataLoadSuccess(GetUserDataResult result)
    {
        if (result.Data == null)
        {
            Debug.Log("No data found for the user, initializing defaults.");
            InitializeDefaultData();
            return;
        }

        // 데이터가 있는 경우 각 필드를 불러오기
        g_NickName = result.Data.ContainsKey("NickName") ? result.Data["NickName"].Value : "SBS영웅";
        g_BestScore = result.Data.ContainsKey("BestScore") ? int.Parse(result.Data["BestScore"].Value) : 0;
        g_UserGold = result.Data.ContainsKey("UserGold") ? int.Parse(result.Data["UserGold"].Value) : 0;

        // 스킬 개수 로드
        for (int i = 0; i < g_SkillCount.Length; i++)
        {
            string skillKey = $"SkItem_{i}";
            g_SkillCount[i] = result.Data.ContainsKey(skillKey) ? int.Parse(result.Data[skillKey].Value) : 1;
        }

        // 층수 정보 로드
        g_BestFloor = result.Data.ContainsKey("BestFloorNum") ? int.Parse(result.Data["BestFloorNum"].Value) : 1;
        g_CurFloorNum = result.Data.ContainsKey("CurFloorNum") ? int.Parse(result.Data["CurFloorNum"].Value) : 1;

        Debug.Log("Data loaded successfully from PlayFab.");
    }

    // PlayFab에서 데이터 로드 실패 시 호출
    private static void OnDataLoadFailure(PlayFabError error)
    {
        Debug.LogError("Failed to load data from PlayFab: " + error.GenerateErrorReport());
        InitializeDefaultData(); // 기본값 설정
    }

    // 기본값 초기화 메서드
    private static void InitializeDefaultData()
    {
        g_NickName = "SBS영웅";
        g_BestScore = 0;
        g_UserGold = 0;
        g_SkillCount = new int[] { 1, 1, 1 };
        g_BestFloor = 1;
        g_CurFloorNum = 1;

        SaveGameData(); // 기본값을 PlayFab에 저장
    }

    // 유저 데이터를 PlayFab에 저장하는 메서드
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

        // 각 스킬의 개수를 데이터에 추가
        for (int i = 0; i < g_SkillCount.Length; i++)
        {
            data[$"SkItem_{i}"] = g_SkillCount[i].ToString();
        }

        var request = new UpdateUserDataRequest { Data = data };
        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    // 데이터 저장 성공 시 호출
    private static void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Data saved successfully to PlayFab.");
    }

    // 데이터 저장 실패 시 호출
    private static void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Failed to save data to PlayFab: " + error.GenerateErrorReport());
    }


}
