using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public delegate void PlayerShoot();
public class Tps_ActionController : MonoBehaviour
{
    public static event PlayerShoot PlayerShootEvent;
    public float fireRate = 0.1f;
    public float damage = 40;
    public float reloadTime = 1.5f;
    public float flashRate = 0.2f;
    public AudioClip fireAudio;
    public AudioClip damageAudio;
    public AudioClip reloadAudio;
    public AudioClip dryFireAudio;
    public GameObject explosion;
    public int bulletCount = 30;
    public int chargerBulletCount = 120;
    public string weaponInfo= "武器: ";
    public Text bulletText;
    public Text healthText;
    public Text weaponText;

    public float TimeToDestroy = 5.0f;

    private string reloadAnim = "heavy_combat_reload";
    private string fireAnim = "heavy_crouch_shoot";
    private string walkAnim = "heavy_combat_walk";
    private string runAnim = "heavy_combat_run";
    private string jumpAnim = "heavy_jump_3_land";
    private string idleAnim = "heavy_combat_idle";

    private Animation anim;
    private float nextFireTime = 0.0f;
    //private MeshRenderer flash;
    private int currentBullet;
    private int currentChargerBullet;
    private Tps_PlayerParameter parameter;
    private Tps_PlayerControl playerControl;
    private Tps_PlayerHealth playerHealth;
    private Tps_EnemyHealth enemyHealth;
    private Tps_PlayerInventory playerInventory;
    private List<Tps_Weapon> weaponContainer;
    private Tps_MessageController messageController;
    private NetworkManager networkManager;
    

    private float healthTimer;
    private float destroyTimer = 0f;
    private bool isDestroy = false;

    private void Start()
    {
        parameter = GameObject.FindGameObjectWithTag(Tags.player).GetComponent<Tps_PlayerParameter>();
        playerControl = GameObject.FindGameObjectWithTag(Tags.player).GetComponent<Tps_PlayerControl>();
        playerHealth = GameObject.FindGameObjectWithTag(Tags.player).GetComponent<Tps_PlayerHealth>();
        playerInventory = GameObject.FindGameObjectWithTag(Tags.player).GetComponent<Tps_PlayerInventory>();
        messageController = Tps_MessageController.Instance;
        anim = this.GetComponent<Animation>();
        enemyHealth = GameObject.FindGameObjectWithTag(Tags.enermy).GetComponent<Tps_EnemyHealth>();

        networkManager = GameObject.FindGameObjectWithTag(Tags.networkManager).GetComponent<NetworkManager>();

        weaponContainer = new List<Tps_Weapon>(GameObject.FindGameObjectWithTag(Tags.weaponContainer).GetComponentsInChildren<Tps_Weapon>());
        weaponContainer[0].gameObject.SetActive(true);
        weaponContainer[1].gameObject.SetActive(false);
        //flash = this.transform.FindChild("muzzle_flash").GetComponent<MeshRenderer>();
        //currentBullet = bulletCount;
        //currentChargerBullet = chargerBulletCount;
        bulletText.text = currentBullet + "/" + currentChargerBullet;
        healthText.text = "血量: "+ playerHealth.hp + "";
        weaponText.text = weaponInfo;
        healthTimer = 0;

    }

    private void Update()
    {

        if (healthTimer < 1.0f)
            healthTimer += Time.deltaTime;
        else
        {
            healthText.text = "血量: "+Mathf.RoundToInt(playerHealth.hp) + "";
            healthTimer = 0;
        }
        if (!isDestroy && enemyHealth.hp <= 0)
        {
            if (destroyTimer < TimeToDestroy)
                destroyTimer += Time.deltaTime;
            else
            {
                destroyTimer = 0f;
                isDestroy = true;
                Destroy(enemyHealth.gameObject);
            }
        }


        if (playerInventory.WeaponTypeNumber >= 2)
        {
            if (parameter.inputFirstWeapon && weaponContainer.Count >= 1)
            {
                weaponContainer[0].gameObject.SetActive(true);
                weaponContainer[1].gameObject.SetActive(false);


            }

            if (parameter.inputSecondWeapon && weaponContainer.Count >= 2)
            {
                weaponContainer[0].gameObject.SetActive(false);
                weaponContainer[1].gameObject.SetActive(true);
            }
        }

        if (parameter.inputReload && currentBullet < bulletCount)
            Reload();

        if (parameter.inputFire && !anim.IsPlaying(reloadAnim))
            Fire();
        else if (!anim.IsPlaying(reloadAnim))
            StateAnim(playerControl.State);

        SendGameDataToMessageController();

    }

    private void ReloadAnim()
    {
        anim.Stop(reloadAnim);
        anim[reloadAnim].speed = (anim[reloadAnim].clip.length / reloadTime);
        anim.Rewind(reloadAnim);
        anim.Play(reloadAnim);
    }

    private IEnumerator ReloadFinish()
    {
        yield return new WaitForSeconds(reloadTime);
        if (currentChargerBullet >= bulletCount - currentBullet)
        {
            currentChargerBullet -= (bulletCount - currentBullet);
            currentBullet = bulletCount;
        }
        else
        {
            currentBullet += currentChargerBullet;
            currentChargerBullet = 0;
        }
        bulletText.text = currentBullet + "/" + currentChargerBullet;
    }

    private void Reload()
    {
        if (!anim.IsPlaying(reloadAnim))
        {
            if (currentChargerBullet > 0)
                StartCoroutine(ReloadFinish());
            else
            {
                anim.Rewind(fireAnim);
                anim.Play(fireAnim);
                AudioSource.PlayClipAtPoint(dryFireAudio, transform.position);
                return;
            }
            AudioSource.PlayClipAtPoint(reloadAudio, transform.position);
            ReloadAnim();
        }
    }
    /*
    private IEnumerator Flash()
    {
        flash.enabled=true;
        yield return new WaitForSeconds(flashRate);
        flash.enabled=false;
    }
    */

    private void Fire()
    {
        if (Time.time > nextFireTime)
        {
            if (currentBullet <= 0)
            {
                Reload();
                nextFireTime = Time.time + fireRate;
                return;
            }
            currentBullet--;
            bulletText.text = currentBullet + "/" + currentChargerBullet;
            DamageEnermy();
            if (PlayerShootEvent != null)
                PlayerShootEvent();
            AudioSource.PlayClipAtPoint(fireAudio, transform.position);
            nextFireTime = Time.time + fireRate;
            anim.Rewind(fireAnim);
            anim.Play(fireAnim);
            //StartCoroutine(Flash());
        }
    }

    private void DamageEnermy()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.tag == Tags.enermy && hit.collider is CapsuleCollider)
            {
                AudioSource.PlayClipAtPoint(damageAudio, hit.transform.position);
                GameObject go = Instantiate(explosion, hit.point, Quaternion.identity);
                Destroy(go, 3);
                hit.transform.GetComponent<Tps_EnemyHealth>().TakeDamage(damage);
            }
        }
    }

    private void PlayerStateAnim(string animName)
    {
        if (!anim.IsPlaying(animName))
        {
            anim.Rewind(animName);
            anim.Play(animName);
        }
    }

    private void StateAnim(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
                PlayerStateAnim(idleAnim);
                break;
            case PlayerState.Walk:
                PlayerStateAnim(walkAnim);
                break;
            case PlayerState.Crouch:
                PlayerStateAnim(walkAnim);
                break;
            case PlayerState.Run:
                PlayerStateAnim(runAnim);
                break;
        }
    }

    private void SendGameDataToMessageController()
    {
        Dictionary<string, string> info = new Dictionary<string, string>()
        {
            { "code", "gds" },
            { "bulletCount", bulletCount.ToString() },
            { "chargerBulletCount", chargerBulletCount.ToString() },
            { "playerHealth", playerHealth.ToString() },
            { "weaponInfo", weaponInfo }
        };
        messageController.AddSndInfoIntoQueue(info);

    }
    public void AddBullet(int BulletPackageAmount)
    {
        currentChargerBullet += BulletPackageAmount;
        bulletText.text = currentBullet + "/" + currentChargerBullet;
    }

    public void AddWeaponType(Tps_Weapon weapon)
    {
        playerInventory.AddWeapon(weapon);
        weaponText.text += (" " + (playerInventory.FindeWeaponIndex(weapon)+1) + ": " + weapon.WeaponName);

        if (!playerInventory.HasWeapon(weapon))
        {
            weaponContainer.Add(weapon);
        }
    }

    public void SetBullet(int cb,int ccb)
    {
        currentBullet = cb;
        currentChargerBullet = ccb;
    }
    public void SetWeaponInfo(string wi)
    {
        weaponInfo = wi;
    }

}
