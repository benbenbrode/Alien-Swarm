using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Boss : MonoBehaviour
{
    //������ ���� ������ �ִ� Enumerable ���� ����
    public enum MonsterState { idle, trace, attack, die };

    //������ ���� ���� ������ ������ Enum ����
    public MonsterState monsterState = MonsterState.idle;

    //�ӵ� ����� ���� ���� ������Ʈ�� ������ �Ҵ�
    private Transform monsterTr;
    private Transform playerTr;
    //private NavMeshAgent nvAgent;
    private Animator animator;

    //���� �����Ÿ�
    public float traceDist = 10.0f;
    //���� �����Ÿ�
    public float attackDist = 1.5f; //2.0f;

    //������ ��� ����
    private bool isDie = false;

    //���� ȿ�� ������
    public GameObject bloodEffect;
    //���� ��Į ȿ�� ������
    public GameObject bloodDecal;

    //���� ���� ����
    private int hp = 50000;
    Rigidbody m_Rigid = null;

    //--- �Ѿ� �߻� ���� ����
    public GameObject bullet;       //�Ѿ� ������
    float m_BLTime = 0.0f;
    LayerMask m_LaserMask = -1;
    //--- �Ѿ� �߻� ���� ����

    public GameObject Player;
    public GameObject Mgr;

    private List<GameObject> swampObjects = new List<GameObject>();
    private List<GameObject> activeSwamps = new List<GameObject>();
    private float activateDuration = 5f; // Ȱ��ȭ �ð� (5��)
    private float cycleDuration = 15f; // �ֱ� (15��)
    private GameObject parentObject;
    //������

    void Awake()
    {
        traceDist = 10000.0f;
        attackDist = 1.5f;

        //������ Transform �Ҵ�
        monsterTr = this.gameObject.GetComponent<Transform>();
        //���� ����� Player�� Transform �Ҵ�
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();
        ////NavMeshAgent ������Ʈ �Ҵ�
        //nvAgent = this.gameObject.GetComponent<NavMeshAgent>();

        ////���� ����� ��ġ�� �����ϸ� �ٷ� ���� ����
        //nvAgent.destination = playerTr.position;

        //Animator ������Ʈ �Ҵ�
        animator = this.gameObject.GetComponent<Animator>();

        m_Rigid = GetComponent<Rigidbody>();
        Player = GameObject.Find("Player");
        Mgr = GameObject.Find("GameMgr");
    }

    ////�̺�Ʈ �߻��� ������ �Լ� ����
    //void OnEnable()
    //{
    //    //������ �������� ������ �ൿ ���¸� üũ�ϴ� �ڷ�ƾ �Լ� ����
    //    StartCoroutine(this.CheckMonsterState());

    //    //������ ���¿� ���� �����ϴ� ��ƾ�� �����ϴ� �ڷ�ƾ �Լ� ����
    //    StartCoroutine(this.MonsterAction());
    //}

    // Start is called before the first frame update
    void Start()
    {
        GameObject stagesObject = GameObject.Find("_STAGES");
        if (stagesObject != null)
        {
            Transform swampposTransform = stagesObject.transform.Find("swamppos");
            if (swampposTransform != null)
            {
                parentObject = swampposTransform.gameObject;
            }
            else
            {
                Debug.LogWarning("swamppos object not found as a child of _STAGES.");
                return;
            }
        }
        else
        {
            Debug.LogWarning("_STAGES object not found.");
            return;
        }

        // "swamp"��� �̸��� ���� ������Ʈ���� ã�� ����Ʈ�� �߰�
        FindSwampsInChildren(parentObject);
        Debug.Log($"Found {swampObjects.Count} swamp objects."); // �����: swamp ���� Ȯ��
        // �ֱ������� Ȱ��ȭ/��Ȱ��ȭ�� �ݺ�
        StartCoroutine(ActivateSwampsCycle());

        m_LaserMask = 1 << LayerMask.NameToLayer("Default");
        //"Default" ���̾ ���� �ɽ�Ʈ üũ �ϰڴٴ� ����

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            // ��ο� ����� ���� (RGB: 128, 0, 128)
            Color demonPurpleColor = new Color(0.5f, 0.0f, 0.5f);
            renderer.material.color = demonPurpleColor;

            // ��Ż���� �۷ν� ���� �����Ͽ� ��ο� �ݼ� ���� �߰�
            renderer.material.SetFloat("_Metallic", 0.9f);    // ���� ��Ż�� �������� ���̰� ��ī�ο� ����
            renderer.material.SetFloat("_Glossiness", 0.8f);  // ���� ������ ��ο� ��ä ȿ��
        }

    }

    // Update is called once per frame
    void FixedUpdate()  //void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        if (playerTr.gameObject.activeSelf == false)
            playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();

        CheckMonStateUpdate();
        MonActionUpdate();
        Vector3 position = transform.position;


        if (transform.position.y < -2)
        {
            Vector3 correctedPosition = transform.position;
            correctedPosition.y = 0;
            transform.position = correctedPosition;
        }

    }

    #region -- ���� AI

    //������ �������� ������ �ൿ ���¸� üũ�ϰ� monsterState �� ����
    float m_AI_Delay = 0.0f;

    void CheckMonStateUpdate()
    {
        if (isDie == true)
            return;

        //0.1�� �ֱ�θ� üũ�ϱ� ���� ������ ��� �κ�
        m_AI_Delay -= Time.deltaTime;
        if (0.0f < m_AI_Delay)
            return;

        m_AI_Delay = 0.1f;
        //0.1�� �ֱ�θ� üũ�ϱ� ���� ������ ��� �κ�

        //���Ϳ� �÷��̾� ������ �Ÿ� ����
        float dist = Vector3.Distance(playerTr.position, monsterTr.position);
        float Ydist = Mathf.Abs(playerTr.position.y - monsterTr.position.y);
        //���ΰ��� 2���� ���� �� 1���� �ִ� ���Ͱ� �������� ���ϰ� ó�� �ϱ� ���� �ڵ�

        if (dist <= attackDist) //���ݰŸ� ���� �̳��� ���Դ��� Ȯ��
        {
            monsterState = MonsterState.attack;
        }
        else if (dist <= traceDist && Ydist < 20.0f) //�����Ÿ� ���� �̳��� ���Դ��� Ȯ��
        {
            monsterState = MonsterState.trace; //������ ���¸� �������� ����
        }
        else
        {
            monsterState = MonsterState.idle;  //������ ���¸� idle ���� ����
        }

    }//void CheckMonStateUpdate()

    //������ ���°��� ���� ������ ������ �����ϴ� �Լ�
    void MonActionUpdate()
    {
        if (isDie == true)
            return;

        switch (monsterState)
        {
            //idle ����
            case MonsterState.idle:
                //Animator�� IsTrace ������ false�� ����
                animator.SetBool("IsTrace", false);
                break;

            //��������
            case MonsterState.trace:
                {
                    //���� �̵� ����
                    float a_MoveVelocity = 6.0f;    //��� �ʴ� �̵� �ӵ�...
                    Vector3 a_MoveDir = playerTr.position - transform.position;
                    a_MoveDir.y = 0.0f;

                    if (0.0f < a_MoveDir.magnitude)
                    {
                        Vector3 a_StepVec = a_MoveDir.normalized * a_MoveVelocity * Time.deltaTime;
                        transform.Translate(a_StepVec, Space.World);

                        //--- �̵� ������ �ٶ� ������ ȸ�� ó��
                        float a_RotSpeed = 7.0f;  //�ʴ� ȸ�� �ӵ�
                        Quaternion a_TargetRot = Quaternion.LookRotation(a_MoveDir.normalized);
                        transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      a_TargetRot, Time.deltaTime * a_RotSpeed);
                        //--- �̵� ������ �ٶ� ������ ȸ�� ó��
                    }//if(0.0f < a_MoveDir.magnitude)

                    //Animator�� IsAttack ������ false�� ����
                    animator.SetBool("IsAttack", false);

                    //Animator�� IsTrace ������ true�� ����
                    animator.SetBool("IsTrace", true);
                }
                break;

            //���� ����
            case MonsterState.attack:
                {
                    //IsAttack�� true�� ������ attack State�� ����
                    animator.SetBool("IsAttack", true);

                    //--- ���Ͱ� ���ΰ��� �����ϸ鼭 �ٶ� ������ ó��
                    float a_RotSpeed = 6.0f;   //�ʴ� ȸ�� �ӵ�
                    Vector3 a_CacDir = playerTr.position - transform.position;
                    a_CacDir.y = 0.0f;
                    if (0.0f < a_CacDir.magnitude)
                    {
                        //--- AI 0.1�� ���� üũ ������ ���� �Ÿ����� �־��� ��� ��ġ ����
                        if (attackDist < a_CacDir.magnitude)
                        {
                            float a_MoveVelocity = 2.0f;
                            Vector3 a_StepVec = a_CacDir.normalized * a_MoveVelocity * Time.deltaTime;
                            transform.Translate(a_StepVec, Space.World);
                        }
                        //--- AI 0.1�� ���� üũ ������ ���� �Ÿ����� �־��� ��� ��ġ ����

                        Quaternion a_TargetRot = Quaternion.LookRotation(a_CacDir.normalized);
                        transform.rotation = Quaternion.Slerp(
                                    transform.rotation, a_TargetRot, Time.deltaTime * a_RotSpeed);
                    }
                    //--- ���Ͱ� ���ΰ��� �����ϸ鼭 �ٶ� ������ ó��
                }
                break;
        }//switch(monsterState)

        //--- �Ѿ˹߻�
            FireUpdate();
        //--- �Ѿ˹߻�

    }//void MonActionUpdate()

    #endregion

    void FireUpdate()  //�ֱ������� �Ѿ� �߻��ϴ� �Լ�
    {
        Vector3 a_PlayerPos = playerTr.position;
        a_PlayerPos.y = a_PlayerPos.y + 1.5f;
        Vector3 a_MonPos = transform.position;
        a_MonPos.y = a_MonPos.y + 1.5f;
        Vector3 a_CacDir = a_PlayerPos - a_MonPos;
        float a_RayUDLimit = 3.5f;

        m_BLTime = m_BLTime - Time.deltaTime;
        if (m_BLTime <= 0.0f)
        {
            m_BLTime = 0.0f;
        }



        //���̰� �Ѱ�ġ ���� ��, �Ʒ� -3 ~ +3 ���̷� ���� ������ �Ѿ�� ����
        if ((-a_RayUDLimit <= a_CacDir.y && a_CacDir.y <= a_RayUDLimit) == false)
            return;

        bool IsRayOk = false;
        if (Physics.Raycast(a_MonPos, a_CacDir.normalized,
                            out RaycastHit hit, 100.0f, m_LaserMask) == true)
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                IsRayOk = true;
            }
        }

        //���Ϳ��� ���ΰ������� ���� �þ߿� ���ΰ��� ������ ������ ����
        if (IsRayOk == false)
            return;

        //--- ���Ͱ� ���ΰ��� ���� �ٶ� ������ ȸ�� ó��
        Vector3 a_CacVLen = playerTr.position - transform.position;
        a_CacVLen.y = 0.0f;
        Quaternion a_TargetRot = Quaternion.LookRotation(a_CacVLen.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation,
                                    a_TargetRot, Time.deltaTime * 6.0f);
        //--- ���Ͱ� ���ΰ��� ���� �ٶ� ������ ȸ�� ó��

        if (m_BLTime <= 0.0f)
        {
            Vector3 a_StartPos = a_MonPos + a_CacDir.normalized * 1.5f;
            GameObject a_Bullet = Instantiate(bullet, a_StartPos, Quaternion.identity);
            a_Bullet.layer = LayerMask.NameToLayer("E_BULLET");
            a_Bullet.tag = "E_BULLET";
            a_Bullet.transform.forward = a_CacDir.normalized;

            //������ �Ѿ� �߻� �ֱ� : 2��(10������)���� ~ 1��(99��)���� ���ϰ�...
            float a_CacDf = (GlobalValue.g_CurFloorNum - 10) * 0.012f;
            if (a_CacDf < 0.0f)
                a_CacDf = 0.0f;
            if (1.0f < a_CacDf)
                a_CacDf = 1.0f;

            m_BLTime = 2f - a_CacDf;
        }//if(m_BLTime <= 0.0f)

    }//void FireUpdate()

    ////������ �������� ������ �ൿ ���¸� üũ�ϰ� monsterState �� ����
    //IEnumerator CheckMonsterState()
    //{
    //    while(!isDie)
    //    {
    //        //0.2�� ���� ��ٷȴٰ� �������� �Ѿ
    //        yield return new WaitForSeconds(0.2f);

    //        //���Ϳ� �÷��̾� ������ �Ÿ� ����
    //        float dist = Vector3.Distance(playerTr.position, monsterTr.position);

    //        if (dist <= attackDist)  //���ݰŸ� ���� �̳��� ���Դ��� Ȯ��
    //        {
    //            monsterState = MonsterState.attack;
    //        }
    //        else if (dist <= traceDist) //�����Ÿ� ���� �̳��� ���Դ��� Ȯ��
    //        {
    //            monsterState = MonsterState.trace; //������ ���¸� �������� ����
    //        }
    //        else
    //        {
    //            monsterState = MonsterState.idle;  //������ ���¸� idle ���� ����
    //        }

    //    }//while(!isDie)
    //}//IEnumerator CheckMonsterState()

    ////������ ���°��� ���� ������ ������ �����ϴ� �Լ�
    //IEnumerator MonsterAction()
    //{
    //    while(!isDie)
    //    {
    //        //if (isDie == true)
    //        //    yield break;  //�ڷ�ƾ �Լ��� ��� ���������� �ڵ�

    //        switch(monsterState)
    //        {
    //            //idle ����
    //            case MonsterState.idle:
    //                ////���� ����
    //                //nvAgent.isStopped = true; //<-- nvAgent.Stop();
    //                //Animator�� IsTrace ������ false�� ����
    //                animator.SetBool("IsTrace", false);
    //                break;

    //            //���� ����
    //            case MonsterState.trace:
    //                ////���� ����� ��ġ�� �Ѱ���
    //                //nvAgent.destination = playerTr.position;
    //                ////������ �����
    //                //nvAgent.isStopped = false; //<--nvAgent.Resume();

    //                //Animator�� IsAttack ������ false�� ����
    //                animator.SetBool("IsAttack", false);
    //                //Animator�� IsTrace �������� true�� ����
    //                animator.SetBool("IsTrace", true);
    //                break;

    //            //���� ����
    //            case MonsterState.attack:
    //                {
    //                    ////���� ����
    //                    //nvAgent.isStopped = true; //<-- nvAgent.Stop();
    //                    //IsAttack�� true�� ������ attack State�� ����
    //                    animator.SetBool("IsAttack", true);

    //                    //--- ���Ͱ� ������ �����ϸ鼭 �ٶ� ������ ó��
    //                    float a_RotSpeed = 6.0f;
    //                    Vector3 a_CacDir = playerTr.position - transform.position;
    //                    a_CacDir.y = 0.0f;
    //                    if (0.0f < a_CacDir.magnitude)
    //                    {
    //                        Quaternion a_TargetRot = Quaternion.LookRotation(a_CacDir.normalized);
    //                        transform.rotation = Quaternion.Slerp(
    //                                transform.rotation, a_TargetRot, Time.deltaTime * a_RotSpeed ); 
    //                    }
    //                    //--- ���Ͱ� ������ �����ϸ鼭 �ٶ� ������ ó��
    //                }
    //                break;
    //        }//switch(monsterState)

    //        yield return null; //<-- �� �÷����� ���� ���� ��� ���

    //    }//while(!isDie)
    //}//IEnumerator MonsterAction()

    //Bullet�� �浹 üũ
    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "BULLET")
        {
            ////�Ӹ��� �ؽ�Ʈ ����
            //GameMgr.Inst.SpawnHealText(-(int)(coll.gameObject.GetComponent<BulletCtrl>().damage),
            //                            transform.position, Color.red);

            //���� ȿ�� �Լ� ȣ��
            CreateBloodEffect(coll.transform.position);

            //���� �Ѿ��� Damage�� ������ ���� hp ����
            //hp -= coll.gameObject.GetComponent<BulletCtrl>().damage;
            hp -= Player.GetComponent<PlayerCtrl>().power;
            if (hp <= 0)
            {
                MonsterDie();
            }

            //Bullet ����
            Destroy(coll.gameObject);

            //IsHit Trigger�� �߻���Ű�� Any State���� gothit�� ���̵�
            animator.SetTrigger("IsHit");
        }
        if (coll.gameObject.tag == "TBULLET")
        {
            ////�Ӹ��� �ؽ�Ʈ ����
            //GameMgr.Inst.SpawnHealText(-(int)(coll.gameObject.GetComponent<BulletCtrl>().damage),
            //                            transform.position, Color.red);

            //���� ȿ�� �Լ� ȣ��
            CreateBloodEffect(coll.transform.position);

            //���� �Ѿ��� Damage�� ������ ���� hp ����
            //hp -= coll.gameObject.GetComponent<BulletCtrl>().damage;
            hp -= Player.GetComponent<PlayerCtrl>().Tpower;
            if (hp <= 0)
            {
                MonsterDie();
            }

            //Bullet ����
            Destroy(coll.gameObject);

            //IsHit Trigger�� �߻���Ű�� Any State���� gothit�� ���̵�
            animator.SetTrigger("IsHit");
        }
    }

    //���� ����� ó�� ��ƾ
    public void MonsterDie()
    {
        GameMgr.Inst.DispScore(1000); //50);
        if (Player.GetComponent<PlayerCtrl>().blood == true)
        {
            Player.GetComponent<PlayerCtrl>().hp += 1;
            Player.GetComponent<PlayerCtrl>().hpui();
        }
        //����� ������ �±׸� Untagged�� ����
        gameObject.tag = "Untagged";

        //��� �ڷ�ƾ�� ����
        StopAllCoroutines();
        Player.GetComponent<PlayerCtrl>().PlayerClear();
        Time.timeScale = 0f;
        Destroy(gameObject);
        isDie = true;
        monsterState = MonsterState.die;
        //nvAgent.isStopped = true;
        animator.SetTrigger("IsDie");

        m_Rigid.useGravity = false;

        //���Ϳ� �߰��� Collider�� ��Ȱ��ȭ
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;

        foreach (Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = false;
        }

        //GameMgr�� ���ھ� ������ ���ھ� ǥ�� �Լ� ȣ��



    }//void MonsterDie()

    IEnumerator PushObjectPool()
    {
        yield return new WaitForSeconds(3.0f);

        //���� ���� �ʱ�ȭ
        isDie = false;
        hp = 100;
        gameObject.tag = "MONSTER";
        monsterState = MonsterState.idle;

        m_Rigid.useGravity = true;

        //���Ϳ� �߰��� Collider�� �ٽ� Ȱ��ȭ
        gameObject.GetComponentInChildren<CapsuleCollider>().enabled = true;

        foreach (Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
        {
            coll.enabled = true;
        }

        //���͸� ��Ȱ��ȭ
        gameObject.SetActive(false);

    }// IEnumerator PushObjectPool()

    void CreateBloodEffect(Vector3 pos)
    {
        //���� ȿ�� ����
        GameObject blood1 = (GameObject)Instantiate(bloodEffect, pos, Quaternion.identity);
        blood1.GetComponent<ParticleSystem>().Play();
        Destroy(blood1, 3.0f);

        //��Į ���� ��ġ - �ٴڿ��� ���� �ø� ��ġ ����
        Vector3 decalPos = monsterTr.position + (Vector3.up * 0.05f);
        //��Į�� ȸ������ �������� ����
        Quaternion decalRot = Quaternion.Euler(90, 0, Random.Range(0, 360));

        //��Į ������ ����
        GameObject blood2 = (GameObject)Instantiate(bloodDecal, decalPos, decalRot);
        //��Į�� ũ�⵵ �ұ�Ģ������ ��Ÿ���Բ� ������ ����
        float scale = Random.Range(1.5f, 3.5f);
        blood2.transform.localScale = Vector3.one * scale;

        //5�� �Ŀ� ����ȿ�� �������� ����
        Destroy(blood2, 5.0f);

    }//void CreateBloodEffect(Vector3 pos)

    //�÷��̾ ������� �� ����Ǵ� �Լ�
    void OnPlayerDie()
    {
        //������ ���¸� üũ�ϴ� �ڷ�ƾ �Լ��� ��� ������Ŵ
        StopAllCoroutines();
        ////������ �����ϰ� �ִϸ��̼��� ����
        //nvAgent.isStopped = true;  //<-- nvAgent.Stop();
        if (isDie == false)
            animator.SetTrigger("IsPlayerDie");
    }

    public void TakeDamage(int a_Value)
    {
        if (hp <= 0.0f)     //�̷��� �ϸ� ��� ó���� �ѹ��� �� ����
            return;

        //���� ȿ�� �Լ� ȣ��
        CreateBloodEffect(transform.position);

        hp -= a_Value;
        if (hp <= 0)
        {
            hp = 0;
            MonsterDie();
            return;
        }

        animator.SetTrigger("IsHit");
    }

    private IEnumerator ActivateSwampsCycle() // ��
    {
        while (true)
        {
            // 15�� �ֱ⸦ ���߱� ���� ���� 10�� ��� (15�� - 5��)
            yield return new WaitForSeconds(cycleDuration - activateDuration);

            // 13���� ������Ʈ�� �������� �����Ͽ� Ȱ��ȭ
            ActivateRandomSwamps();

            // 5�� ���� Ȱ��ȭ ����
            yield return new WaitForSeconds(activateDuration);

            // Ȱ��ȭ�� ������Ʈ ��Ȱ��ȭ
            DeactivateSwamps();

           
        }
    }

    private void ActivateRandomSwamps() // ��
    {
        activeSwamps.Clear();

        // �������� 13���� ������Ʈ ����, ����Ʈ ũ�� Ȯ�� �߰�
        List<GameObject> shuffledSwamps = new List<GameObject>(swampObjects);
        int count = Mathf.Min(13, shuffledSwamps.Count); // ����Ʈ�� ũ��� 13 �� ���� �� ����
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, shuffledSwamps.Count);
            GameObject selectedSwamp = shuffledSwamps[randomIndex];
            selectedSwamp.SetActive(true);
            activeSwamps.Add(selectedSwamp);
            shuffledSwamps.RemoveAt(randomIndex); // ���õ� ������Ʈ�� �ٽ� ���õ��� �ʵ��� ����
        }
    }

    private void DeactivateSwamps() // ��
    {
        // Ȱ��ȭ�� ������Ʈ���� ��Ȱ��ȭ
        foreach (GameObject swamp in activeSwamps)
        {
            swamp.SetActive(false);
        }
    }
    private void FindSwampsInChildren(GameObject parent) // ��
    {
        if (parent == null)
        {
            Debug.LogWarning("Parent object is not assigned.");
            return;
        }

        Transform[] childTransforms = parent.GetComponentsInChildren<Transform>(true); // ��Ȱ��ȭ�� �ڽĵ� �����Ͽ� �˻�
        foreach (Transform child in childTransforms)
        {
            if (child.CompareTag("swamp"))
            {
                swampObjects.Add(child.gameObject);
                child.gameObject.SetActive(false); // ��� ������Ʈ�� ��Ȱ��ȭ ���·� ����
            }
        }
    }
}

