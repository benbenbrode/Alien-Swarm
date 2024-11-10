using System.Collections.Generic;
using UnityEngine;

public class AttriAction : MonoBehaviour
{
    public GameObject panel; // attri��� �̸��� �ǳ�
    public GameObject Player;
    public GameObject mgr;
    private List<GameObject> prefabList;
    public Transform[] spawnPoints = new Transform[3];
    public GameObject turret;
    public GameObject mapmgr;
    public void Start()
    {
        panel = GameObject.Find("Canvas/attri");
        Player = GameObject.Find("Player");
        mgr = GameObject.Find("GameMgr");
        mapmgr = GameObject.Find("mgr");
        prefabList = mgr.GetComponent<AttributeManager>().prefabList;
        turret = Resources.Load<GameObject>("turret_main");
        spawnPoints[0] = GameObject.Find("Turretpos1")?.transform;
        spawnPoints[1] = GameObject.Find("Turretpos2")?.transform;
        spawnPoints[2] = GameObject.Find("Turretpos3")?.transform;
    }
    public void OnButtonClick()
    {
        if (gameObject.name.Contains("attri_jump"))
        {
            Player.GetComponent<PlayerCtrl>().m_JumpPower = 30;
            Player.GetComponent<PlayerCtrl>().m_GravitySpeed = 40;
            prefabList.RemoveAll(obj => obj != null && obj.name.Contains("attri_jump"));
        }
        else if (gameObject.name.Contains("attri_atkup"))
        {
            Player.GetComponent<PlayerCtrl>().power += 25;
        }
        else if (gameObject.name.Contains("attri_fireup"))
        {
            Player.GetComponent<PlayerCtrl>().firespeed -= 0.05f;
            if (Player.GetComponent<PlayerCtrl>().firespeed < 0.15) // ���� ���׼�
            {
                prefabList.RemoveAll(obj => obj != null && obj.name.Contains("attri_fireup"));
            }
        }
        else if (gameObject.name.Contains("attri_speedup"))
        {
            Player.GetComponent<PlayerCtrl>().moveSpeed += 2f;
        }
        else if (gameObject.name.Contains("attri_double"))
        {
            Player.GetComponent<PlayerCtrl>().doubleshot = true;
            prefabList.RemoveAll(obj => obj != null && obj.name.Contains("attri_double"));
        }
        else if (gameObject.name.Contains("attri_blood"))
        {
            Player.GetComponent<PlayerCtrl>().blood = true;
            prefabList.RemoveAll(obj => obj != null && obj.name.Contains("attri_blood"));
        }
        else if (gameObject.name.Contains("attri_alldie"))
        {
            // "MONSTER" �±׸� ���� ��� ���� ������Ʈ�� ������
            GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

            foreach (GameObject monster in monsters)
            {
                // "boss" �̸��� ���Ե� ������Ʈ�� ����
                if (monster.name.Contains("boss"))
                {
                    continue;
                }

                // Ȱ��ȭ�� ������Ʈ�� ó��
                if (monster.activeInHierarchy)
                {
                    // MonsterCtrl ��ũ��Ʈ�� ���� ���
                    MonsterCtrl monsterCtrl = monster.GetComponent<MonsterCtrl>();
                    if (monsterCtrl != null)
                    {
                        monsterCtrl.MonsterDie();
                    }

                    // GoldMonsterCtrl ��ũ��Ʈ�� ���� ���
                    GoldMonsterCtrl goldMonsterCtrl = monster.GetComponent<GoldMonsterCtrl>();
                    if (goldMonsterCtrl != null)
                    {
                        goldMonsterCtrl.MonsterDie();
                    }
                }
            }
        }

        else if (gameObject.name.Contains("attri_heal"))
        {
            GlobalValue.g_SkillCount[0]++;
            string a_MkKey = "SkItem_" + 0.ToString();
            PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[0]);
            mgr.GetComponent<GameMgr>().RefreshGameUI();
        }
        else if (gameObject.name.Contains("attri_exploxion"))
        {
            GlobalValue.g_SkillCount[1]++;
            string a_MkKey = "SkItem_" + 0.ToString();
            PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[1]);
            mgr.GetComponent<GameMgr>().RefreshGameUI();
        }
        else if (gameObject.name.Contains("attri_shiled"))
        {
            GlobalValue.g_SkillCount[2]++;
            string a_MkKey = "SkItem_" + 0.ToString();
            PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[2]);
            mgr.GetComponent<GameMgr>().RefreshGameUI();
        }
        else if (gameObject.name.Contains("attri_grshot"))
        {
            Player.GetComponent<PlayerCtrl>().grshot = true;
            prefabList.RemoveAll(obj => obj != null && obj.name.Contains("attri_grshot"));
        }
        else if (gameObject.name.Contains("attri_turret"))
        {
            int spawnIndex = Player.GetComponent<PlayerCtrl>().TIndex;
            if (spawnIndex == 0)
            {
                GameObject atkPrefab = Resources.Load<GameObject>("attri_tatk");
                GameObject firePrefab = Resources.Load<GameObject>("attri_tfire");
                if (atkPrefab != null) prefabList.Add(atkPrefab);
                if (firePrefab != null) prefabList.Add(firePrefab);
            }
            if (spawnIndex > 1)
            {
                prefabList.RemoveAll(obj => obj != null && obj.name.Contains("attri_turret"));

            }
            Instantiate(turret, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
            Player.GetComponent<PlayerCtrl>().TIndex += 1;

        }
        else if (gameObject.name.Contains("attri_tfire"))
        {
            Player.GetComponent<PlayerCtrl>().Tfirespeed -= 0.05f;
            if (Player.GetComponent<PlayerCtrl>().Tfirespeed < 0.3) // ���� ���׼�
            {
                prefabList.RemoveAll(obj => obj != null && obj.name.Contains("attri_tfire"));
            }
        }
        else if (gameObject.name.Contains("attri_tatk"))
        {
            Player.GetComponent<PlayerCtrl>().Tpower += 25;
        }
        else if (gameObject.name.Contains("attri_goldmon"))
        {
            mapmgr.GetComponent<GoldeSw>().spawnInterval = mapmgr.GetComponent<GoldeSw>().spawnInterval * 0.8f;

            if (mapmgr.GetComponent<GoldeSw>().spawnInterval < 10f)
                prefabList.RemoveAll(obj => obj != null && obj.name.Contains("attri_goldmon"));
        }
        // 1. attri �±׸� ���� ��� ������Ʈ ����
        GameObject[] objectsToDelete = GameObject.FindGameObjectsWithTag("attri");
        foreach (GameObject obj in objectsToDelete)
        {
            Destroy(obj);
        }

        // 2. �ǳ� ��Ȱ��ȭ
        if (panel != null)
        {
            panel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Panel not assigned.");
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // 3. Ÿ�ӽ������� 1�� �����Ͽ� ���� �簳
        Time.timeScale = 1.0f;
    }
}
