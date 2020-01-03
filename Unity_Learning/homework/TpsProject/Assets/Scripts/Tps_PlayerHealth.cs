using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tps_PlayerHealth : MonoBehaviour
{
    public bool isDead;
    public float resetAfterDeathTime = 0.5f;
    public AudioClip deathClip;
    public AudioClip damageClip;
    public float maxHp = 100;
    public float hp = 100;
    public float recoverSpeed = 1;


    private float timer = 0;
    private float healthTimer = 0;
    private FadeInOut fader;

    private void Start()
    {
        hp = maxHp;
        fader = GameObject.FindGameObjectWithTag(Tags.fader).GetComponent<FadeInOut>();

        
    }

    private void Update()
    {
        if (!isDead)
        {
            hp += recoverSpeed * Time.deltaTime;
            if (hp > maxHp)
                hp = maxHp;

            

        }
        if (hp <= 0)
        {
            if (!isDead)
                PlayerDead();
            else
                LevelReset();
        }

    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;
        AudioSource.PlayClipAtPoint(damageClip, transform.position);
        hp -= damage;
    }
    //禁用用户输入
    public void DisableInput()
    {
        this.GetComponent<AudioSource>().enabled = false;
        this.GetComponent<Tps_PlayerControl>().enabled = false;
        this.GetComponent<Tps_TpInput>().enabled = false;
        if (GameObject.Find("Canvas") != null)
            GameObject.Find("Canvas").SetActive(false);
        GameObject.FindGameObjectWithTag(Tags.mainCamera).GetComponent<Tps_TpCamera>().enabled = false;
    }

    public void PlayerDead()
    {
        isDead = true;
        DisableInput();
        AudioSource.PlayClipAtPoint(deathClip, transform.position);
    }

    public void LevelReset()
    {
        timer += Time.deltaTime;
        if (timer >= resetAfterDeathTime)
            fader.EndScene();

    }
}
