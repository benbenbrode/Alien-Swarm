using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//클래스에 System.Serializable 이라는 어트리뷰트(Attribute)를 명시해야
//Inspector 뷰에 노출됨
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

    //이동 속도 변수
    public float moveSpeed = 10.0f;
    public float m_GReturnTime = 0.69f; //m_JumpPower를 다 깍아먹고 (-m_VelocityY)에 도달해야 하는데 걸리는 시간
    public float m_GravitySpeed = 36.2f;
    public float m_VelocityY = -12.0f;  //중력(밑으로 끌어 내리는 힘), 중력 가속도의 최대치 : -12.0f
    public float m_JumpPower = 13.0f;   //점프시 뛰어 오르는 힘
    //bool m_CanDoubleJump = false;

    //회전 속도 변수
    public float rotSpeed = 100.0f;
    Vector3 m_CacVec = Vector3.zero;

    //인스펙터뷰에 표시할 애니메이션 클래스 변수
    public Anim anim;

    //아래에 있는 3D 모델의 Animation 컴포넌트에 접근하기 위한 변수
    public Animation _animation;

    //Player의 생명 변수
    public float hp = 100;
    //Plyaer의 생명 초기값
    private float initHp;
    //Player의 Health bar 이미지
    public Image imgHpbar;

    CharacterController m_ChrCtrl;  //현재 캐릭터가 가지고 있는 캐릭터 컨트롤러 참조 변수

    [Header("--- Sound ---")]
    public AudioClip CoinSfx;
    public AudioClip DiamondSfx;
    AudioSource Ad_Source = null;

    FireCtrl m_FireCtrl = null;
    public GameObject bloodEffect;  //혈흔 효과 프리팹

    //--- 쉴드 스킬
    float m_SdDuration = 20.0f;
    public float m_SdOnTime = 0.0f;
    public GameObject ShieldObj = null;
    //--- 쉴드 스킬

    //--- Ending Scene 로딩 즉시 비행선과 충돌되는 형상을 방지하기 위한 타이머
    float m_CkTimer = 0.3f;
    //--- Ending Scene 로딩 즉시 비행선과 충돌되는 형상을 방지하기 위한 타이머

    // Start is called before the first frame update

    // 데미지
    public int power = 20;
    //연사 속도
    public float firespeed = 0.5f;
    //더블샷 체크
    public bool doubleshot = false;
    //흡혈 체크
    public bool blood = false;
    //폭탄샷 체크
    public bool grshot = false;
    // 터렛 능력치
    public int Tpower = 20;
    public float Tfirespeed = 0.5f;
    public int TIndex = 0;

    public Camera mainCamera;                   // 카메라 객체
    public float rotationSpeed = 30f;           // 회전 속도 (초당 회전 각도)
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

    //충돌한 Collider의 IsTrigger 옵션이 체크됐을 때 발생
    void OnTriggerEnter(Collider coll)
    {
        //충돌한 Collider가 몬스터의 PUNCH이면 Player의 HP 차감
        if(coll.gameObject.tag == "PUNCH")
        {
            if (0.0f < m_SdOnTime)  //쉴드 발동 중이면..
                return;

            if (hp <= 0.0f) //이미 사망한 상태면...
                return;

            hp -= 10;

            hpui();

            //Debug.Log("Player HP = " + hp.ToString());

            //Player의 생명이 0이하이면 사망 처리
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
                PlayerDie();        //게임 엔딩
        }


    }//void OnTriggerEnter(Collider coll)

    public void hpui() 
    {
        //Image UI 항목의 fillAmount 속성을 조절해 생명 게이지 값 조절
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
            if (0.0f < m_SdOnTime)  //쉴드 발동 중이면..
                return;
            //--혈흔 효과 생성
            GameObject blood = (GameObject)Instantiate(bloodEffect,
                                        coll.transform.position, Quaternion.identity);

            blood.GetComponent<ParticleSystem>().Play();
            Destroy(blood, 3.0f);
            //--혈흔 효과 생성

            Destroy(coll.gameObject); //E_BULLET 삭제

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

    //Player의 사망 처리 루틴
    public void PlayerDie()
    {
        //Debug.Log("Player Die !!");

        //MONSTER라는 Tag를 가진 모든 게임오브젝트를 찾아옴
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
        foreach(GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
            //MonsterCtrl a_MonCtrl = monster.GetComponent<MonsterCtrl>();
            //a_MonCtrl.OnPlayerDie();
        }

        _animation.Stop();  //애니메이터 컴포넌트의 애니메이션 중지 함수

        GameMgr.s_GameState = GameState.GameEnd;
        //GameMgr의 싱글턴 인스턴스를 접근해 isGameOver 변숫값을 변경
        GameMgr.Inst.isGameOver = true;
        Time.timeScale = 0.0f;  //일시정지
        GameMgr.Inst.GameOverMethod();

    }//void PlayerDie()
    public void PlayerClear()
    {

        //MONSTER라는 Tag를 가진 모든 게임오브젝트를 찾아옴
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        //모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
        foreach (GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
            //MonsterCtrl a_MonCtrl = monster.GetComponent<MonsterCtrl>();
            //a_MonCtrl.OnPlayerDie();
        }

        _animation.Stop();  //애니메이터 컴포넌트의 애니메이션 중지 함수

        GameMgr.s_GameState = GameState.GameEnd;
        //GameMgr의 싱글턴 인스턴스를 접근해 isGameOver 변숫값을 변경
        GameMgr.Inst.isGameOver = true;
        Time.timeScale = 0.0f;  //일시정지
        GameMgr.Inst.GameClearMethod();
        StartCoroutine(RotateCameraAroundObject());

    }

    IEnumerator RotateCameraAroundObject()
    {
        while (true)
        {
            // 카메라를 오브젝트 중심으로 회전
            mainCamera.transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.unscaledDeltaTime);

            // 카메라가 항상 오브젝트를 바라보도록 설정
            mainCamera.transform.LookAt(transform);

            // 한 프레임 대기 (타임스케일이 0이므로 실제 시간에 맞춰 대기)
            yield return null;
        }
    }

    void SkillUpdate()
    {
        //쉴드 상태 업데이트
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
        //쉴드 상태 업데이트
    }

    public void UseSkill_Item(SkillType a_SkType)
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        if (a_SkType == SkillType.Skill_0)  //30% 힐링 아이템 스킬
        {
            //머리위 텍스트 띄우기
            GameMgr.Inst.SpawnHealText((int)(initHp * 0.3f), transform.position, Color.white);

            hp += (int)(initHp * 0.3f);

            if(initHp < hp)
                hp = initHp;

            if(imgHpbar != null)
                imgHpbar.fillAmount = hp / (float)initHp;
        }
        else if(a_SkType == SkillType.Skill_1)  //수류탄
        {
            if (m_FireCtrl != null)
                m_FireCtrl.FireGrenade();
        }
        else if(a_SkType == SkillType.Skill_2)  //보호막
        {
            if (0.0f < m_SdOnTime)
                return;

            m_SdOnTime = m_SdDuration;

            //쿨타임 발동
            GameMgr.Inst.SkillTimeMethod(m_SdOnTime, m_SdDuration);
        }

        int a_SkIdx = (int)a_SkType;
        GlobalValue.g_SkillCount[a_SkIdx]--;
        string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);

    }//public void UseSkill_Item(SkillType a_SkType)
}
