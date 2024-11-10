using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//반드시 필요한 컴포넌트를 명시해 해당 컴포넌트가 삭제되는 것을 방지하는 Attribute
[RequireComponent(typeof(AudioSource))]
public class FireCtrl : MonoBehaviour
{
    //총알 프리팹
    public GameObject bullet;
    //총알 발사좌표
    public Transform firePos;

    // 더블샷용
    public Transform DfirePos;
    public Transform DfirePos2;

    float fireTimer = 0.0f;

    //충알 발사 사운드
    public AudioClip fireSfx;
    //AudioSource 컴포넌트를 저장할 변수
    private AudioSource source = null;
    //MuzzleFlash의 MeshRenderer 컴포넌트 연결 변수
    public MeshRenderer muzzleFlash;

    //--- 레이저 포인터 관련 변수
    public GameObject LaserPointer = null;
    LayerMask m_LaserMask = -1;
    //--- 레이저 포인터 관련 변수

    //수류탄 프리팹
    public GameObject m_Grenade;

    private int bulletCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        //AudioSource 컴포넌트를 추출한 후 변수에 할당
        source = GetComponent<AudioSource>();
        //최초에 MuzzleFlash MeshRenderer를 비활성화
        muzzleFlash.enabled = false;

        m_LaserMask = 1 << LayerMask.NameToLayer("BULLET");
        m_LaserMask |= 1 << LayerMask.NameToLayer("E_BULLET");
        m_LaserMask = ~m_LaserMask; //위에 설정한 레이어만 제외
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMgr.s_GameState == GameState.GameEnd)
            return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0.0f)
        {
            fireTimer = 0.0f;

            //마우스 왼쪽 버튼을 클릭했을 때 Fire 함수 호출
            if (Input.GetMouseButton(0))
            {
                if (GameMgr.IsPointerOverUIObject() == false)
                {
                    Fire();
                }

                fireTimer = gameObject.GetComponent<PlayerCtrl>().firespeed;
            }
        }//if (fireTimer <= 0.0f)

#region --- LaserPointer 표시
        if(Physics.Raycast(firePos.position, FollowCam.m_RifleDir.normalized, 
                            out RaycastHit hit, 60.0f, m_LaserMask) == true) 
        {
            float a_Dist = Vector3.Distance(firePos.position, hit.point);
            float a_CacScale = a_Dist * 0.3f;
            if (a_CacScale < 1.5f)
                a_CacScale = 1.5f;
            if (5.0f < a_CacScale)
                a_CacScale = 5.0f;  //거리에 따라 레이저 도트 크기 계산

            float a_CacOffSet = a_Dist / 30.0f;
            if (a_CacOffSet < 0.1f)
                a_CacOffSet = 0.1f;
            if (0.5f < a_CacOffSet)
                a_CacOffSet = 0.5f;  //벽에서 떨어트려 줄 거리 계산

            LaserPointer.transform.position =
                          hit.point + (-FollowCam.m_RifleDir.normalized * a_CacOffSet);
            LaserPointer.transform.localScale =
                          new Vector3(a_CacScale, a_CacScale, a_CacScale);
            LaserPointer.transform.forward = Camera.main.transform.forward;  //빌보드 처리
        }
        else //벽이 60m 밖에 있을 때 
        {
            LaserPointer.transform.position = firePos.position +
                                            FollowCam.m_RifleDir.normalized * 59.0f;
            LaserPointer.transform.localScale = new Vector3(5.0f, 5.0f, 5.0f);
            LaserPointer.transform.forward = Camera.main.transform.forward; //빌보드 처리
        }
#endregion

    }//void Update()

    void Fire()
    {
        //if (GameMgr.IsPointerOverUIObject() == true)
        //    return;

        if (gameObject.GetComponent<PlayerCtrl>().doubleshot == false)
            //동적으로 총알을 생성하는 함수
            CreateBullet();
        else
        {
            DoubleCreateBullet();
        }
        //사운드 발생 함수
        source.PlayOneShot(fireSfx, 0.9f);
        //잠시 기다리는 루틴을 위해 코루틴 함수로 호출
        StartCoroutine(this.ShowMuzzleFlash());
    }

    void CreateBullet()
    {
        if (bulletCount >= 100 && gameObject.GetComponent<PlayerCtrl>().grshot == true)
        {
            FireGrenade();
            bulletCount = 0; // 카운트 초기화
            return;
        }

        Instantiate(bullet, firePos.position, firePos.rotation);
        bulletCount++; // 탄환 수 증가
    }

    void DoubleCreateBullet()
    {
        if (bulletCount >= 100 && gameObject.GetComponent<PlayerCtrl>().grshot == true)
        {
            FireGrenade();
            bulletCount = 0; // 카운트 초기화
            return;
        }
        //Bullet 프리팹을 동적으로 생성
        Instantiate(bullet, DfirePos.position, DfirePos.rotation);
        Instantiate(bullet, DfirePos2.position, DfirePos2.rotation);
        bulletCount++; // 탄환 수 증가
    }
    //MuzzleFlash 활성 / 비활성화를 짧은 시간 동안 반복
    IEnumerator ShowMuzzleFlash()
    {
        //MuzzleFlash 스케일을 불규칙하게 변경
        float scale = Random.Range(1.0f, 2.0f);
        muzzleFlash.transform.localScale = Vector3.one * scale;

        //MuzzleFlash를 Z축을 기준으로 불규칙하게 회전시킴
        Quaternion rot = Quaternion.Euler(0, 0, Random.Range(0, 360.0f));
        muzzleFlash.transform.localRotation = rot;

        //활성화해서 보이게 함
        muzzleFlash.enabled = true;

        //불규칙적인 시간 동안 Delay한 다음 MeshRenderer를 비활성화
        yield return new WaitForSeconds(Random.Range(0.01f, 0.03f));

        //비활성화해서 보이지 않게 함
        muzzleFlash.enabled = false;

    }//IEnumerator ShowMuzzleFlash()

    public void FireGrenade()
    {
        GameObject a_Grenade = Instantiate(m_Grenade, firePos.position, firePos.rotation);

        if(a_Grenade != null)
        {
            GrenadeCtrl a_Grd = a_Grenade.GetComponent<GrenadeCtrl>();
            if (a_Grd != null)
                a_Grd.SetForwardDir(FollowCam.m_RifleDir.normalized);
        }
    }//public void FireGrenade()
}
