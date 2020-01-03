using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tps_EnemyHealth : MonoBehaviour
{
    public float hp = 100;
   // public float TimeToDestroy = 10.0f;
    private Animator anim;
    private HashIDs hash;
    private bool isDead = false;
   // private float timer = 0f;


    private void Start()
    {
        anim = this.GetComponent<Animator>();
        hash = GameObject.FindGameObjectWithTag(Tags.gameController).GetComponent<HashIDs>();
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0 && !isDead)
        {
            isDead = true;
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<Tps_EnemyAnimation>().enabled = false;
            GetComponent<Tps_EnemyAI>().enabled = false;
            GetComponent<Tps_EnemySight>().enabled = false;
            GetComponent<Tps_EnemyShoot>().enabled = false;
            GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
            GetComponentInChildren<Light>().enabled = false;
            GetComponentInChildren<LineRenderer>().enabled = false;

            anim.SetBool(hash.playerInSightBool, false);
            anim.SetBool(hash.deadBool, true);




        }
    }
}
