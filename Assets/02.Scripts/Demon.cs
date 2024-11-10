using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Demon : MonoBehaviour
{
    //������ ���� ������ �ִ� Enumerable ���� ����
    public enum MonsterState { idle, trace, attack, die };

    //������ ���� ���� ������ ������ Enum ����
    public MonsterState monsterState = MonsterState.idle;

    //�ӵ� ����� ���� ���� ������Ʈ�� ������ �Ҵ�
    private Transform monsterTr;
    private Transform playerTr;

    //���� �����Ÿ�
    public float traceDist = 10.0f;
    //���� �����Ÿ�
    public float attackDist = 1.5f;

    //������ ��� ����
    private bool isDie = false;

    //���� ȿ�� ������
    public GameObject bloodEffect;
    //���� ��Į ȿ�� ������
    public GameObject bloodDecal;

    //���� ���� ����
    private int hp = 1;
    Rigidbody m_Rigid = null;

    //�Ѿ� �߻� ���� ����
    public GameObject bullet;
    float m_BLTime = 0.0f;
    LayerMask m_LaserMask = -1;

    public GameObject Player;

    void Awake()
    {
        traceDist = 10000.0f;
        attackDist = 1.5f;

        //������ Transform �Ҵ�
        monsterTr = this.gameObject.GetComponent<Transform>();
        //���� ����� Player�� Transform �Ҵ�
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();

        m_Rigid = GetComponent<Rigidbody>();
        Player = GameObject.Find("Player");
    }

    void Start()
    {
        m_LaserMask = 1 << LayerMask.NameToLayer("Default");
    }

    void FixedUpdate()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        if (playerTr.gameObject.activeSelf == false)
            playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();

        CheckMonStateUpdate();
        MonActionUpdate();
    }

    #region -- ���� AI

    float m_AI_Delay = 0.0f;

    void CheckMonStateUpdate()
    {
        if (isDie == true)
            return;

        m_AI_Delay -= Time.deltaTime;
        if (0.0f < m_AI_Delay)
            return;

        m_AI_Delay = 0.1f;

        //���Ϳ� �÷��̾� ������ �Ÿ� ����
        float dist = Vector3.Distance(playerTr.position, monsterTr.position);
        float Ydist = Mathf.Abs(playerTr.position.y - monsterTr.position.y);

        if (dist <= attackDist)
        {
            monsterState = MonsterState.attack;
        }
        else if (dist <= traceDist && Ydist < 20.0f)
        {
            monsterState = MonsterState.trace;
        }
        else
        {
            monsterState = MonsterState.idle;
        }
    }

    void MonActionUpdate()
    {
        if (isDie == true)
            return;

        switch (monsterState)
        {
            case MonsterState.idle:
                break;

            case MonsterState.trace:
                {
                    // ���� �̵� ����
                    float a_MoveVelocity = 1.0f; // �ʴ� �̵� �ӵ�
                    Vector3 a_MoveDir = playerTr.position - transform.position; // y�� ������ �̵� ����

                    if (0.0f < a_MoveDir.magnitude)
                    {
                        // �̵� ���� ��� �� �̵�
                        Vector3 a_StepVec = a_MoveDir.normalized * a_MoveVelocity * Time.deltaTime;
                        transform.Translate(a_StepVec, Space.World);

                        // �̵� ������ �ٶ󺸵��� ȸ�� ó��
                        float a_RotSpeed = 7.0f; // �ʴ� ȸ�� �ӵ�
                        Quaternion a_TargetRot = Quaternion.LookRotation(a_MoveDir.normalized);
                        transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * a_RotSpeed);
                    }
                }
                break;

            case MonsterState.attack:
                {
                    float a_RotSpeed = 6.0f;
                    Vector3 a_CacDir = playerTr.position - transform.position; // y�� ������ ���� ���

                    if (0.0f < a_CacDir.magnitude)
                    {
                        // ���� �Ÿ����� �־��� ��� ��ġ ����
                        if (attackDist < a_CacDir.magnitude)
                        {
                            float a_MoveVelocity = 2.0f;
                            Vector3 a_StepVec = a_CacDir.normalized * a_MoveVelocity * Time.deltaTime;
                            transform.Translate(a_StepVec, Space.World);
                        }

                        // �ٶ󺸴� ���� ����
                        Quaternion a_TargetRot = Quaternion.LookRotation(a_CacDir.normalized);
                        transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * a_RotSpeed);
                    }
                }
                break;
        }

        FireUpdate();
    }


    #endregion

    void FireUpdate()
    {
        Vector3 a_PlayerPos = playerTr.position;
        a_PlayerPos.y = a_PlayerPos.y + 1.5f;
        Vector3 a_MonPos = transform.position;
        a_MonPos.y = a_MonPos.y + 1.5f;
        Vector3 a_CacDir = a_PlayerPos - a_MonPos;
        float a_RayUDLimit = 3.5f;

        m_BLTime -= Time.deltaTime;
        if (m_BLTime <= 0.0f)
        {
            m_BLTime = 0.0f;
        }

        if ((-a_RayUDLimit <= a_CacDir.y && a_CacDir.y <= a_RayUDLimit) == false)
            return;

        bool IsRayOk = false;
        if (Physics.Raycast(a_MonPos, a_CacDir.normalized, out RaycastHit hit, 100.0f, m_LaserMask))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                IsRayOk = true;
            }
        }

        if (IsRayOk == false)
            return;

        Vector3 a_CacVLen = playerTr.position - transform.position;
        a_CacVLen.y = 0.0f;
        Quaternion a_TargetRot = Quaternion.LookRotation(a_CacVLen.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * 6.0f);

        if (m_BLTime <= 0.0f)
        {
            Vector3 a_StartPos = a_MonPos + a_CacDir.normalized * 1.5f;
            GameObject a_Bullet = Instantiate(bullet, a_StartPos, Quaternion.identity);
            a_Bullet.layer = LayerMask.NameToLayer("E_BULLET");
            a_Bullet.tag = "E_BULLET";
            a_Bullet.transform.forward = a_CacDir.normalized;

            float a_CacDf = (GlobalValue.g_CurFloorNum - 10) * 0.012f;
            a_CacDf = Mathf.Clamp(a_CacDf, 0.0f, 1.0f);

            m_BLTime = 2.0f - a_CacDf;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "BULLET")
        {
            CreateBloodEffect(coll.transform.position);

            hp -= Player.GetComponent<PlayerCtrl>().power;
            if (hp <= 0)
            {
                MonsterDie();
            }

            Destroy(coll.gameObject);
        }

        if (coll.gameObject.tag == "TBULLET")
        {
            CreateBloodEffect(coll.transform.position);

            hp -= Player.GetComponent<PlayerCtrl>().Tpower;
            if (hp <= 0)
            {
                MonsterDie();
            }

            Destroy(coll.gameObject);
        }
    }

    public void MonsterDie()
    {
        if (Player.GetComponent<PlayerCtrl>().blood == true)
        {
            Player.GetComponent<PlayerCtrl>().hp += 0.2f;
            Player.GetComponent<PlayerCtrl>().hpui();
        }
        //��� �ڷ�ƾ�� ����
        StopAllCoroutines();
        Destroy(gameObject);
        GameMgr.Inst.DispScore(10);

    }//void MonsterDie()

 
    

    IEnumerator PushObjectPool()
    {
        yield return new WaitForSeconds(3.0f);

        isDie = false;
        hp = 100;
        gameObject.tag = "MONSTER";
        monsterState = MonsterState.idle;

        m_Rigid.useGravity = true;

        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = true;

        foreach (Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = true;
        }

        gameObject.SetActive(false);
    }

    void CreateBloodEffect(Vector3 pos)
    {
        GameObject blood1 = Instantiate(bloodEffect, pos, Quaternion.identity);
        blood1.GetComponent<ParticleSystem>().Play();
        Destroy(blood1, 3.0f);

        Vector3 decalPos = monsterTr.position + (Vector3.up * 0.05f);
        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

        GameObject blood2 = Instantiate(bloodDecal, decalPos, decalRot);
        float scale = Random.Range(1.5f, 3.5f);
        blood2.transform.localScale = Vector3.one * scale;

        Destroy(blood2, 5.0f);
    }

    void OnPlayerDie()
    {
        StopAllCoroutines();
    }

    public void TakeDamage(int a_Value)
    {
        if (hp <= 0.0f)
            return;

        CreateBloodEffect(transform.position);

        hp -= a_Value;
        if (hp <= 0)
        {
            hp = 0;
            MonsterDie();
        }
    }
}
