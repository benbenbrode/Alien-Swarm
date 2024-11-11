# Alien-Swarm


게임 장르 : 3D FPS
---

게임 소개 : 
몰려오는 외계인을 처치하며 8분후에 등장하는 최종보스를 공략하는 게임
---

게임 규칙 : 
게임이 시작되면 외계인이 주기적으로 스폰되어 플레이어를 추격합니다.
외계인은 시간이 지날수록 점점 빠르게 스폰됩니다.
플레이어의 채력이 0이 되면 게임 오버입니다.
맵에 존재하는 외계인 수가 200마리에 도달하면 게임 오버됩니다.
게임 시작 후 8분이 지나면 보스가 등잡합니다.
보스를 처치하면 게임ㅇ 클리어입니다.
---

개발 목적 : 직접 3D TPS게임을 구현해보고 다른 장르의 요소(뱀파이어 서바이벌, 랜덤디펜스)를 결합하여 나만의 게임을 만들어보기 위해
---

사용 엔진 : UNITY 2022.3.15f1
---

시연 영상 : 
---

주요 활용 기술
---
#01) PlayFab을 활용하여 회원가입, 로그인, 아이템 및 골드 저장 구현
<details>
<summary>예시 코드</summary>
  
```csharp
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;  // 싱글턴 인스턴스

    private void Awake()
    {
        // 이미 인스턴스가 존재하면 자신을 파괴하고, 아니면 유지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 변경 시 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject);  // 중복된 매니저가 있을 경우 자신을 파괴
        }
    }

    // 1. 회원가입 메서드
    public void RegisterUser(string username, string email, string password)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Email = email,
            Password = password,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Registration successful! UserId: " + result.PlayFabId);
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogError("Registration failed: " + error.GenerateErrorReport());
    }

    // 2. 로그인 메서드
    public void LoginUser(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful! UserId: " + result.PlayFabId);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }

    // 3. 아이템 개수를 저장하는 메서드
    public void SaveItemCounts(int itemACount, int itemBCount, int itemCCount)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "ItemA", itemACount.ToString() },
                { "ItemB", itemBCount.ToString() },
                { "ItemC", itemCCount.ToString() }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }

    private void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Item counts saved successfully!");
    }

    private void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Failed to save item counts: " + error.GenerateErrorReport());
    }
}


```

</details>
#02) 오브젝트 풀링으로 몬스터 스폰
<details>
<summary>예시 코드</summary>
몬스터 풀 생성

  
Start() 메서드에서 for 루프를 통해 미리 지정된 최대 수 (maxMonster) 만큼의 몬스터 객체를 생성하고 monsterPool 리스트에 추가합니다. 각 몬스터 객체는 SetActive(false)로 비활성화된 상태로 저장됩니다.

  
```csharp
for(int i = 0; i < maxMonster; i++)
{
    GameObject monster = (GameObject)Instantiate(monsterPrefab);
    monster.name = "Monster_" + i.ToString();
    monster.SetActive(false);
    monsterPool.Add(monster);
}

```

오브젝트 풀에서 비활성화된 몬스터 재활용

CreateMonster() 코루틴에서 몬스터 스폰 시 오브젝트 풀의 monsterPool 리스트를 순회하며 비활성화된(activeSelf == false) 몬스터를 찾아 활성화하고, 위치를 설정한 뒤 스폰합니다.

```csharp
foreach(GameObject monster in monsterPool)
{
    if(monster.activeSelf == false)
    {
        int idx = Random.Range(1, points.Length);
        monster.transform.position = points[idx].position;
        monster.SetActive(true);
        break;
    }
}

```


몬스터의 상태 초기화

PushObjectPool 코루틴 함수에서는 몬스터가 죽은 후 다시 풀로 반환될 때 필요한 초기화 작업을 수행합니다. 이 함수는 3초 대기 후 실행되며, 몬스터의 상태와 관련된 여러 속성을 초기화합니다.


```csharp
IEnumerator PushObjectPool()
{
    yield return new WaitForSeconds(3.0f);

    // 각종 변수 초기화
    isDie = false;
    hp = 100;
    gameObject.tag = "MONSTER";
    monsterState = MonsterState.idle;

    m_Rigid.useGravity = true;

    // 몬스터에 추가된 Collider를 다시 활성화
    gameObject.GetComponentInChildren<CapsuleCollider>().enabled = true;

    foreach (Collider coll in gameObject.GetComponentsInChildren<SphereCollider>())
    {
        coll.enabled = true;   
    }

    // 몬스터를 비활성화
    gameObject.SetActive(false);
}

```


</details>

#03) 플레이어 캐릭터의 움직임, 상태 관리, 스킬 사용등 캐릭터 조작
<details>
<summary>예시 코드</summary>
  
```csharp
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


```
</details>


#04) 몬스터의 상태 관리
<details>
몬스터의 상태 관리는 MonsterState라는 열거형과 monsterState 변수를 사용하여 이루어집니다. 이 코드는 몬스터가 idle, trace, attack, die 상태 중 하나를 갖도록 하며, 플레이어와의 거리에 따라 상태를 업데이트합니다.
<summary>예시 코드</summary>
  
```csharp
public enum MonsterState { idle, trace, attack, die };

public MonsterState monsterState = MonsterState.idle;

float m_AI_Delay = 0.0f;

void CheckMonStateUpdate()
{
    if (isDie == true)
        return;

    // 0.1초 주기로 상태 체크
    m_AI_Delay -= Time.deltaTime;
    if (0.0f < m_AI_Delay)
        return;

    m_AI_Delay = 0.1f;

    // 플레이어와 몬스터 사이의 거리 계산
    float dist = Vector3.Distance(playerTr.position, monsterTr.position);
    float Ydist = Mathf.Abs(playerTr.position.y - monsterTr.position.y);

    if (dist <= attackDist) // 공격 거리 이내
    {
        monsterState = MonsterState.attack;
    }
    else if (dist <= traceDist && Ydist < 20.0f) // 추적 거리 이내
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

    switch(monsterState)
    {
        case MonsterState.idle:
            animator.SetBool("IsTrace", false);
            break;

        case MonsterState.trace:
            {
                float a_MoveVelocity = 2.0f;
                Vector3 a_MoveDir = playerTr.position - transform.position;
                a_MoveDir.y = 0.0f;

                if(0.0f < a_MoveDir.magnitude)
                {
                    Vector3 a_StepVec = a_MoveDir.normalized * a_MoveVelocity * Time.deltaTime;
                    transform.Translate(a_StepVec, Space.World);

                    float a_RotSpeed = 7.0f;
                    Quaternion a_TargetRot = Quaternion.LookRotation(a_MoveDir.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * a_RotSpeed);
                }

                animator.SetBool("IsAttack", false);
                animator.SetBool("IsTrace", true);
            }
            break;

        case MonsterState.attack:
            {
                animator.SetBool("IsAttack", true);

                float a_RotSpeed = 6.0f;
                Vector3 a_CacDir = playerTr.position - transform.position;
                a_CacDir.y = 0.0f;

                if (0.0f < a_CacDir.magnitude)
                {
                    if (attackDist < a_CacDir.magnitude)
                    {
                        float a_MoveVelocity = 2.0f;
                        Vector3 a_StepVec = a_CacDir.normalized * a_MoveVelocity * Time.deltaTime;
                        transform.Translate(a_StepVec, Space.World);
                    }

                    Quaternion a_TargetRot = Quaternion.LookRotation(a_CacDir.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * a_RotSpeed);
                }
            }
            break;
    }
}



```

</details>


#05) 플레이어 추격
<details>
기본 몬스터는 플레이어를 수평 이동으로 추격
<summary>예시 코드</summary>
  
```csharp
case MonsterState.trace:
{
    // 추적 이동 구현
    float a_MoveVelocity = 2.0f;  // 평면 초당 이동 속도
    Vector3 a_MoveDir = playerTr.position - transform.position;
    a_MoveDir.y = 0.0f;  // 높이값을 고정해 수평 이동만 처리

    if (0.0f < a_MoveDir.magnitude)  // 이동할 거리가 있는 경우에만 이동 처리
    {
        // 목표 방향으로 이동
        Vector3 a_StepVec = a_MoveDir.normalized * a_MoveVelocity * Time.deltaTime;
        transform.Translate(a_StepVec, Space.World);

        //--- 이동 방향을 바라보도록 회전 처리
        float a_RotSpeed = 7.0f;  // 초당 회전 속도
        Quaternion a_TargetRot = Quaternion.LookRotation(a_MoveDir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * a_RotSpeed);
    }

    // 추격 애니메이션 활성화
    animator.SetBool("IsAttack", false);
    animator.SetBool("IsTrace", true);
}
break;

```

날아다니는 몬스터는 y축을 포함하여 추격
<summary>예시 코드</summary>
  
```csharp
case MonsterState.trace:
{
    // 추적 이동 구현
    float a_MoveVelocity = 1.0f; // 초당 이동 속도
    Vector3 a_MoveDir = playerTr.position - transform.position; // y축 포함한 이동 방향

    if (0.0f < a_MoveDir.magnitude)
    {
        // 이동 벡터 계산 및 이동
        Vector3 a_StepVec = a_MoveDir.normalized * a_MoveVelocity * Time.deltaTime;
        transform.Translate(a_StepVec, Space.World);

        // 이동 방향을 바라보도록 회전 처리
        float a_RotSpeed = 7.0f; // 초당 회전 속도
        Quaternion a_TargetRot = Quaternion.LookRotation(a_MoveDir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * a_RotSpeed);
    }

```
</details>

#06) 드래그 앤 드롭으로 상점 구현
<details>
<summary>예시 코드</summary>
```csharp
  
using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DragAndDropMgr : MonoBehaviour
{
    public SlotScript[] m_ProductSlots;     // 상품 슬롯
    public SlotScript[] m_InvenSlots;       // TargetSlots
    public Image m_MsObj = null;            //마우스를 따라 다녀야 하는 오브젝트
    int m_SaveIndex = -1;       //-1이 아니면 아템을 잡은 상태에서 드래그 중이라는 뜻

    public Text m_BagSizeText;
    public Text m_HelpText;
    float m_HelpDuring = 2.0f;
    float m_HelpAddTimer = 0.0f;
    float m_CacTimer = 0.0f;
    Color m_Color;

    Store_Mgr m_StMgr = null;

    // Start is called before the first frame update
    void Start()
    {
        m_StMgr = GameObject.FindObjectOfType<Store_Mgr>();

        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) == true)
        {   //왼쪽 마우스 버튼 클릭하는 순간
            MouseBtnDown();
        }

        if(Input.GetMouseButton(0) == true)
        {   //왼쪽 마우스 버튼을 누르고 있는 동안
            MousePress();
        }

        if(Input.GetMouseButtonUp(0) == true)
        {   //왼쪽 마우스 버튼을 누르다가 떼는 순간
            MouseBtnUp();
        }

        //--- HelpText 서서히 사라지게 처리하는 연출
        if(0.0f < m_HelpAddTimer)
        {
            m_HelpAddTimer -= Time.deltaTime;
            m_CacTimer = m_HelpAddTimer / (m_HelpDuring - 1.0f);
            if (1.0f < m_CacTimer)
                m_CacTimer = 1.0f;
            m_Color = m_HelpText.color;
            m_Color.a = m_CacTimer;
            m_HelpText.color = m_Color;

            if (m_HelpAddTimer <= 0.0f)
                m_HelpText.gameObject.SetActive(false);
        }
        //--- HelpText 서서히 사라지게 처리하는 연출

    }//void Update()

    void MouseBtnDown()
    {
        m_SaveIndex = -1;

        for(int i = 0; i < m_ProductSlots.Length; i++)
        {
            if (m_ProductSlots[i].ItemImg.gameObject.activeSelf == true &&
                IsCollSlot(m_ProductSlots[i]) == true)
            {
                m_SaveIndex = i;
                Transform a_ChildImg = m_MsObj.transform.Find("MsIconImg");
                if (a_ChildImg != null)
                    a_ChildImg.GetComponent<Image>().sprite =
                                    m_ProductSlots[i].ItemImg.sprite;
                //m_ProductSlots[i].ItemImg.gameObject.SetActive(false);
                m_MsObj.gameObject.SetActive(true);
                break;
            }
        }//for(int i = 0; i < m_ProductSlots.Length; i++)

    }//void MouseBtnDown()

    void MousePress()
    {
        if (0 <= m_SaveIndex)
            m_MsObj.transform.position = Input.mousePosition;

    }//void MousePress()

    void MouseBtnUp()
    {
        if (m_SaveIndex < 0 || m_ProductSlots.Length <= m_SaveIndex)
            return;

        //장착하기 코드...
        int a_BuyIndex = -1;
        for(int i = 0; i < m_InvenSlots.Length; i++)
        {
            if (IsCollSlot(m_InvenSlots[i]) == true)
            {
                if(m_SaveIndex != i)  //다른 슬롯에 장착하려고 시도한 경우
                {
                    //메시지 출력
                    ShowMessage("해당 슬롯에는 아이템을 장착할 수 없습니다.");
                    continue;
                }

                if (BuySkItem(m_SaveIndex) == true)
                { //여기서 상품 구매 시도 함수 호출 (함수 호출 결과 성공이 일때만 아래 코드 실행 되게 처리)
                    a_BuyIndex = i;
                    break;
                }
            }//if (IsCollSlot(m_InvenSlots[i]) == true)
        }//for(int i = 0; i < m_InvenSlots.Length; i++)

        if(0 <= a_BuyIndex)
        {
            Sprite a_MsIconImg = null;
            Transform a_ChildImg = m_MsObj.transform.Find("MsIconImg");
            if (a_ChildImg != null)
                a_MsIconImg = a_ChildImg.GetComponent<Image>().sprite;

            m_InvenSlots[a_BuyIndex].ItemImg.sprite = a_MsIconImg;
            m_InvenSlots[a_BuyIndex].ItemImg.gameObject.SetActive(true);
            m_InvenSlots[a_BuyIndex].m_CurItemIdx = a_BuyIndex;
        }//if(0 <= a_BuyIndex)
        //else
        //{
        //    m_ProductSlots[m_SaveIndex].ItemImg.gameObject.SetActive(true);
        //}

        m_SaveIndex = -1;
        m_MsObj.gameObject.SetActive(false);

    }//void MouseBtnUp()

    bool IsCollSlot(SlotScript a_CkSlot)
    {   //마우스가 UI 슬롯 위헤 있는지? 판단하는 함수

        if(a_CkSlot == null)
            return false;

        Vector3[] v = new Vector3[4];
        a_CkSlot.GetComponent<RectTransform>().GetWorldCorners(v);
        //v[0] : 좌측하단   v[1] : 좌측상단   v[2] : 우측상단   v[3] : 우측하단
        //v변수의 좌표계 : 화면의 좌측하단 0, 0이고 우측상단(최고점 예 1280, 720)인 좌표계
        //마우스 좌표계 : 화면의 좌측하단이 0, 0이고 우측상단(최고점 예 1280, 720)인 좌표계
        //UI 좌표계 : 앵커가 센터일 때 중앙 0, 0 인 좌표계
        if (v[0].x <= Input.mousePosition.x && Input.mousePosition.x <= v[2].x &&
            v[0].y <= Input.mousePosition.y && Input.mousePosition.y <= v[2].y)
        {
            return true;
        }

        return false;

    }// bool IsCollSlot(SlotScript a_CkSlot)

    void ShowMessage(string a_Mess)
    {
        if (m_HelpText == null)
            return;

        m_HelpText.text = a_Mess;
        m_HelpText.gameObject.SetActive(true);
        m_HelpAddTimer = m_HelpDuring;
    }

    bool BuySkItem(int a_SkIdx)  //구매 시도 함수
    {
        int a_Cost = 300;
        if (a_SkIdx == 1)
            a_Cost = 500;
        else if (a_SkIdx == 2)
            a_Cost = 1000;

        if (GlobalValue.g_UserGold < a_Cost)
        {
            ShowMessage("골드가 부족합니다.");
            return false;
        }

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_CurBagSize += GlobalValue.g_SkillCount[i];

        if(10 <= a_CurBagSize)
        {
            ShowMessage("가방이 가득 찼습니다.");
            return false;
        }

        // 정식 구매 과정은 드래그 앤 드롭 시 확인 다이알로그를 띄우고 유저의 동의 후
        // 서버에서 구매 확인, 승인 후 클라이언트에 응답을 주고 UI를 갱신해 주는
        // 과정으로 진행해야 한다.

        GlobalValue.g_SkillCount[a_SkIdx]++;
        GlobalValue.g_UserGold -= a_Cost;

        //--- 변동 사항 로컬에 저장
        string a_MkKey = "SkItem_" + a_SkIdx.ToString();
        PlayerPrefs.SetInt(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx]);
        PlayerPrefs.SetInt("UserGold", GlobalValue.g_UserGold);
        //--- 변동 사항 로컬에 저장

        // 플레이팹에 저장
        SaveDataToPlayFab(a_MkKey, GlobalValue.g_SkillCount[a_SkIdx].ToString());
        SaveDataToPlayFab("UserGold", GlobalValue.g_UserGold.ToString());

        RefreshUI();  //<-- UI 갱신

        return true;
    }

    void RefreshUI()
    {
        for(int i = 0; i < m_InvenSlots.Length; i++)
        {
            if(0 < GlobalValue.g_SkillCount[i])
            {
                m_InvenSlots[i].ItemCountText.text = GlobalValue.g_SkillCount[i].ToString();
                m_InvenSlots[i].ItemImg.sprite = m_ProductSlots[i].ItemImg.sprite;
                m_InvenSlots[i].ItemImg.gameObject.SetActive(true);
                m_InvenSlots[i].m_CurItemIdx = i;
            }
            else
            {
                m_InvenSlots[i].ItemCountText.text = "0";
                m_InvenSlots[i].ItemImg.gameObject.SetActive(false);
            }
        }//for(int i = 0; i < m_InvenSlots.Length; i++)

        if (m_StMgr != null && m_StMgr.m_UserInfoText != null)
            m_StMgr.m_UserInfoText.text = "별명(" + GlobalValue.g_NickName + ") : 보유골드(" +
                                        GlobalValue.g_UserGold + ")";

        int a_CurBagSize = 0;
        for (int i = 0; i < GlobalValue.g_SkillCount.Length; i++)
            a_CurBagSize += GlobalValue.g_SkillCount[i];
        m_BagSizeText.text = "가방사이즈 : " + a_CurBagSize + " / 10";

    }//void RefreshUI()

    private void SaveDataToPlayFab(string key, string value)
    {
        var data = new Dictionary<string, string> { { key, value } };
        var request = new UpdateUserDataRequest { Data = data };

        PlayFabClientAPI.UpdateUserData(request, OnDataSaveSuccess, OnDataSaveFailure);
    }
    private void OnDataSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Data saved successfully to PlayFab.");
    }

    private void OnDataSaveFailure(PlayFabError error)
    {
        Debug.LogError("Failed to save data to PlayFab: " + error.GenerateErrorReport());
    }

}

```

![Alien Swarm 2024-11-11 15-12-07](https://github.com/user-attachments/assets/7cfed9ce-57b1-48dd-a438-b66cc8bd4ac4)

</details>


#06) 몬스터를 자동으로 공격하는 터렛 구현
<details>
<summary>예시 코드</summary>
```csharp
using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameObject projectilePrefab;   // 발사할 프리팹
    public float fireRange = 100f;         // 공격 범위
    public Transform firePoint;           // 발사 위치

    private float fireTimer;
    public GameObject Player;

    private void Start()
    {
        Player = GameObject.Find("Player");


        // Resources 폴더에서 TBullet 프리팹을 로드
        projectilePrefab = Resources.Load<GameObject>("TBullet");

        // 자식 오브젝트 중 firepos라는 이름의 오브젝트를 firePoint로 설정
        Transform foundFirePoint = transform.Find("firepos");
        if (foundFirePoint != null)
        {
            firePoint = foundFirePoint;
        }
        else
        {
            Debug.LogWarning("firepos라는 이름의 자식 오브젝트를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= Player.GetComponent<PlayerCtrl>().Tfirespeed)
        {
            GameObject target = FindClosestMonster();
            if (target != null)
            {
                RotateTowardsTarget(target);
                FireProjectile();
            }
            fireTimer = 0f;
        }
    }

    GameObject FindClosestMonster()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");
        GameObject closestMonster = null;
        float shortestDistance = fireRange;

        foreach (GameObject monster in monsters)
        {
            float distanceToMonster = Vector3.Distance(transform.position, monster.transform.position);
            if (distanceToMonster < shortestDistance)
            {
                shortestDistance = distanceToMonster;
                closestMonster = monster;
            }
        }
        return closestMonster;
    }

    void RotateTowardsTarget(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;  // Y축 회전만 고려

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
    }

    void FireProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // 현재 터렛의 Y 회전을 가져와서 firePoint의 회전에 적용
            Quaternion projectileRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            Instantiate(projectilePrefab, firePoint.position, projectileRotation);
        }
    }
}

```
![Alien Swarm 2024-11-11 15-19-18 (3)](https://github.com/user-attachments/assets/a3705a06-b26c-4872-ad22-44cb4d552179)
</details>
