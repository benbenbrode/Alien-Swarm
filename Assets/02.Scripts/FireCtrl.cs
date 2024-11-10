using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�ݵ�� �ʿ��� ������Ʈ�� ����� �ش� ������Ʈ�� �����Ǵ� ���� �����ϴ� Attribute
[RequireComponent(typeof(AudioSource))]
public class FireCtrl : MonoBehaviour
{
    //�Ѿ� ������
    public GameObject bullet;
    //�Ѿ� �߻���ǥ
    public Transform firePos;

    // ������
    public Transform DfirePos;
    public Transform DfirePos2;

    float fireTimer = 0.0f;

    //��� �߻� ����
    public AudioClip fireSfx;
    //AudioSource ������Ʈ�� ������ ����
    private AudioSource source = null;
    //MuzzleFlash�� MeshRenderer ������Ʈ ���� ����
    public MeshRenderer muzzleFlash;

    //--- ������ ������ ���� ����
    public GameObject LaserPointer = null;
    LayerMask m_LaserMask = -1;
    //--- ������ ������ ���� ����

    //����ź ������
    public GameObject m_Grenade;

    private int bulletCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        //AudioSource ������Ʈ�� ������ �� ������ �Ҵ�
        source = GetComponent<AudioSource>();
        //���ʿ� MuzzleFlash MeshRenderer�� ��Ȱ��ȭ
        muzzleFlash.enabled = false;

        m_LaserMask = 1 << LayerMask.NameToLayer("BULLET");
        m_LaserMask |= 1 << LayerMask.NameToLayer("E_BULLET");
        m_LaserMask = ~m_LaserMask; //���� ������ ���̾ ����
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

            //���콺 ���� ��ư�� Ŭ������ �� Fire �Լ� ȣ��
            if (Input.GetMouseButton(0))
            {
                if (GameMgr.IsPointerOverUIObject() == false)
                {
                    Fire();
                }

                fireTimer = gameObject.GetComponent<PlayerCtrl>().firespeed;
            }
        }//if (fireTimer <= 0.0f)

#region --- LaserPointer ǥ��
        if(Physics.Raycast(firePos.position, FollowCam.m_RifleDir.normalized, 
                            out RaycastHit hit, 60.0f, m_LaserMask) == true) 
        {
            float a_Dist = Vector3.Distance(firePos.position, hit.point);
            float a_CacScale = a_Dist * 0.3f;
            if (a_CacScale < 1.5f)
                a_CacScale = 1.5f;
            if (5.0f < a_CacScale)
                a_CacScale = 5.0f;  //�Ÿ��� ���� ������ ��Ʈ ũ�� ���

            float a_CacOffSet = a_Dist / 30.0f;
            if (a_CacOffSet < 0.1f)
                a_CacOffSet = 0.1f;
            if (0.5f < a_CacOffSet)
                a_CacOffSet = 0.5f;  //������ ����Ʈ�� �� �Ÿ� ���

            LaserPointer.transform.position =
                          hit.point + (-FollowCam.m_RifleDir.normalized * a_CacOffSet);
            LaserPointer.transform.localScale =
                          new Vector3(a_CacScale, a_CacScale, a_CacScale);
            LaserPointer.transform.forward = Camera.main.transform.forward;  //������ ó��
        }
        else //���� 60m �ۿ� ���� �� 
        {
            LaserPointer.transform.position = firePos.position +
                                            FollowCam.m_RifleDir.normalized * 59.0f;
            LaserPointer.transform.localScale = new Vector3(5.0f, 5.0f, 5.0f);
            LaserPointer.transform.forward = Camera.main.transform.forward; //������ ó��
        }
#endregion

    }//void Update()

    void Fire()
    {
        //if (GameMgr.IsPointerOverUIObject() == true)
        //    return;

        if (gameObject.GetComponent<PlayerCtrl>().doubleshot == false)
            //�������� �Ѿ��� �����ϴ� �Լ�
            CreateBullet();
        else
        {
            DoubleCreateBullet();
        }
        //���� �߻� �Լ�
        source.PlayOneShot(fireSfx, 0.9f);
        //��� ��ٸ��� ��ƾ�� ���� �ڷ�ƾ �Լ��� ȣ��
        StartCoroutine(this.ShowMuzzleFlash());
    }

    void CreateBullet()
    {
        if (bulletCount >= 100 && gameObject.GetComponent<PlayerCtrl>().grshot == true)
        {
            FireGrenade();
            bulletCount = 0; // ī��Ʈ �ʱ�ȭ
            return;
        }

        Instantiate(bullet, firePos.position, firePos.rotation);
        bulletCount++; // źȯ �� ����
    }

    void DoubleCreateBullet()
    {
        if (bulletCount >= 100 && gameObject.GetComponent<PlayerCtrl>().grshot == true)
        {
            FireGrenade();
            bulletCount = 0; // ī��Ʈ �ʱ�ȭ
            return;
        }
        //Bullet �������� �������� ����
        Instantiate(bullet, DfirePos.position, DfirePos.rotation);
        Instantiate(bullet, DfirePos2.position, DfirePos2.rotation);
        bulletCount++; // źȯ �� ����
    }
    //MuzzleFlash Ȱ�� / ��Ȱ��ȭ�� ª�� �ð� ���� �ݺ�
    IEnumerator ShowMuzzleFlash()
    {
        //MuzzleFlash �������� �ұ�Ģ�ϰ� ����
        float scale = Random.Range(1.0f, 2.0f);
        muzzleFlash.transform.localScale = Vector3.one * scale;

        //MuzzleFlash�� Z���� �������� �ұ�Ģ�ϰ� ȸ����Ŵ
        Quaternion rot = Quaternion.Euler(0, 0, Random.Range(0, 360.0f));
        muzzleFlash.transform.localRotation = rot;

        //Ȱ��ȭ�ؼ� ���̰� ��
        muzzleFlash.enabled = true;

        //�ұ�Ģ���� �ð� ���� Delay�� ���� MeshRenderer�� ��Ȱ��ȭ
        yield return new WaitForSeconds(Random.Range(0.01f, 0.03f));

        //��Ȱ��ȭ�ؼ� ������ �ʰ� ��
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
