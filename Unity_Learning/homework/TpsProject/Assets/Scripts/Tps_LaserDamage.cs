using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tps_LaserDamage : MonoBehaviour
{
    public int damage = 30;
    public float damageDelay = 1;

    private float lastDamageTime = 0;
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag(Tags.player);

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player && Time.time > lastDamageTime + damageDelay)
        {
            player.GetComponent<Tps_PlayerHealth>().TakeDamage(damage);
            lastDamageTime = Time.time;

        }
    }
}
