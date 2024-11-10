using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Ŭ������ System.Serializable �̶�� ��Ʈ����Ʈ(Attribute)�� ����ؾ�
//Inspector �信 �����
[System.Serializable]
public class Anim
{
    public AnimationClip idle;
    public AnimationClip runForward;
    public AnimationClip runBackward;
    public AnimationClip runRight;
    public AnimationClip runLeft;
}

public class PlayerCtrl : MonoBehaviour
{
    private float h = 0.0f;
    private float v = 0.0f;

    //�̵� �ӵ� ����
    public float moveSpeed = 10.0f;
    public float m_GReturnTime = 0.69f; //m_JumpPower�� �� ��Ƹ԰� (-m_VelocityY)�� �����ؾ� �ϴµ� �ɸ��� �ð�
    public float m_GravitySpeed = 36.2f;
    public float m_VelocityY = -12.0f;  //�߷�(������ ���� ������ ��), �߷� ���ӵ��� �ִ�ġ : -12.0f
    public float m_JumpPower = 13.0f;   //������ �پ� ������ ��
    //bool m_CanDoubleJump = false;

    //ȸ�� �ӵ� ����
    public float rotSpeed = 100.0f;
    Vector3 m_CacVec = Vector3.zero;

    //�ν����ͺ信 ǥ���� �ִϸ��̼� Ŭ���� ����
    public Anim anim;

    //�Ʒ��� �ִ� 3D ���� Animation ������Ʈ�� �����ϱ� ���� ����
    public Animation _animation;

    //Player�� ���� ����
    public float hp = 100;
    //Plyaer�� ���� �ʱⰪ
    private float initHp;
    //Player�� Health bar �̹���
    public Image imgHpbar;

    CharacterController m_ChrCtrl;  //���� ĳ���Ͱ� ������ �ִ� ĳ���� ��Ʈ�ѷ� ���� ����

    [Header("--- Sound ---")]
    public AudioClip CoinSfx;
    public AudioClip DiamondSfx;
    AudioSource Ad_Source = null;

    FireCtrl m_FireCtrl = null;
    public GameObject bloodEffect;  //���� ȿ�� ������

    //--- ���� ��ų
    float m_SdDuration = 20.0f;
    public float m_SdOnTime = 0.0f;
    public GameObject ShieldObj = null;
    //--- ���� ��ų

    //--- Ending Scene �ε� ��� ���༱�� �浹�Ǵ� ������ �����ϱ� ���� Ÿ�̸�
    float m_CkTimer = 0.3f;
    //--- Ending Scene �ε� ��� ���༱�� �浹�Ǵ� ������ �����ϱ� ���� Ÿ�̸�

    // Start is called before the first frame update

    // ������
    public int power = 20;
    //���� �ӵ�
    public float firespeed = 0.5f;
    //���� üũ
    public bool doubleshot = false;
    //���� üũ
    public bool blood = false;
    //��ź�� üũ
    public bool grshot = false;
    // �ͷ� �ɷ�ġ
    public int Tpower = 20;
    public float Tfirespeed = 0.5f;
    public int TIndex = 0;

    public Camera mainCamera;                   // ī�޶� ��ü
    public float rotationSpeed = 30f;           // ȸ�� �ӵ� (�ʴ� ȸ�� ����)
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        // Initialize movement speed
        moveSpeed = 7.0f;
        m_GravitySpeed = (m_JumpPower + (-m_VelocityY)) / m_GReturnTime;
        initHp = hp;  // Set initial health

        _animation = GetComponentInChildren<Animation>();
        _animation.clip = anim.idle;
        _animation.Play();

        m_ChrCtrl = GetComponent<CharacterController>();
        m_FireCtrl = GetComponent<FireCtrl>();

        Transform a_PMesh = transform.Find("PlayerModel");
        if (a_PMesh != null)
            Ad_Source = a_PMesh.GetComponent<AudioSource>();

        // Lock and hide the cursor to keep it at the screen's center
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "scLevel02")
        {
            if (0.0f < m_CkTimer)
            {
                transform.position = new Vector3(11.0f, 0.07f, 12.7f);
                transform.eulerAngles = new Vector3(0.0f, -127.7f, 0.0f);

                m_CkTimer -= Time.deltaTime;
                return;
            }
        }

        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        if (moveDir.magnitude > 1.0f)
            moveDir.Normalize();

        if (m_ChrCtrl != null)
        {
            moveDir = transform.TransformDirection(moveDir) * moveSpeed;

            if (m_ChrCtrl.isGrounded)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    m_VelocityY = m_JumpPower;
                }
            }
            if (m_VelocityY > -12.0f)
                m_VelocityY -= m_GravitySpeed * Time.deltaTime;

            moveDir.y = m_VelocityY;
            m_ChrCtrl.Move(moveDir * Time.deltaTime);
        }

        // Rotate based on mouse movement (without requiring mouse buttons)
        if (GameMgr.IsPointerOverUIObject() == false)
        {
            float mouseX = Input.GetAxis("Mouse X") * GlobalValue.g_mouse;
            transform.Rotate(Vector3.up * Time.deltaTime * rotSpeed * mouseX * 3.0f);
        }

        // Handle animations based on movement
        if (v >= 0.01f)
            _animation.CrossFade(anim.runForward.name, 0.3f);
        else if (v <= -0.01f)
            _animation.CrossFade(anim.runBackward.name, 0.3f);
        else if (h >= 0.01f)
            _animation.CrossFade(anim.runRight.name, 0.3f);
        else if (h <= -0.01f)
            _animation.CrossFade(anim.runLeft.name, 0.3f);
        else
            _animation.CrossFade(anim.idle.name, 0.3f);


        SkillUpdate();
    }

    //�浹�� Collider�� IsTrigger �ɼ��� üũ���� �� �߻�
    void OnTriggerEnter(Collider coll)
    {
        //�浹�� Collider�� ������ PUNCH�̸� Player�� HP ����
        if(coll.gameObject.tag == "PUNCH")
        {
            if (0.0f < m_SdOnTime)  //���� �ߵ� ���̸�..
                return;

            if (hp <= 0.0f) //�̹� ����� ���¸�...
                return;

            hp -= 10;

            hpui();

            //Debug.Log("Player HP = " + hp.ToString());

            //Player�� ������ 0�����̸� ��� ó��
            if (hp <= 0)
            {
                PlayerDie();
            }
        }
        else if(coll.gameObject.name.Contains("CoinPrefab") == true)
        {
            int a_CacGold = 50;

            GameMgr.Inst.AddGold(a_CacGold);

            if (Ad_Source != null && CoinSfx != null)
                Ad_Source.PlayOneShot(CoinSfx, 0.3f);

            Destroy(coll.gameObject);
        }
        else if(coll.gameObject.name.Contains("Gate_Exit_1") == true ||
                coll.gameObject.name.Contains("Gate_Exit_2") == true)
        {
            GlobalValue.g_CurFloorNum++;
            PlayerPrefs.SetInt("CurFloorNum", GlobalValue.g_CurFloorNum);

            if(GlobalValue.g_BestFloor < GlobalValue.g_CurFloorNum)
            {
                GlobalValue.g_BestFloor = GlobalValue.g_CurFloorNum;
                PlayerPrefs.SetInt("BestFloorNum", GlobalValue.g_BestFloor);
            }

            if(GlobalValue.g_CurFloorNum < 100)
            {
                SceneManager.LoadScene("scLevel01");
                SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadScene("scLevel02");
                SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
            }

        }//else if(coll.gameObject.name.Contains("Gate_Exit_1") == true ||
        else if(coll.gameObject.name.Contains("RTS15_desert") == true)
        {
            if (m_CkTimer <= 0.0f)
                PlayerDie();        //���� ����
        }


    }//void OnTriggerEnter(Collider coll)

    public void hpui() 
    {
        //Image UI �׸��� fillAmount �Ӽ��� ������ ���� ������ �� ����
        imgHpbar.fillAmount = (float)hp / (float)initHp;
    } 
    void OnTriggerStay(Collider coll)
    {
        if(coll.gameObject.name.Contains("Gate_In_1") == true)
        {
            if (Input.GetKey(KeyCode.LeftShift) == false)
                return;

            GlobalValue.g_CurFloorNum--;
            if (GlobalValue.g_CurFloorNum < 1)
                GlobalValue.g_CurFloorNum = 1;

            PlayerPrefs.SetInt("CurFloorNum", GlobalValue.g_CurFloorNum);

            SceneManager.LoadScene("scLevel01");
            SceneManager.LoadScene("scPlay", LoadSceneMode.Additive);
        }
    }//void OnTriggerStay(Collider coll)

    void OnCollisionEnter(Collision coll)
    {
        if(coll.gameObject.tag == "E_BULLET")
        {
            if (0.0f < m_SdOnTime)  //���� �ߵ� ���̸�..
                return;
            //--���� ȿ�� ����
            GameObject blood = (GameObject)Instantiate(bloodEffect,
                                        coll.transform.position, Quaternion.identity);

            blood.GetComponent<ParticleSystem>().Play();
            Destroy(blood, 3.0f);
            //--���� ȿ�� ����

            Destroy(coll.gameObject); //E_BULLET ����

            if (hp <= 0.0f)
                return;

            hp -= 10;

            if (imgHpbar == null)
                imgHpbar = GameObject.Find("Hp_Image").GetComponent<Image>();

            if (imgHpbar != null)
                imgHpbar.fillAmount = (float)hp / (float)initHp;

            if(hp <= 0)
            {
                PlayerDie();
            }
        }//if(coll.gameObject.tag == "E_BULLET")

    }//void OnCollisionEnter(Collision coll)

    //Player�� ��� ó�� ��ƾ
    public void PlayerDie()
    {
        //Debug.Log("Player Die !!");

        //MONSTER��� Tag�� ���� ��� ���ӿ�����Ʈ�� ã�ƿ�
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //��� ������ OnPlayerDie �Լ��� ���������� ȣ��
        foreach(GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
            //MonsterCtrl a_MonCtrl = monster.GetComponent<MonsterCtrl>();
            //a_MonCtrl.OnPlayerDie();
        }

        _animation.Stop();  //�ִϸ����� ������Ʈ�� �ִϸ��̼� ���� �Լ�

        GameMgr.s_GameState = GameState.GameEnd;
        //GameMgr�� �̱��� �ν��Ͻ��� ������ isGameOver �������� ����
        GameMgr.Inst.isGameOver = true;
        Time.timeScale = 0.0f;  //�Ͻ�����
        GameMgr.Inst.GameOverMethod();

    }//void PlayerDie()
    public void PlayerClear()
    {

        //MONSTER��� Tag�� ���� ��� ���ӿ�����Ʈ�� ã�ƿ�
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //��� ������ OnPlayerDie �Լ��� ���������� ȣ��
        foreach (GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
            //MonsterCtrl a_MonCtrl = monster.GetComponent<MonsterCtrl>();
            //a_MonCtrl.OnPlayerDie();
        }

        _animation.Stop();  //�ִϸ����� ������Ʈ�� �ִϸ��̼� ���� �Լ�

        GameMgr.s_GameState = GameState.GameEnd;
        //GameMgr�� �̱��� �ν��Ͻ��� ������ isGameOver �������� ����
        GameMgr.Inst.isGameOver = true;
        Time.timeScale = 0.0f;  //�Ͻ�����
        GameMgr.Inst.GameClearMethod();
        StartCoroutine(RotateCameraAroundObject());

    }

    IEnumerator RotateCameraAroundObject()
    {
        while (true)
        {
            // ī�޶� ������Ʈ �߽����� ȸ��
            mainCamera.transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.unscaledDeltaTime);

            // ī�޶� �׻� ������Ʈ�� �ٶ󺸵��� ����
            mainCamera.transform.LookAt(transform);

            // �� ������ ��� (Ÿ�ӽ������� 0�̹Ƿ� ���� �ð��� ���� ���)
            yield return null;
        }
    }

    void SkillUpdate()
    {
        //���� ���� ������Ʈ
        if(0.0f < m_SdOnTime)
        {
            m_SdOnTime -= Time.deltaTime;
            if (ShieldObj != null && ShieldObj.activeSelf == false)
                ShieldObj.SetActive(true);
        }
        else
        {
            if(ShieldObj != null && ShieldObj.activeSelf == true)
                ShieldObj.SetActive(false);
        }
        //���� ���� ������Ʈ
    }

    public void UseSkill_Item(SkillType a_SkType)
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        if (a_SkType == SkillType.Skill_0)  //30% ���� ������ ��ų
        {
            //�Ӹ��� �ؽ�Ʈ ����
            GameMgr.Inst.SpawnHealText((int)(initHp * 0.3f), transform.position, Color.white);

            hp += (int)(initHp * 0.3f);

            if(initHp < hp)
                hp = initHp;

            if(imgHpbar != null)
                imgHpbar.fillAmount = hp / (float)initHp;
        }
        else if(a_SkType == SkillType.Skill_1)  //����ź
        {
            if (m_FireCtrl != null)
                m_FireCtrl.FireGrenade();
        }
        else if(a_SkType == SkillType.Skill_2)  //��ȣ��
        {
            if (0.0f < m_SdOnTime)
                return;

            m_SdOnTime = m_SdDuration;

            //��Ÿ�� �ߵ�
            GameMgr.Inst.SkillTimeMethod(m_SdOnTime, m_SdDuration);
        }

        int a_SkIdx = (int)a_SkType;
        GlobalValue.g_SkillCount[a_SkIdx]--;
        string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);

    }//public void UseSkill_Item(SkillType a_SkType)
}
